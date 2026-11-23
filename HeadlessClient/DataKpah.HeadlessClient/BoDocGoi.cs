namespace DataKpah;

/// <summary>Đọc các gói KPAH theo đúng bố cục <c>MessageHandler.onMessage</c> của client 2.7.6.</summary>
public sealed class BoDocGoi
{
    private static readonly HashSet<sbyte> GoiCanGiu = [1, 25, 35];

    /// <summary>Tên kỹ năng gốc của 5 phái (<c>SkillManager.SKILL_NAME</c>).</summary>
    public static readonly string[][] TenKyNang =
    [
        ["Chém", "Kim tinh pháp", "Lôi điện pháp", "Kinh lôi bát thủ", "Hộ sát tiến", "Dĩ lực đáo công", "Thiên lôi điện trảm", "Sấm động dương gian", "Kiếm phi kinh thiên"],
        ["Chém", "Hỏa kinh thiên", "Nhất hỏa long", "Bát đại hỏa long", "Cường thân giáp", "Hộ công tiến", "Thiên long bạo kích", "Liệt hỏa bạo kích", "Sao băng giáng thế"],
        ["Đánh", "Thủy giáng minh", "Thần long thủy", "Bát đại hải long", "Hồi công lực đan", "Hồi lực tiến", "Hồi sinh", "Song hộ công thủ", "Hải long xuất thế", "Song long thị uy", "Hàn băng vũ"],
        ["Đập", "Thổ Tú", "Kim sơn thủy", "Khổng kình bát vĩ", "Bất di biến", "Hộ thủ tiến", "Kinh thiên động địa", "Sơn Tinh bộ thiên", "Thạch nhũ công tâm"],
        ["Bắn", "Nhất hồn tiễn", "Phi thiên tiễn", "Bát kim tễn đáo", "Độc lưu tiễn", "Hộ độc tiễn", "Thập diện tâm tiễn", "Thăng thiên loạn tiễn", "Vạn tiễn quy tâm"]
    ];

    // FilePack.code: khoá XOR của gói ảnh quái ("NguyenVanMinh").
    private static readonly byte[] MaFilePack = "NguyenVanMinh"u8.ToArray();

    private readonly DuLieuKpah data;
    private readonly string thuMuc;
    private readonly System.Collections.Concurrent.ConcurrentDictionary<sbyte, int> demGoi = new();
    private ChiSoKyNang? chiSoCho;

    public TaskCompletionSource CoNhanVat { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource CoVatPham { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource CoQuai { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource DaVaoGame { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public TaskCompletionSource DangNhapHong { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    /// <summary>Máy chủ cấp provider/agent (cmd 2 tiền tố "3"); client gốc lưu lại và dùng cho lần đăng nhập sau.</summary>
    public TaskCompletionSource CoProvider { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public string? Provider { get; private set; }
    public string? Agent { get; private set; }
    public string? ThongBao { get; private set; }

    public int SoMauQuaiNhan;
    public int SoAnhQuaiNhan;
    public int SoIconNhan;

    public BoDocGoi(DuLieuKpah data, string thuMuc)
    {
        this.data = data;
        this.thuMuc = thuMuc;
    }

    public string? LoiDangNhap => data.LoiDangNhap;

    public string TomTatGoi() => demGoi.IsEmpty
        ? "chưa nhận gói nào"
        : string.Join(", ", demGoi.OrderBy(x => x.Key).Select(x => x.Value == 1 ? $"{x.Key}" : $"{x.Key}×{x.Value}"));

    /// <summary>Tạo sẵn các kỹ năng theo tên gốc để có dữ liệu dù máy chủ không gửi bảng học kỹ năng.</summary>
    public static void TaoKyNangGoc(DuLieuKpah d)
    {
        lock (d.Khoa)
            for (sbyte lop = 0; lop < TenKyNang.Length; lop++)
                for (sbyte id = 0; id < TenKyNang[lop].Length; id++)
                {
                    var k = LayKyNang(d, lop, id);
                    if (string.IsNullOrEmpty(k.Ten)) k.Ten = TenKyNang[lop][id];
                }
    }

    private static KyNang LayKyNang(DuLieuKpah d, sbyte lop, sbyte id)
    {
        var khoa = $"{lop}:{id}";
        if (!d.KyNang.TryGetValue(khoa, out var k))
            d.KyNang[khoa] = k = new KyNang { Lop = lop, Id = id };
        return k;
    }

    public Task NhanAsync(GoiTin goi)
    {
        demGoi.AddOrUpdate(goi.Lenh, 1, (_, n) => n + 1);
        if (GoiCanGiu.Contains(goi.Lenh)) LuuGoiTho(goi);
        try
        {
            switch (goi.Lenh)
            {
                case 1: DocVaoGame(goi.DuLieu); break;
                case 2: DocThongBaoDangNhap(goi.DuLieu); break;
                case 3: DocNhanVatChinh(goi.DuLieu); DaVaoGame.TrySetResult(); break;
                case 12: DaVaoGame.TrySetResult(); break;
                case 13: DocDanhSachNhanVat(goi.DuLieu); break;
                case 25: DocVatPham(goi.DuLieu); CoVatPham.TrySetResult(); break;
                case 26: DocSoQuai(goi.DuLieu); CoQuai.TrySetResult(); break;
                case 35: DocChiSoKyNang(goi.DuLieu); break;
                case 37: DocThongBao(goi.DuLieu); break;
                case 100: DocMauQuai(goi.DuLieu); break;
                case -8: DocBoAnh(goi.DuLieu); break;
                case -47: DocAnhQuai(goi.DuLieu); break;
                case -51: LuuIcon(goi.DuLieu); break;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"  gói {goi.Lenh} đọc dở: {e.Message}");
        }
        return Task.CompletedTask;
    }

    private void LuuGoiTho(GoiTin goi)
    {
        var dir = Path.Combine(thuMuc, "RawPackets");
        Directory.CreateDirectory(dir);
        File.WriteAllBytes(Path.Combine(dir, $"Command_{goi.Lenh}.bin"), goi.DuLieu);
    }

    // cmd 2: "3…" chỉ là cập nhật provider/agent; các chuỗi khác là thông báo lỗi đăng nhập.
    private void DocThongBaoDangNhap(byte[] raw)
    {
        var r = new BoDoc(raw);
        var text = r.Utf();
        if (text.StartsWith('3'))
        {
            Provider = r.Utf();
            Agent = r.Utf();
            Console.WriteLine($"Máy chủ cấp provider {Provider}, agent {Agent}");
            CoProvider.TrySetResult();
            return;
        }
        data.LoiDangNhap = text.Length > 1 && char.IsDigit(text[0]) ? text[1..] : text;
        Console.WriteLine("Máy chủ từ chối: " + data.LoiDangNhap);
        DangNhapHong.TrySetResult();
    }

    // cmd 37 (SERVER_MESSAGE): thông báo hộp thoại; có url nghĩa là máy chủ đòi cập nhật phiên bản.
    private void DocThongBao(byte[] raw)
    {
        var r = new BoDoc(raw);
        var text = r.Utf();
        var url = r.Con >= 2 ? r.Utf() : "";
        ThongBao = text;
        Console.WriteLine($"Thông báo máy chủ: {text}{(url.Length > 0 ? $" (cập nhật: {url})" : "")}");
    }

    private void DocDanhSachNhanVat(byte[] raw)
    {
        var r = new BoDoc(raw);
        var n = r.Byte();
        if (n == -1)
        {
            data.LoiDangNhap = r.Utf();
            Console.WriteLine("Máy chủ từ chối: " + data.LoiDangNhap);
            DangNhapHong.TrySetResult();
            return;
        }

        var ds = new List<NhanVat>();
        for (var i = 0; i < n; i++)
        {
            var nv = new NhanVat { Id = r.Int(), Ten = r.Utf() };
            r.Byte(); // kiểu đầu
            var soPart = r.Byte();
            for (var j = 0; j < soPart; j++)
            {
                var loai = r.Byte();
                r.Byte();
                // Vũ khí loại 3..7 (kiếm, đao, đũa, rìu, cung) ứng với phái 0..4 (CreateCharScr).
                if (loai is >= 3 and <= 7) nv.Lop = (sbyte)(loai - 3);
            }
            nv.Cap = r.Short();
            r.Byte(); // đang dùng
            nv.QuocGia = r.Byte();
            r.Short(); // số ngày
            if (r.Byte() != -1)
            {
                r.Bytes(r.Short());
                r.Bytes(r.Short());
                r.Byte();
                r.Byte();
            }
            ds.Add(nv);
        }

        lock (data.Khoa)
        {
            data.NhanVat.Clear();
            data.NhanVat.AddRange(ds);
        }
        Console.WriteLine($"Danh sách nhân vật: {ds.Count}");
        CoNhanVat.TrySetResult();
    }

    private void DocNhanVatChinh(byte[] raw)
    {
        var r = new BoDoc(raw);
        r.Short();
        var ten = r.Utf();
        for (var i = 0; i < 4; i++) r.Int();
        r.Byte();
        var lop = r.Byte();
        Console.WriteLine($"Vào game: {ten}, phái {lop}");
        lock (data.Khoa)
        {
            data.LopHienTai = lop;
            if (chiSoCho != null) GhepChiSo(chiSoCho, lop);
            chiSoCho = null;
        }
    }

    // cmd 1 sau khi chọn nhân vật: bình thuốc và danh sách kỹ năng có thể học của cả 5 phái.
    private void DocVaoGame(byte[] raw)
    {
        var r = new BoDoc(raw);
        r.Byte();  // tốc độ
        r.Short(); // vé
        var soPotion = r.UByte();
        lock (data.Khoa)
        {
            for (short i = 0; i < soPotion; i++)
            {
                if (!data.Potion.TryGetValue(i, out var p)) data.Potion[i] = p = new Potion { Id = i };
                var index = r.UByte();
                p.IconId = (short)(index + 5500);
                p.Ten = r.Utf();
                p.MoTa = r.Utf();
                p.HoiChieuMs = r.Short();
                p.CoTheGiaoDich = r.Bool();
            }

            r.Bytes(5); // số kỹ năng đã học của từng phái
            for (sbyte lop = 0; lop < 5; lop++)
            {
                var n = r.Byte();
                for (var i = 0; i < n; i++)
                {
                    var k = LayKyNang(data, lop, r.Byte());
                    var ten = r.Utf();
                    if (!string.IsNullOrWhiteSpace(ten)) k.Ten = ten;
                    k.MoTa = r.Utf();
                    k.GiaHoc = r.Int();
                }
            }
        }
        Console.WriteLine($"Gói vào game: {soPotion} bình thuốc, kỹ năng 5 phái");
    }

    private void DocVatPham(byte[] raw)
    {
        var r = new BoDoc(raw);
        lock (data.Khoa)
        {
            var soChiSo = r.UByte();
            for (var i = 0; i < soChiSo; i++)
            {
                var id = r.UByte();
                data.MauChiSo[id] = new MauChiSo(id, r.Utf(), r.Byte(), r.Byte());
            }

            var soPotion = r.Byte();
            for (short i = 0; i < soPotion; i++)
            {
                if (!data.Potion.TryGetValue(i, out var p)) data.Potion[i] = p = new Potion { Id = i };
                p.Gia = r.Short();
                p.HoiPhuc = r.Short();
            }

            r.Bytes(5); // Res.percent

            var soTrangBi = r.Short();
            for (var i = 0; i < soTrangBi; i++)
            {
                var t = new TrangBi
                {
                    Id = r.Short(), Ten = r.Utf(), Loai = r.Byte(), KieuDang = r.Byte(), He = r.Byte(),
                    GioiTinh = r.Byte(), Cap = r.Byte(), DoBen = r.Short()
                };
                var cs = new short[10];
                for (var j = 0; j < cs.Length; j++) cs[j] = r.Short();
                t.ChiSoGoc = cs;
                t.Gia = r.Int();
                t.Lop = r.Byte();
                t.Mau = r.Byte();
                t.IconId = r.Short();
                t.SoNgayThue = r.Short();
                data.TrangBi[t.Id] = t;
            }

            var soNgoc = r.Short();
            for (var i = 0; i < soNgoc; i++)
            {
                var g = new Ngoc { Id = r.Short(), AnhId = r.Byte(), Gia = r.Int(), Ten = r.Utf(), MoTa = r.Utf() };
                g.IconId = (short)(g.AnhId + 6500);
                g.Loai = r.Byte();
                g.CoTheBan = r.Bool();
                g.KieuEp = r.Byte();
                g.LoaiTien = r.Byte();
                data.Ngoc[g.Id] = g;
            }

            var soShop = r.UByte();
            for (var i = 0; i < soShop; i++)
            {
                var s = new VatPhamShop { Id = r.UByte(), AnhId = r.UByte(), Gia = r.Int(), Ten = r.Utf(), MoTa = r.Utf() };
                s.IconId = (short)(s.AnhId + 5500);
                s.LoaiTien = r.Byte();
                s.LoaiShop = r.Byte();
                s.CoTheBan = r.Bool();
                s.GiaTri = r.Short();
                data.VatPhamShop[s.Id] = s;
            }
            Console.WriteLine($"Bảng vật phẩm: {soTrangBi} trang bị, {soNgoc} ngọc, {soShop} vật phẩm shop, {soPotion} bình thuốc");
        }
    }

    private void DocSoQuai(byte[] raw)
    {
        var n = new BoDoc(raw).UByte();
        lock (data.Khoa) data.SoMauQuai = n;
        Console.WriteLine($"Số mẫu quái: {n}");
    }

    private sealed record ChiSoKyNang(short[,] SatThuong, int[,] HoiChieu, short[] Tam, int[,] Mp, short[,] Buff, sbyte[,] CapThem, sbyte[][] Aoe);

    // cmd 35: chỉ số 15 kỹ năng × 11 cấp của phái đang chơi.
    private void DocChiSoKyNang(byte[] raw)
    {
        var r = new BoDoc(raw);
        var st = new short[15, 11];
        var hc = new int[15, 11];
        var tam = new short[15];
        var mp = new int[15, 11];
        var buff = new short[15, 11];
        var capThem = new sbyte[15, 11];
        for (var i = 0; i < 15; i++) for (var j = 0; j < 11; j++) st[i, j] = r.Short();
        for (var i = 0; i < 15; i++) for (var j = 0; j < 11; j++) hc[i, j] = r.Short() * 100;
        for (var i = 0; i < 15; i++) tam[i] = r.Short();
        for (var i = 0; i < 15; i++) for (var j = 0; j < 11; j++) mp[i, j] = r.UByte();
        for (var i = 0; i < 15; i++) for (var j = 0; j < 11; j++) buff[i, j] = r.Short();
        for (var i = 0; i < 15; i++) for (var j = 0; j < 11; j++) capThem[i, j] = r.Byte();
        var aoe = new sbyte[5][];
        for (var i = 0; i < 5; i++)
        {
            aoe[i] = new sbyte[r.Byte()];
            for (var j = 0; j < aoe[i].Length; j++) aoe[i][j] = r.Byte();
        }

        var cs = new ChiSoKyNang(st, hc, tam, mp, buff, capThem, aoe);
        lock (data.Khoa)
        {
            if (data.LopHienTai >= 0) GhepChiSo(cs, data.LopHienTai);
            else chiSoCho = cs;
        }
    }

    private void GhepChiSo(ChiSoKyNang cs, sbyte lop)
    {
        if (lop is < 0 or > 4) return;
        var soKyNang = Math.Max(TenKyNang[lop].Length,
            data.KyNang.Values.Where(x => x.Lop == lop).Select(x => x.Id + 1).DefaultIfEmpty(0).Max());
        for (sbyte id = 0; id < Math.Min(15, soKyNang); id++)
        {
            var k = LayKyNang(data, lop, id);
            k.TamDanh = cs.Tam[id];
            k.DanhLan = cs.Aoe[lop].Contains(id);
            var cap = new List<CapKyNang>();
            for (var j = 0; j < 11; j++)
            {
                if (cs.SatThuong[id, j] == 0 && cs.HoiChieu[id, j] == 0 && cs.Mp[id, j] == 0 && cs.Buff[id, j] == 0) continue;
                cap.Add(new CapKyNang
                {
                    Cap = j, SatThuongPhanTram = cs.SatThuong[id, j], HoiChieuMs = cs.HoiChieu[id, j],
                    Mp = cs.Mp[id, j], ThoiGianBuff = cs.Buff[id, j], CapCongThem = cs.CapThem[id, j]
                });
            }
            k.CacCap = [.. cap];
        }
        Console.WriteLine($"Chỉ số kỹ năng phái {lop}: đã ghép");
    }

    private void DocMauQuai(byte[] raw)
    {
        var r = new BoDoc(raw);
        var loai = r.UByte();
        var id = r.Short();
        if (loai != 0) return;
        var info = r.Bytes(r.Byte());
        var q = new Quai { Id = id };
        if (info.Length >= 9)
        {
            q.KieuDiChuyen = unchecked((sbyte)info[0]);
            q.TocDo = unchecked((sbyte)info[1]);
            q.KhungRong = info[3];
            q.KhungCao = info[4];
            q.TamX = unchecked((sbyte)info[5]);
            q.TamY = unchecked((sbyte)info[6]);
        }
        q.Ten = r.Utf();
        q.He = r.Byte();
        q.Hp = r.Int();
        q.Palate = r.Byte();
        q.SPalate = r.Byte();
        q.QuaiMoi = r.Byte();
        lock (data.Khoa)
        {
            if (data.Quai.TryGetValue(id, out var cu)) q.Anh = cu.Anh;
            data.Quai[id] = q;
        }
        Interlocked.Increment(ref SoMauQuaiNhan);
    }

    /// <summary>Đường dẫn ảnh quái đã giải: TeaMobi/…/Monsters/{id}.png.</summary>
    public static string DuongAnhQuai(string thuMuc, short id) => Path.Combine(thuMuc, "Monsters", id + ".png");

    // cmd -47 (MonsterTemplate.onImage): quái thường là FilePack mã XOR chứa "{spalate}_h" (đầu PNG
    // kèm bảng màu) và "data"; quái kiểu mới là ImageObj: danh sách PNG có độ dài short.
    private void DocAnhQuai(byte[] raw)
    {
        var r = new BoDoc(raw);
        var id = r.Short();
        r.Bool();
        var goi = r.Bytes(r.Short());
        Quai? q;
        lock (data.Khoa) data.Quai.TryGetValue(id, out q);
        if (q == null || goi.Length == 0) return;

        byte[]? png;
        string kieu;
        if (q.Palate is >= 70 and <= 88 or 90 or 91 or 92 || q.QuaiMoi == 1)
        {
            var d = new BoDoc(goi);
            var n = d.Byte();
            png = n > 0 ? d.Bytes(d.Short()) : null;
            kieu = "obj";
        }
        else
        {
            var tep = MoFilePack(goi);
            png = tep.TryGetValue(q.SPalate + "_h", out var dau) && tep.TryGetValue("data", out var than)
                ? [.. dau, .. than]
                : null;
            kieu = q.Palate is 39 or 40 ? "obj" : "strip";
        }

        if (png == null || png.Length < 8 || png[0] != 0x89 || png[1] != 0x50) return;
        var path = DuongAnhQuai(thuMuc, id);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, png);
        lock (data.Khoa) q.Anh = kieu;
        Interlocked.Increment(ref SoAnhQuaiNhan);
    }

    private static Dictionary<string, byte[]> MoFilePack(byte[] goi)
    {
        var r = new BoDoc(goi);
        var soTep = r.UByte();
        var ten = new string[soTep];
        var dai = new int[soTep];
        for (var i = 0; i < soTep; i++)
        {
            var b = r.Bytes(r.Byte());
            GiaiMa(b);
            ten[i] = System.Text.Encoding.Latin1.GetString(b);
            dai[i] = r.UShort();
        }
        var tatCa = r.Bytes(dai.Sum());
        GiaiMa(tatCa);
        var kq = new Dictionary<string, byte[]>();
        var pos = 0;
        for (var i = 0; i < soTep; i++)
        {
            kq[ten[i]] = tatCa.AsSpan(pos, dai[i]).ToArray();
            pos += dai[i];
        }
        return kq;
    }

    private static void GiaiMa(byte[] b)
    {
        for (var i = 0; i < b.Length; i++) b[i] ^= MaFilePack[i % MaFilePack.Length];
    }

    // cmd -8 loại 1 (GameData.saveImgSkill): bảng icon kỹ năng 20×20 của phái đang chơi.
    private void DocBoAnh(byte[] raw)
    {
        var r = new BoDoc(raw);
        var loai = r.Byte();
        r.Byte();
        var anh = r.Bytes(r.Short());
        if (loai != 1 || anh.Length < 8 || anh[0] != 0x89) return;
        sbyte lop;
        lock (data.Khoa) lop = data.LopHienTai;
        if (lop < 0) return;
        var dir = Path.Combine(thuMuc, "SkillIcons");
        Directory.CreateDirectory(dir);
        File.WriteAllBytes(Path.Combine(dir, lop + ".png"), anh);
        Console.WriteLine($"Bảng icon kỹ năng phái {lop}: {anh.Length} byte");
    }

    private void LuuIcon(byte[] raw)
    {
        var r = new BoDoc(raw);
        var id = r.Short();
        var bytes = r.ConLai();
        if (bytes.Length == 0) return;
        var dir = Path.Combine(thuMuc, "Icons", (id / 1000).ToString());
        Directory.CreateDirectory(dir);
        File.WriteAllBytes(Path.Combine(dir, id + Duoi(bytes)), bytes);
        Interlocked.Increment(ref SoIconNhan);
    }

    private static string Duoi(byte[] b)
    {
        if (b.Length >= 8 && b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47) return ".png";
        if (b.Length >= 3 && b[0] == 0x47 && b[1] == 0x49 && b[2] == 0x46) return ".gif";
        if (b.Length >= 2 && b[0] == 0xFF && b[1] == 0xD8) return ".jpg";
        return ".bin";
    }
}
