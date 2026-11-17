using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace BaseCore.IntegrationTests;

/// <summary>
/// Pha 6 – đặt chỗ 24 giờ (file 06), tài khoản bạn đọc (file 07) qua gateway. Mỗi kiểm thử tạo Bạn đọc mới qua 01.01.01
/// (tài khoản cổng “Chưa kích hoạt”) rồi kích hoạt qua /api/cong/kich-hoat – không phụ thuộc dữ liệu mẫu. Đầu sách chỉ có
/// MỘT bản được tạo trực tiếp bằng SQL (CSDL ThuVien_Test) cho các kiểm thử “bản cuối cùng”.
/// Tác vụ nền quét mỗi 3 giây trong bộ kiểm thử (DATCHO_CHU_KY_PHUT=0.05).
/// </summary>
[Collection("ThongKe")]
public class Pha6ApiTests
{
    private const string MatKhauBd = "Docgia@123";

    // ───────────── Tiện ích ─────────────
    private static async Task<HttpClient> NhanVienAsync(string ten)
    {
        var http = MoiTruong.Http();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await MoiTruong.DangNhapNhanVienAsync(http, ten));
        return http;
    }

    private static async Task<JsonElement> JsonAsync(HttpResponseMessage r)
    {
        var t = await r.Content.ReadAsStringAsync();
        Assert.True(r.IsSuccessStatusCode, $"{(int)r.StatusCode} {t}");
        return JsonDocument.Parse(t).RootElement.Clone();
    }

    private static async Task<(HttpStatusCode Ma, JsonElement Json)> GoiAsync(HttpResponseMessage r)
    {
        var t = await r.Content.ReadAsStringAsync();
        return (r.StatusCode, string.IsNullOrWhiteSpace(t) ? default : JsonDocument.Parse(t).RootElement.Clone());
    }

    private static async Task<object?> ScalarAsync(string sql, params (string, object)[] ts)
    {
        await using var cn = MoiTruong.KetNoiSa();
        await cn.OpenAsync();
        await using var cmd = new SqlCommand(sql, cn);
        foreach (var (k, v) in ts) cmd.Parameters.AddWithValue(k, v);
        return await cmd.ExecuteScalarAsync();
    }

    private static Task ExecAsync(string sql, params (string, object)[] ts) => ScalarAsync(sql, ts);

    private static string Ngau(string tienTo, int doDai) => (tienTo + Guid.NewGuid().ToString("N").ToUpperInvariant())[..doDai];

    private sealed record BanDocKt(string Ma, string MaSv, string SoThe, DateOnly NgaySinh, HttpClient Http);

    /// <summary>Bạn đọc mới (01.01.01) → tài khoản cổng “Chưa kích hoạt” → kích hoạt → đăng nhập → HttpClient mang token bạn đọc.</summary>
    private static async Task<BanDocKt> BanDocAsync(HttpClient n1, string doiTuong = "Cán bộ")
    {
        var ma = Ngau("BK", 10);
        var sv = Ngau("DCK", 12);
        var homNay = DateOnly.FromDateTime(GioVietNam.HomNay);
        var ngaySinh = new DateOnly(1995, 3, 14);
        await JsonAsync(await n1.PostAsJsonAsync("/api/quan-tri/ban-doc", new
        {
            maBanDoc = ma, hoTen = "Bạn đọc kiểm thử đặt chỗ", doiTuong, maSvHvCb = sv, lopDonVi = "Phòng thử nghiệm",
            ngaySinh, soTheBd = "T" + sv, hsdTuNgay = homNay.AddDays(-1), hsdDenNgay = homNay.AddYears(2)
        }));
        using var an = MoiTruong.Http();
        await JsonAsync(await an.PostAsJsonAsync("/api/cong/kich-hoat",
            new { maSvHvCb = sv, soTheBd = "T" + sv, ngaySinh, matKhau = MatKhauBd, nhapLai = MatKhauBd }));
        var dn = await JsonAsync(await an.PostAsJsonAsync("/api/cong/dang-nhap", new { maSvHvCb = sv, matKhau = MatKhauBd }));
        var http = MoiTruong.Http();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", dn.GetProperty("token").GetString());
        return new BanDocKt(ma, sv, "T" + sv, ngaySinh, http);
    }

    /// <summary>Đầu sách mới có đúng <paramref name="soBan"/> bản “Sẵn sàng” ở Kệ mở (tạo bằng SQL trên CSDL kiểm thử).</summary>
    private static async Task<(string MaDauSach, List<string> Ban)> DauSachMoiAsync(int soBan = 1)
    {
        var ma = Ngau("DK", 10);
        var ban = Enumerable.Range(0, soBan).Select(_ => Ngau("SK", 10)).ToList();
        await ExecAsync("""
            DECLARE @kv nvarchar(10) = (SELECT TOP 1 ma_khuvuc FROM khosach.KHUVUC WHERE loai_khuvuc = N'Kệ mở' ORDER BY ma_khuvuc);
            INSERT INTO khosach.DAUSACH (ma_dausach, ten_sach, tacgia, namxb, ngaytao) VALUES (@ma, N'Sách kiểm thử đặt chỗ ' + @ma, N'Tác giả kiểm thử', 2026, SYSDATETIME());
            """, ("@ma", ma));
        foreach (var b in ban)
            await ExecAsync("""
                DECLARE @kv nvarchar(10) = (SELECT TOP 1 ma_khuvuc FROM khosach.KHUVUC WHERE loai_khuvuc = N'Kệ mở' ORDER BY ma_khuvuc);
                INSERT INTO khosach.SACH (ma_bansach, ma_khuvuc, ten_sach, tacgia, tinhtrang) VALUES (@b, @kv, N'Sách kiểm thử đặt chỗ ' + @ma, N'Tác giả kiểm thử', N'Sẵn sàng');
                INSERT INTO khosach.DAUSACH_BANSACH (ma_bansach, ma_dausach) VALUES (@b, @ma);
                """, ("@b", b), ("@ma", ma));
        return (ma, ban);
    }

    private static Task<HttpResponseMessage> DatAsync(BanDocKt bd, string maDauSach) =>
        bd.Http.PostAsJsonAsync("/api/cong/dat-cho", new { maDauSach });

    private static async Task<string> DatThanhCongAsync(BanDocKt bd, string maDauSach)
    {
        var kq = await JsonAsync(await DatAsync(bd, maDauSach));
        return kq.GetProperty("datCho").GetProperty("maDatCho").GetString()!;
    }

    private static async Task<int> SoBanCoTheMuonAsync(string maDauSach)
    {
        using var an = MoiTruong.Http();
        return (await JsonAsync(await an.GetAsync("/api/cong/sach/" + maDauSach))).GetProperty("soBanCoTheMuon").GetInt32();
    }

    private static async Task<string> MaLoiAsync(HttpResponseMessage r, HttpStatusCode mong)
    {
        var (ma, json) = await GoiAsync(r);
        Assert.True(ma == mong, $"Mong {(int)mong}, nhận {(int)ma}: {json}");
        return json.GetProperty("ma").GetString()!;
    }

    // ───────────── Đặt chỗ – quy tắc và điều kiện (file 06 mục 2, 8) ─────────────

    [Fact]
    public async Task DatCho_ThanhCong_TaoPhieu24Gio_ThongBao_KhongDoiTinhTrangSach()
    {
        using var n1 = await NhanVienAsync("muontra");
        var bd = await BanDocAsync(n1);
        var (ds, ban) = await DauSachMoiAsync(2);
        Assert.Equal(2, await SoBanCoTheMuonAsync(ds));

        var kq = await JsonAsync(await DatAsync(bd, ds));
        var dc = kq.GetProperty("datCho");
        var ma = dc.GetProperty("maDatCho").GetString()!;
        Assert.Matches(@"^DC\d{8}-\d{4}$", ma);
        Assert.Equal("Đang giữ", dc.GetProperty("trangThai").GetString());
        Assert.Equal(ban.Min(), dc.GetProperty("maBanSach").GetString()); // ưu tiên Kệ mở, mã nhỏ nhất
        var tgDat = dc.GetProperty("tgDat").GetDateTime();
        Assert.Equal(TimeSpan.FromHours(24), dc.GetProperty("tgHetHan").GetDateTime() - tgDat);

        Assert.Equal(1, await SoBanCoTheMuonAsync(ds)); // số bản có thể mượn = Sẵn sàng trừ bản đang giữ
        Assert.Equal("Sẵn sàng", await ScalarAsync("SELECT tinhtrang FROM khosach.SACH WHERE ma_bansach = @b", ("@b", dc.GetProperty("maBanSach").GetString()!)));
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM muontra.THONGBAO WHERE ma_bandoc = @bd AND loai = N'Đặt chỗ thành công' AND lienket LIKE '%' + @ma", ("@bd", bd.Ma), ("@ma", ma)));

        var cuaToi = await JsonAsync(await bd.Http.GetAsync("/api/cong/dat-cho/cua-toi"));
        Assert.Contains(cuaToi.EnumerateArray(), x => x.GetProperty("maDatCho").GetString() == ma);
        var tt = await JsonAsync(await bd.Http.GetAsync("/api/cong/dat-cho/dau-sach/" + ds));
        Assert.Equal(ma, tt.GetProperty("datCho").GetProperty("maDatCho").GetString());
    }

    [Fact]
    public async Task DieuKien_DaGiuDauSach_VuotSoDatCho_HetBan()
    {
        using var n1 = await NhanVienAsync("muontra");
        var a = await BanDocAsync(n1);
        var b = await BanDocAsync(n1);
        var (ds1, _) = await DauSachMoiAsync(1);
        var (ds2, _) = await DauSachMoiAsync(1);
        var (ds3, _) = await DauSachMoiAsync(1);
        var (ds4, _) = await DauSachMoiAsync(1);

        await DatThanhCongAsync(a, ds1);
        Assert.Equal("DA_GIU_DAU_SACH", await MaLoiAsync(await DatAsync(a, ds1), HttpStatusCode.Conflict));
        // Bạn đọc khác: bản duy nhất đã được giữ → hết bản có thể đặt
        Assert.Equal("HET_BAN_CO_THE_DAT", await MaLoiAsync(await DatAsync(b, ds1), HttpStatusCode.Conflict));

        await DatThanhCongAsync(a, ds2);
        await DatThanhCongAsync(a, ds3);
        var (ma, json) = await GoiAsync(await DatAsync(a, ds4));
        Assert.Equal(HttpStatusCode.Conflict, ma);
        Assert.Equal("VUOT_SO_DAT_CHO", json.GetProperty("ma").GetString());
        Assert.False(string.IsNullOrEmpty(json.GetProperty("truong").GetProperty("huongXuLy").GetString()));
    }

    [Fact]
    public async Task DieuKien_VuotSoBanToiDa_DangMuonCongDangGiu()
    {
        using var n1 = await NhanVienAsync("muontra");
        var bd = await BanDocAsync(n1, "Sinh viên"); // tối đa 5 bản
        // Mượn 3 bản (bản sách mới, không thuộc đầu sách cần đặt)
        var (_, muon) = await DauSachMoiAsync(3);
        await JsonAsync(await n1.PostAsJsonAsync("/api/quan-tri/phieu-muon", new
        {
            maBanDoc = bd.Ma, soPMuon = (string?)null,
            chiTiet = muon.Select(m => new { maBanSach = m, tinhTrangMuon = "Bình thường", ghiChuMuon = (string?)null })
        }));
        var (ds1, _) = await DauSachMoiAsync(1);
        var (ds2, _) = await DauSachMoiAsync(1);
        var (ds3, _) = await DauSachMoiAsync(1);
        await DatThanhCongAsync(bd, ds1);
        await DatThanhCongAsync(bd, ds2); // 3 đang mượn + 2 đang giữ = 5
        Assert.Equal("VUOT_SO_BAN_TOI_DA", await MaLoiAsync(await DatAsync(bd, ds3), HttpStatusCode.Conflict));
    }

    [Fact]
    public async Task DieuKien_TaiKhoanKhoa_TheHetHan_QuaHan_ViPhamChuaThanhToan()
    {
        using var n1 = await NhanVienAsync("muontra");
        using var n4 = await NhanVienAsync("admin");
        var (ds, _) = await DauSachMoiAsync(2);

        // 1. Tài khoản bị N4 khóa (mở rộng Danh mục Bạn đọc)
        var k = await BanDocAsync(n1);
        var khoa = await JsonAsync(await n4.PutAsJsonAsync($"/api/quan-tri/ban-doc/{k.Ma}/tai-khoan-cong", new { khoa = true }));
        Assert.Equal("Khóa", khoa.GetProperty("tinhTrang").GetString());
        Assert.Equal("TAI_KHOAN_KHONG_HOAT_DONG", await MaLoiAsync(await DatAsync(k, ds), HttpStatusCode.Forbidden));
        using (var an = MoiTruong.Http())
            Assert.Equal("TAI_KHOAN_KHOA", await MaLoiAsync(await an.PostAsJsonAsync("/api/cong/dang-nhap", new { maSvHvCb = k.MaSv, matKhau = MatKhauBd }), HttpStatusCode.Forbidden));
        await JsonAsync(await n4.PutAsJsonAsync($"/api/quan-tri/ban-doc/{k.Ma}/tai-khoan-cong", new { khoa = false }));
        Assert.Equal("Hoạt động", await ScalarAsync("SELECT tinhtrang FROM muontra.TAIKHOAN_BANDOC WHERE ma_bandoc = @m", ("@m", k.Ma)));

        // 2. Thẻ hết hạn
        var t = await BanDocAsync(n1);
        await ExecAsync("UPDATE muontra.BANDOC SET hsd_tungay = DATEADD(day, -30, CAST(SYSDATETIME() AS date)), hsd_denngay = DATEADD(day, -1, CAST(SYSDATETIME() AS date)) WHERE ma_bandoc = @m", ("@m", t.Ma));
        Assert.Equal("THE_HET_HAN", await MaLoiAsync(await DatAsync(t, ds), HttpStatusCode.Conflict));

        // 3. Có sách quá hạn chưa trả
        var q = await BanDocAsync(n1);
        var (_, muon) = await DauSachMoiAsync(1);
        var pm = await JsonAsync(await n1.PostAsJsonAsync("/api/quan-tri/phieu-muon", new
        {
            maBanDoc = q.Ma, soPMuon = (string?)null, chiTiet = new[] { new { maBanSach = muon[0], tinhTrangMuon = "Bình thường", ghiChuMuon = (string?)null } }
        }));
        await ExecAsync("UPDATE muontra.CT_PHIEUMUON SET hantra = DATEADD(day, -3, CAST(SYSDATETIME() AS date)) WHERE so_pmuon = @s", ("@s", pm.GetProperty("so").GetString()!));
        Assert.Equal("CO_SACH_QUA_HAN", await MaLoiAsync(await DatAsync(q, ds), HttpStatusCode.Conflict));

        // 4. Có biên bản chưa thanh toán
        var v = await BanDocAsync(n1);
        var (_, muon2) = await DauSachMoiAsync(1);
        var pm2 = await JsonAsync(await n1.PostAsJsonAsync("/api/quan-tri/phieu-muon", new
        {
            maBanDoc = v.Ma, soPMuon = (string?)null, chiTiet = new[] { new { maBanSach = muon2[0], tinhTrangMuon = "Bình thường", ghiChuMuon = (string?)null } }
        }));
        await ExecAsync("""
            INSERT INTO vipham.BIENBAN (so_bienban, so_pmuon, ma_bandoc, thoigian_lap, ht_thanhtoan, ma_nd, ten_nd, tong_tien)
            VALUES (@so, @pm, @bd, SYSDATETIME(), N'Chưa thanh toán', 'ND0003', N'Kiểm thử', 5000)
            """, ("@so", Ngau("BX", 10)), ("@pm", pm2.GetProperty("so").GetString()!), ("@bd", v.Ma));
        Assert.Equal("CO_VI_PHAM_CHUA_THANH_TOAN", await MaLoiAsync(await DatAsync(v, ds), HttpStatusCode.Conflict));

        // Chưa đăng nhập
        using var an2 = MoiTruong.Http();
        Assert.Equal(HttpStatusCode.Unauthorized, (await an2.PostAsJsonAsync("/api/cong/dat-cho", new { maDauSach = ds })).StatusCode);
    }

    /// <summary>File 06 mục 8: hai (nhiều) bạn đọc cùng đặt bản cuối cùng – 20 request song song: đúng 1 thành công, còn lại 409.</summary>
    [Fact]
    public async Task DongThoi_20YeuCau_BanCuoiCung_Dung1ThanhCong()
    {
        using var n1 = await NhanVienAsync("muontra");
        var banDoc = new List<BanDocKt>();
        for (var i = 0; i < 10; i++) banDoc.Add(await BanDocAsync(n1));
        var (ds, ban) = await DauSachMoiAsync(1);

        using var batDau = new SemaphoreSlim(0);
        var viec = Enumerable.Range(0, 20).Select(async i =>
        {
            await batDau.WaitAsync();
            using var r = await DatAsync(banDoc[i % 10], ds);
            return r.StatusCode;
        }).ToList();
        batDau.Release(20);
        var kq = await Task.WhenAll(viec);

        Assert.Equal(1, kq.Count(x => x == HttpStatusCode.OK));
        Assert.Equal(19, kq.Count(x => x == HttpStatusCode.Conflict));
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM muontra.PHIEUDATCHO WHERE ma_dausach = @d AND trangthai = N'Đang giữ'", ("@d", ds)));
        Assert.Equal(0, await SoBanCoTheMuonAsync(ds));
        Assert.Equal(ban[0], await ScalarAsync("SELECT ma_bansach FROM muontra.PHIEUDATCHO WHERE ma_dausach = @d", ("@d", ds)));
    }

    // ───────────── Vòng đời (file 06 mục 3) ─────────────

    /// <summary>Tua thời gian qua 24 giờ: bản sách trở lại có thể mượn NGAY (không chờ tác vụ nền); tác vụ nền chuyển “Hết hạn” + thông báo.</summary>
    [Fact]
    public async Task QuaHan24Gio_TroLaiCoTheMuon_TacVuNenChuyenHetHan_ThongBao()
    {
        using var n1 = await NhanVienAsync("muontra");
        var bd = await BanDocAsync(n1);
        var (ds, _) = await DauSachMoiAsync(1);
        var ma = await DatThanhCongAsync(bd, ds);
        Assert.Equal(0, await SoBanCoTheMuonAsync(ds));

        await ExecAsync("UPDATE muontra.PHIEUDATCHO SET tg_dat = DATEADD(hour, -25, tg_dat), tg_hethan = DATEADD(hour, -25, tg_hethan) WHERE ma_datcho = @m", ("@m", ma));
        Assert.Equal(1, await SoBanCoTheMuonAsync(ds)); // truy vấn luôn coi tg_hethan < bây giờ là đã hết

        string? tt = null;
        for (var i = 0; i < 40 && tt != "Hết hạn"; i++)
        {
            await Task.Delay(500);
            tt = (string?)await ScalarAsync("SELECT trangthai FROM muontra.PHIEUDATCHO WHERE ma_datcho = @m", ("@m", ma));
        }
        Assert.Equal("Hết hạn", tt);
        Assert.NotNull(await ScalarAsync("SELECT tg_xuly FROM muontra.PHIEUDATCHO WHERE ma_datcho = @m", ("@m", ma)));
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM muontra.THONGBAO WHERE ma_bandoc = @bd AND loai = N'Hết hạn giữ'", ("@bd", bd.Ma)));
        // Đặt lại được sau khi hết hạn
        await DatThanhCongAsync(bd, ds);
    }

    [Fact]
    public async Task ConDuoi2Gio_NhacSapHetHan_MotLan()
    {
        using var n1 = await NhanVienAsync("muontra");
        var bd = await BanDocAsync(n1);
        var (ds, _) = await DauSachMoiAsync(1);
        var ma = await DatThanhCongAsync(bd, ds);
        await ExecAsync("UPDATE muontra.PHIEUDATCHO SET tg_dat = DATEADD(minute, -23 * 60, tg_dat), tg_hethan = DATEADD(minute, -23 * 60, tg_hethan) WHERE ma_datcho = @m", ("@m", ma));
        object? so = 0;
        for (var i = 0; i < 40 && (int)so! == 0; i++)
        {
            await Task.Delay(500);
            so = await ScalarAsync("SELECT COUNT(*) FROM muontra.THONGBAO WHERE ma_bandoc = @bd AND loai = N'Sắp hết hạn giữ'", ("@bd", bd.Ma));
        }
        Assert.Equal(1, so);
        await Task.Delay(7000); // thêm ≥ 2 chu kỳ quét – không nhắc lặp
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM muontra.THONGBAO WHERE ma_bandoc = @bd AND loai = N'Sắp hết hạn giữ'", ("@bd", bd.Ma)));
        Assert.Equal("Đang giữ", await ScalarAsync("SELECT trangthai FROM muontra.PHIEUDATCHO WHERE ma_datcho = @m", ("@m", ma)));
    }

    [Fact]
    public async Task BanDocHuy_NhanVienHuyCoLyDo_KhongHuyLaiDuoc()
    {
        using var n1 = await NhanVienAsync("muontra");
        var bd = await BanDocAsync(n1);
        var (ds, _) = await DauSachMoiAsync(2);
        var ma1 = await DatThanhCongAsync(bd, ds);
        var huy = await JsonAsync(await bd.Http.DeleteAsync("/api/cong/dat-cho/" + ma1));
        Assert.Equal("Đã hủy", huy.GetProperty("trangThai").GetString());
        Assert.Equal("DAT_CHO_KHONG_CON_GIU", await MaLoiAsync(await bd.Http.DeleteAsync("/api/cong/dat-cho/" + ma1), HttpStatusCode.Conflict));

        var ma2 = await DatThanhCongAsync(bd, ds);
        // Bạn đọc khác không hủy được đặt chỗ của người khác
        var khac = await BanDocAsync(n1);
        Assert.Equal(HttpStatusCode.NotFound, (await khac.Http.DeleteAsync("/api/cong/dat-cho/" + ma2)).StatusCode);
        // Nhân viên hủy: bắt buộc lý do
        Assert.Equal(HttpStatusCode.UnprocessableEntity, (await n1.PostAsJsonAsync($"/api/quan-tri/dat-cho/{ma2}/huy", new { lyDo = "  " })).StatusCode);
        var h2 = await JsonAsync(await n1.PostAsJsonAsync($"/api/quan-tri/dat-cho/{ma2}/huy", new { lyDo = "Bản sách cần sửa gáy" }));
        Assert.Equal("Đã hủy", h2.GetProperty("trangThai").GetString());
        Assert.Equal("Bản sách cần sửa gáy", h2.GetProperty("lyDoHuy").GetString());
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM quantri.NHATKY_HOATDONG WHERE noidung LIKE N'%hủy đặt chỗ ' + @m + '%'", ("@m", ma2)));
        // D63: thông báo loại “Đặt chỗ bị hủy” kèm lý do; bạn đọc tự hủy thì không gửi
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM muontra.THONGBAO WHERE ma_bandoc = @bd AND loai = N'Đặt chỗ bị hủy' AND noidung LIKE N'%Bản sách cần sửa gáy%'", ("@bd", bd.Ma)));
    }

    /// <summary>File 06 mục 4, 8: nhận tại quầy qua Lập Phiếu mượn → “Đã nhận” + so_pmuon; không cho người khác mượn bản đang giữ.</summary>
    [Fact]
    public async Task NhanTaiQuay_QuaLapPhieuMuon_DaNhan_KhongChoNguoiKhacMuon()
    {
        using var n1 = await NhanVienAsync("muontra");
        var a = await BanDocAsync(n1);
        var b = await BanDocAsync(n1);
        var (ds, ban) = await DauSachMoiAsync(1);
        var ma = await DatThanhCongAsync(a, ds);

        // dgvDatCho của A
        var giu = await JsonAsync(await n1.GetAsync("/api/quan-tri/dat-cho/ban-doc/" + a.Ma));
        Assert.Contains(giu.EnumerateArray(), x => x.GetProperty("maDatCho").GetString() == ma);

        // btnThemSach cho B với bản đang giữ cho A → từ chối
        var kt = await JsonAsync(await n1.GetAsync($"/api/quan-tri/phieu-muon/ban-sach?maBanSach={ban[0]}&maBanDoc={b.Ma}"));
        Assert.False(kt.GetProperty("dapUng").GetBoolean());
        Assert.Contains("đang được giữ chỗ cho bạn đọc khác", kt.GetProperty("lyDo").GetString());
        var luuB = await n1.PostAsJsonAsync("/api/quan-tri/phieu-muon", new
        {
            maBanDoc = b.Ma, soPMuon = (string?)null, chiTiet = new[] { new { maBanSach = ban[0], tinhTrangMuon = "Bình thường", ghiChuMuon = (string?)null } }
        });
        Assert.False(luuB.IsSuccessStatusCode);

        // A nhận: btnThemSach đáp ứng; btnIn → đặt chỗ “Đã nhận”
        Assert.True((await JsonAsync(await n1.GetAsync($"/api/quan-tri/phieu-muon/ban-sach?maBanSach={ban[0]}&maBanDoc={a.Ma}"))).GetProperty("dapUng").GetBoolean());
        var pm = await JsonAsync(await n1.PostAsJsonAsync("/api/quan-tri/phieu-muon", new
        {
            maBanDoc = a.Ma, soPMuon = (string?)null, chiTiet = new[] { new { maBanSach = ban[0], tinhTrangMuon = "Bình thường", ghiChuMuon = (string?)null } }
        }));
        var so = pm.GetProperty("so").GetString()!;
        Assert.Contains("Đã giao 1 sách đặt chỗ", pm.GetProperty("thongBao").GetString());
        Assert.Equal("Đã nhận", await ScalarAsync("SELECT trangthai FROM muontra.PHIEUDATCHO WHERE ma_datcho = @m", ("@m", ma)));
        Assert.Equal(so, await ScalarAsync("SELECT so_pmuon FROM muontra.PHIEUDATCHO WHERE ma_datcho = @m", ("@m", ma)));
        Assert.Equal("ND0002", await ScalarAsync("SELECT ma_nd_xuly FROM muontra.PHIEUDATCHO WHERE ma_datcho = @m", ("@m", ma)));
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM muontra.THONGBAO WHERE ma_bandoc = @bd AND loai = N'Đã nhận sách'", ("@bd", a.Ma)));
    }

    // ───────────── Tài khoản bạn đọc (file 07) ─────────────

    [Fact]
    public async Task KichHoat_SaiThongTin_KhoaTam_MatKhauYeu_DatLaiMatKhau()
    {
        using var n1 = await NhanVienAsync("muontra");
        var ma = Ngau("BK", 10);
        var sv = Ngau("KHK", 12);
        var homNay = DateOnly.FromDateTime(GioVietNam.HomNay);
        var ngaySinh = new DateOnly(2001, 6, 2);
        await JsonAsync(await n1.PostAsJsonAsync("/api/quan-tri/ban-doc", new
        {
            maBanDoc = ma, hoTen = "Bạn đọc kiểm thử kích hoạt", doiTuong = "Sinh viên", maSvHvCb = sv, lopDonVi = "K26",
            ngaySinh, soTheBd = "T" + sv, hsdTuNgay = homNay.AddDays(-1), hsdDenNgay = homNay.AddYears(1)
        }));
        Assert.Equal("Chưa kích hoạt", await ScalarAsync("SELECT tinhtrang FROM muontra.TAIKHOAN_BANDOC WHERE ma_bandoc = @m", ("@m", ma)));
        using var an = MoiTruong.Http();
        // Chưa kích hoạt → không đăng nhập được
        Assert.Equal("CHUA_KICH_HOAT", await MaLoiAsync(await an.PostAsJsonAsync("/api/cong/dang-nhap", new { maSvHvCb = sv, matKhau = MatKhauBd }), HttpStatusCode.Forbidden));
        // Mật khẩu yếu
        Assert.Equal(HttpStatusCode.UnprocessableEntity, (await an.PostAsJsonAsync("/api/cong/kich-hoat",
            new { maSvHvCb = sv, soTheBd = "T" + sv, ngaySinh, matKhau = "abcdefgh", nhapLai = "abcdefgh" })).StatusCode);
        // Sai ngày sinh 5 lần → tạm khóa 15 phút; kể cả nhập đúng sau đó cũng bị chặn
        for (var i = 1; i <= 4; i++)
            Assert.Equal("SAI_THONG_TIN_KICH_HOAT", await MaLoiAsync(await an.PostAsJsonAsync("/api/cong/kich-hoat",
                new { maSvHvCb = sv, soTheBd = "T" + sv, ngaySinh = ngaySinh.AddDays(i), matKhau = MatKhauBd, nhapLai = MatKhauBd }), HttpStatusCode.UnprocessableEntity));
        Assert.Equal("TAM_KHOA", await MaLoiAsync(await an.PostAsJsonAsync("/api/cong/kich-hoat",
            new { maSvHvCb = sv, soTheBd = "X" + sv, ngaySinh, matKhau = MatKhauBd, nhapLai = MatKhauBd }), HttpStatusCode.Forbidden));
        Assert.Equal("TAM_KHOA", await MaLoiAsync(await an.PostAsJsonAsync("/api/cong/kich-hoat",
            new { maSvHvCb = sv, soTheBd = "T" + sv, ngaySinh, matKhau = MatKhauBd, nhapLai = MatKhauBd }), HttpStatusCode.Forbidden));
        // Hết thời gian khóa → kích hoạt đúng thành công → đăng nhập được; kích hoạt lại = đặt lại mật khẩu
        await ExecAsync("UPDATE muontra.TAIKHOAN_BANDOC SET tg_khoa_den = DATEADD(minute, -1, SYSDATETIME()) WHERE ma_bandoc = @m", ("@m", ma));
        await JsonAsync(await an.PostAsJsonAsync("/api/cong/kich-hoat", new { maSvHvCb = sv, soTheBd = "T" + sv, ngaySinh, matKhau = MatKhauBd, nhapLai = MatKhauBd }));
        Assert.Equal("Hoạt động", await ScalarAsync("SELECT tinhtrang FROM muontra.TAIKHOAN_BANDOC WHERE ma_bandoc = @m", ("@m", ma)));
        await JsonAsync(await an.PostAsJsonAsync("/api/cong/dang-nhap", new { maSvHvCb = sv, matKhau = MatKhauBd }));
        var dat = await JsonAsync(await an.PostAsJsonAsync("/api/cong/kich-hoat", new { maSvHvCb = sv, soTheBd = "T" + sv, ngaySinh, matKhau = "Moi12345", nhapLai = "Moi12345" }));
        Assert.Contains("Đặt lại mật khẩu", dat.GetProperty("thongBao").GetString());
        await JsonAsync(await an.PostAsJsonAsync("/api/cong/dang-nhap", new { maSvHvCb = sv, matKhau = "Moi12345" }));
    }

    [Fact]
    public async Task TrangCaNhan_SoLieuKhopTruyVanKiemChung_YeuThich_ThongBao_DoiMatKhau()
    {
        using var n1 = await NhanVienAsync("muontra");
        var bd = await BanDocAsync(n1);
        var (_, muon) = await DauSachMoiAsync(2);
        await JsonAsync(await n1.PostAsJsonAsync("/api/quan-tri/phieu-muon", new
        {
            maBanDoc = bd.Ma, soPMuon = (string?)null, chiTiet = muon.Select(m => new { maBanSach = m, tinhTrangMuon = "Bình thường", ghiChuMuon = (string?)null })
        }));
        var (ds, _) = await DauSachMoiAsync(1);
        await DatThanhCongAsync(bd, ds);

        var tq = await JsonAsync(await bd.Http.GetAsync("/api/cong/tai-khoan/tong-quan"));
        Assert.Equal(await ScalarAsync("""
            SELECT COUNT(*) FROM muontra.CT_PHIEUMUON cm JOIN muontra.PHIEUMUON pm ON pm.so_pmuon = cm.so_pmuon
            WHERE pm.ma_bandoc = @bd AND NOT EXISTS (SELECT 1 FROM muontra.CT_PHIEUTRA ct JOIN muontra.PHIEUTRA pt ON pt.so_ptra = ct.so_ptra
                                                     WHERE pt.so_pmuon = cm.so_pmuon AND ct.ma_bansach = cm.ma_bansach)
            """, ("@bd", bd.Ma)), tq.GetProperty("dangMuon").GetInt32());
        Assert.Equal(await ScalarAsync("SELECT COUNT(*) FROM muontra.PHIEUDATCHO WHERE ma_bandoc = @bd AND trangthai = N'Đang giữ' AND tg_hethan > SYSDATETIME()", ("@bd", bd.Ma)),
            tq.GetProperty("datChoDangGiu").GetInt32());
        Assert.Equal(12, tq.GetProperty("muonTheoThang").GetArrayLength());
        Assert.Equal(2, tq.GetProperty("muonTheoThang").EnumerateArray().Last().GetProperty("soBan").GetInt32());
        Assert.Equal(2, (await JsonAsync(await bd.Http.GetAsync("/api/cong/tai-khoan/dang-muon"))).GetArrayLength());
        Assert.True((await JsonAsync(await bd.Http.GetAsync("/api/cong/tai-khoan/lich-su"))).GetProperty("dong").GetArrayLength() >= 1);

        // Yêu thích
        Assert.True((await JsonAsync(await bd.Http.PutAsync("/api/cong/yeu-thich/" + ds, null))).GetProperty("yeuThich").GetBoolean());
        Assert.Equal(1, (await JsonAsync(await bd.Http.GetAsync("/api/cong/yeu-thich"))).GetArrayLength());
        Assert.False((await JsonAsync(await bd.Http.DeleteAsync("/api/cong/yeu-thich/" + ds))).GetProperty("yeuThich").GetBoolean());

        // Thông báo: chưa đọc → đánh dấu tất cả
        var tb = await JsonAsync(await bd.Http.GetAsync("/api/cong/thong-bao"));
        Assert.True(tb.GetProperty("chuaDoc").GetInt32() >= 1);
        await JsonAsync(await bd.Http.PostAsync("/api/cong/thong-bao/da-doc-tat-ca", null));
        Assert.Equal(0, (await JsonAsync(await bd.Http.GetAsync("/api/cong/thong-bao"))).GetProperty("chuaDoc").GetInt32());

        // Đổi mật khẩu
        Assert.Equal(HttpStatusCode.UnprocessableEntity, (await bd.Http.PostAsJsonAsync("/api/cong/tai-khoan/doi-mat-khau",
            new { matKhauCu = "SaiMatKhau1", matKhauMoi = "Moi12345", nhapLai = "Moi12345" })).StatusCode);
        await JsonAsync(await bd.Http.PostAsJsonAsync("/api/cong/tai-khoan/doi-mat-khau", new { matKhauCu = MatKhauBd, matKhauMoi = "Moi12345", nhapLai = "Moi12345" }));
        using var an = MoiTruong.Http();
        await JsonAsync(await an.PostAsJsonAsync("/api/cong/dang-nhap", new { maSvHvCb = bd.MaSv, matKhau = "Moi12345" }));
    }

    // ───────────── Phân quyền, Book Preview, tổng quan N1, dữ liệu mẫu ─────────────

    [Fact]
    public async Task PhanQuyen_QuayDatCho_ChiN1_TokenBanDocKhongVaoQuanTri()
    {
        using var n1 = await NhanVienAsync("muontra");
        using var n2 = await NhanVienAsync("vipham");
        using var n3 = await NhanVienAsync("khosach");
        var bd = await BanDocAsync(n1);
        Assert.Equal(HttpStatusCode.OK, (await n1.GetAsync("/api/quan-tri/dat-cho")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await n2.GetAsync("/api/quan-tri/dat-cho")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await n3.PostAsJsonAsync("/api/quan-tri/dat-cho/DC20260101-0001/huy", new { lyDo = "x" })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await n2.GetAsync("/api/quan-tri/ban-doc/" + bd.Ma + "/tai-khoan-cong")).StatusCode);
        // Token bạn đọc không vào được API quản trị và ngược lại (file 07 mục 5)
        Assert.True((await bd.Http.GetAsync("/api/quan-tri/dat-cho")).StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized);
        Assert.True((await n1.GetAsync("/api/cong/dat-cho/cua-toi")).StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task BookPreview_CongKhai_DuThongTin_KhuVuc_404()
    {
        var (ds, _) = await DauSachMoiAsync(2);
        using var an = MoiTruong.Http();
        var s = await JsonAsync(await an.GetAsync("/api/cong/sach/" + ds.ToLowerInvariant()));
        Assert.Equal(ds, s.GetProperty("maDauSach").GetString());
        Assert.Equal(2, s.GetProperty("tongSoBan").GetInt32());
        Assert.Equal(2, s.GetProperty("soBanCoTheMuon").GetInt32());
        var kv = s.GetProperty("khuVuc").EnumerateArray().Single();
        Assert.Equal("Kệ mở", kv.GetProperty("loaiKhuVuc").GetString());
        Assert.Equal(2, kv.GetProperty("soBan").GetInt32());
        Assert.Equal(HttpStatusCode.NotFound, (await an.GetAsync("/api/cong/sach/KHONGCO123")).StatusCode);
    }

    [Fact]
    public async Task TongQuanN1_DatCho_KhopTruyVanKiemChung()
    {
        using var n1 = await NhanVienAsync("muontra");
        var d = await JsonAsync(await n1.GetAsync("/api/quan-tri/thong-ke/n1"));
        var dangGiu = Convert.ToInt32(await ScalarAsync("SELECT COUNT(*) FROM muontra.PHIEUDATCHO WHERE trangthai = N'Đang giữ' AND tg_hethan > SYSDATETIME()"));
        // Song song với kiểm thử khác nên chênh lệch ±vài đặt chỗ; kiểm tra cấu trúc và chiều
        Assert.InRange(d.GetProperty("datChoDangGiu").GetInt32(), Math.Max(0, dangGiu - 15), dangGiu + 15);
        var kq = d.GetProperty("datChoThangNay").EnumerateArray().Select(x => x.GetProperty("ketQua").GetString()).ToList();
        Assert.Equal(["Đã nhận", "Hết hạn", "Đã hủy"], kq);
        // Top 10 theo đầu sách (D64) – có mã đầu sách
        Assert.All(d.GetProperty("top10").EnumerateArray(), x => Assert.False(string.IsNullOrEmpty(x.GetProperty("maDauSach").GetString())));
    }

    /// <summary>File 02 mục 4: dữ liệu mẫu đặt chỗ đủ 4 trạng thái, 2 đặt chỗ đang giữ còn dưới 2 giờ (lúc nạp).</summary>
    [Fact]
    public async Task DuLieuMau_DatCho_Du4TrangThai()
    {
        Assert.Equal(4, await ScalarAsync("SELECT COUNT(DISTINCT trangthai) FROM muontra.PHIEUDATCHO WHERE ma_datcho LIKE 'DC________-____' AND ma_bandoc LIKE 'BD____'"));
        // Hai đặt chỗ mẫu đặt cách lúc nạp hơn 22 giờ (còn < 2 giờ); bộ kiểm thử chạy xong trong khoảng 30 phút sau khi nạp
        Assert.True(Convert.ToInt32(await ScalarAsync(
            "SELECT COUNT(*) FROM muontra.PHIEUDATCHO WHERE ma_bandoc LIKE 'BD____' AND trangthai = N'Đang giữ' AND tg_hethan > SYSDATETIME() AND tg_dat <= DATEADD(hour, -22, SYSDATETIME())")) >= 2,
            "Phải có ít nhất 2 đặt chỗ mẫu đang giữ còn dưới 2 giờ");
        Assert.Equal(0, await ScalarAsync("SELECT COUNT(*) FROM muontra.PHIEUDATCHO WHERE ma_bandoc = 'BD0001' AND trangthai = N'Đang giữ'"));
    }
}
