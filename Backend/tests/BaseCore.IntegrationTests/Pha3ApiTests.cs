using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace BaseCore.IntegrationTests;

/// <summary>
/// Pha 3 – chứng từ SPEC 6.2.1–6.2.4 qua gateway: luồng mượn → gia hạn → trả → biên bản, số chứng từ cấp khi lưu
/// (PMyyMM####…), bổ sung phiếu trong ngày, cấp số đồng thời không trùng, quyền 3.4, danh sách “chuyển đến” suy ra từ dữ liệu.
/// Mỗi kiểm thử tạo Bạn đọc riêng (Cán bộ – 10 bản, 30 ngày) để chạy lặp không phụ thuộc dữ liệu mẫu.
/// </summary>
public class Pha3ApiTests
{
    private static async Task<HttpClient> HttpCuaAsync(string ten)
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

    private static async Task<object?> ScalarAsync(string sql, params (string, object)[] ts)
    {
        await using var cn = MoiTruong.KetNoiSa();
        await cn.OpenAsync();
        await using var cmd = new SqlCommand(sql, cn);
        foreach (var (k, v) in ts) cmd.Parameters.AddWithValue(k, v);
        return await cmd.ExecuteScalarAsync();
    }

    private static string Ngau(string tienTo, int doDai) => (tienTo + Guid.NewGuid().ToString("N").ToUpperInvariant())[..doDai];
    private static string HomNay => DateTime.Today.ToString("yyyy-MM-dd");

    /// <summary>Bạn đọc mới (Cán bộ) để kiểm thử – trả về (maBanDoc, maSvHvCb).</summary>
    private static async Task<(string Ma, string MaSv)> BanDocMoiAsync(HttpClient n1)
    {
        var ma = Ngau("BK", 10);
        var sv = Ngau("CBK", 12);
        var homNay = DateOnly.FromDateTime(DateTime.Today);
        await JsonAsync(await n1.PostAsJsonAsync("/api/quan-tri/ban-doc", new
        {
            maBanDoc = ma, hoTen = "Bạn đọc kiểm thử chứng từ", doiTuong = "Cán bộ", maSvHvCb = sv, lopDonVi = "Phòng thử nghiệm",
            ngaySinh = new DateOnly(1990, 1, 1), soTheBd = "K" + sv, hsdTuNgay = homNay.AddDays(-1), hsdDenNgay = homNay.AddYears(2)
        }));
        return (ma, sv);
    }

    /// <summary>Bản sách đang đáp ứng được (Sẵn sàng, không thuộc phiếu mượn chưa trả).</summary>
    private static async Task<List<string>> SachSanSangAsync(HttpClient n1, string maBanDoc, int soLuong)
    {
        var ds = new List<string>();
        var batDau = Random.Shared.Next(1, 100);
        for (var k = 0; k < 112 && ds.Count < soLuong; k++)
        {
            var ma = $"BS{(batDau + k) % 112 + 1:D5}";
            var r = await n1.GetAsync($"/api/quan-tri/phieu-muon/ban-sach?maBanSach={ma}&maBanDoc={maBanDoc}");
            if (r.IsSuccessStatusCode && (await JsonAsync(r)).GetProperty("dapUng").GetBoolean()) ds.Add(ma);
        }
        Assert.True(ds.Count == soLuong, "Không đủ bản sách sẵn sàng để kiểm thử.");
        return ds;
    }

    private static object DongMuon(string ma) => new { maBanSach = ma, tinhTrangMuon = "Bình thường", ghiChuMuon = (string?)null };

    [Fact]
    public async Task LuongDayDu_Muon_GiaHan_Tra_BienBan()
    {
        using var n1 = await HttpCuaAsync("muontra");
        using var n2 = await HttpCuaAsync("vipham");
        using var n3 = await HttpCuaAsync("khosach");
        var (bd, sv) = await BanDocMoiAsync(n1);

        // 6.2.1 – tìm Bạn đọc: đủ điều kiện, chưa có phiếu trong ngày
        var tim = await JsonAsync(await n1.GetAsync($"/api/quan-tri/phieu-muon/tim-ban-doc?maSv={sv}"));
        Assert.True(tim.GetProperty("dieuKien").GetProperty("duDieuKien").GetBoolean());
        Assert.Equal(JsonValueKind.Null, tim.GetProperty("phieuTrongNgay").ValueKind);
        Assert.Equal(30, tim.GetProperty("soNgayMuon").GetInt32());

        var sach = await SachSanSangAsync(n1, bd, 3);
        var pm = await JsonAsync(await n1.PostAsJsonAsync("/api/quan-tri/phieu-muon", new { maBanDoc = bd, chiTiet = new[] { DongMuon(sach[0]), DongMuon(sach[1]) } }));
        var soPm = pm.GetProperty("so").GetString()!;
        Assert.Matches(new Regex($@"^PM{DateTime.Today:yyMM}\d{{4}}$"), soPm); // số cấp khi lưu, đúng C(10)
        Assert.True(pm.GetProperty("laPhieuMoi").GetBoolean());
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM quantri.NHATKY_HOATDONG WHERE noidung LIKE @c", ("@c", $"01.01.03 Lập Phiếu mượn sách {soPm}%")));
        var hanTra = (DateTime)(await ScalarAsync("SELECT TOP 1 hantra FROM muontra.CT_PHIEUMUON WHERE so_pmuon = @s", ("@s", soPm)))!;
        Assert.Equal(DateTime.Today.AddDays(30), hanTra); // hantra = ngaymuon + 30 ngày (Cán bộ)

        // Bổ sung trong ngày: không cấp số mới (SPEC mục 4)
        var bs = await JsonAsync(await n1.PostAsJsonAsync("/api/quan-tri/phieu-muon", new { maBanDoc = bd, chiTiet = new[] { DongMuon(sach[2]) } }));
        Assert.Equal(soPm, bs.GetProperty("so").GetString());
        Assert.False(bs.GetProperty("laPhieuMoi").GetBoolean());
        Assert.Equal(soPm, (await JsonAsync(await n1.GetAsync($"/api/quan-tri/phieu-muon/tim-ban-doc?maSv={sv}"))).GetProperty("phieuTrongNgay").GetProperty("soPMuon").GetString());
        var ct = await JsonAsync(await n1.GetAsync($"/api/quan-tri/phieu-muon/{soPm}"));
        Assert.Equal(3, ct.GetProperty("tongSoBan").GetInt32()); // MB02: tổng số bản = COUNT(CT_PHIEUMUON)

        // Bản sách vừa cho mượn: không đáp ứng cho phiếu khác; có trong dgvChuyenDen 03.01.04
        var lai = await JsonAsync(await n1.GetAsync($"/api/quan-tri/phieu-muon/ban-sach?maBanSach={sach[0]}&maBanDoc={bd}"));
        Assert.False(lai.GetProperty("dapUng").GetBoolean());
        var chuyenDen = await JsonAsync(await n3.GetAsync("/api/quan-tri/sach/chuyen-den"));
        Assert.Contains(chuyenDen.EnumerateArray(), x => x.GetProperty("maBanSach").GetString() == sach[0] && x.GetProperty("tienTrinh").GetString() == "03.01.04");

        // 6.2.2 – gia hạn: đồng ý 1 bản (hạn mới = hạn cũ + 7), từ chối có lý do 1 bản; lần 2 vượt số lần
        var gh = await JsonAsync(await n1.PostAsJsonAsync("/api/quan-tri/phieu-gia-han", new
        {
            soPMuon = soPm, ngayDeNghi = HomNay,
            chiTiet = new object[] { new { maBanSach = sach[0] }, new { maBanSach = sach[1], ketQuaGiaHan = "Không đồng ý", lyDoKhongGh = "Bạn đọc rút đề nghị" } }
        }));
        var soGh = gh.GetProperty("so").GetString()!;
        Assert.StartsWith("GH" + DateTime.Today.ToString("yyMM"), soGh);
        var ctGh = (await JsonAsync(await n1.GetAsync($"/api/quan-tri/phieu-gia-han/{soGh}"))).GetProperty("chiTiet").EnumerateArray().ToList();
        var dongY = ctGh.Single(x => x.GetProperty("maBanSach").GetString() == sach[0]);
        Assert.Equal("Đồng ý", dongY.GetProperty("ketQuaGiaHan").GetString());
        Assert.Equal(DateOnly.FromDateTime(hanTra).AddDays(7).ToString("yyyy-MM-dd"), dongY.GetProperty("hanTraMoi").GetString());
        Assert.Equal(1, dongY.GetProperty("soLanGiaHan").GetInt32());
        Assert.Equal("Bạn đọc rút đề nghị", ctGh.Single(x => x.GetProperty("maBanSach").GetString() == sach[1]).GetProperty("lyDoKhongGh").GetString());
        var lan2 = await JsonAsync(await n1.PostAsJsonAsync("/api/quan-tri/phieu-gia-han/kiem-tra", new { soPMuon = soPm, ngayDeNghi = HomNay, chiTiet = new[] { new { maBanSach = sach[0] } } }));
        Assert.Equal("Không đồng ý", lan2[0].GetProperty("ketQuaGiaHan").GetString());
        Assert.Contains("gia hạn đủ", lan2[0].GetProperty("lyDoKhongGh").GetString());

        // 6.2.3 – trả 2 lần: lần 1 bình thường (phiếu mới), lần 2 mất sách (bổ sung, cùng số); sai lệch bị chặn
        var pt = await JsonAsync(await n1.PostAsJsonAsync("/api/quan-tri/phieu-tra", new { soPMuon = soPm, ngayTra = HomNay, chiTiet = new[] { new { maBanSach = sach[0], tinhTrangTra = "Bình thường" } } }));
        var soPt = pt.GetProperty("so").GetString()!;
        Assert.StartsWith("PT" + DateTime.Today.ToString("yyMM"), soPt);
        var pt2 = await JsonAsync(await n1.PostAsJsonAsync("/api/quan-tri/phieu-tra", new { soPMuon = soPm, ngayTra = HomNay, chiTiet = new[] { new { maBanSach = sach[1], tinhTrangTra = "Mất", ghiChuTra = "Bạn đọc báo mất" } } }));
        Assert.Equal(soPt, pt2.GetProperty("so").GetString());
        Assert.Contains("vi phạm", pt2.GetProperty("thongBao").GetString());
        var saiLech = await n1.PostAsJsonAsync("/api/quan-tri/phieu-tra", new { soPMuon = soPm, ngayTra = HomNay, chiTiet = new[] { new { maBanSach = sach[0], tinhTrangTra = "Bình thường" } } });
        Assert.Equal((HttpStatusCode)422, saiLech.StatusCode);
        Assert.Contains("Sai lệch", await saiLech.Content.ReadAsStringAsync());
        // Sách mất đã trả: chưa cho mượn lại khi kho chưa cập nhật (D47)
        Assert.False((await JsonAsync(await n1.GetAsync($"/api/quan-tri/phieu-muon/ban-sach?maBanSach={sach[1]}&maBanDoc={bd}"))).GetProperty("dapUng").GetBoolean());

        // 6.2.4 – trường hợp vi phạm chuyển sang N2; biên bản tính tiền; ghi nhận thanh toán
        var truongHop = await JsonAsync(await n2.GetAsync("/api/quan-tri/bien-ban/truong-hop"));
        Assert.Contains(truongHop.EnumerateArray(), x => x.GetProperty("soPMuon").GetString() == soPm);
        var timBb = await JsonAsync(await n2.GetAsync($"/api/quan-tri/bien-ban/phieu-muon/{soPm}"));
        Assert.Equal(sach[1], timBb.GetProperty("sachViPham")[0].GetProperty("maBanSach").GetString());
        var donGia = (decimal)(await ScalarAsync("SELECT dongia FROM vipham.KHOANTHU WHERE ma_khoanthu = 'KT02'"))!;
        var bb = await JsonAsync(await n2.PostAsJsonAsync("/api/quan-tri/bien-ban", new
        {
            soPMuon = soPm, htThanhToan = "Chưa thanh toán", chiTiet = new[] { new { maKhoanThu = "KT02", maBanSach = sach[1], soLuong = 2 } }
        }));
        var soBb = bb.GetProperty("so").GetString()!;
        Assert.StartsWith("BB" + DateTime.Today.ToString("yyMM"), soBb);
        var ctBb = await JsonAsync(await n2.GetAsync($"/api/quan-tri/bien-ban/{soBb}"));
        Assert.Equal(2 * donGia, ctBb.GetProperty("tongTien").GetDecimal()); // thanh_tien = soluong × dongia; tong_tien = SUM
        Assert.Equal(2 * donGia, (decimal)(await ScalarAsync("SELECT tong_tien FROM vipham.BIENBAN WHERE so_bienban = @s", ("@s", soBb)))!);
        Assert.DoesNotContain((await JsonAsync(await n2.GetAsync("/api/quan-tri/bien-ban/truong-hop"))).EnumerateArray(), x => x.GetProperty("soPMuon").GetString() == soPm);
        // Biên bản chưa thanh toán → Bạn đọc không đủ điều kiện mượn (kết quả xử lý chuyển về N1)
        Assert.False((await JsonAsync(await n1.GetAsync($"/api/quan-tri/phieu-muon/tim-ban-doc?maSv={sv}"))).GetProperty("dieuKien").GetProperty("duDieuKien").GetBoolean());
        await JsonAsync(await n2.PostAsJsonAsync("/api/quan-tri/bien-ban", new
        {
            soBienBan = soBb, htThanhToan = "Tiền mặt", chiTiet = new[] { new { maKhoanThu = "KT02", maBanSach = sach[1], soLuong = 1 } }
        }));
        Assert.Equal(donGia, (decimal)(await ScalarAsync("SELECT tong_tien FROM vipham.BIENBAN WHERE so_bienban = @s", ("@s", soBb)))!);
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM quantri.NHATKY_HOATDONG WHERE noidung LIKE @c", ("@c", $"02.03.01 Ghi nhận thanh toán Biên bản vi phạm {soBb}%")));
        // Sách mất có biên bản → dgvChuyenDen 03.03.02 cho N3
        var chuyenDen2 = await JsonAsync(await n3.GetAsync("/api/quan-tri/sach/chuyen-den"));
        Assert.Contains(chuyenDen2.EnumerateArray(), x => x.GetProperty("maBanSach").GetString() == sach[1] && x.GetProperty("tienTrinh").GetString() == "03.03.02");

        // Trả nốt bản còn lại cho bạn đọc kiểm thử
        await JsonAsync(await n1.PostAsJsonAsync("/api/quan-tri/phieu-tra", new { soPMuon = soPm, ngayTra = HomNay, chiTiet = new[] { new { maBanSach = sach[2], tinhTrangTra = "Bình thường" } } }));
    }

    private static async Task<bool> DapUngAsync(HttpClient n1, string ma, string bd) =>
        (await JsonAsync(await n1.GetAsync($"/api/quan-tri/phieu-muon/ban-sach?maBanSach={ma}&maBanDoc={bd}"))).GetProperty("dapUng").GetBoolean();

    private static async Task<bool> CoChuyenDenAsync(HttpClient n3, string ma, string tienTrinh) =>
        (await JsonAsync(await n3.GetAsync("/api/quan-tri/sach/chuyen-den"))).EnumerateArray()
            .Any(x => x.GetProperty("maBanSach").GetString() == ma && x.GetProperty("tienTrinh").GetString() == tienTrinh);

    /// <summary>btnSua của N3: giữ nguyên thông tin khác, chỉ đổi tình trạng.</summary>
    private static async Task SuaTinhTrangAsync(HttpClient n3, string ma, string tinhTrang)
    {
        await using var cn = MoiTruong.KetNoiSa();
        await cn.OpenAsync();
        await using var cmd = new SqlCommand("SELECT ma_khuvuc, ten_sach, tacgia FROM khosach.SACH WHERE ma_bansach = @m", cn);
        cmd.Parameters.AddWithValue("@m", ma);
        await using var r = await cmd.ExecuteReaderAsync();
        Assert.True(await r.ReadAsync());
        var s = new { maKhuVuc = r.GetString(0), tenSach = r.GetString(1), tacGia = r.GetString(2), tinhTrang };
        await JsonAsync(await n3.PutAsJsonAsync($"/api/quan-tri/sach/{ma}", s));
    }

    /// <summary>
    /// Cho mượn 1 bản (có <paramref name="n3"/>: N3 xử lý 03.01.04 → “Đang được mượn”) → trả Hư hỏng → biên bản (KT04, đã thanh toán).
    /// </summary>
    private static async Task<string> TraHuHongCoBienBanAsync(HttpClient n1, HttpClient n2, string bd, string ma, HttpClient? n3 = null)
    {
        var soPm = (await JsonAsync(await n1.PostAsJsonAsync("/api/quan-tri/phieu-muon", new { maBanDoc = bd, chiTiet = new[] { DongMuon(ma) } })))
            .GetProperty("so").GetString()!;
        if (n3 is not null)
            await JsonAsync(await n3.PostAsJsonAsync($"/api/quan-tri/sach/{ma}/cap-nhat-tinh-trang", new { loai = "Sách đã cho mượn" }));
        await JsonAsync(await n1.PostAsJsonAsync("/api/quan-tri/phieu-tra", new
        {
            soPMuon = soPm, ngayTra = HomNay, chiTiet = new[] { new { maBanSach = ma, tinhTrangTra = "Hư hỏng", ghiChuTra = "Rách bìa" } }
        }));
        Assert.False(await DapUngAsync(n1, ma, bd)); // chờ N3 xử lý → không cho mượn
        await JsonAsync(await n2.PostAsJsonAsync("/api/quan-tri/bien-ban", new
        {
            soPMuon = soPm, htThanhToan = "Tiền mặt", chiTiet = new[] { new { maKhoanThu = "KT04", maBanSach = ma, soLuong = 1 } }
        }));
        return soPm;
    }

    /// <summary>
    /// GIA_DINH D47: trả hư hỏng → N3 cập nhật 03.03.02 (Hư hỏng) → sửa sách xong đặt lại “Sẵn sàng” → cho mượn lại thành công;
    /// dòng 03.03.02 không xuất hiện lại.
    /// </summary>
    [Fact]
    public async Task HuHong_N3CapNhat_SanSang_ChoMuonLai()
    {
        using var n1 = await HttpCuaAsync("muontra");
        using var n2 = await HttpCuaAsync("vipham");
        using var n3 = await HttpCuaAsync("khosach");
        var (bd, _) = await BanDocMoiAsync(n1);
        var ma = (await SachSanSangAsync(n1, bd, 1))[0];
        await TraHuHongCoBienBanAsync(n1, n2, bd, ma);

        Assert.True(await CoChuyenDenAsync(n3, ma, "03.03.02"));
        var capNhat = await JsonAsync(await n3.PostAsJsonAsync($"/api/quan-tri/sach/{ma}/cap-nhat-tinh-trang", new { loai = "Sách bị mất, hư hỏng" }));
        Assert.Equal("Hư hỏng", capNhat.GetProperty("tinhTrang").GetString());
        Assert.False(await CoChuyenDenAsync(n3, ma, "03.03.02"));
        Assert.False(await DapUngAsync(n1, ma, bd)); // đang “Hư hỏng”
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM khosach.XULY_CHUYENDEN WHERE ma_bansach = @m AND tientrinh = '03.03.02'", ("@m", ma)));

        await SuaTinhTrangAsync(n3, ma, "Sẵn sàng"); // sửa sách xong
        Assert.False(await CoChuyenDenAsync(n3, ma, "03.03.02"));
        Assert.True(await DapUngAsync(n1, ma, bd));
        var (bd2, _) = await BanDocMoiAsync(n1);
        var muonLai = await JsonAsync(await n1.PostAsJsonAsync("/api/quan-tri/phieu-muon", new { maBanDoc = bd2, chiTiet = new[] { DongMuon(ma) } }));
        await JsonAsync(await n1.PostAsJsonAsync("/api/quan-tri/phieu-tra", new
        {
            soPMuon = muonLai.GetProperty("so").GetString(), ngayTra = HomNay, chiTiet = new[] { new { maBanSach = ma, tinhTrangTra = "Bình thường" } }
        }));
    }

    /// <summary>GIA_DINH D47: N3 sửa thẳng tình trạng về “Sẵn sàng” bằng btnSua (sách sửa ngay) cũng là đã xử lý 03.03.02.</summary>
    [Fact]
    public async Task HuHong_N3SuaThangSanSang_ChoMuonLai()
    {
        using var n1 = await HttpCuaAsync("muontra");
        using var n2 = await HttpCuaAsync("vipham");
        using var n3 = await HttpCuaAsync("khosach");
        var (bd, _) = await BanDocMoiAsync(n1);
        var ma = (await SachSanSangAsync(n1, bd, 1))[0];
        await TraHuHongCoBienBanAsync(n1, n2, bd, ma, n3); // SACH “Đang được mượn”
        Assert.True(await CoChuyenDenAsync(n3, ma, "03.03.02"));

        await SuaTinhTrangAsync(n3, ma, "Sẵn sàng"); // sách sửa ngay: “Đang được mượn” → “Sẵn sàng” = đã xử lý 03.03.02
        Assert.False(await CoChuyenDenAsync(n3, ma, "03.03.02"));
        Assert.True(await DapUngAsync(n1, ma, bd));
        Assert.Equal(1, await ScalarAsync("SELECT COUNT(*) FROM quantri.NHATKY_HOATDONG WHERE noidung LIKE @c", ("@c", $"Sửa bản sách {ma}:%03.03.02 theo phiếu trả%")));
        var (bd2, _) = await BanDocMoiAsync(n1); // bạn đọc khác (phiếu trong ngày của bd đã có bản này)
        var muonLai = await JsonAsync(await n1.PostAsJsonAsync("/api/quan-tri/phieu-muon", new { maBanDoc = bd2, chiTiet = new[] { DongMuon(ma) } }));
        await JsonAsync(await n1.PostAsJsonAsync("/api/quan-tri/phieu-tra", new
        {
            soPMuon = muonLai.GetProperty("so").GetString(), ngayTra = HomNay, chiTiet = new[] { new { maBanSach = ma, tinhTrangTra = "Bình thường" } }
        }));
    }

    /// <summary>Hai nhân viên lưu phiếu cùng lúc: số phiếu khác nhau, liên tiếp trong tháng (usp_CapSoChungTu khóa bộ đếm).</summary>
    [Fact]
    public async Task CapSoDongThoi_KhongTrung()
    {
        using var n1 = await HttpCuaAsync("muontra");
        var bd = new List<string>();
        for (var i = 0; i < 4; i++) bd.Add((await BanDocMoiAsync(n1)).Ma);
        var sach = await SachSanSangAsync(n1, bd[0], 4);
        var kq = await Task.WhenAll(bd.Select((b, i) => n1.PostAsJsonAsync("/api/quan-tri/phieu-muon", new { maBanDoc = b, chiTiet = new[] { DongMuon(sach[i]) } })));
        var so = new List<string>();
        foreach (var r in kq) so.Add((await JsonAsync(r)).GetProperty("so").GetString()!);
        Assert.Equal(4, so.Distinct().Count());
        // Trả lại sách
        foreach (var s in so)
        {
            var ma = (string)(await ScalarAsync("SELECT ma_bansach FROM muontra.CT_PHIEUMUON WHERE so_pmuon = @s", ("@s", s)))!;
            await JsonAsync(await n1.PostAsJsonAsync("/api/quan-tri/phieu-tra", new { soPMuon = s, ngayTra = HomNay, chiTiet = new[] { new { maBanSach = ma, tinhTrangTra = "Bình thường" } } }));
        }
    }

    [Fact]
    public async Task Quyen_TienTrinh_TheoNhom()
    {
        using var n2 = await HttpCuaAsync("vipham");
        using var n3 = await HttpCuaAsync("khosach");
        using var n1 = await HttpCuaAsync("muontra");
        // Lưu chứng từ chỉ đúng nhóm (SPEC 3.4)
        Assert.Equal(HttpStatusCode.Forbidden, (await n2.PostAsJsonAsync("/api/quan-tri/phieu-muon", new { })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await n3.PostAsJsonAsync("/api/quan-tri/phieu-tra", new { })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await n1.PostAsJsonAsync("/api/quan-tri/bien-ban", new { })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await n1.GetAsync("/api/quan-tri/bien-ban/truong-hop")).StatusCode);
        // Kiểm soát dữ liệu nhập: phiếu không có dòng → 422
        Assert.Equal((HttpStatusCode)422, (await n1.PostAsJsonAsync("/api/quan-tri/phieu-muon", new { maBanDoc = "BD0001", chiTiet = Array.Empty<object>() })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await n1.GetAsync("/api/quan-tri/phieu-muon/PM00000000")).StatusCode);
        // Bạn đọc thẻ hết hạn (BD0004) không lập được phiếu mượn
        var r = await n1.PostAsJsonAsync("/api/quan-tri/phieu-muon", new { maBanDoc = "BD0004", chiTiet = new[] { DongMuon("BS00099") } });
        Assert.True(r.StatusCode is HttpStatusCode.Conflict or (HttpStatusCode)422, ((int)r.StatusCode).ToString());
    }

    /// <summary>Dữ liệu mẫu (SPEC mục 7): lịch sử chứng từ có đủ quá hạn, mất, hư hỏng, biên bản chưa thanh toán.</summary>
    [Fact]
    public async Task DuLieuMau_LichSuChungTu()
    {
        Assert.True(Convert.ToInt32(await ScalarAsync("SELECT COUNT(*) FROM muontra.PHIEUMUON")) >= 25);
        Assert.True(Convert.ToInt32(await ScalarAsync("SELECT COUNT(*) FROM muontra.PHIEUGIAHAN")) >= 8);
        Assert.True(Convert.ToInt32(await ScalarAsync("SELECT COUNT(*) FROM muontra.PHIEUTRA")) >= 18);
        Assert.True(Convert.ToInt32(await ScalarAsync("SELECT COUNT(*) FROM vipham.BIENBAN")) >= 5);
        Assert.True(Convert.ToInt32(await ScalarAsync("SELECT COUNT(*) FROM muontra.CT_PHIEUTRA WHERE tinhtrang_tra = N'Mất'")) >= 1);
        Assert.True(Convert.ToInt32(await ScalarAsync("SELECT COUNT(*) FROM muontra.CT_PHIEUTRA WHERE tinhtrang_tra = N'Hư hỏng'")) >= 1);
        Assert.True(Convert.ToInt32(await ScalarAsync("SELECT COUNT(*) FROM muontra.CT_PHIEUTRA WHERE so_ngay_quahan > 0")) >= 3);
        Assert.True(Convert.ToInt32(await ScalarAsync("SELECT COUNT(*) FROM vipham.BIENBAN WHERE ht_thanhtoan = N'Chưa thanh toán'")) >= 1);
        // Sách mất mẫu đã được N3 xử lý 03.03.02 (GIA_DINH D47)
        Assert.True(Convert.ToInt32(await ScalarAsync("SELECT COUNT(*) FROM khosach.XULY_CHUYENDEN WHERE tientrinh = '03.03.02' AND tinhtrang_moi = N'Mất'")) >= 1);
        // Bộ đếm số chứng từ đồng bộ với số lớn nhất đã cấp trong tháng (không cấp trùng)
        var lech = await ScalarAsync("""
            SELECT COUNT(*) FROM muontra.PHIEUMUON p
            WHERE CAST(RIGHT(p.so_pmuon, 4) AS int) > (SELECT so_cuoi FROM muontra.SOCHUNGTU s
                WHERE s.loai = 'PM' AND s.ngay = DATEFROMPARTS(2000 + CAST(SUBSTRING(p.so_pmuon, 3, 2) AS int), CAST(SUBSTRING(p.so_pmuon, 5, 2) AS int), 1))
            """);
        Assert.Equal(0, lech);
    }
}
