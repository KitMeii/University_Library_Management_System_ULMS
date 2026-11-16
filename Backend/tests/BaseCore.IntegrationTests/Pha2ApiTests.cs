using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace BaseCore.IntegrationTests;

/// <summary>Pha 2 – API danh mục (SPEC 6.1) và quản trị (6.3): điều kiện toàn vẹn, ràng buộc xóa, quyền 3.2, nhật ký.</summary>
public class Pha2ApiTests
{
    private static async Task<HttpClient> HttpCuaAsync(string ten)
    {
        var http = MoiTruong.Http();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await MoiTruong.DangNhapNhanVienAsync(http, ten));
        return http;
    }

    private static async Task<JsonElement> JsonAsync(HttpResponseMessage r) =>
        JsonDocument.Parse(await r.Content.ReadAsStringAsync()).RootElement.Clone();

    private static async Task<object?> ScalarAsync(string sql, params (string, object)[] ts)
    {
        await using var cn = MoiTruong.KetNoiSa();
        await cn.OpenAsync();
        await using var cmd = new SqlCommand(sql, cn);
        foreach (var (k, v) in ts) cmd.Parameters.AddWithValue(k, v);
        return await cmd.ExecuteScalarAsync();
    }

    private static Task<object?> DemNhatKyAsync(string maNd, string chua) =>
        ScalarAsync("SELECT COUNT(*) FROM quantri.NHATKY_HOATDONG WHERE ma_nd = @nd AND noidung LIKE @c", ("@nd", maNd), ("@c", "%" + chua + "%"));

    private static string MaNgauNhien(string tienTo, int doDai = 10) =>
        (tienTo + Guid.NewGuid().ToString("N").ToUpperInvariant())[..doDai];

    // ---------- 6.1.4 Khu vực ----------
    [Fact]
    public async Task KhuVuc_ToanVen_Them_Sua_Xoa_NhatKy()
    {
        using var http = await HttpCuaAsync("khosach"); // N3: C, E, R
        var ma = MaNgauNhien("KV", 8);
        var loi = await http.PostAsJsonAsync("/api/quan-tri/khu-vuc", new { maKhuVuc = ma, viTri = "", loaiKhuVuc = "Ban công" });
        Assert.Equal((HttpStatusCode)422, loi.StatusCode);
        var truong = (await JsonAsync(loi)).GetProperty("truong");
        Assert.True(truong.TryGetProperty("viTri", out _));
        Assert.True(truong.TryGetProperty("loaiKhuVuc", out _));

        var ok = await http.PostAsJsonAsync("/api/quan-tri/khu-vuc", new { maKhuVuc = ma, viTri = "Tầng thử", loaiKhuVuc = "Kho" });
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        Assert.Equal(1, await DemNhatKyAsync("ND0004", $"Thêm khu vực lưu trữ {ma}"));
        var trung = await http.PostAsJsonAsync("/api/quan-tri/khu-vuc", new { maKhuVuc = ma, viTri = "X", loaiKhuVuc = "Kho" });
        Assert.Equal((HttpStatusCode)422, trung.StatusCode); // không trùng mã

        var sua = await http.PutAsJsonAsync($"/api/quan-tri/khu-vuc/{ma}", new { viTri = "Tầng thử (sửa)", loaiKhuVuc = "Kệ mở" });
        Assert.Equal(HttpStatusCode.OK, sua.StatusCode);
        // N3 không có quyền D (SPEC 3.2) → 403
        Assert.Equal(HttpStatusCode.Forbidden, (await http.DeleteAsync($"/api/quan-tri/khu-vuc/{ma}")).StatusCode);
        using var admin = await HttpCuaAsync("admin");
        Assert.Equal(HttpStatusCode.NoContent, (await admin.DeleteAsync($"/api/quan-tri/khu-vuc/{ma}")).StatusCode);
    }

    [Fact]
    public async Task KhuVuc_ConSach_KhongXoaDuoc()
    {
        using var admin = await HttpCuaAsync("admin");
        var r = await admin.DeleteAsync("/api/quan-tri/khu-vuc/KV01");
        Assert.Equal(HttpStatusCode.Conflict, r.StatusCode);
        Assert.Equal("KHU_VUC_CON_SACH", (await JsonAsync(r)).GetProperty("ma").GetString());
    }

    // ---------- 6.1.2 Khoản thu ----------
    [Fact]
    public async Task KhoanThu_DonGiaPhaiDuong_N2KhongThemDuoc()
    {
        using var n2 = await HttpCuaAsync("vipham");
        Assert.Equal(HttpStatusCode.Forbidden,
            (await n2.PostAsJsonAsync("/api/quan-tri/khoan-thu", new { maKhoanThu = "KTX", noiDungTt = "x", donGia = 1000 })).StatusCode);
        using var admin = await HttpCuaAsync("admin");
        var r = await admin.PostAsJsonAsync("/api/quan-tri/khoan-thu", new { maKhoanThu = MaNgauNhien("KT", 6), noiDungTt = "Thử", donGia = 0 });
        Assert.Equal((HttpStatusCode)422, r.StatusCode);
        Assert.True((await JsonAsync(r)).GetProperty("truong").TryGetProperty("donGia", out _));
    }

    // ---------- 6.1.1 Sách ----------
    [Fact]
    public async Task Sach_ThongKe_Them_Xoa_Quyen()
    {
        using var n1 = await HttpCuaAsync("muontra");
        Assert.Equal(HttpStatusCode.OK, (await n1.GetAsync("/api/quan-tri/sach")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await n1.PostAsJsonAsync("/api/quan-tri/sach", new { })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await n1.GetAsync("/api/quan-tri/sach/chuyen-den")).StatusCode);

        // 03.04.01: tổng theo tình trạng = số bản sách
        var tk = await JsonAsync(await n1.GetAsync("/api/quan-tri/sach/thong-ke-tinh-trang"));
        var tong = tk.EnumerateArray().Sum(x => x.GetProperty("soSach").GetInt32());
        Assert.Equal(Convert.ToInt32(await ScalarAsync("SELECT COUNT(*) FROM khosach.SACH")), tong);

        using var n3 = await HttpCuaAsync("khosach");
        var sai = await n3.PostAsJsonAsync("/api/quan-tri/sach", new { maBanSach = "BS-01", maKhuVuc = "KHONGCO", tenSach = "", tacGia = "" });
        var truong = (await JsonAsync(sai)).GetProperty("truong");
        foreach (var t in new[] { "maBanSach", "maKhuVuc", "tenSach", "tacGia" }) Assert.True(truong.TryGetProperty(t, out _), t);

        var ma = MaNgauNhien("T", 10);
        var them = await JsonAsync(await n3.PostAsJsonAsync("/api/quan-tri/sach", new { maBanSach = ma, maKhuVuc = "KV01", tenSach = "Sách thử", tacGia = "Tác giả thử" }));
        Assert.Equal("Sẵn sàng", them.GetProperty("tinhTrang").GetString()); // mặc định
        using var admin = await HttpCuaAsync("admin");
        Assert.Equal(HttpStatusCode.NoContent, (await admin.DeleteAsync($"/api/quan-tri/sach/{ma}")).StatusCode);
    }

    // ---------- 6.1.3 Bạn đọc ----------
    [Fact]
    public async Task BanDoc_Them_TaoTaiKhoanChoKichHoat_ToanVen_Xoa()
    {
        using var n1 = await HttpCuaAsync("muontra");
        var ma = MaNgauNhien("BT", 10);
        var sv = MaNgauNhien("SVT", 12);
        var homNay = DateOnly.FromDateTime(DateTime.Today);

        var sai = await n1.PostAsJsonAsync("/api/quan-tri/ban-doc", new
        {
            maBanDoc = ma, hoTen = "Thử", doiTuong = "Khách", maSvHvCb = "SV22000001", lopDonVi = "X",
            ngaySinh = homNay.AddDays(1), soTheBd = "T1", hsdTuNgay = homNay, hsdDenNgay = homNay
        });
        var truong = (await JsonAsync(sai)).GetProperty("truong");
        foreach (var t in new[] { "doiTuong", "maSvHvCb", "ngaySinh", "hsdDenNgay" }) Assert.True(truong.TryGetProperty(t, out _), t);

        var ok = await n1.PostAsJsonAsync("/api/quan-tri/ban-doc", new
        {
            maBanDoc = ma, hoTen = "Bạn đọc thử", doiTuong = "Sinh viên", maSvHvCb = sv, lopDonVi = "K26",
            ngaySinh = new DateOnly(2004, 1, 1), soTheBd = "T" + sv, hsdTuNgay = homNay, hsdDenNgay = homNay.AddYears(4)
        });
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        // File 07: tài khoản cổng tạo cùng transaction, trạng thái “Chưa kích hoạt”
        Assert.Equal("Chưa kích hoạt", await ScalarAsync("SELECT tinhtrang FROM muontra.TAIKHOAN_BANDOC WHERE ma_bandoc = @m", ("@m", ma)));
        Assert.Equal(1, await DemNhatKyAsync("ND0002", $"Lưu thông tin Thẻ Bạn đọc mới {ma}"));

        // txtMaSV, btnTimKiem + btnKiemTraDK
        var tim = await JsonAsync(await n1.GetAsync($"/api/quan-tri/ban-doc/tim?maSv={sv}"));
        Assert.Equal(ma, tim.GetProperty("banDoc").GetProperty("maBanDoc").GetString());
        var dk = await JsonAsync(await n1.GetAsync($"/api/quan-tri/ban-doc/{ma}/dieu-kien-muon"));
        Assert.True(dk.GetProperty("duDieuKien").GetBoolean());
        Assert.Equal(HttpStatusCode.NotFound, (await n1.GetAsync("/api/quan-tri/ban-doc/tim?maSv=KHONGTONTAI")).StatusCode);

        Assert.Equal(HttpStatusCode.NoContent, (await n1.DeleteAsync($"/api/quan-tri/ban-doc/{ma}")).StatusCode);
        Assert.Equal(0, await ScalarAsync("SELECT COUNT(*) FROM muontra.TAIKHOAN_BANDOC WHERE ma_bandoc = @m", ("@m", ma)));
    }

    /// <summary>
    /// GIA_DINH D41: bạn đọc đã có phiếu mượn / gia hạn / trả / biên bản → chặn xóa (409, nêu rõ lý do), dữ liệu còn nguyên;
    /// chưa có chứng từ → xóa được kèm dữ liệu mở rộng.
    /// </summary>
    [Fact]
    public async Task BanDoc_DaCoChungTu_KhongXoaDuoc()
    {
        using var n1 = await HttpCuaAsync("muontra");
        var ma = MaNgauNhien("BX", 10);
        var sv = MaNgauNhien("SVX", 12);
        var homNay = DateOnly.FromDateTime(DateTime.Today);
        Assert.Equal(HttpStatusCode.OK, (await n1.PostAsJsonAsync("/api/quan-tri/ban-doc", new
        {
            maBanDoc = ma, hoTen = "Bạn đọc có chứng từ", doiTuong = "Sinh viên", maSvHvCb = sv, lopDonVi = "K26",
            ngaySinh = new DateOnly(2004, 1, 1), soTheBd = "X" + sv, hsdTuNgay = homNay, hsdDenNgay = homNay.AddYears(4)
        })).StatusCode);

        var pm = MaNgauNhien("ZM", 10);
        var gh = MaNgauNhien("ZG", 10);
        var pt = MaNgauNhien("ZT", 10);
        var bb = MaNgauNhien("ZB", 10);
        const string ChungTu = """
            INSERT muontra.PHIEUMUON (so_pmuon, ma_bandoc, ngaylap_pmuon, hoten, ma_nd, ten_nd)
                VALUES (@pm, @bd, CAST(GETDATE() AS date), N'Bạn đọc có chứng từ', 'ND0002', N'Thử');
            INSERT muontra.PHIEUGIAHAN (so_pgiahan, so_pmuon, ma_bandoc, ngaylap_pgiahan, ngay_denghi, ma_nd, ten_nd)
                VALUES (@gh, @pm, @bd, CAST(GETDATE() AS date), CAST(GETDATE() AS date), 'ND0002', N'Thử');
            INSERT muontra.PHIEUTRA (so_ptra, so_pmuon, ma_bandoc, ngaylap_ptra, ngaytra, ma_nd, ten_nd)
                VALUES (@pt, @pm, @bd, CAST(GETDATE() AS date), CAST(GETDATE() AS date), 'ND0002', N'Thử');
            INSERT vipham.BIENBAN (so_bienban, so_pmuon, ma_bandoc, thoigian_lap, ht_thanhtoan, ma_nd, ten_nd, tong_tien)
                VALUES (@bb, @pm, @bd, SYSDATETIME(), N'Tiền mặt', 'ND0003', N'Thử', 0);
            """;
        const string XoaChungTu = """
            DELETE vipham.BIENBAN WHERE so_bienban = @bb; DELETE muontra.PHIEUTRA WHERE so_ptra = @pt;
            DELETE muontra.PHIEUGIAHAN WHERE so_pgiahan = @gh; DELETE muontra.PHIEUMUON WHERE so_pmuon = @pm;
            """;
        (string, object)[] ts = [("@pm", pm), ("@gh", gh), ("@pt", pt), ("@bb", bb), ("@bd", ma)];
        await ScalarAsync(ChungTu, ts);
        try
        {
            var r = await n1.DeleteAsync($"/api/quan-tri/ban-doc/{ma}");
            Assert.Equal(HttpStatusCode.Conflict, r.StatusCode);
            var loi = await JsonAsync(r);
            Assert.Equal("DA_CO_CHUNG_TU", loi.GetProperty("ma").GetString());
            var thongBao = loi.GetProperty("thongBao").GetString()!;
            foreach (var t in new[] { "phiếu mượn", "phiếu gia hạn", "phiếu trả", "biên bản vi phạm" }) Assert.Contains(t, thongBao);
            Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM muontra.BANDOC WHERE ma_bandoc = @m", ("@m", ma)));
            Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM muontra.TAIKHOAN_BANDOC WHERE ma_bandoc = @m", ("@m", ma)));

            // Chỉ còn phiếu gia hạn/trả/biên bản (không có phiếu mượn của chính bạn đọc) vẫn bị chặn
            await ScalarAsync("""
                UPDATE muontra.PHIEUMUON SET ma_bandoc = 'BD0001' WHERE so_pmuon = @pm;
                """, ("@pm", pm));
            var r2 = await n1.DeleteAsync($"/api/quan-tri/ban-doc/{ma}");
            Assert.Equal(HttpStatusCode.Conflict, r2.StatusCode);
            Assert.DoesNotContain("phiếu mượn", (await JsonAsync(r2)).GetProperty("thongBao").GetString()!);
        }
        finally
        {
            await ScalarAsync(XoaChungTu, ts);
        }
        Assert.Equal(HttpStatusCode.NoContent, (await n1.DeleteAsync($"/api/quan-tri/ban-doc/{ma}")).StatusCode);
        Assert.Equal(0, await ScalarAsync("SELECT COUNT(*) FROM muontra.TAIKHOAN_BANDOC WHERE ma_bandoc = @m", ("@m", ma)));
    }

    [Fact]
    public async Task BanDoc_TheHetHan_KhongDuDieuKien()
    {
        using var n1 = await HttpCuaAsync("muontra");
        var dk = await JsonAsync(await n1.GetAsync("/api/quan-tri/ban-doc/BD0004/dieu-kien-muon"));
        Assert.False(dk.GetProperty("duDieuKien").GetBoolean());
        Assert.Contains(dk.GetProperty("lyDo").EnumerateArray(), l => l.GetString()!.Contains("hết hạn"));
    }

    // ---------- 6.3.2 Tài khoản ----------
    [Fact]
    public async Task TaiKhoan_Them_MatKhauMacDinh_KhongTuKhoa()
    {
        using var admin = await HttpCuaAsync("admin");
        var tdn = ("tk" + Guid.NewGuid().ToString("N"))[..12];
        var tk = await JsonAsync(await admin.PostAsJsonAsync("/api/quan-tri/tai-khoan", new { tenNd = "NV thử", tenDangNhap = tdn, maNhom = "N1" }));
        Assert.StartsWith("ND", tk.GetProperty("maNd").GetString());
        Assert.Equal("Hoạt động", tk.GetProperty("tinhTrang").GetString());
        // Mật khẩu mặc định (SPEC mục 4) đăng nhập được, chuỗi băm đúng 64 ký tự
        using var http = MoiTruong.Http();
        Assert.False(string.IsNullOrEmpty(await MoiTruong.DangNhapNhanVienAsync(http, tdn)));
        Assert.Equal(64, Convert.ToInt32(await ScalarAsync("SELECT LEN(matkhau) FROM quantri.NGUOIDUNG WHERE tendangnhap = @t", ("@t", tdn))));

        var r = await admin.PostAsync("/api/quan-tri/tai-khoan/ND0001/khoa", null);
        Assert.Equal(HttpStatusCode.Conflict, r.StatusCode); // không tự khóa chính mình
        Assert.Equal(HttpStatusCode.Forbidden, (await (await HttpCuaAsync("muontra")).GetAsync("/api/quan-tri/tai-khoan")).StatusCode);
    }

    // ---------- 6.3.3 Phân quyền ----------
    [Fact]
    public async Task PhanQuyen_KhongBoQuyenQuanTriN4_LuuVaHoanTac()
    {
        using var admin = await HttpCuaAsync("admin");
        var n4 = await JsonAsync(await admin.GetAsync("/api/quan-tri/phan-quyen/N4"));
        var coN4 = n4.EnumerateArray().Where(q => q.GetProperty("coQuyen").GetBoolean()).Select(q => q.GetProperty("maQuyen").GetString()!).ToList();
        var r = await admin.PutAsJsonAsync("/api/quan-tri/phan-quyen/N4", new { maQuyen = coN4.Where(m => m != "QT040002").ToList() });
        Assert.Equal((HttpStatusCode)422, r.StatusCode);

        var n2 = await JsonAsync(await admin.GetAsync("/api/quan-tri/phan-quyen/N2"));
        var coN2 = n2.EnumerateArray().Where(q => q.GetProperty("coQuyen").GetBoolean()).Select(q => q.GetProperty("maQuyen").GetString()!).ToList();
        Assert.DoesNotContain("TT001", coN2);
        var them = await JsonAsync(await admin.PutAsJsonAsync("/api/quan-tri/phan-quyen/N2", new { maQuyen = coN2.Append("TT001").ToList() }));
        Assert.Equal(1, them.GetProperty("soThem").GetInt32());
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM quantri.PHANQUYEN WHERE ma_nhom = 'N2' AND ma_quyen = 'TT001'"));
        var bo = await JsonAsync(await admin.PutAsJsonAsync("/api/quan-tri/phan-quyen/N2", new { maQuyen = coN2 }));
        Assert.Equal(1, bo.GetProperty("soBo").GetInt32());
    }

    // ---------- 6.3.4 Nhật ký ----------
    [Fact]
    public async Task NhatKy_KiemTraKhoangNgay_ChiN4()
    {
        using var admin = await HttpCuaAsync("admin");
        Assert.Equal((HttpStatusCode)422, (await admin.GetAsync("/api/quan-tri/nhat-ky/dang-nhap?tuNgay=2030-01-02&denNgay=2030-01-01")).StatusCode);
        var homNay = DateTime.Today.ToString("yyyy-MM-dd");
        var kq = await JsonAsync(await admin.GetAsync($"/api/quan-tri/nhat-ky/dang-nhap?tuNgay={homNay}&denNgay={homNay}&maNd=ND0001"));
        Assert.True(kq.GetProperty("tongSo").GetInt32() >= 1);
        Assert.Equal(HttpStatusCode.Forbidden, (await (await HttpCuaAsync("khosach")).GetAsync($"/api/quan-tri/nhat-ky/hoat-dong?tuNgay={homNay}&denNgay={homNay}")).StatusCode);
    }

    // ---------- 6.3.5 Sao lưu ----------
    [Fact]
    public async Task SaoLuu_TaoTepThat_ChongDuongDanRaNgoai()
    {
        using var admin = await HttpCuaAsync("admin");
        Assert.Equal((HttpStatusCode)422, (await admin.GetAsync("/api/quan-tri/sao-luu/duyet?duongDan=../../etc")).StatusCode);
        var bd = await JsonAsync(await admin.PostAsJsonAsync("/api/quan-tri/sao-luu/sao-luu", new { duongDan = "" }));
        var ma = bd.GetProperty("ma").GetString();
        JsonElement t = default;
        for (var i = 0; i < 60; i++)
        {
            t = await JsonAsync(await admin.GetAsync($"/api/quan-tri/sao-luu/tien-trinh/{ma}"));
            if (t.GetProperty("xong").GetBoolean()) break;
            await Task.Delay(1000);
        }
        Assert.True(t.GetProperty("thanhCong").GetBoolean(), t.GetProperty("thongBao").GetString());
        // Tệp sao lưu có thật (msdb ghi nhận) – chống báo thành công giả
        var tep = await ScalarAsync("""
            SELECT TOP 1 bmf.physical_device_name FROM msdb.dbo.backupset bs
            JOIN msdb.dbo.backupmediafamily bmf ON bs.media_set_id = bmf.media_set_id
            WHERE bs.database_name = @csdl ORDER BY bs.backup_finish_date DESC
            """, ("@csdl", MoiTruong.TenCsdl));
        Assert.Contains(t.GetProperty("thongBao").GetString()!.Split(": ")[1], (string)tep!);
        // Dọn tệp kiểm thử – thư mục sao lưu có thể là thư mục Backup dùng chung của SQL Server cài sẵn (D44)
        await ScalarAsync("EXEC master.sys.xp_delete_files @tep;", ("@tep", (string)tep!));
    }

    /// <summary>Thư mục sao lưu của SQL Server cài sẵn chứa .bak của CSDL khác (D44) → phục hồi phải từ chối, không đụng ThuVien.</summary>
    [Fact]
    public async Task PhucHoi_TuChoiTepCuaCsdlKhac()
    {
        using var admin = await HttpCuaAsync("admin");
        var ten = "KiemTra_master_" + Guid.NewGuid().ToString("N")[..8] + ".bak";
        // Cùng quy tắc với service: SAO_LUU_THU_MUC (SaoLuu__ThuMuc), trống thì thư mục sao lưu mặc định của SQL Server
        var goc = Environment.GetEnvironmentVariable("SAO_LUU_THU_MUC") is { Length: > 0 } cauHinh
            ? cauHinh
            : (string)(await ScalarAsync("SELECT CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS nvarchar(400))"))!;
        var tach = goc.Contains('\\') ? "\\" : "/";
        var duongDan = goc.TrimEnd('\\', '/') + tach + ten;
        await ScalarAsync("BACKUP DATABASE [master] TO DISK = @tep WITH INIT, COPY_ONLY;", ("@tep", duongDan));
        try
        {
            var bd = await JsonAsync(await admin.PostAsJsonAsync("/api/quan-tri/sao-luu/phuc-hoi", new { duongDan = ten }));
            JsonElement t = default;
            for (var i = 0; i < 60; i++)
            {
                t = await JsonAsync(await admin.GetAsync($"/api/quan-tri/sao-luu/tien-trinh/{bd.GetProperty("ma").GetString()}"));
                if (t.GetProperty("xong").GetBoolean()) break;
                await Task.Delay(500);
            }
            Assert.False(t.GetProperty("thanhCong").GetBoolean());
            Assert.Contains("không phải ThuVien", t.GetProperty("thongBao").GetString());
            Assert.Equal(HttpStatusCode.OK, (await admin.GetAsync("/api/quan-tri/tai-khoan")).StatusCode); // phiên không bị hủy
        }
        finally
        {
            await ScalarAsync("EXEC master.sys.xp_delete_files @tep;", ("@tep", duongDan));
        }
    }
}
