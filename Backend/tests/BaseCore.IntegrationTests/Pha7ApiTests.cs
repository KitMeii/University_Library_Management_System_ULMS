using System.Net;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace BaseCore.IntegrationTests;

/// <summary>
/// Pha 7 – trang chủ cổng bạn đọc (file 04): tìm kiếm/khám phá sách, danh mục thể loại, gợi ý từ khóa, “Thư viện bằng số”, quy định
/// công khai. Mọi số liệu so với truy vấn kiểm chứng trực tiếp trên CSDL kiểm thử (không số liệu bịa).
/// </summary>
[Collection("ThongKe")]
public class Pha7ApiTests
{
    private static async Task<JsonElement> LayAsync(string url)
    {
        using var http = MoiTruong.Http();
        var r = await http.GetAsync(url);
        var t = await r.Content.ReadAsStringAsync();
        Assert.True(r.IsSuccessStatusCode, $"{(int)r.StatusCode} {t}");
        return JsonDocument.Parse(t).RootElement.Clone();
    }

    private static async Task<object?> ScalarAsync(string sql)
    {
        await using var cn = MoiTruong.KetNoiSa();
        await cn.OpenAsync();
        await using var cmd = new SqlCommand(sql, cn);
        return await cmd.ExecuteScalarAsync();
    }

    private const string CoBan = "EXISTS (SELECT 1 FROM khosach.DAUSACH_BANSACH b WHERE b.ma_dausach = d.ma_dausach)";

    [Fact]
    public async Task TimKiem_KhongDau_NhieuTu_TheoTacGia_Isbn_TuKhoa()
    {
        // Không dấu, không phân biệt hoa thường; đ ≡ d
        var a = await LayAsync("/api/cong/sach?q=lap%20trinh&coTrang=48");
        Assert.Contains(a.GetProperty("sach").EnumerateArray(), s => s.GetProperty("tenSach").GetString()!.Contains("Lập trình"));
        var kc = Convert.ToInt32(await ScalarAsync($"""
            SELECT COUNT(*) FROM khosach.DAUSACH d WHERE {CoBan} AND
              ((REPLACE(REPLACE(d.ten_sach, N'đ', N'd'), N'Đ', N'D') COLLATE Latin1_General_100_CI_AI LIKE N'%lap%'
                OR REPLACE(REPLACE(ISNULL(d.tacgia, N''), N'đ', N'd'), N'Đ', N'D') COLLATE Latin1_General_100_CI_AI LIKE N'%lap%'
                OR ISNULL(d.isbn, N'') COLLATE Latin1_General_100_CI_AI LIKE N'%lap%'
                OR REPLACE(REPLACE(ISNULL(d.tukhoa, N''), N'đ', N'd'), N'Đ', N'D') COLLATE Latin1_General_100_CI_AI LIKE N'%lap%')
               AND (REPLACE(REPLACE(d.ten_sach, N'đ', N'd'), N'Đ', N'D') COLLATE Latin1_General_100_CI_AI LIKE N'%trinh%'
                OR REPLACE(REPLACE(ISNULL(d.tacgia, N''), N'đ', N'd'), N'Đ', N'D') COLLATE Latin1_General_100_CI_AI LIKE N'%trinh%'
                OR ISNULL(d.isbn, N'') COLLATE Latin1_General_100_CI_AI LIKE N'%trinh%'
                OR REPLACE(REPLACE(ISNULL(d.tukhoa, N''), N'đ', N'd'), N'Đ', N'D') COLLATE Latin1_General_100_CI_AI LIKE N'%trinh%'))
            """));
        Assert.Equal(kc, a.GetProperty("tongSo").GetInt32());

        // Tác giả, ISBN đầy đủ, từ khóa
        var tg = (string)(await ScalarAsync($"SELECT TOP 1 d.tacgia FROM khosach.DAUSACH d WHERE {CoBan} AND d.tacgia IS NOT NULL ORDER BY d.ma_dausach"))!;
        Assert.True((await LayAsync("/api/cong/sach?q=" + Uri.EscapeDataString(tg))).GetProperty("tongSo").GetInt32() >= 1);
        var (ma, isbn) = await DauSachIsbnAsync();
        var theoIsbn = await LayAsync("/api/cong/sach?q=" + isbn);
        Assert.Equal(1, theoIsbn.GetProperty("tongSo").GetInt32());
        Assert.Equal(ma, theoIsbn.GetProperty("sach")[0].GetProperty("maDauSach").GetString());
        // Ký tự đặc biệt của LIKE không làm lỗi truy vấn
        Assert.Equal(0, (await LayAsync("/api/cong/sach?q=" + Uri.EscapeDataString("%_[x"))).GetProperty("tongSo").GetInt32());
        using var http = MoiTruong.Http();
        Assert.Equal(HttpStatusCode.UnprocessableEntity, (await http.GetAsync("/api/cong/sach?sapXep=sai")).StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, (await http.GetAsync("/api/cong/sach?namTu=2020&namDen=2000")).StatusCode);
    }

    private static async Task<(string Ma, string Isbn)> DauSachIsbnAsync()
    {
        await using var cn = MoiTruong.KetNoiSa();
        await cn.OpenAsync();
        await using var cmd = new SqlCommand($"SELECT TOP 1 d.ma_dausach, d.isbn FROM khosach.DAUSACH d WHERE {CoBan} AND d.isbn IS NOT NULL ORDER BY d.ma_dausach", cn);
        await using var r = await cmd.ExecuteReaderAsync();
        await r.ReadAsync();
        return (r.GetString(0), r.GetString(1));
    }

    [Fact]
    public async Task Loc_TheLoai_Nam_ConSach_SapXep_PhanTrang()
    {
        var tl = await LayAsync("/api/cong/the-loai");
        var t = tl[0];
        var ma = t.GetProperty("maTheLoai").GetString()!;
        var theoTl = await LayAsync($"/api/cong/sach?theLoai={ma}&coTrang=48");
        Assert.Equal(t.GetProperty("soDauSach").GetInt32(), theoTl.GetProperty("tongSo").GetInt32());
        Assert.All(theoTl.GetProperty("sach").EnumerateArray(), s => Assert.Equal(ma, s.GetProperty("maTheLoai").GetString()));

        var nam = await LayAsync("/api/cong/sach?namTu=2010&namDen=2019&coTrang=48");
        Assert.Equal(Convert.ToInt32(await ScalarAsync($"SELECT COUNT(*) FROM khosach.DAUSACH d WHERE {CoBan} AND d.namxb BETWEEN 2010 AND 2019")),
            nam.GetProperty("tongSo").GetInt32());
        Assert.All(nam.GetProperty("sach").EnumerateArray(), s => Assert.InRange(s.GetProperty("namXb").GetInt32(), 2010, 2019));

        var con = await LayAsync("/api/cong/sach?conSach=true&coTrang=48");
        Assert.All(con.GetProperty("sach").EnumerateArray(), s => Assert.True(s.GetProperty("soBanCoTheMuon").GetInt32() > 0));
        Assert.True(con.GetProperty("tongSo").GetInt32() <= (await LayAsync("/api/cong/sach")).GetProperty("tongSo").GetInt32());

        var ten = (await LayAsync("/api/cong/sach?sapXep=nam&coTrang=48")).GetProperty("sach").EnumerateArray()
            .Select(s => s.GetProperty("namXb").GetInt32()).ToList();
        Assert.Equal(ten.OrderByDescending(x => x).ToList(), ten);

        // Phân trang “Xem thêm”: 2 trang 12 không trùng, đủ tổng
        var p1 = await LayAsync("/api/cong/sach?coTrang=12&trang=1");
        var p2 = await LayAsync("/api/cong/sach?coTrang=12&trang=2");
        var ma1 = p1.GetProperty("sach").EnumerateArray().Select(s => s.GetProperty("maDauSach").GetString()).ToHashSet();
        Assert.Equal(12, ma1.Count);
        Assert.DoesNotContain(p2.GetProperty("sach").EnumerateArray(), s => ma1.Contains(s.GetProperty("maDauSach").GetString()));
        Assert.Equal(Convert.ToInt32(await ScalarAsync($"SELECT COUNT(*) FROM khosach.DAUSACH d WHERE {CoBan}")), p1.GetProperty("tongSo").GetInt32());
    }

    [Fact]
    public async Task TheLoai_SoLieu_TuKhoa_QuyDinh_KhopTruyVanKiemChung()
    {
        var tl = await LayAsync("/api/cong/the-loai");
        foreach (var t in tl.EnumerateArray())
            Assert.Equal(Convert.ToInt32(await ScalarAsync($"SELECT COUNT(*) FROM khosach.DAUSACH d WHERE {CoBan} AND d.ma_theloai = '{t.GetProperty("maTheLoai").GetString()}'")),
                t.GetProperty("soDauSach").GetInt32());
        Assert.All(tl.EnumerateArray(), t => Assert.True(t.GetProperty("soDauSach").GetInt32() > 0)); // không hiện thể loại rỗng

        var sl = await LayAsync("/api/cong/sach/so-lieu");
        Assert.Equal(Convert.ToInt32(await ScalarAsync($"SELECT COUNT(*) FROM khosach.DAUSACH d WHERE {CoBan}")), sl.GetProperty("soDauSach").GetInt32());
        Assert.Equal(Convert.ToInt32(await ScalarAsync("SELECT COUNT(*) FROM khosach.SACH WHERE tinhtrang <> N'Mất'")), sl.GetProperty("soBanSach").GetInt32());
        Assert.Equal(Convert.ToInt32(await ScalarAsync("SELECT COUNT(*) FROM muontra.BANDOC WHERE hsd_tungay <= CAST(SYSDATETIME() AS date) AND hsd_denngay >= CAST(SYSDATETIME() AS date)")),
            sl.GetProperty("soBanDoc").GetInt32());
        // Lượt mượn tháng này: các kiểm thử khác chạy song song có thể lập thêm phiếu → so sánh trong cùng khoảng thời điểm
        var luot = sl.GetProperty("luotMuonThangNay").GetInt32();
        var kc = Convert.ToInt32(await ScalarAsync("SELECT COUNT(*) FROM muontra.CT_PHIEUMUON WHERE ngaymuon >= DATEFROMPARTS(YEAR(SYSDATETIME()), MONTH(SYSDATETIME()), 1) AND ngaymuon <= CAST(SYSDATETIME() AS date)"));
        Assert.InRange(luot, 0, kc);

        var tk = await LayAsync("/api/cong/sach/tu-khoa?soLuong=8");
        Assert.InRange(tk.GetArrayLength(), 1, 8);
        var soDs = tk.EnumerateArray().Select(x => x.GetProperty("soDauSach").GetInt32()).ToList();
        Assert.Equal(soDs.OrderByDescending(x => x).ToList(), soDs);
        // Từ khóa gợi ý tìm ra đúng số đầu sách đã đếm (ít nhất)
        var dau = tk[0];
        Assert.True((await LayAsync("/api/cong/sach?q=" + Uri.EscapeDataString(dau.GetProperty("tuKhoa").GetString()!))).GetProperty("tongSo").GetInt32()
                    >= dau.GetProperty("soDauSach").GetInt32());

        // Quy định công khai (không cần đăng nhập)
        var qd = await LayAsync("/api/cong/quy-dinh");
        Assert.Equal(24, qd.GetProperty("soGioGiu").GetInt32());
        Assert.Equal(3, qd.GetProperty("toiDaDangGiu").GetInt32());
        Assert.Equal(3, qd.GetProperty("doiTuong").GetArrayLength());
    }
}
