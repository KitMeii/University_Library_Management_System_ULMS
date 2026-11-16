using BaseCore.Common.NghiepVu;
using BaseCore.Tests.Shared;

namespace BaseCore.UnitTests;

/// <summary>Bảng trạng thái nút 6.2.1–6.2.4 đọc từ SPEC (dùng lại ở E2E) và quy tắc số chứng từ khi lưu.</summary>
public class TrangThaiNutSpecTests
{
    [Fact]
    public void LapPhieuMuon_TrangThai1_DocDungTuSpec()
    {
        var tt = SpecDoc.TrangThaiNut("6.2.1");
        var (bat, tat) = tt["frmLapPhieuMuon (Load)"];
        Assert.Equal(["txtMaSV", "btnTimBanDoc", "btnTimPhieu"], bat);
        Assert.Equal(["txtMaBanSach", "btnThemSach", "dgvChiTiet", "btnIn", "btnHuy"], tat);
        Assert.Contains("btnIn", tt["txtMaBanSach, btnThemSach"].Bat);
    }

    [Theory]
    [InlineData("6.2.1", "frmLapPhieuMuon (Load)")]
    [InlineData("6.2.2", "frmLapPhieuGiaHan (Load)")]
    [InlineData("6.2.3", "frmLapPhieuTra (Load)")]
    [InlineData("6.2.4", "frmLapBienBan (Load)")]
    public void MoiFormChungTu_CoBaTrangThai_VaNutIn(string muc, string load)
    {
        var tt = SpecDoc.TrangThaiNut(muc);
        Assert.Contains(load, tt.Keys);
        Assert.Contains("btnIn", tt[load].Tat);               // trạng thái 1: btnIn tắt
        var timDaLuu = tt.ContainsKey("btnTimBienBan") ? "btnTimBienBan" : "btnTimPhieu";
        Assert.Contains("btnIn", tt[timDaLuu].Bat); // mở chứng từ đã lưu → trạng thái 3
        Assert.Equal(tt[load].Bat.Order(), tt["btnHuy"].Bat.Order()); // btnHuy → về trạng thái 1
        Assert.Equal(tt[load].Bat.Order(), tt["btnIn"].Bat.Order()); // in xong → về trạng thái 1
    }

    [Theory]
    [InlineData("PM", 2026, 11, 1, "PM26110001")]
    [InlineData("BB", 2026, 12, 9999, "BB26129999")]
    public void SoChungTu_DungDinhDangVaDoDai(string loai, int nam, int thang, int so, string mong)
    {
        var kq = SoChungTu.Tao(loai, new DateOnly(nam, thang, 15), so);
        Assert.Equal(mong, kq);
        Assert.Equal(10, kq.Length);
        Assert.True(SoChungTu.HopLe(kq));
    }
}
