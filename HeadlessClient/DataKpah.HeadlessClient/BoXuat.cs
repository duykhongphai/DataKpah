using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace DataKpah;

public static class BoXuat
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
    };

    public static void NapCu(DuLieuKpah d, string thuMuc)
    {
        Nap(Path.Combine(thuMuc, "ItemOptions.json"), d.MauChiSo, x => x.Id);
        Nap(Path.Combine(thuMuc, "EquipmentItems.json"), d.TrangBi, x => x.Id);
        Nap(Path.Combine(thuMuc, "Potions.json"), d.Potion, x => x.Id);
        Nap(Path.Combine(thuMuc, "Gems.json"), d.Ngoc, x => x.Id);
        Nap(Path.Combine(thuMuc, "ShopItems.json"), d.VatPhamShop, x => x.Id);
        Nap(Path.Combine(thuMuc, "Monsters.json"), d.Quai, x => x.Id);
        Nap(Path.Combine(thuMuc, "Skills.json"), d.KyNang, x => $"{x.Lop}:{x.Id}");
    }

    private static void Nap<T, TKey>(string path, Dictionary<TKey, T> dich, Func<T, TKey> khoa)
        where TKey : notnull
    {
        if (!File.Exists(path)) return;
        try
        {
            foreach (var x in JsonSerializer.Deserialize<List<T>>(File.ReadAllText(path), Json) ?? [])
                dich[khoa(x)] = x;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Không đọc được dữ liệu cũ {Path.GetFileName(path)}: {e.Message}");
        }
    }

    public static void Ghi(DuLieuKpah d, string thuMuc, MayChu mayChu, int nhanVat)
    {
        Directory.CreateDirectory(thuMuc);
        lock (d.Khoa)
        {
            Tep(thuMuc, "ItemOptions.json", d.MauChiSo.Values.OrderBy(x => x.Id));
            Tep(thuMuc, "EquipmentItems.json", d.TrangBi.Values.OrderBy(x => x.Id));
            Tep(thuMuc, "Potions.json", d.Potion.Values.OrderBy(x => x.Id));
            Tep(thuMuc, "Gems.json", d.Ngoc.Values.OrderBy(x => x.Id));
            Tep(thuMuc, "ShopItems.json", d.VatPhamShop.Values.OrderBy(x => x.Id));
            Tep(thuMuc, "Monsters.json", d.Quai.Values.OrderBy(x => x.Id));
            Tep(thuMuc, "Skills.json", d.KyNang.Values.OrderBy(x => x.Lop).ThenBy(x => x.Id));
            Tep(thuMuc, "Server.json", new
            {
                game = "Khí Phách Anh Hùng",
                ten = mayChu.Ten,
                host = mayChu.Host,
                port = mayChu.Port,
                characterIndex = nhanVat,
                updatedAt = DateTimeOffset.UtcNow
            });
        }

        File.WriteAllText(Path.Combine(thuMuc, "LastUpdated"), DateTimeOffset.UtcNow.ToString("O") + "\n");
        GhiDanhSachAnh(thuMuc);
    }

    private static void GhiDanhSachAnh(string thuMuc)
    {
        var dir = Path.Combine(thuMuc, "Icons");
        if (!Directory.Exists(dir)) return;
        var ids = Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories)
            .Select(Path.GetFileNameWithoutExtension)
            .Select(x => short.TryParse(x, out var id) ? (short?)id : null)
            .Where(x => x.HasValue).Select(x => x!.Value).Distinct().OrderBy(x => x);
        Tep(thuMuc, "Images.json", ids);
    }

    private static void Tep<T>(string thuMuc, string ten, T duLieu) =>
        File.WriteAllText(Path.Combine(thuMuc, ten), JsonSerializer.Serialize(duLieu, Json) + "\n");
}
