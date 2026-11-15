using System.Text.RegularExpressions;

namespace BaseCore.Tests.Shared;

/// <summary>Cột theo SPEC mục 2: tên, kiểu SPEC (C/N/D), độ dài (C(n)).</summary>
public sealed record CotSpec(string Bang, string Ten, string Kieu, int? DoDai, bool LaKhoa);

/// <summary>
/// Đọc trực tiếp SPEC_NGHIEP_VU_BT4.md để kiểm thử đối chiếu (không chép tay danh sách bảng/quyền).
/// </summary>
public static partial class SpecDoc
{
    private static readonly Lazy<string> NoiDung = new(() => File.ReadAllText(TimFile("Prompt/Prompt_ClaudeCode_HeThong/docs/SPEC_NGHIEP_VU_BT4.md")));

    public static string TimFile(string duongDanTuGoc)
    {
        var bien = Environment.GetEnvironmentVariable("ULMS_ROOT");
        if (!string.IsNullOrEmpty(bien) && File.Exists(Path.Combine(bien, duongDanTuGoc))) return Path.Combine(bien, duongDanTuGoc);
        for (var d = new DirectoryInfo(AppContext.BaseDirectory); d is not null; d = d.Parent)
        {
            var p = Path.Combine(d.FullName, duongDanTuGoc);
            if (File.Exists(p)) return p;
        }
        foreach (var goc in new[] { "/src", Directory.GetCurrentDirectory() })
        {
            var p = Path.Combine(goc, duongDanTuGoc);
            if (File.Exists(p)) return p;
        }
        throw new FileNotFoundException("Không tìm thấy " + duongDanTuGoc + " (đặt biến ULMS_ROOT = thư mục gốc dự án).");
    }

    /// <summary>18 bảng SPEC mục 2 (tên bảng → danh sách cột).</summary>
    public static IReadOnlyDictionary<string, List<CotSpec>> BangGoc()
    {
        var kq = new Dictionary<string, List<CotSpec>>(StringComparer.Ordinal);
        var muc2 = NoiDung.Value.Split("## 3. Kiểm soát")[0];
        string? bang = null;
        foreach (var dong in muc2.Split('\n'))
        {
            var t = TieuDeBang().Match(dong);
            if (t.Success) { bang = t.Groups[1].Value; kq[bang] = []; continue; }
            if (bang is null || !dong.StartsWith('|') || dong.StartsWith("|---") || dong.StartsWith("| Trường")) continue;
            var o = dong.Trim().Trim('|').Split('|').Select(x => x.Trim()).ToArray();
            if (o.Length < 3) continue;
            var laKhoa = o[0].StartsWith("**");
            var ten = o[0].Trim('*');
            var kieu = KieuSpec().Match(o[2]);
            kq[bang].Add(new CotSpec(bang, ten, kieu.Groups[1].Value,
                kieu.Groups[2].Success ? int.Parse(kieu.Groups[2].Value) : null, laKhoa));
        }
        return kq;
    }

    /// <summary>SPEC 3.4: STT → nhóm được phép (A).</summary>
    public static IReadOnlyDictionary<int, string[]> QuyenTienTrinh()
    {
        var phan = NoiDung.Value.Split("### 3.4")[1].Split("## 4.")[0];
        var kq = new Dictionary<int, string[]>();
        foreach (var dong in phan.Split('\n').Where(d => DongSo().IsMatch(d)))
        {
            var o = dong.Trim().Trim('|').Split('|').Select(x => x.Trim()).ToArray();
            var nhom = new[] { "N1", "N2", "N3", "N4" };
            kq[int.Parse(o[0])] = nhom.Where((_, i) => o[2 + i] == "A").ToArray();
        }
        return kq;
    }

    /// <summary>SPEC 3.2: mã bảng (B01…) → nhóm → các thao tác C/E/R/D.</summary>
    public static IReadOnlyDictionary<string, Dictionary<string, string[]>> QuyenBang()
    {
        var phan = NoiDung.Value.Split("### 3.2")[1].Split("### 3.3")[0];
        var kq = new Dictionary<string, Dictionary<string, string[]>>();
        foreach (var dong in phan.Split('\n').Where(d => MaBang().IsMatch(d)))
        {
            var o = dong.Trim().Trim('|').Split('|').Select(x => x.Trim()).ToArray();
            var ma = o[0][..3];
            kq[ma] = new Dictionary<string, string[]>();
            for (var i = 0; i < 4; i++)
                kq[ma]["N" + (i + 1)] = o[1 + i] == "–" ? [] : o[1 + i].Split(',').Select(x => x.Trim()).ToArray();
        }
        return kq;
    }

    /// <summary>Kiểu SQL Server mong đợi theo file 02 mục 1.</summary>
    public static (string Kieu, int? DoDai) KieuSql(CotSpec c)
    {
        if (c.Kieu == "C") return ("nvarchar", c.DoDai);
        if (c.Ten is "dongia" or "thanh_tien" or "tong_tien") return ("decimal", null);
        if (c.Kieu == "N") return ("int", null);
        // D: thời điểm (tg_*, thoigian_lap) → datetime2(0); còn lại → date
        return c.Ten.StartsWith("tg_") || c.Ten == "thoigian_lap" ? ("datetime2", null) : ("date", null);
    }

    [GeneratedRegex(@"^### B\d+(?:\.\d)?\. (\w+)")] private static partial Regex TieuDeBang();
    [GeneratedRegex(@"^([CND])(?:\((\d+)\))?")] private static partial Regex KieuSpec();
    [GeneratedRegex(@"^\| \d+ \|")] private static partial Regex DongSo();
    [GeneratedRegex(@"^\| B\d{2}\. ")] private static partial Regex MaBang();
}
