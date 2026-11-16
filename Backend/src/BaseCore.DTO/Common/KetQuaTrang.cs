namespace BaseCore.DTO.Common;

/// <summary>Tham số lưới dữ liệu phân trang phía server (file 03 mục D – DataGrid).</summary>
public sealed class ThamSoLuoi
{
    public string? TuKhoa { get; set; }
    public int Trang { get; set; } = 1;
    public int CoTrang { get; set; } = 20;
    /// <summary>Tên thuộc tính để sắp xếp (đã kiểm tra theo danh sách cho phép của từng API).</summary>
    public string? SapXep { get; set; }
    public bool Tang { get; set; } = true;

    public int TrangHopLe => Math.Max(Trang, 1);
    public int CoTrangHopLe => Math.Clamp(CoTrang, 1, 200);
    public int BoQua => (TrangHopLe - 1) * CoTrangHopLe;
}

/// <summary>Kết quả một trang của lưới.</summary>
public sealed record KetQuaTrang<T>(int TongSo, int Trang, int CoTrang, IReadOnlyList<T> DuLieu);
