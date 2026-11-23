namespace DataKpah;

/// <summary>Tên chỉ số trang bị (cmd 25, <c>NameAttributeItem</c>).</summary>
public sealed record MauChiSo(int Id, string Ten, sbyte LaPhanTram, sbyte Mau);

/// <summary>Mẫu trang bị (cmd 25, <c>ItemTemplate</c>).</summary>
public sealed class TrangBi
{
    public short Id { get; set; }
    public string Ten { get; set; } = "";
    public sbyte Loai { get; set; }
    public sbyte KieuDang { get; set; }
    public sbyte He { get; set; }
    public sbyte GioiTinh { get; set; }
    public sbyte Cap { get; set; }
    public short DoBen { get; set; }
    public short[] ChiSoGoc { get; set; } = [];
    public int Gia { get; set; }
    public sbyte Lop { get; set; }
    public sbyte Mau { get; set; }
    public short IconId { get; set; }
    public short SoNgayThue { get; set; }
}

/// <summary>Bình thuốc: giá và lượng hồi từ cmd 25, tên từ gói vào game (cmd 1).</summary>
public sealed class Potion
{
    public short Id { get; set; }
    public short IconId { get; set; } = -1;
    public string Ten { get; set; } = "";
    public string MoTa { get; set; } = "";
    public int HoiChieuMs { get; set; }
    public bool CoTheGiaoDich { get; set; }
    public short Gia { get; set; }
    public short HoiPhuc { get; set; }
}

/// <summary>Ngọc, đá, vật phẩm nâng cấp (cmd 25, <c>Res.gemTemplate</c>).</summary>
public sealed class Ngoc
{
    public short Id { get; set; }
    public short AnhId { get; set; }
    public short IconId { get; set; }
    public int Gia { get; set; }
    public string Ten { get; set; } = "";
    public string MoTa { get; set; } = "";
    public sbyte Loai { get; set; }
    public bool CoTheBan { get; set; }
    public sbyte KieuEp { get; set; }
    public sbyte LoaiTien { get; set; }
}

/// <summary>Vật phẩm cửa hàng (cmd 25, <c>Res.shopTemplate</c>).</summary>
public sealed class VatPhamShop
{
    public short Id { get; set; }
    public short AnhId { get; set; }
    public short IconId { get; set; }
    public int Gia { get; set; }
    public string Ten { get; set; } = "";
    public string MoTa { get; set; } = "";
    public sbyte LoaiTien { get; set; }
    public sbyte LoaiShop { get; set; }
    public bool CoTheBan { get; set; }
    public short GiaTri { get; set; }
}

/// <summary>Mẫu quái (cmd 100 loại 0, <c>GameScr.onTemplateInfo</c>) và ảnh (cmd -47).</summary>
public sealed class Quai
{
    public short Id { get; set; }
    public string Ten { get; set; } = "";
    public sbyte He { get; set; }
    public int Hp { get; set; }
    public sbyte KieuDiChuyen { get; set; }
    public sbyte TocDo { get; set; }
    public byte KhungRong { get; set; }
    public byte KhungCao { get; set; }
    public sbyte TamX { get; set; }
    public sbyte TamY { get; set; }
    public sbyte Palate { get; set; }
    public sbyte SPalate { get; set; }
    public sbyte QuaiMoi { get; set; }
    /// <summary>"strip": dải dọc khung <see cref="KhungRong"/>×<see cref="KhungCao"/>; "obj": ảnh ghép tĩnh.</summary>
    public string? Anh { get; set; }
}

public sealed class CapKyNang
{
    public int Cap { get; set; }
    public short SatThuongPhanTram { get; set; }
    public int HoiChieuMs { get; set; }
    public int Mp { get; set; }
    public short ThoiGianBuff { get; set; }
    public sbyte CapCongThem { get; set; }
}

public sealed class KyNang
{
    public sbyte Lop { get; set; }
    public sbyte Id { get; set; }
    public string Ten { get; set; } = "";
    public string MoTa { get; set; } = "";
    public int GiaHoc { get; set; }
    public short TamDanh { get; set; }
    public bool DanhLan { get; set; }
    public CapKyNang[] CacCap { get; set; } = [];
}

public sealed class NhanVat
{
    public int Id { get; set; }
    public string Ten { get; set; } = "";
    public short Cap { get; set; }
    public sbyte Lop { get; set; } = -1;
    public sbyte QuocGia { get; set; }
}

public sealed class DuLieuKpah
{
    public object Khoa { get; } = new();
    public Dictionary<int, MauChiSo> MauChiSo { get; } = [];
    public Dictionary<short, TrangBi> TrangBi { get; } = [];
    public Dictionary<short, Potion> Potion { get; } = [];
    public Dictionary<short, Ngoc> Ngoc { get; } = [];
    public Dictionary<short, VatPhamShop> VatPhamShop { get; } = [];
    public Dictionary<short, Quai> Quai { get; } = [];
    // ID kỹ năng lặp lại ở mỗi phái nên khóa là "phái:id".
    public Dictionary<string, KyNang> KyNang { get; } = [];
    public List<NhanVat> NhanVat { get; } = [];
    public int SoMauQuai { get; set; }
    public sbyte LopHienTai { get; set; } = -1;
    public string? LoiDangNhap { get; set; }
}
