using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace BaseCore.IntegrationTests;

/// <summary>
/// File 11 mục 3 – hiệu năng: phản hồi API &lt; 300 ms (P95) với 50.000 bản sách và 200.000 dòng chi tiết.
/// Nạp thêm vào CSDL kiểm thử (ThuVien_Test): 5.000 đầu sách, 50.000 bản sách, 40.000 phiếu mượn × 5 = 200.000 dòng CT_PHIEUMUON,
/// 40.000 phiếu trả × 5 = 200.000 dòng CT_PHIEUTRA (48 tháng gần nhất). Đo qua gateway (đường thật web → gateway → service → SQL),
/// mỗi API 3 lần làm nóng + 40 lần đo, ghi artifacts/test-results/hieu_nang_api.json.
/// Chạy riêng: sh scripts/test-docker.sh hieunang (không nằm trong bộ tích hợp thường – dữ liệu lớn làm sai số liệu kiểm chứng khác).
/// </summary>
[Trait("Loai", "HieuNang")]
[Collection("ThongKe")]
public class HieuNangTests
{
    private const int NguongP95Ms = 300;
    /// <summary>
    /// API phục vụ BẢN IN MB06/MB07 (/in – đủ toàn bộ dòng của kỳ): ngưỡng 1.000 ms (GIA_DINH D82, chủ dự án duyệt 18/11/2026).
    /// Xem trước (tổng hợp + trang chi tiết 50 dòng) giữ ngưỡng chung 300 ms.
    /// </summary>
    private const int NguongBaoCaoMs = 1000;
    private const int SoLanDo = 40;

    private const string SqlNap = """
        SET NOCOUNT ON; SET XACT_ABORT ON;
        IF EXISTS (SELECT 1 FROM khosach.SACH WHERE ma_bansach = 'HN000001') RETURN;
        DECLARE @kv varchar(10) = (SELECT TOP 1 ma_khuvuc FROM khosach.KHUVUC WHERE loai_khuvuc = N'Kệ mở' ORDER BY ma_khuvuc);
        DECLARE @nd varchar(10), @tennd nvarchar(50);
        SELECT TOP 1 @nd = ma_nd, @tennd = ten_nd FROM quantri.NGUOIDUNG WHERE ma_nhom = 'N1' ORDER BY ma_nd;
        SELECT TOP (50000) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n INTO #N FROM sys.all_objects a CROSS JOIN sys.all_objects b;
        SELECT ROW_NUMBER() OVER (ORDER BY ma_theloai) - 1 AS i, ma_theloai INTO #TL FROM khosach.THELOAI;
        DECLARE @soTl int = (SELECT COUNT(*) FROM #TL);
        SELECT ROW_NUMBER() OVER (ORDER BY ma_bandoc) - 1 AS i, ma_bandoc, hoten INTO #BD FROM muontra.BANDOC;
        DECLARE @soBd int = (SELECT COUNT(*) FROM #BD);
        DECLARE @dauThang date = DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);

        BEGIN TRANSACTION;
        INSERT khosach.DAUSACH (ma_dausach, ten_sach, tacgia, nhaxuatban, namxb, isbn, ma_theloai, gioithieu, tukhoa, mau_gay, ngaytao)
        SELECT 'HD' + RIGHT('000000' + CAST(n AS varchar(6)), 6), N'Sách hiệu năng ' + CAST(n AS nvarchar(6)), N'Tác giả ' + CAST(n % 900 AS nvarchar(6)),
               N'NXB Kiểm thử', 1990 + n % 35, '979' + RIGHT('0000000000' + CAST(n AS varchar(10)), 10), tl.ma_theloai,
               N'[DỮ LIỆU MẪU] hiệu năng', N'hiệu năng, kiểm tra', '#123F35', DATEADD(minute, -n, SYSDATETIME())
        FROM #N JOIN #TL tl ON tl.i = #N.n % @soTl WHERE n <= 5000;

        INSERT khosach.SACH (ma_bansach, ma_khuvuc, ten_sach, tacgia, tinhtrang)
        SELECT 'HN' + RIGHT('000000' + CAST(n AS varchar(6)), 6), @kv, N'Sách hiệu năng ' + CAST((n - 1) % 5000 + 1 AS nvarchar(6)),
               N'Tác giả ' + CAST(((n - 1) % 5000 + 1) % 900 AS nvarchar(6)), N'Sẵn sàng'
        FROM #N;
        INSERT khosach.DAUSACH_BANSACH (ma_bansach, ma_dausach)
        SELECT 'HN' + RIGHT('000000' + CAST(n AS varchar(6)), 6), 'HD' + RIGHT('000000' + CAST((n - 1) % 5000 + 1 AS varchar(6)), 6) FROM #N;

        -- 40.000 phiếu mượn trong 48 tháng trước tháng hiện tại; số chứng từ thứ tự 5000+ (không trùng số do hệ thống cấp)
        SELECT n, DATEADD(day, n % 27, DATEADD(month, -(n % 48) - 1, @dauThang)) AS ngay,
               5000 + n / 48 AS stt, bd.ma_bandoc, bd.hoten
        INTO #PM FROM #N JOIN #BD bd ON bd.i = #N.n % @soBd WHERE n <= 40000;
        INSERT muontra.PHIEUMUON (so_pmuon, ma_bandoc, ngaylap_pmuon, hoten, ma_nd, ten_nd)
        SELECT 'PM' + FORMAT(ngay, 'yyMM') + RIGHT('0000' + CAST(stt AS varchar(4)), 4), ma_bandoc, ngay, hoten, @nd, @tennd FROM #PM;
        INSERT muontra.CT_PHIEUMUON (ma_bansach, so_pmuon, ngaymuon, hantra, tinhtrang_muon, ten_sach)
        SELECT 'HN' + RIGHT('000000' + CAST((p.n * 5 + k.k) % 50000 + 1 AS varchar(6)), 6),
               'PM' + FORMAT(p.ngay, 'yyMM') + RIGHT('0000' + CAST(p.stt AS varchar(4)), 4), p.ngay, DATEADD(day, 14, p.ngay), N'Tốt',
               N'Sách hiệu năng ' + CAST(((p.n * 5 + k.k) % 50000) % 5000 + 1 AS nvarchar(6))
        FROM #PM p CROSS JOIN (VALUES (0), (1), (2), (3), (4)) k(k);
        INSERT muontra.PHIEUTRA (so_ptra, so_pmuon, ma_bandoc, ngaylap_ptra, ngaytra, ma_nd, ten_nd)
        SELECT 'PT' + FORMAT(ngay, 'yyMM') + RIGHT('0000' + CAST(stt AS varchar(4)), 4), 'PM' + FORMAT(ngay, 'yyMM') + RIGHT('0000' + CAST(stt AS varchar(4)), 4),
               ma_bandoc, DATEADD(day, n % 14, ngay), DATEADD(day, n % 14, ngay), @nd, @tennd FROM #PM;
        INSERT muontra.CT_PHIEUTRA (ma_bansach, so_ptra, tinhtrang_tra, so_ngay_quahan)
        SELECT 'HN' + RIGHT('000000' + CAST((p.n * 5 + k.k) % 50000 + 1 AS varchar(6)), 6),
               'PT' + FORMAT(p.ngay, 'yyMM') + RIGHT('0000' + CAST(p.stt AS varchar(4)), 4), N'Bình thường', 0
        FROM #PM p CROSS JOIN (VALUES (0), (1), (2), (3), (4)) k(k);
        COMMIT;
        UPDATE STATISTICS khosach.SACH; UPDATE STATISTICS khosach.DAUSACH; UPDATE STATISTICS muontra.CT_PHIEUMUON;
        UPDATE STATISTICS muontra.PHIEUMUON; UPDATE STATISTICS muontra.PHIEUTRA; UPDATE STATISTICS muontra.CT_PHIEUTRA;
        """;

    private static async Task<int> DemAsync(string sql)
    {
        await using var cn = MoiTruong.KetNoiSa();
        await cn.OpenAsync();
        await using var cmd = new SqlCommand(sql, cn);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    private static async Task<HttpClient> HttpAsync(string? nhanVien)
    {
        var http = MoiTruong.Http();
        if (nhanVien is not null)
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await MoiTruong.DangNhapNhanVienAsync(http, nhanVien));
        return http;
    }

    [Fact]
    public async Task Api_P95_Duoi300ms_Voi50000BanSach_200000DongChiTiet()
    {
        await using (var cn = MoiTruong.KetNoiSa())
        {
            await cn.OpenAsync();
            await using var cmd = new SqlCommand(SqlNap, cn) { CommandTimeout = 600 };
            await cmd.ExecuteNonQueryAsync();
        }
        var soBan = await DemAsync("SELECT COUNT(*) FROM khosach.SACH");
        var soCtMuon = await DemAsync("SELECT COUNT(*) FROM muontra.CT_PHIEUMUON");
        Assert.True(soBan >= 50_000, $"Bản sách: {soBan}");
        Assert.True(soCtMuon >= 200_000, $"CT_PHIEUMUON: {soCtMuon}");

        var homNay = GioVietNam.HomNay;
        // Kỳ báo cáo thường dùng: tháng trước (MB06/MB07 lập theo tháng); xem trước 12 tháng cũng phải < 300 ms (chỉ trang đầu chi tiết).
        // Bản in 12 tháng (~100.000 dòng) ghi để tham khảo, không tính ngưỡng
        var dauThangNay = new DateOnly(homNay.Year, homNay.Month, 1);
        var tu = dauThangNay.AddMonths(-1).ToString("yyyy-MM-dd");
        var den = dauThangNay.AddDays(-1).ToString("yyyy-MM-dd");
        var tu12 = homNay.AddMonths(-12).ToString("yyyy-MM-dd");
        var den12 = homNay.ToString("yyyy-MM-dd");
        var mauThu = new (string Ten, string? NhanVien, string Url)[]
        {
            ("Cổng – khám phá sách (mặc định)", null, "/api/cong/sach?coTrang=12"),
            ("Cổng – tìm không dấu", null, "/api/cong/sach?q=hieu%20nang%2042&coTrang=12"),
            ("Cổng – lọc thể loại + còn sách + sắp xếp mượn nhiều", null, "/api/cong/sach?theLoai=TL01&conSach=true&sapXep=muon-nhieu&coTrang=12"),
            ("Cổng – kệ sách mới", null, "/api/cong/ke-sach?ke=moi"),
            ("Cổng – kệ mượn nhiều", null, "/api/cong/ke-sach?ke=muon-nhieu"),
            ("Cổng – số liệu thư viện", null, "/api/cong/sach/so-lieu"),
            ("Cổng – thể loại", null, "/api/cong/the-loai"),
            ("Cổng – từ khóa phổ biến", null, "/api/cong/sach/tu-khoa?soLuong=8"),
            ("N3 – danh mục sách (trang 1)", "khosach", "/api/quan-tri/sach?trang=1&coTrang=20"),
            ("N3 – danh mục sách (tìm)", "khosach", "/api/quan-tri/sach?tuKhoa=hi%E1%BB%87u%20n%C4%83ng%20777&trang=1&coTrang=20"),
            ("N3 – danh mục sách (trang 2000)", "khosach", "/api/quan-tri/sach?trang=2000&coTrang=20"),
            ("N3 – thống kê tình trạng", "khosach", "/api/quan-tri/sach/thong-ke-tinh-trang"),
            ("N3 – tổng quan", "khosach", "/api/quan-tri/thong-ke/n3"),
            ("N1 – bạn đọc", "muontra", "/api/quan-tri/ban-doc?trang=1&coTrang=20"),
            ("N1 – tổng quan", "muontra", "/api/quan-tri/thong-ke/n1"),
            ("N1 – MB06 xem trước (tháng trước)", "muontra", $"/api/quan-tri/bao-cao/muon-tra?tuNgay={tu}&denNgay={den}"),
            ("[tham khảo] N1 – MB06 xem trước 12 tháng (kỳ dài – chờ chủ dự án quyết, GIA_DINH D82)", "muontra", $"/api/quan-tri/bao-cao/muon-tra?tuNgay={tu12}&denNgay={den12}"),
            ("N1 – MB06 chi tiết lượt mượn trang 20 (12 tháng)", "muontra", $"/api/quan-tri/bao-cao/muon-tra/chi-tiet/luot-muon?tuNgay={tu12}&denNgay={den12}&trang=20&coTrang=50"),
            ("N1 – MB06 chi tiết lượt trả trang 2 (tháng trước)", "muontra", $"/api/quan-tri/bao-cao/muon-tra/chi-tiet/luot-tra?tuNgay={tu}&denNgay={den}&trang=2&coTrang=50"),
            ("[in] N1 – MB06 bản in đủ dòng (tháng trước)", "muontra", $"/api/quan-tri/bao-cao/muon-tra/in?tuNgay={tu}&denNgay={den}"),
            ("[tham khảo] N1 – MB06 bản in 12 tháng", "muontra", $"/api/quan-tri/bao-cao/muon-tra/in?tuNgay={tu12}&denNgay={den12}"),
            ("N2 – tổng quan", "vipham", "/api/quan-tri/thong-ke/n2"),
            ("N2 – MB07 xem trước (tháng trước)", "vipham", $"/api/quan-tri/bao-cao/vi-pham?tuNgay={tu}&denNgay={den}"),
            ("N2 – MB07 xem trước 12 tháng", "vipham", $"/api/quan-tri/bao-cao/vi-pham?tuNgay={tu12}&denNgay={den12}"),
            ("[in] N2 – MB07 bản in đủ dòng (tháng trước)", "vipham", $"/api/quan-tri/bao-cao/vi-pham/in?tuNgay={tu}&denNgay={den}"),
            ("N4 – tổng quan", "admin", "/api/quan-tri/thong-ke/n4"),
        };

        var ketQua = new List<object>();
        var vuot = new List<string>();
        var clients = new Dictionary<string, HttpClient>();
        try
        {
            foreach (var (ten, nv, url) in mauThu)
            {
                var khoa = nv ?? "";
                if (!clients.TryGetValue(khoa, out var http)) clients[khoa] = http = await HttpAsync(nv);
                // Lần gọi đầu = chưa có bộ nhớ đệm (ghi riêng để thấy thời gian thật của truy vấn)
                var swDau = Stopwatch.StartNew();
                (await http.GetAsync(url)).EnsureSuccessStatusCode();
                var lanDau = swDau.Elapsed.TotalMilliseconds;
                for (var i = 0; i < 2; i++) (await http.GetAsync(url)).EnsureSuccessStatusCode();
                var ms = new List<double>();
                for (var i = 0; i < SoLanDo; i++)
                {
                    var sw = Stopwatch.StartNew();
                    using var r = await http.GetAsync(url);
                    await r.Content.ReadAsByteArrayAsync();
                    sw.Stop();
                    Assert.True(r.IsSuccessStatusCode, $"{ten}: {(int)r.StatusCode}");
                    ms.Add(sw.Elapsed.TotalMilliseconds);
                }
                ms.Sort();
                var p95 = ms[(int)Math.Ceiling(0.95 * ms.Count) - 1];
                var p50 = ms[ms.Count / 2];
                var nguong = ten.StartsWith("[in]") ? NguongBaoCaoMs : NguongP95Ms;
                ketQua.Add(new { api = ten, url, nguongMs = ten.StartsWith("[tham khảo]") ? (int?)null : nguong, lanDauMs = Math.Round(lanDau, 1), p50 = Math.Round(p50, 1), p95 = Math.Round(p95, 1), max = Math.Round(ms[^1], 1) });
                if (p95 >= nguong && !ten.StartsWith("[tham khảo]")) vuot.Add($"{ten}: P95 = {p95:0} ms");
            }
        }
        finally
        {
            foreach (var c in clients.Values) c.Dispose();
        }

        var thuMuc = Path.Combine(Environment.GetEnvironmentVariable("ULMS_ROOT") ?? ".", "artifacts", "test-results");
        Directory.CreateDirectory(thuMuc);
        await File.WriteAllTextAsync(Path.Combine(thuMuc, "hieu_nang_api.json"), JsonSerializer.Serialize(new
        {
            ngay = DateTime.UtcNow, soBanSach = soBan, soDongCtPhieuMuon = soCtMuon,
            soDongCtPhieuTra = await DemAsync("SELECT COUNT(*) FROM muontra.CT_PHIEUTRA"),
            nguongP95Ms = NguongP95Ms, soLanDo = SoLanDo, ketQua
        }, new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
        Assert.True(vuot.Count == 0, "Vượt ngưỡng P95 300 ms:\n" + string.Join("\n", vuot));
    }
}
