namespace BaseCore.Common.NghiepVu;

/// <summary>
/// Tham số nghiệp vụ tập trung (SPEC mục 4) – mục cấu hình "ThuVien:QuyDinh". Giá trị mặc định là đề xuất của SPEC;
/// thư viện điều chỉnh theo quy định thật trước khi đưa vào dùng.
/// </summary>
public sealed class QuyDinhOptions
{
    public const string Muc = "ThuVien:QuyDinh";

    /// <summary>Theo BANDOC.doituong: số ngày được mượn, số bản tối đa.</summary>
    public Dictionary<string, QuyDinhDoiTuong> DoiTuong { get; set; } = new()
    {
        ["Sinh viên"] = new(14, 5),
        ["Học viên"] = new(21, 7),
        ["Cán bộ"] = new(30, 10),
    };

    public int SoNgayGiaHan { get; set; } = 7;
    public int SoLanGiaHanToiDa { get; set; } = 1;
    /// <summary>Mật khẩu mặc định khi tạo/đặt lại tài khoản nhân viên (SPEC mục 4, 6.3.2).</summary>
    public string MatKhauMacDinh { get; set; } = "Thuvien@123";

    public QuyDinhDoiTuong Cua(string doiTuong) =>
        DoiTuong.TryGetValue(doiTuong, out var q) ? q : throw new InvalidOperationException("Chưa cấu hình quy định cho đối tượng " + doiTuong);
}

public sealed record QuyDinhDoiTuong(int SoNgayMuon, int SoBanToiDa)
{
    public QuyDinhDoiTuong() : this(0, 0) { }
}
