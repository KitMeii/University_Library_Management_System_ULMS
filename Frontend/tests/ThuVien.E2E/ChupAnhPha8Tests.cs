namespace ThuVien.E2E;

/// <summary>
/// Ảnh Pha 8 (file 11 mục 6: “ảnh mọi giao diện, cả bản máy tính và điện thoại”): bản điện thoại (iPhone 13, 390×844) của mọi giao diện
/// dashboard và cổng chưa có ảnh điện thoại, tên &lt;ảnh máy tính&gt;_mobile. Không ghi đè ảnh Bài 4 (tên 4.x không có hậu tố _mobile).
/// </summary>
[Collection("TrinhDuyet")]
public class ChupAnhPha8Tests(TrinhDuyet td)
{
    private static bool DaCo(string ten) => File.Exists(Path.Combine(TrinhDuyet.ThuMucAnh, ten + ".png"));

    [Fact]
    public async Task Chup_DienThoai_MoiGiaoDienNghiepVu()
    {
        foreach (var nhom in Pha8Tests.TrangNghiepVu.GroupBy(t => t.NhanVien))
        {
            await using var ctx = await td.DienThoaiAsync();
            await TrinhDuyet.BatMoRongAsync(ctx);
            var p = await ctx.NewPageAsync();
            await TrinhDuyet.DangNhapNhanVienAsync(p, nhom.Key);
            foreach (var (_, duong, ten) in nhom)
            {
                // Ảnh điện thoại đã chụp ở pha trước (có kịch bản riêng, trạng thái riêng) thì giữ nguyên
                if (ten is "4.2.1_Danh_muc_sach" or "4.3.1_Lap_phieu_muon" or "4.5.1_Tong_hop_muon_tra" or "4.5.2_Tong_hop_vi_pham"
                    or "4.6.2_Quan_ly_tai_khoan" or "Tong_quan_N1") continue;
                await Pha8Tests.MoTrangAsync(p, duong);
                await p.WaitForTimeoutAsync(800);
                await TrinhDuyet.ChupAsync(p, ten + "_mobile", toanTrang: true);
            }
        }
    }

    [Fact]
    public async Task Chup_DienThoai_CongBanDoc()
    {
        await using var ctx = await td.DienThoaiAsync();
        var p = await ctx.NewPageAsync();
        var daDangNhap = false;
        foreach (var (banDoc, duong, ten) in Pha8Tests.TrangCong)
        {
            if (ten is "Cong_Trang_chu_hoan_chinh" or "Cong_Dang_nhap" or "Cong_Tai_khoan_Dat_cho") continue; // đã có bản _mobile
            if (banDoc && !daDangNhap) { await TrinhDuyet.DangNhapBanDocAsync(p, "SV22000001"); daDangNhap = true; }
            await Pha8Tests.MoTrangAsync(p, duong);
            await p.WaitForTimeoutAsync(1000);
            await TrinhDuyet.ChupAsync(p, ten + "_mobile", toanTrang: true);
        }
        Assert.True(DaCo("Cong_Kham_pha_toan_trang_mobile"));

        // Hai trang cá nhân chưa có ảnh máy tính
        await using var ctx2 = await td.MayTinhAsync();
        var q = await ctx2.NewPageAsync();
        await TrinhDuyet.DangNhapBanDocAsync(q, "SV22000001");
        foreach (var (duong, ten) in new[] { ("/tai-khoan/vi-pham", "Cong_Tai_khoan_Vi_pham"), ("/tai-khoan/doi-mat-khau", "Cong_Tai_khoan_Doi_mat_khau") })
        {
            await Pha8Tests.MoTrangAsync(q, duong);
            await q.WaitForTimeoutAsync(1000);
            await TrinhDuyet.ChupAsync(q, ten, toanTrang: true);
        }
    }
}
