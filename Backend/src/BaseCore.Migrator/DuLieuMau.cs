using BaseCore.Common.Auth;
using BaseCore.Common.ThoiGian;
using BaseCore.Entities;
using BaseCore.Entities.KhoSach;
using BaseCore.Entities.MuonTra;
using BaseCore.Entities.QuanTri;
using BaseCore.Entities.ViPham;
using BaseCore.Repository;
using Microsoft.EntityFrameworkCore;

namespace BaseCore.Migrator;

/// <summary>
/// DỮ LIỆU MẪU – chỉ nạp ở Development (SPEC mục 7, file 02 mục 4). Tên người, tên sách là dữ liệu demo.
/// Pha 1: tài khoản nhân viên, khu vực, thể loại, đầu sách, bản sách, bạn đọc + tài khoản cổng, khoản thu.
/// Lịch sử phiếu mượn/gia hạn/trả/biên bản nạp ở Pha 3 (dùng chính quy tắc nghiệp vụ); đặt chỗ ở Pha 6.
/// </summary>
public static class DuLieuMau
{
    public const string MatKhauNhanVien = "Thuvien@123";
    public const string MatKhauBanDoc = "Docgia@123";

    private static readonly (string Ma, string Ten, string Icon, string MoTa)[] TheLoaiMau =
    [
        ("TL01", "Công nghệ thông tin", "bi-cpu", "Lập trình, cơ sở dữ liệu, mạng máy tính"),
        ("TL02", "Kinh tế – Quản trị", "bi-graph-up-arrow", "Kinh tế học, quản trị kinh doanh, tài chính"),
        ("TL03", "Khoa học tự nhiên", "bi-lightbulb", "Toán học, vật lý, hóa học, sinh học"),
        ("TL04", "Ngoại ngữ", "bi-translate", "Tiếng Anh, tiếng Nhật, tiếng Hàn"),
        ("TL05", "Văn học", "bi-book", "Văn học Việt Nam và thế giới"),
        ("TL06", "Lịch sử – Chính trị", "bi-bank", "Lịch sử, triết học, chính trị học"),
        ("TL07", "Kỹ thuật", "bi-gear", "Cơ khí, điện – điện tử, xây dựng"),
        ("TL08", "Kỹ năng – Tâm lý", "bi-people", "Kỹ năng mềm, tâm lý học, giáo dục"),
    ];

    private static readonly (string TenSach, string TacGia, string TheLoai)[] DauSachMau =
    [
        ("Nhập môn lập trình C", "Nguyễn Văn Hùng", "TL01"),
        ("Cấu trúc dữ liệu và giải thuật", "Trần Minh Tâm", "TL01"),
        ("Cơ sở dữ liệu quan hệ", "Lê Thị Hồng Nhung", "TL01"),
        ("Phân tích và thiết kế hệ thống thông tin", "Phạm Quốc Bảo", "TL01"),
        ("Mạng máy tính căn bản", "Đỗ Thanh Long", "TL01"),
        ("Lập trình web với ASP.NET Core", "Vũ Hoàng Nam", "TL01"),
        ("Trí tuệ nhân tạo nhập môn", "Hoàng Gia Khánh", "TL01"),
        ("An toàn thông tin", "Bùi Đức Thắng", "TL01"),
        ("Kinh tế vi mô", "Nguyễn Thị Lan", "TL02"),
        ("Kinh tế vĩ mô", "Trần Văn Phúc", "TL02"),
        ("Nguyên lý kế toán", "Lê Minh Châu", "TL02"),
        ("Quản trị học", "Phạm Thị Thu Hà", "TL02"),
        ("Marketing căn bản", "Đặng Quang Huy", "TL02"),
        ("Tài chính doanh nghiệp", "Ngô Thanh Sơn", "TL02"),
        ("Quản trị chuỗi cung ứng", "Lý Hải Yến", "TL02"),
        ("Giải tích 1", "Nguyễn Đình Trí", "TL03"),
        ("Đại số tuyến tính", "Trần Quang Vinh", "TL03"),
        ("Xác suất thống kê", "Lê Văn Thành", "TL03"),
        ("Vật lý đại cương", "Phạm Ngọc Anh", "TL03"),
        ("Hóa học hữu cơ", "Võ Thị Mai", "TL03"),
        ("Sinh học tế bào", "Hồ Minh Đức", "TL03"),
        ("Toán rời rạc", "Đinh Công Lý", "TL03"),
        ("Ngữ pháp tiếng Anh thực hành", "Nguyễn Thu Trang", "TL04"),
        ("Luyện nghe tiếng Anh học thuật", "Trần Bích Ngọc", "TL04"),
        ("Tiếng Anh chuyên ngành công nghệ", "Lê Hoàng Phương", "TL04"),
        ("Tiếng Nhật sơ cấp", "Phạm Thanh Hương", "TL04"),
        ("Tiếng Hàn tổng hợp", "Vũ Minh Hằng", "TL04"),
        ("Kỹ năng viết luận tiếng Anh", "Đỗ Khánh Linh", "TL04"),
        ("Tuyển tập truyện ngắn Việt Nam", "Nhiều tác giả", "TL05"),
        ("Thơ ca Việt Nam hiện đại", "Nguyễn Hữu Thỉnh", "TL05"),
        ("Văn học dân gian Việt Nam", "Đinh Gia Khánh", "TL05"),
        ("Lý luận văn học", "Trần Đình Sử", "TL05"),
        ("Văn học phương Tây", "Lê Huy Bắc", "TL05"),
        ("Tiểu thuyết Việt Nam thế kỷ XX", "Phan Cự Đệ", "TL05"),
        ("Văn học so sánh", "Lưu Văn Bổng", "TL05"),
        ("Lịch sử Việt Nam cận đại", "Nguyễn Văn Kiệm", "TL06"),
        ("Lịch sử văn minh thế giới", "Vũ Dương Ninh", "TL06"),
        ("Triết học Mác – Lênin", "Hội đồng biên soạn", "TL06"),
        ("Kinh tế chính trị Mác – Lênin", "Hội đồng biên soạn", "TL06"),
        ("Tư tưởng Hồ Chí Minh", "Hội đồng biên soạn", "TL06"),
        ("Pháp luật đại cương", "Hoàng Thế Liên", "TL06"),
        ("Lịch sử Đảng Cộng sản Việt Nam", "Hội đồng biên soạn", "TL06"),
        ("Vẽ kỹ thuật cơ khí", "Trần Hữu Quế", "TL07"),
        ("Sức bền vật liệu", "Lê Ngọc Hồng", "TL07"),
        ("Kỹ thuật điện", "Đặng Văn Đào", "TL07"),
        ("Điện tử số", "Nguyễn Thúy Vân", "TL07"),
        ("Cơ học kết cấu", "Lều Thọ Trình", "TL07"),
        ("Vật liệu xây dựng", "Phùng Văn Lự", "TL07"),
        ("Kỹ thuật vi xử lý", "Văn Thế Minh", "TL07"),
        ("Hệ thống nhúng", "Ngô Diên Tập", "TL07"),
        ("Kỹ năng thuyết trình", "Dương Thị Liễu", "TL08"),
        ("Kỹ năng làm việc nhóm", "Nguyễn Thị Oanh", "TL08"),
        ("Tâm lý học đại cương", "Nguyễn Quang Uẩn", "TL08"),
        ("Phương pháp nghiên cứu khoa học", "Vũ Cao Đàm", "TL08"),
        ("Kỹ năng quản lý thời gian", "Lê Thẩm Dương", "TL08"),
        ("Tư duy phản biện", "Phạm Văn Tuân", "TL08"),
        ("Giao tiếp ứng xử", "Trần Tuấn Lộ", "TL08"),
        ("Kỹ năng học tập hiệu quả", "Hà Thanh Bình", "TL08"),
        ("Học máy ứng dụng", "Tạ Quang Hiếu", "TL01"),
        ("Quản trị nguồn nhân lực", "Trần Kim Dung", "TL02"),
    ];

    private static readonly string[] NhaXuatBan =
    [
        "NXB Giáo dục Việt Nam", "NXB Đại học Quốc gia Hà Nội", "NXB Khoa học và Kỹ thuật", "NXB Trẻ",
        "NXB Lao động", "NXB Kinh tế TP. Hồ Chí Minh", "NXB Văn học", "NXB Chính trị quốc gia Sự thật"
    ];

    private static readonly string[] MauGay =
    [
        "#123F35", "#176B54", "#8B2E2E", "#1F3A68", "#806126", "#3D3D3D", "#5B3A6B", "#2F6F73",
        "#A9793E", "#264653", "#6B4226", "#2A4D3E", "#7A1F3D", "#344E41", "#4A5568"
    ];

    public static async Task NapAsync(ThuVienDbContext db, MatKhauHasher hasher, IClock clock)
    {
        var bayGio = clock.BayGio;
        await NapNhanVienAsync(db, hasher);

        if (await db.Sach.AnyAsync())
        {
            Console.WriteLine("[migrator] Đã có dữ liệu mẫu danh mục – bỏ qua.");
            return;
        }

        var rnd = new Random(2026);

        db.KhuVuc.AddRange(
            new KhuVuc { MaKhuVuc = "KV01", ViTri = "Tầng 1 – Kệ mở A", LoaiKhuVuc = MienGiaTri.LoaiKhuVuc.KeMo },
            new KhuVuc { MaKhuVuc = "KV02", ViTri = "Tầng 2 – Kệ mở B", LoaiKhuVuc = MienGiaTri.LoaiKhuVuc.KeMo },
            new KhuVuc { MaKhuVuc = "KV03", ViTri = "Kho tầng hầm – Dãy 1", LoaiKhuVuc = MienGiaTri.LoaiKhuVuc.Kho },
            new KhuVuc { MaKhuVuc = "KV04", ViTri = "Kho tầng hầm – Dãy 2", LoaiKhuVuc = MienGiaTri.LoaiKhuVuc.Kho });

        for (var i = 0; i < TheLoaiMau.Length; i++)
        {
            var t = TheLoaiMau[i];
            db.TheLoai.Add(new TheLoai { MaTheLoai = t.Ma, TenTheLoai = t.Ten, BieuTuong = t.Icon, MoTa = t.MoTa, ThuTuHienThi = i + 1 });
        }

        // 60 đầu sách, năm xuất bản rải đều 1990–2026 (cho thanh thời gian kệ coverflow)
        var soBan = 0;
        var tinhTrangDacBiet = new Queue<string>(
        [
            MienGiaTri.TinhTrangSach.Mat, MienGiaTri.TinhTrangSach.HuHong, MienGiaTri.TinhTrangSach.Mat,
            MienGiaTri.TinhTrangSach.HuHong, MienGiaTri.TinhTrangSach.HuHong, MienGiaTri.TinhTrangSach.Mat
        ]);
        for (var i = 0; i < DauSachMau.Length; i++)
        {
            var d = DauSachMau[i];
            var maDs = $"DS{i + 1:D4}";
            var nam = 1990 + (int)Math.Round(i * 36.0 / (DauSachMau.Length - 1));
            db.DauSach.Add(new DauSach
            {
                MaDauSach = maDs,
                TenSach = d.TenSach,
                TacGia = d.TacGia,
                NhaXuatBan = NhaXuatBan[i % NhaXuatBan.Length],
                NamXb = nam,
                Isbn = $"978604{i + 1:D7}",
                MaTheLoai = d.TheLoai,
                GioiThieu = $"[DỮ LIỆU MẪU] “{d.TenSach}” – tài liệu tham khảo dùng cho thử nghiệm hệ thống thư viện. " +
                            "Nội dung giới thiệu sẽ được thư viện cập nhật khi nhập sách thật.",
                TuKhoa = string.Join(", ", d.TenSach.ToLowerInvariant().Split(' ').Where(w => w.Length > 3).Take(3)),
                MauGay = MauGay[i % MauGay.Length],
                NgayTao = bayGio.AddDays(-(DauSachMau.Length - i))
            });

            var soBanCuaDau = 1 + rnd.Next(3);
            for (var k = 0; k < soBanCuaDau; k++)
            {
                soBan++;
                var maBs = $"BS{soBan:D5}";
                var tinhTrang = soBan % 17 == 0 && tinhTrangDacBiet.Count > 0
                    ? tinhTrangDacBiet.Dequeue()
                    : MienGiaTri.TinhTrangSach.SanSang;
                db.Sach.Add(new Sach
                {
                    MaBanSach = maBs,
                    MaKhuVuc = k == 0 ? (i % 2 == 0 ? "KV01" : "KV02") : (k == 1 ? "KV03" : "KV04"),
                    TenSach = d.TenSach,
                    TacGia = d.TacGia,
                    TinhTrang = tinhTrang
                });
                db.DauSachBanSach.Add(new DauSachBanSach { MaBanSach = maBs, MaDauSach = maDs });
            }
        }

        // 15 bạn đọc: đủ 3 đối tượng, 2 thẻ hết hạn (SPEC mục 7)
        var hoTen = new[]
        {
            "Nguyễn Minh Anh", "Trần Quốc Bảo", "Lê Thu Cúc", "Phạm Đức Duy", "Hoàng Thị Giang",
            "Vũ Hải Đăng", "Đặng Ngọc Hà", "Bùi Tuấn Kiệt", "Đỗ Khánh Linh", "Ngô Thanh Mai",
            "Dương Văn Nam", "Lý Phương Thảo", "Mai Xuân Quang", "Tạ Thu Uyên", "Châu Gia Vĩnh"
        };
        var homNay = DateOnly.FromDateTime(bayGio);
        for (var i = 0; i < hoTen.Length; i++)
        {
            var (doiTuong, maSv, lop) = i switch
            {
                < 9 => (MienGiaTri.DoiTuong.SinhVien, $"SV22{i + 1:D6}", $"K22-CNTT{i % 3 + 1}"),
                < 12 => (MienGiaTri.DoiTuong.HocVien, $"HV25{i + 1:D4}", "Cao học Quản trị kinh doanh K25"),
                _ => (MienGiaTri.DoiTuong.CanBo, $"CB{i + 1:D4}", "Khoa Công nghệ thông tin")
            };
            var hetHan = i is 3 or 10;
            var maBd = $"BD{i + 1:D4}";
            db.BanDoc.Add(new BanDoc
            {
                MaBanDoc = maBd,
                HoTen = hoTen[i],
                DoiTuong = doiTuong,
                MaSvHvCb = maSv,
                LopDonVi = lop,
                NgaySinh = new DateOnly(doiTuong == MienGiaTri.DoiTuong.CanBo ? 1985 : 2003, 1 + i % 12, 1 + i * 2 % 28),
                SoTheBd = $"TBD{i + 1:D6}",
                DienThoaiEmail = $"09{i:D2}12345{i % 10}",
                HsdTuNgay = hetHan ? homNay.AddYears(-2) : homNay.AddMonths(-3),
                HsdDenNgay = hetHan ? homNay.AddDays(-20) : homNay.AddYears(2)
            });
            // Tài khoản cổng tạo kèm thẻ (file 07); 10 tài khoản đã kích hoạt (file 02 mục 4)
            var kichHoat = i < 12 && !hetHan;
            db.TaiKhoanBanDoc.Add(new TaiKhoanBanDoc
            {
                MaBanDoc = maBd,
                MatKhau = kichHoat ? hasher.Bam(MatKhauBanDoc) : null,
                TinhTrang = kichHoat ? MienGiaTri.TinhTrangTaiKhoanBanDoc.HoatDong : MienGiaTri.TinhTrangTaiKhoanBanDoc.ChuaKichHoat,
                TgTao = bayGio.AddMonths(-3)
            });
        }

        // 5 khoản thu (SPEC mục 7)
        db.KhoanThu.AddRange(
            new KhoanThu { MaKhoanThu = "KT01", NoiDungTt = "Phạt trả sách quá hạn (mỗi ngày)", DonGia = 5_000 },
            new KhoanThu { MaKhoanThu = "KT02", NoiDungTt = "Bồi thường mất sách", DonGia = 200_000 },
            new KhoanThu { MaKhoanThu = "KT03", NoiDungTt = "Bồi thường sách hư hỏng nhẹ", DonGia = 30_000 },
            new KhoanThu { MaKhoanThu = "KT04", NoiDungTt = "Bồi thường sách hư hỏng nặng", DonGia = 100_000 },
            new KhoanThu { MaKhoanThu = "KT05", NoiDungTt = "Phí làm lại thẻ bạn đọc", DonGia = 20_000 });

        await db.SaveChangesAsync();
        Console.WriteLine($"[migrator] Đã nạp {DauSachMau.Length} đầu sách, {soBan} bản sách, {hoTen.Length} bạn đọc.");
    }

    /// <summary>4 tài khoản nhân viên SPEC mục 7 (admin đã có từ dữ liệu khởi tạo).</summary>
    private static async Task NapNhanVienAsync(ThuVienDbContext db, MatKhauHasher hasher)
    {
        var ds = new (string Ma, string Ten, string Tdn, string Nhom, string ChucVu)[]
        {
            ("ND0002", "Nguyễn Thị Mượn Trả", "muontra", NhomNguoiDungMa.N1, "Nhân viên mượn trả"),
            ("ND0003", "Trần Văn Vi Phạm", "vipham", NhomNguoiDungMa.N2, "Nhân viên xử lý vi phạm"),
            ("ND0004", "Lê Thị Kho Sách", "khosach", NhomNguoiDungMa.N3, "Nhân viên kho sách"),
        };
        foreach (var x in ds)
        {
            if (await db.NguoiDung.AnyAsync(n => n.TenDangNhap == x.Tdn)) continue;
            db.NguoiDung.Add(new NguoiDung
            {
                MaNd = x.Ma, TenNd = x.Ten, TenDangNhap = x.Tdn, MatKhau = hasher.Bam(MatKhauNhanVien),
                ChucVu = x.ChucVu, Sdt = "0900000" + x.Ma[^3..], TinhTrang = MienGiaTri.TinhTrangNguoiDung.HoatDong, MaNhom = x.Nhom
            });
        }
        await db.SaveChangesAsync();
    }
}
