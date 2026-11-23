namespace DataKpah;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        var c = CauHinh.Doc(args);
        if (c.Loi() is { } loi)
        {
            Console.Error.WriteLine(loi);
            return 2;
        }

        using var het = new CancellationTokenSource(c.ChoDuLieuMs);
        var ct = het.Token;
        try
        {
            var mayChu = MayChu.Tim(c.MayChu);
            Console.WriteLine($"DataKpah: {mayChu.Ten} ({mayChu.Host}:{mayChu.Port}), nhân vật index {c.NhanVat}");
            Console.WriteLine($"Thư mục ra: {Path.GetFullPath(c.ThuMucRa)}");

            var data = new DuLieuKpah();
            BoXuat.NapCu(data, c.ThuMucRa);
            BoDocGoi.TaoKyNangGoc(data);

            var doc = new BoDocGoi(data, c.ThuMucRa);
            await using var phien = new PhienKpah();
            phien.Nhan += doc.NhanAsync;

            await phien.NoiAsync(mayChu.Host, mayChu.Port, ct);
            await phien.DaBatTay.Task.WaitAsync(TimeSpan.FromSeconds(30), ct);

            // LoginScr.doLogin: setClientType (cmd -1, byte 2) rồi GameService.login (cmd 1).
            await phien.GuiAsync(-1, new BoGhi().Byte(2).Xong(), ct);
            await phien.GuiAsync(1, new BoGhi()
                .Utf(c.TaiKhoan).Utf(c.MatKhau).Utf(c.PhienBan)
                .Utf("nokia/1/1").Utf("0").Utf("-1").Utf("-1")
                .Byte(4)       // Main.isPC
                .Short(800)    // Canvas.w
                .Byte(0)       // không xin tile từ máy chủ
                .Byte(mayChu.Route)
                .Xong(), ct);

            var ketQua = await Task.WhenAny(doc.CoNhanVat.Task, doc.DangNhapHong.Task, phien.DaNgat.Task,
                Task.Delay(90_000, ct));
            if (!doc.CoNhanVat.Task.IsCompleted)
            {
                var lyDo = doc.DangNhapHong.Task.IsCompleted ? doc.LoiDangNhap ?? "đăng nhập thất bại"
                    : phien.DaNgat.Task.IsCompleted ? "máy chủ ngắt kết nối"
                    : "hết thời gian chờ danh sách nhân vật";
                Console.WriteLine($"Không vào được màn chọn nhân vật: {lyDo}");
                Console.WriteLine($"  gói đã nhận: {doc.TomTatGoi()}");
                BoXuat.Ghi(data, c.ThuMucRa, mayChu, c.NhanVat); // giữ các bảng đã nhận
                return 1;
            }

            NhanVat? nhanVat;
            lock (data.Khoa)
                nhanVat = c.NhanVat < data.NhanVat.Count ? data.NhanVat[c.NhanVat] : null;
            if (nhanVat == null)
            {
                Console.WriteLine($"Không có nhân vật index {c.NhanVat}; tài khoản có {data.NhanVat.Count} nhân vật.");
                BoXuat.Ghi(data, c.ThuMucRa, mayChu, c.NhanVat);
                return 3;
            }

            Console.WriteLine($"Chọn [{c.NhanVat}] {nhanVat.Ten} (id {nhanVat.Id}, cấp {nhanVat.Cap}, phái {nhanVat.Lop})");
            lock (data.Khoa) data.LopHienTai = nhanVat.Lop;
            await phien.GuiAsync(13, new BoGhi().Byte(1).Int(nhanVat.Id).Xong(), ct); // CharSelectScr: type 1 = vào game
            await Task.WhenAny(doc.DaVaoGame.Task, phien.DaNgat.Task, Task.Delay(90_000, ct));
            if (!doc.DaVaoGame.Task.IsCompleted)
            {
                Console.WriteLine($"Chưa vào được game sau khi chọn nhân vật. Gói đã nhận: {doc.TomTatGoi()}");
                BoXuat.Ghi(data, c.ThuMucRa, mayChu, c.NhanVat);
                return 1;
            }

            // Báo đã nạp xong map như GameScr.onMap (changeMapOK(-500, 0, 0)).
            await phien.GuiAsync(12, new BoGhi().Short(-500).Short(0).Short(0).Xong(), ct);
            await Task.WhenAny(Task.WhenAll(doc.CoVatPham.Task, doc.CoQuai.Task), phien.DaNgat.Task, Task.Delay(45_000, ct));
            Console.WriteLine($"Gói đã nhận: {doc.TomTatGoi()}");
            BoXuat.Ghi(data, c.ThuMucRa, mayChu, c.NhanVat);

            // Bảng icon kỹ năng của phái đang chơi (GameData.saveImgSkill).
            await phien.GuiAsync(-8, new BoGhi().Byte(1).Byte(0).Xong(), ct);

            int soQuai;
            lock (data.Khoa) soQuai = data.SoMauQuai;
            if (soQuai > 0)
            {
                Console.WriteLine($"Hỏi {soQuai} mẫu quái...");
                for (var id = 0; id < soQuai && !phien.DaNgat.Task.IsCompleted; id++)
                {
                    await phien.GuiAsync(100, new BoGhi().Byte(0).Short(id).Xong(), ct);
                    await Task.Delay(c.NhipQuaiMs, ct);
                }
                await ChoYenAsync(() => doc.SoMauQuaiNhan, ct);
                Console.WriteLine($"  mẫu quái nhận: {doc.SoMauQuaiNhan}");
                BoXuat.Ghi(data, c.ThuMucRa, mayChu, c.NhanVat);
            }

            if (c.TaiAnh && !phien.DaNgat.Task.IsCompleted)
            {
                List<Quai> canAnh;
                lock (data.Khoa)
                    canAnh = data.Quai.Values.Where(q => q.Palate >= 0 && (q.Anh == null || !File.Exists(BoDocGoi.DuongAnhQuai(c.ThuMucRa, q.Id)))).ToList();
                Console.WriteLine($"Tải ảnh quái: cần {canAnh.Count}");
                foreach (var q in canAnh)
                {
                    if (phien.DaNgat.Task.IsCompleted) break;
                    await phien.GuiAsync(-47, new BoGhi().Byte(q.Palate).Byte(q.SPalate).Short(q.Id).Xong(), ct);
                    await Task.Delay(c.NhipAnhMs, ct);
                }
                await ChoYenAsync(() => doc.SoAnhQuaiNhan, ct);

                var icon = GomIcon(data);
                var canHoi = icon.Where(id => !DaCoIcon(c.ThuMucRa, id)).ToList();
                Console.WriteLine($"Tải icon: {icon.Count} id tham chiếu, đã có {icon.Count - canHoi.Count}, cần hỏi {canHoi.Count}");
                foreach (var id in canHoi)
                {
                    if (phien.DaNgat.Task.IsCompleted) break;
                    await phien.GuiAsync(-51, new BoGhi().Short(id).Xong(), ct);
                    await Task.Delay(c.NhipAnhMs, ct);
                }
                await ChoYenAsync(() => doc.SoIconNhan, ct);
                Console.WriteLine($"  ảnh quái mới: {doc.SoAnhQuaiNhan}, icon mới: {doc.SoIconNhan}");
            }

            BoXuat.Ghi(data, c.ThuMucRa, mayChu, c.NhanVat);
            if (phien.DaNgat.Task.IsCompleted)
            {
                Console.WriteLine("Máy chủ ngắt kết nối giữa chừng; đã lưu phần dữ liệu nhận được.");
                return 1;
            }
            Console.WriteLine("Hoàn tất.");
            return 0;
        }
        catch (OperationCanceledException)
        {
            Console.Error.WriteLine("Hết thời gian lấy dữ liệu.");
            return 4;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return 1;
        }
    }

    /// <summary>Chờ tới khi bộ đếm gói phản hồi đứng yên 5 giây (tối đa 60 giây).</summary>
    private static async Task ChoYenAsync(Func<int> dem, CancellationToken ct)
    {
        var truoc = -1;
        var yen = 0;
        for (var i = 0; i < 60 && yen < 5; i++)
        {
            await Task.Delay(1_000, ct);
            var bay = dem();
            yen = bay == truoc ? yen + 1 : 0;
            truoc = bay;
        }
    }

    private static SortedSet<short> GomIcon(DuLieuKpah d)
    {
        var ids = new SortedSet<short>();
        lock (d.Khoa)
        {
            foreach (var x in d.TrangBi.Values) Them(x.IconId);
            foreach (var x in d.Potion.Values) Them(x.IconId);
            foreach (var x in d.Ngoc.Values) Them(x.IconId);
            foreach (var x in d.VatPhamShop.Values) Them(x.IconId);
        }
        return ids;

        void Them(int id)
        {
            if (id is >= 0 and <= short.MaxValue) ids.Add((short)id);
        }
    }

    private static bool DaCoIcon(string thuMuc, short id)
    {
        var dir = Path.Combine(thuMuc, "Icons", (id / 1000).ToString());
        return Directory.Exists(dir) && Directory.EnumerateFiles(dir, id + ".*").Any();
    }
}
