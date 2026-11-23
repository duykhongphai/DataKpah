using System.Globalization;
using System.Text;

namespace DataKpah;

public sealed class CauHinh
{
    public string MayChu { get; set; } = "Vạn Xuân";
    public string TaiKhoan { get; set; } = "";
    public string MatKhau { get; set; } = "";
    public int NhanVat { get; set; }
    public string PhienBan { get; set; } = "2.7.6";
    public string ThuMuc { get; set; } = "DuLieu";
    public string Ra { get; set; } = ".";
    public int NhipQuaiMs { get; set; } = 80;
    public int NhipAnhMs { get; set; } = 80;
    public int ChoDuLieuMs { get; set; } = 900_000;
    public bool TaiAnh { get; set; } = true;

    public string ThuMucRa => Path.Combine(Ra, "TeaMobi", ThuMuc);

    public static CauHinh Doc(string[] args)
    {
        var c = new CauHinh();
        Moi("KPAH_MAY_CHU", v => c.MayChu = v);
        Moi("KPAH_TK", v => c.TaiKhoan = v);
        Moi("KPAH_MK", v => c.MatKhau = v);
        Moi("KPAH_NHAN_VAT", v => c.NhanVat = So(v, 0));
        Moi("KPAH_PHIEN_BAN", v => c.PhienBan = v);
        Moi("KPAH_THUMUC", v => c.ThuMuc = v);
        Moi("KPAH_RA", v => c.Ra = v);
        Moi("KPAH_NHIP_QUAI", v => c.NhipQuaiMs = So(v, c.NhipQuaiMs));
        Moi("KPAH_NHIP_ANH", v => c.NhipAnhMs = So(v, c.NhipAnhMs));
        Moi("KPAH_CHO", v => c.ChoDuLieuMs = So(v, c.ChoDuLieuMs));
        Moi("KPAH_KHONG_ANH", _ => c.TaiAnh = false);

        for (var i = 0; i < args.Length; i++)
        {
            string KeTiep() => i + 1 < args.Length ? args[++i] : "";
            switch (args[i])
            {
                case "--maychu": c.MayChu = KeTiep(); break;
                case "--tk": c.TaiKhoan = KeTiep(); break;
                case "--mk": c.MatKhau = KeTiep(); break;
                case "--nhan-vat": c.NhanVat = So(KeTiep(), 0); break;
                case "--phien-ban": c.PhienBan = KeTiep(); break;
                case "--thumuc": c.ThuMuc = KeTiep(); break;
                case "--ra": c.Ra = KeTiep(); break;
                case "--nhip-quai": c.NhipQuaiMs = So(KeTiep(), c.NhipQuaiMs); break;
                case "--nhip-anh": c.NhipAnhMs = So(KeTiep(), c.NhipAnhMs); break;
                case "--cho": c.ChoDuLieuMs = So(KeTiep(), c.ChoDuLieuMs); break;
                case "--khong-anh": c.TaiAnh = false; break;
            }
        }

        // Client hạ chữ thường và bỏ khoảng trắng hai đầu tài khoản trước khi gửi (LoginScr.doLogin).
        c.TaiKhoan = c.TaiKhoan.Trim().ToLowerInvariant();
        return c;

        static void Moi(string ten, Action<string> dat)
        {
            var v = Environment.GetEnvironmentVariable(ten);
            if (!string.IsNullOrWhiteSpace(v)) dat(v.Trim());
        }
    }

    public string? Loi()
    {
        if (string.IsNullOrWhiteSpace(TaiKhoan)) return "Thiếu tài khoản: KPAH_TK hoặc --tk";
        if (string.IsNullOrWhiteSpace(MatKhau)) return "Thiếu mật khẩu: KPAH_MK hoặc --mk";
        if (NhanVat < 0) return "Index nhân vật phải từ 0 trở lên";
        return null;
    }

    private static int So(string value, int macDinh) =>
        int.TryParse(value, out var n) ? n : macDinh;

    public static string KhongDau(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var b = new StringBuilder();
        foreach (var ch in normalized)
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark &&
                char.IsLetterOrDigit(ch)) b.Append(ch == 'đ' || ch == 'Đ' ? 'd' : ch);
        return b.Length == 0 ? "Server" : b.ToString().ToLowerInvariant();
    }
}

/// <summary>
/// Máy chủ KPAH lấy từ <c>ServerListScr.loadSv(isKPAH)</c> của client 2.7.6. <see cref="Route"/> là
/// <c>index_server[index]</c>, byte cuối của gói đăng nhập.
/// </summary>
public sealed record MayChu(string Ten, string Host, int Port, sbyte Route)
{
    public static readonly MayChu[] DanhSach =
    [
        new("Vạn Xuân", "112.213.84.163", 19129, 0),
        new("Cổ Loa", "27.0.12.112", 19130, 0),
        new("Thăng Long", "103.77.167.209", 19129, 0),
        new("Hoa Lư", "103.77.167.209", 19129, 0),
        new("Kinh Môn", "112.213.94.124", 19129, 0),
        new("Hoàng Vương", "112.213.94.124", 19129, 3),
        new("Mỹ Sơn", "112.213.94.124", 19129, 4),
        new("Tràng An", "112.213.94.124", 19129, 1),
        new("Tiên La", "112.213.94.124", 19129, 2)
    ];

    public static MayChu Tim(string ten)
    {
        var khoa = CauHinh.KhongDau(ten);
        return DanhSach.FirstOrDefault(x => CauHinh.KhongDau(x.Ten) == khoa)
               ?? DanhSach.FirstOrDefault(x => CauHinh.KhongDau(x.Ten).Contains(khoa) || khoa.Contains(CauHinh.KhongDau(x.Ten)))
               ?? throw new InvalidOperationException(
                   $"Không có máy chủ '{ten}'. Có: {string.Join(", ", DanhSach.Select(x => x.Ten))}");
    }
}
