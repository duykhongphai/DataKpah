using System.Buffers.Binary;
using System.Net.Sockets;
using System.Text;

namespace DataKpah;

public sealed record GoiTin(sbyte Lenh, byte[] DuLieu);

public sealed class BoDoc
{
    private readonly byte[] data;
    private int pos;

    public BoDoc(byte[] data) => this.data = data;
    public int Con => data.Length - pos;

    public sbyte Byte()
    {
        Can(1);
        return unchecked((sbyte)data[pos++]);
    }

    public byte UByte()
    {
        Can(1);
        return data[pos++];
    }

    public bool Bool() => Byte() > 0;

    public short Short()
    {
        Can(2);
        var n = BinaryPrimitives.ReadInt16BigEndian(data.AsSpan(pos, 2));
        pos += 2;
        return n;
    }

    public ushort UShort()
    {
        Can(2);
        var n = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(pos, 2));
        pos += 2;
        return n;
    }

    public int Int()
    {
        Can(4);
        var n = BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(pos, 4));
        pos += 4;
        return n;
    }

    public string Utf()
    {
        var n = UShort();
        return Encoding.UTF8.GetString(Bytes(n));
    }

    public byte[] Bytes(int n)
    {
        Can(n);
        var b = data.AsSpan(pos, n).ToArray();
        pos += n;
        return b;
    }

    public byte[] ConLai() => Bytes(Con);

    private void Can(int n)
    {
        if (n < 0 || pos + n > data.Length)
            throw new EndOfStreamException($"Gói thiếu dữ liệu tại {pos}, cần {n}, còn {Con}.");
    }
}

public sealed class BoGhi
{
    private readonly MemoryStream stream = new();

    public BoGhi Byte(int n) { stream.WriteByte(unchecked((byte)n)); return this; }

    public BoGhi Short(int n)
    {
        Span<byte> b = stackalloc byte[2];
        BinaryPrimitives.WriteInt16BigEndian(b, unchecked((short)n));
        stream.Write(b);
        return this;
    }

    public BoGhi Int(int n)
    {
        Span<byte> b = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(b, n);
        stream.Write(b);
        return this;
    }

    public BoGhi Utf(string value)
    {
        var b = Encoding.UTF8.GetBytes(value ?? "");
        if (b.Length > ushort.MaxValue) throw new InvalidDataException("Chuỗi UTF quá dài.");
        Short(b.Length);
        stream.Write(b);
        return this;
    }

    public byte[] Xong() => stream.ToArray();
}

/// <summary>
/// Kết nối TeaMobi/ME của KPAH, theo <c>Session_ME</c> của client 2.7.6: mọi byte sau bắt tay
/// (cmd -40) XOR với khoá cuộn; gói lớn mở đầu bằng lệnh -128, byte kế là lệnh thật rồi 4 byte
/// độ dài (giải khoá từ byte cuối về byte đầu, đúng thứ tự client).
/// </summary>
public sealed class PhienKpah : IAsyncDisposable
{
    private const sbyte LenhKhoa = -40;
    private const sbyte LenhLon = sbyte.MinValue;

    private readonly TcpClient tcp = new();
    private readonly SemaphoreSlim khoaGhi = new(1, 1);
    private readonly CancellationTokenSource dung = new();
    private NetworkStream? stream;
    private byte[]? key;
    private int viTriDoc;
    private int viTriGhi;
    private Task? viecDoc;

    public event Func<GoiTin, Task>? Nhan;
    public TaskCompletionSource DaBatTay { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource DaNgat { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public async Task NoiAsync(string host, int port, CancellationToken ct)
    {
        await tcp.ConnectAsync(host, port, ct);
        tcp.ReceiveBufferSize = 256_000;
        stream = tcp.GetStream();
        viecDoc = Task.Run(DocAsync);
        await GuiAsync(LenhKhoa, [], ct);
    }

    public async Task GuiAsync(sbyte lenh, byte[] duLieu, CancellationToken ct = default)
    {
        if (stream == null) throw new InvalidOperationException("Chưa nối máy chủ.");
        if (duLieu.Length > ushort.MaxValue) throw new InvalidDataException("Gói gửi quá 65535 byte.");

        await khoaGhi.WaitAsync(ct);
        try
        {
            var goi = new byte[3 + duLieu.Length];
            goi[0] = MaHoaGhi(unchecked((byte)lenh));
            goi[1] = MaHoaGhi((byte)(duLieu.Length >> 8));
            goi[2] = MaHoaGhi((byte)duLieu.Length);
            for (var i = 0; i < duLieu.Length; i++) goi[i + 3] = MaHoaGhi(duLieu[i]);
            await stream.WriteAsync(goi, ct);
            await stream.FlushAsync(ct);
        }
        finally
        {
            khoaGhi.Release();
        }
    }

    private async Task DocAsync()
    {
        try
        {
            while (!dung.IsCancellationRequested)
            {
                var cmd = unchecked((sbyte)MaHoaDoc(await DocMotByteAsync()));
                int n;
                if (key != null && cmd == LenhLon)
                {
                    cmd = unchecked((sbyte)MaHoaDoc(await DocMotByteAsync()));
                    var a = await DocDuAsync(4);
                    var b3 = MaHoaDoc(a[3]);
                    var b2 = MaHoaDoc(a[2]);
                    var b1 = MaHoaDoc(a[1]);
                    var b0 = MaHoaDoc(a[0]);
                    n = b3 | (b2 << 8) | (b1 << 16) | (b0 << 24);
                }
                else
                {
                    var a = await DocDuAsync(2);
                    n = (MaHoaDoc(a[0]) << 8) | MaHoaDoc(a[1]);
                }

                if (n < 0 || n > 32 * 1024 * 1024) throw new InvalidDataException($"Độ dài gói {cmd} không hợp lệ: {n}");
                var raw = await DocDuAsync(n);
                if (key != null)
                    for (var i = 0; i < raw.Length; i++) raw[i] = MaHoaDoc(raw[i]);

                if (cmd == LenhKhoa && key == null)
                {
                    DocKey(raw);
                    DaBatTay.TrySetResult();
                    continue;
                }

                var xuLy = Nhan;
                if (xuLy != null) await xuLy(new GoiTin(cmd, raw));
            }
        }
        catch (Exception e)
        {
            if (!dung.IsCancellationRequested) Console.WriteLine($"Mất kết nối: {e.Message}");
            DaBatTay.TrySetException(e);
        }
        finally
        {
            DaNgat.TrySetResult();
        }
    }

    private void DocKey(byte[] raw)
    {
        var r = new BoDoc(raw);
        var n = r.UByte();
        key = r.Bytes(n);
        for (var i = 0; i < key.Length - 1; i++) key[i + 1] ^= key[i];
        viTriDoc = viTriGhi = 0;
        Console.WriteLine($"Bắt tay xong: key {key.Length} byte.");
        if (Environment.GetEnvironmentVariable("KPAH_DEBUG") == "1") Console.WriteLine("  gói khoá: " + Convert.ToHexString(raw));
    }

    private byte MaHoaDoc(byte b)
    {
        if (key == null) return b;
        var k = key[viTriDoc++];
        if (viTriDoc >= key.Length) viTriDoc = 0;
        return (byte)(k ^ b);
    }

    private byte MaHoaGhi(byte b)
    {
        if (key == null) return b;
        var k = key[viTriGhi++];
        if (viTriGhi >= key.Length) viTriGhi = 0;
        return (byte)(k ^ b);
    }

    private async Task<byte> DocMotByteAsync() => (await DocDuAsync(1))[0];

    private async Task<byte[]> DocDuAsync(int n)
    {
        if (stream == null) throw new EndOfStreamException();
        var b = new byte[n];
        var daDoc = 0;
        while (daDoc < n)
        {
            var k = await stream.ReadAsync(b.AsMemory(daDoc, n - daDoc), dung.Token);
            if (k == 0) throw new EndOfStreamException("Máy chủ đóng socket.");
            daDoc += k;
        }
        return b;
    }

    public async ValueTask DisposeAsync()
    {
        dung.Cancel();
        tcp.Close();
        if (viecDoc != null)
            try { await viecDoc; } catch { }
        khoaGhi.Dispose();
        dung.Dispose();
    }
}
