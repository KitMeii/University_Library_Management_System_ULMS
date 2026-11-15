using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseCore.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class KhoiTao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "muontra");

            migrationBuilder.EnsureSchema(
                name: "vipham");

            migrationBuilder.EnsureSchema(
                name: "khosach");

            migrationBuilder.EnsureSchema(
                name: "quantri");

            migrationBuilder.CreateSequence(
                name: "SEQ_NHATKY_DANGNHAP",
                schema: "quantri");

            migrationBuilder.CreateSequence(
                name: "SEQ_NHATKY_HOATDONG",
                schema: "quantri");

            migrationBuilder.CreateTable(
                name: "BANDOC",
                schema: "muontra",
                columns: table => new
                {
                    ma_bandoc = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    hoten = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    doituong = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ma_svhvcb = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    lop_donvi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ngaysinh = table.Column<DateOnly>(type: "date", nullable: false),
                    so_thebd = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    dienthoai_email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    hsd_tungay = table.Column<DateOnly>(type: "date", nullable: false),
                    hsd_denngay = table.Column<DateOnly>(type: "date", nullable: false),
                    anh = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BANDOC", x => x.ma_bandoc);
                    table.CheckConstraint("CK_BANDOC_doituong", "[doituong] IN (N'Sinh viên', N'Học viên', N'Cán bộ')");
                    table.CheckConstraint("CK_BANDOC_hsd", "[hsd_denngay] > [hsd_tungay]");
                });

            migrationBuilder.CreateTable(
                name: "KHOANTHU",
                schema: "vipham",
                columns: table => new
                {
                    ma_khoanthu = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    noidung_tt = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    dongia = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KHOANTHU", x => x.ma_khoanthu);
                    table.CheckConstraint("CK_KHOANTHU_dongia", "[dongia] > 0");
                });

            migrationBuilder.CreateTable(
                name: "KHUVUC",
                schema: "khosach",
                columns: table => new
                {
                    ma_khuvuc = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    vitri = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    loai_khuvuc = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KHUVUC", x => x.ma_khuvuc);
                    table.CheckConstraint("CK_KHUVUC_loai_khuvuc", "[loai_khuvuc] IN (N'Kệ mở', N'Kho')");
                });

            migrationBuilder.CreateTable(
                name: "NHOMNGUOIDUNG",
                schema: "quantri",
                columns: table => new
                {
                    ma_nhom = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ten_nhom = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    mota = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NHOMNGUOIDUNG", x => x.ma_nhom);
                });

            migrationBuilder.CreateTable(
                name: "QUYEN",
                schema: "quantri",
                columns: table => new
                {
                    ma_quyen = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ten_quyen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    mota = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QUYEN", x => x.ma_quyen);
                });

            migrationBuilder.CreateTable(
                name: "SOCHUNGTU",
                schema: "muontra",
                columns: table => new
                {
                    loai = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    ngay = table.Column<DateOnly>(type: "date", nullable: false),
                    so_cuoi = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SOCHUNGTU", x => new { x.loai, x.ngay });
                });

            migrationBuilder.CreateTable(
                name: "THELOAI",
                schema: "khosach",
                columns: table => new
                {
                    ma_theloai = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ten_theloai = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    mota = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    bieutuong = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    thutu_hienthi = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_THELOAI", x => x.ma_theloai);
                });

            migrationBuilder.CreateTable(
                name: "PHIEUMUON",
                schema: "muontra",
                columns: table => new
                {
                    so_pmuon = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ma_bandoc = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ngaylap_pmuon = table.Column<DateOnly>(type: "date", nullable: false),
                    hoten = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ma_nd = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ten_nd = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PHIEUMUON", x => x.so_pmuon);
                    table.ForeignKey(
                        name: "FK_PHIEUMUON_BANDOC_ma_bandoc",
                        column: x => x.ma_bandoc,
                        principalSchema: "muontra",
                        principalTable: "BANDOC",
                        principalColumn: "ma_bandoc",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TAIKHOAN_BANDOC",
                schema: "muontra",
                columns: table => new
                {
                    ma_bandoc = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    matkhau = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    tinhtrang = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    so_lan_sai = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    tg_khoa_den = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    tg_dangnhap_cuoi = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    tg_tao = table.Column<DateTime>(type: "datetime2(0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TAIKHOAN_BANDOC", x => x.ma_bandoc);
                    table.CheckConstraint("CK_TAIKHOAN_BANDOC_tinhtrang", "[tinhtrang] IN (N'Chưa kích hoạt', N'Hoạt động', N'Khóa')");
                    table.ForeignKey(
                        name: "FK_TAIKHOAN_BANDOC_BANDOC_ma_bandoc",
                        column: x => x.ma_bandoc,
                        principalSchema: "muontra",
                        principalTable: "BANDOC",
                        principalColumn: "ma_bandoc",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "THONGBAO",
                schema: "muontra",
                columns: table => new
                {
                    ma_thongbao = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ma_bandoc = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    loai = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    noidung = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    lienket = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    tg_tao = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    da_doc = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_THONGBAO", x => x.ma_thongbao);
                    table.CheckConstraint("CK_THONGBAO_loai", "[loai] IN (N'Đặt chỗ thành công', N'Sắp hết hạn giữ', N'Hết hạn giữ', N'Đã nhận sách', N'Sắp đến hạn trả', N'Quá hạn trả', N'Biên bản vi phạm')");
                    table.ForeignKey(
                        name: "FK_THONGBAO_BANDOC_ma_bandoc",
                        column: x => x.ma_bandoc,
                        principalSchema: "muontra",
                        principalTable: "BANDOC",
                        principalColumn: "ma_bandoc",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SACH",
                schema: "khosach",
                columns: table => new
                {
                    ma_bansach = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ma_khuvuc = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ten_sach = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    tacgia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    tinhtrang = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Sẵn sàng")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SACH", x => x.ma_bansach);
                    table.CheckConstraint("CK_SACH_tinhtrang", "[tinhtrang] IN (N'Sẵn sàng', N'Đang được mượn', N'Mất', N'Hư hỏng')");
                    table.ForeignKey(
                        name: "FK_SACH_KHUVUC_ma_khuvuc",
                        column: x => x.ma_khuvuc,
                        principalSchema: "khosach",
                        principalTable: "KHUVUC",
                        principalColumn: "ma_khuvuc",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NGUOIDUNG",
                schema: "quantri",
                columns: table => new
                {
                    ma_nd = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ten_nd = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    tendangnhap = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    matkhau = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    sdt = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    chucvu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    tinhtrang = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ma_nhom = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NGUOIDUNG", x => x.ma_nd);
                    table.CheckConstraint("CK_NGUOIDUNG_tinhtrang", "[tinhtrang] IN (N'Hoạt động', N'Khóa')");
                    table.ForeignKey(
                        name: "FK_NGUOIDUNG_NHOMNGUOIDUNG_ma_nhom",
                        column: x => x.ma_nhom,
                        principalSchema: "quantri",
                        principalTable: "NHOMNGUOIDUNG",
                        principalColumn: "ma_nhom",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PHANQUYEN",
                schema: "quantri",
                columns: table => new
                {
                    ma_nhom = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ma_quyen = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PHANQUYEN", x => new { x.ma_nhom, x.ma_quyen });
                    table.ForeignKey(
                        name: "FK_PHANQUYEN_NHOMNGUOIDUNG_ma_nhom",
                        column: x => x.ma_nhom,
                        principalSchema: "quantri",
                        principalTable: "NHOMNGUOIDUNG",
                        principalColumn: "ma_nhom",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PHANQUYEN_QUYEN_ma_quyen",
                        column: x => x.ma_quyen,
                        principalSchema: "quantri",
                        principalTable: "QUYEN",
                        principalColumn: "ma_quyen",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DAUSACH",
                schema: "khosach",
                columns: table => new
                {
                    ma_dausach = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ten_sach = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    tacgia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    nhaxuatban = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    namxb = table.Column<int>(type: "int", nullable: true),
                    isbn = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ma_theloai = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    gioithieu = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    tukhoa = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    anhbia = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    mau_gay = table.Column<string>(type: "char(7)", nullable: true),
                    ngaytao = table.Column<DateTime>(type: "datetime2(0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DAUSACH", x => x.ma_dausach);
                    table.CheckConstraint("CK_DAUSACH_mau_gay", "[mau_gay] IS NULL OR [mau_gay] LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]'");
                    table.CheckConstraint("CK_DAUSACH_namxb", "[namxb] IS NULL OR ([namxb] >= 1900 AND [namxb] <= YEAR(GETDATE()))");
                    table.ForeignKey(
                        name: "FK_DAUSACH_THELOAI_ma_theloai",
                        column: x => x.ma_theloai,
                        principalSchema: "khosach",
                        principalTable: "THELOAI",
                        principalColumn: "ma_theloai",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BIENBAN",
                schema: "vipham",
                columns: table => new
                {
                    so_bienban = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    so_pmuon = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ma_bandoc = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    thoigian_lap = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    ht_thanhtoan = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ma_nd = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ten_nd = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    tong_tien = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BIENBAN", x => x.so_bienban);
                    table.CheckConstraint("CK_BIENBAN_ht_thanhtoan", "[ht_thanhtoan] IN (N'Tiền mặt', N'Chuyển khoản', N'Chưa thanh toán')");
                    table.ForeignKey(
                        name: "FK_BIENBAN_BANDOC_ma_bandoc",
                        column: x => x.ma_bandoc,
                        principalSchema: "muontra",
                        principalTable: "BANDOC",
                        principalColumn: "ma_bandoc",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BIENBAN_PHIEUMUON_so_pmuon",
                        column: x => x.so_pmuon,
                        principalSchema: "muontra",
                        principalTable: "PHIEUMUON",
                        principalColumn: "so_pmuon",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PHIEUGIAHAN",
                schema: "muontra",
                columns: table => new
                {
                    so_pgiahan = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    so_pmuon = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ma_bandoc = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ngaylap_pgiahan = table.Column<DateOnly>(type: "date", nullable: false),
                    ngay_denghi = table.Column<DateOnly>(type: "date", nullable: false),
                    ma_nd = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ten_nd = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PHIEUGIAHAN", x => x.so_pgiahan);
                    table.ForeignKey(
                        name: "FK_PHIEUGIAHAN_BANDOC_ma_bandoc",
                        column: x => x.ma_bandoc,
                        principalSchema: "muontra",
                        principalTable: "BANDOC",
                        principalColumn: "ma_bandoc",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PHIEUGIAHAN_PHIEUMUON_so_pmuon",
                        column: x => x.so_pmuon,
                        principalSchema: "muontra",
                        principalTable: "PHIEUMUON",
                        principalColumn: "so_pmuon",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PHIEUTRA",
                schema: "muontra",
                columns: table => new
                {
                    so_ptra = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    so_pmuon = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ma_bandoc = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ngaylap_ptra = table.Column<DateOnly>(type: "date", nullable: false),
                    ngaytra = table.Column<DateOnly>(type: "date", nullable: false),
                    ma_nd = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ten_nd = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PHIEUTRA", x => x.so_ptra);
                    table.ForeignKey(
                        name: "FK_PHIEUTRA_BANDOC_ma_bandoc",
                        column: x => x.ma_bandoc,
                        principalSchema: "muontra",
                        principalTable: "BANDOC",
                        principalColumn: "ma_bandoc",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PHIEUTRA_PHIEUMUON_so_pmuon",
                        column: x => x.so_pmuon,
                        principalSchema: "muontra",
                        principalTable: "PHIEUMUON",
                        principalColumn: "so_pmuon",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CT_PHIEUMUON",
                schema: "muontra",
                columns: table => new
                {
                    ma_bansach = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    so_pmuon = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ngaymuon = table.Column<DateOnly>(type: "date", nullable: false),
                    hantra = table.Column<DateOnly>(type: "date", nullable: false),
                    tinhtrang_muon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ghichu_muon = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ten_sach = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CT_PHIEUMUON", x => new { x.ma_bansach, x.so_pmuon });
                    table.ForeignKey(
                        name: "FK_CT_PHIEUMUON_PHIEUMUON_so_pmuon",
                        column: x => x.so_pmuon,
                        principalSchema: "muontra",
                        principalTable: "PHIEUMUON",
                        principalColumn: "so_pmuon",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CT_PHIEUMUON_SACH_ma_bansach",
                        column: x => x.ma_bansach,
                        principalSchema: "khosach",
                        principalTable: "SACH",
                        principalColumn: "ma_bansach",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NHATKY_DANGNHAP",
                schema: "quantri",
                columns: table => new
                {
                    ma_dangnhap = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    ma_nd = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    tg_dangnhap = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    tg_dangxuat = table.Column<DateTime>(type: "datetime2(0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NHATKY_DANGNHAP", x => x.ma_dangnhap);
                    table.ForeignKey(
                        name: "FK_NHATKY_DANGNHAP_NGUOIDUNG_ma_nd",
                        column: x => x.ma_nd,
                        principalSchema: "quantri",
                        principalTable: "NGUOIDUNG",
                        principalColumn: "ma_nd",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NHATKY_HOATDONG",
                schema: "quantri",
                columns: table => new
                {
                    ma_hoatdong = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    ma_nd = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    tg_thuchien = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    noidung = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NHATKY_HOATDONG", x => x.ma_hoatdong);
                    table.ForeignKey(
                        name: "FK_NHATKY_HOATDONG_NGUOIDUNG_ma_nd",
                        column: x => x.ma_nd,
                        principalSchema: "quantri",
                        principalTable: "NGUOIDUNG",
                        principalColumn: "ma_nd",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DAUSACH_BANSACH",
                schema: "khosach",
                columns: table => new
                {
                    ma_bansach = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ma_dausach = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DAUSACH_BANSACH", x => x.ma_bansach);
                    table.ForeignKey(
                        name: "FK_DAUSACH_BANSACH_DAUSACH_ma_dausach",
                        column: x => x.ma_dausach,
                        principalSchema: "khosach",
                        principalTable: "DAUSACH",
                        principalColumn: "ma_dausach",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DAUSACH_BANSACH_SACH_ma_bansach",
                        column: x => x.ma_bansach,
                        principalSchema: "khosach",
                        principalTable: "SACH",
                        principalColumn: "ma_bansach",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PHIEUDATCHO",
                schema: "muontra",
                columns: table => new
                {
                    ma_datcho = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ma_bandoc = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ma_dausach = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ma_bansach = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    tg_dat = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    tg_hethan = table.Column<DateTime>(type: "datetime2(0)", nullable: false),
                    trangthai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    tg_xuly = table.Column<DateTime>(type: "datetime2(0)", nullable: true),
                    ma_nd_xuly = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    so_pmuon = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    lydo_huy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PHIEUDATCHO", x => x.ma_datcho);
                    table.CheckConstraint("CK_PHIEUDATCHO_trangthai", "[trangthai] IN (N'Đang giữ', N'Đã nhận', N'Đã hủy', N'Hết hạn')");
                    table.ForeignKey(
                        name: "FK_PHIEUDATCHO_BANDOC_ma_bandoc",
                        column: x => x.ma_bandoc,
                        principalSchema: "muontra",
                        principalTable: "BANDOC",
                        principalColumn: "ma_bandoc",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PHIEUDATCHO_DAUSACH_ma_dausach",
                        column: x => x.ma_dausach,
                        principalSchema: "khosach",
                        principalTable: "DAUSACH",
                        principalColumn: "ma_dausach",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PHIEUDATCHO_NGUOIDUNG_ma_nd_xuly",
                        column: x => x.ma_nd_xuly,
                        principalSchema: "quantri",
                        principalTable: "NGUOIDUNG",
                        principalColumn: "ma_nd",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PHIEUDATCHO_PHIEUMUON_so_pmuon",
                        column: x => x.so_pmuon,
                        principalSchema: "muontra",
                        principalTable: "PHIEUMUON",
                        principalColumn: "so_pmuon",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PHIEUDATCHO_SACH_ma_bansach",
                        column: x => x.ma_bansach,
                        principalSchema: "khosach",
                        principalTable: "SACH",
                        principalColumn: "ma_bansach",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "YEUTHICH",
                schema: "muontra",
                columns: table => new
                {
                    ma_bandoc = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ma_dausach = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    tg_them = table.Column<DateTime>(type: "datetime2(0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YEUTHICH", x => new { x.ma_bandoc, x.ma_dausach });
                    table.ForeignKey(
                        name: "FK_YEUTHICH_BANDOC_ma_bandoc",
                        column: x => x.ma_bandoc,
                        principalSchema: "muontra",
                        principalTable: "BANDOC",
                        principalColumn: "ma_bandoc",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_YEUTHICH_DAUSACH_ma_dausach",
                        column: x => x.ma_dausach,
                        principalSchema: "khosach",
                        principalTable: "DAUSACH",
                        principalColumn: "ma_dausach",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CT_BIENBAN",
                schema: "vipham",
                columns: table => new
                {
                    ma_khoanthu = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    so_bienban = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ma_bansach = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    soluong = table.Column<int>(type: "int", nullable: false),
                    thanh_tien = table.Column<decimal>(type: "decimal(18,0)", precision: 18, scale: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CT_BIENBAN", x => new { x.ma_khoanthu, x.so_bienban, x.ma_bansach });
                    table.CheckConstraint("CK_CT_BIENBAN_soluong", "[soluong] > 0");
                    table.ForeignKey(
                        name: "FK_CT_BIENBAN_BIENBAN_so_bienban",
                        column: x => x.so_bienban,
                        principalSchema: "vipham",
                        principalTable: "BIENBAN",
                        principalColumn: "so_bienban",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CT_BIENBAN_KHOANTHU_ma_khoanthu",
                        column: x => x.ma_khoanthu,
                        principalSchema: "vipham",
                        principalTable: "KHOANTHU",
                        principalColumn: "ma_khoanthu",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CT_PHIEUGIAHAN",
                schema: "muontra",
                columns: table => new
                {
                    ma_bansach = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    so_pgiahan = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    hantra_cu = table.Column<DateOnly>(type: "date", nullable: false),
                    ketqua_giahan = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    hantra_moi = table.Column<DateOnly>(type: "date", nullable: true),
                    lydo_khonggh = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ten_sach = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    so_lan_giahan = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CT_PHIEUGIAHAN", x => new { x.ma_bansach, x.so_pgiahan });
                    table.CheckConstraint("CK_CT_PHIEUGIAHAN_ketqua", "[ketqua_giahan] IN (N'Đồng ý', N'Không đồng ý')");
                    table.ForeignKey(
                        name: "FK_CT_PHIEUGIAHAN_PHIEUGIAHAN_so_pgiahan",
                        column: x => x.so_pgiahan,
                        principalSchema: "muontra",
                        principalTable: "PHIEUGIAHAN",
                        principalColumn: "so_pgiahan",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CT_PHIEUGIAHAN_SACH_ma_bansach",
                        column: x => x.ma_bansach,
                        principalSchema: "khosach",
                        principalTable: "SACH",
                        principalColumn: "ma_bansach",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CT_PHIEUTRA",
                schema: "muontra",
                columns: table => new
                {
                    ma_bansach = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    so_ptra = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    tinhtrang_tra = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ghichu_tra = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    so_ngay_quahan = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CT_PHIEUTRA", x => new { x.ma_bansach, x.so_ptra });
                    table.CheckConstraint("CK_CT_PHIEUTRA_so_ngay_quahan", "[so_ngay_quahan] >= 0");
                    table.CheckConstraint("CK_CT_PHIEUTRA_tinhtrang_tra", "[tinhtrang_tra] IN (N'Bình thường', N'Hư hỏng', N'Mất')");
                    table.ForeignKey(
                        name: "FK_CT_PHIEUTRA_PHIEUTRA_so_ptra",
                        column: x => x.so_ptra,
                        principalSchema: "muontra",
                        principalTable: "PHIEUTRA",
                        principalColumn: "so_ptra",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CT_PHIEUTRA_SACH_ma_bansach",
                        column: x => x.ma_bansach,
                        principalSchema: "khosach",
                        principalTable: "SACH",
                        principalColumn: "ma_bansach",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BANDOC_hoten",
                schema: "muontra",
                table: "BANDOC",
                column: "hoten");

            migrationBuilder.CreateIndex(
                name: "IX_BANDOC_ma_svhvcb",
                schema: "muontra",
                table: "BANDOC",
                column: "ma_svhvcb",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BANDOC_so_thebd",
                schema: "muontra",
                table: "BANDOC",
                column: "so_thebd",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BIENBAN_ma_bandoc",
                schema: "vipham",
                table: "BIENBAN",
                column: "ma_bandoc");

            migrationBuilder.CreateIndex(
                name: "IX_BIENBAN_so_pmuon",
                schema: "vipham",
                table: "BIENBAN",
                column: "so_pmuon");

            migrationBuilder.CreateIndex(
                name: "IX_BIENBAN_thoigian_lap",
                schema: "vipham",
                table: "BIENBAN",
                column: "thoigian_lap");

            migrationBuilder.CreateIndex(
                name: "IX_CT_BIENBAN_ma_bansach",
                schema: "vipham",
                table: "CT_BIENBAN",
                column: "ma_bansach");

            migrationBuilder.CreateIndex(
                name: "IX_CT_BIENBAN_so_bienban",
                schema: "vipham",
                table: "CT_BIENBAN",
                column: "so_bienban");

            migrationBuilder.CreateIndex(
                name: "IX_CT_PHIEUGIAHAN_so_pgiahan",
                schema: "muontra",
                table: "CT_PHIEUGIAHAN",
                column: "so_pgiahan");

            migrationBuilder.CreateIndex(
                name: "IX_CT_PHIEUMUON_hantra",
                schema: "muontra",
                table: "CT_PHIEUMUON",
                column: "hantra");

            migrationBuilder.CreateIndex(
                name: "IX_CT_PHIEUMUON_ngaymuon",
                schema: "muontra",
                table: "CT_PHIEUMUON",
                column: "ngaymuon");

            migrationBuilder.CreateIndex(
                name: "IX_CT_PHIEUMUON_so_pmuon",
                schema: "muontra",
                table: "CT_PHIEUMUON",
                column: "so_pmuon");

            migrationBuilder.CreateIndex(
                name: "IX_CT_PHIEUTRA_so_ptra",
                schema: "muontra",
                table: "CT_PHIEUTRA",
                column: "so_ptra");

            migrationBuilder.CreateIndex(
                name: "IX_DAUSACH_isbn",
                schema: "khosach",
                table: "DAUSACH",
                column: "isbn",
                unique: true,
                filter: "[isbn] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_DAUSACH_ma_theloai",
                schema: "khosach",
                table: "DAUSACH",
                column: "ma_theloai");

            migrationBuilder.CreateIndex(
                name: "IX_DAUSACH_namxb",
                schema: "khosach",
                table: "DAUSACH",
                column: "namxb");

            migrationBuilder.CreateIndex(
                name: "IX_DAUSACH_tacgia",
                schema: "khosach",
                table: "DAUSACH",
                column: "tacgia");

            migrationBuilder.CreateIndex(
                name: "IX_DAUSACH_ten_sach",
                schema: "khosach",
                table: "DAUSACH",
                column: "ten_sach");

            migrationBuilder.CreateIndex(
                name: "IX_DAUSACH_BANSACH_ma_dausach",
                schema: "khosach",
                table: "DAUSACH_BANSACH",
                column: "ma_dausach");

            migrationBuilder.CreateIndex(
                name: "IX_NGUOIDUNG_ma_nhom",
                schema: "quantri",
                table: "NGUOIDUNG",
                column: "ma_nhom");

            migrationBuilder.CreateIndex(
                name: "IX_NGUOIDUNG_tendangnhap",
                schema: "quantri",
                table: "NGUOIDUNG",
                column: "tendangnhap",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NHATKY_DANGNHAP_ma_nd",
                schema: "quantri",
                table: "NHATKY_DANGNHAP",
                column: "ma_nd");

            migrationBuilder.CreateIndex(
                name: "IX_NHATKY_DANGNHAP_tg_dangnhap",
                schema: "quantri",
                table: "NHATKY_DANGNHAP",
                column: "tg_dangnhap");

            migrationBuilder.CreateIndex(
                name: "IX_NHATKY_HOATDONG_ma_nd",
                schema: "quantri",
                table: "NHATKY_HOATDONG",
                column: "ma_nd");

            migrationBuilder.CreateIndex(
                name: "IX_NHATKY_HOATDONG_tg_thuchien",
                schema: "quantri",
                table: "NHATKY_HOATDONG",
                column: "tg_thuchien");

            migrationBuilder.CreateIndex(
                name: "IX_PHANQUYEN_ma_quyen",
                schema: "quantri",
                table: "PHANQUYEN",
                column: "ma_quyen");

            migrationBuilder.CreateIndex(
                name: "IX_PHIEUDATCHO_ma_dausach",
                schema: "muontra",
                table: "PHIEUDATCHO",
                column: "ma_dausach");

            migrationBuilder.CreateIndex(
                name: "IX_PHIEUDATCHO_ma_nd_xuly",
                schema: "muontra",
                table: "PHIEUDATCHO",
                column: "ma_nd_xuly");

            migrationBuilder.CreateIndex(
                name: "IX_PHIEUDATCHO_so_pmuon",
                schema: "muontra",
                table: "PHIEUDATCHO",
                column: "so_pmuon");

            migrationBuilder.CreateIndex(
                name: "IX_PHIEUDATCHO_trangthai_tg_hethan",
                schema: "muontra",
                table: "PHIEUDATCHO",
                columns: new[] { "trangthai", "tg_hethan" });

            migrationBuilder.CreateIndex(
                name: "UX_DATCHO_BANDOC_DAUSACH_DANGGIU",
                schema: "muontra",
                table: "PHIEUDATCHO",
                columns: new[] { "ma_bandoc", "ma_dausach" },
                unique: true,
                filter: "[trangthai] = N'Đang giữ'");

            migrationBuilder.CreateIndex(
                name: "UX_DATCHO_BANSACH_DANGGIU",
                schema: "muontra",
                table: "PHIEUDATCHO",
                column: "ma_bansach",
                unique: true,
                filter: "[trangthai] = N'Đang giữ'");

            migrationBuilder.CreateIndex(
                name: "IX_PHIEUGIAHAN_ma_bandoc",
                schema: "muontra",
                table: "PHIEUGIAHAN",
                column: "ma_bandoc");

            migrationBuilder.CreateIndex(
                name: "IX_PHIEUGIAHAN_ngaylap_pgiahan",
                schema: "muontra",
                table: "PHIEUGIAHAN",
                column: "ngaylap_pgiahan");

            migrationBuilder.CreateIndex(
                name: "IX_PHIEUGIAHAN_so_pmuon",
                schema: "muontra",
                table: "PHIEUGIAHAN",
                column: "so_pmuon");

            migrationBuilder.CreateIndex(
                name: "IX_PHIEUMUON_ma_bandoc",
                schema: "muontra",
                table: "PHIEUMUON",
                column: "ma_bandoc");

            migrationBuilder.CreateIndex(
                name: "IX_PHIEUMUON_ngaylap_pmuon",
                schema: "muontra",
                table: "PHIEUMUON",
                column: "ngaylap_pmuon");

            migrationBuilder.CreateIndex(
                name: "IX_PHIEUTRA_ma_bandoc",
                schema: "muontra",
                table: "PHIEUTRA",
                column: "ma_bandoc");

            migrationBuilder.CreateIndex(
                name: "IX_PHIEUTRA_ngaytra",
                schema: "muontra",
                table: "PHIEUTRA",
                column: "ngaytra");

            migrationBuilder.CreateIndex(
                name: "IX_PHIEUTRA_so_pmuon",
                schema: "muontra",
                table: "PHIEUTRA",
                column: "so_pmuon",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SACH_ma_khuvuc",
                schema: "khosach",
                table: "SACH",
                column: "ma_khuvuc");

            migrationBuilder.CreateIndex(
                name: "IX_SACH_tacgia",
                schema: "khosach",
                table: "SACH",
                column: "tacgia");

            migrationBuilder.CreateIndex(
                name: "IX_SACH_ten_sach",
                schema: "khosach",
                table: "SACH",
                column: "ten_sach");

            migrationBuilder.CreateIndex(
                name: "IX_SACH_tinhtrang",
                schema: "khosach",
                table: "SACH",
                column: "tinhtrang");

            migrationBuilder.CreateIndex(
                name: "IX_THELOAI_ten_theloai",
                schema: "khosach",
                table: "THELOAI",
                column: "ten_theloai",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_THONGBAO_ma_bandoc_da_doc",
                schema: "muontra",
                table: "THONGBAO",
                columns: new[] { "ma_bandoc", "da_doc" });

            migrationBuilder.CreateIndex(
                name: "IX_YEUTHICH_ma_dausach",
                schema: "muontra",
                table: "YEUTHICH",
                column: "ma_dausach");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CT_BIENBAN",
                schema: "vipham");

            migrationBuilder.DropTable(
                name: "CT_PHIEUGIAHAN",
                schema: "muontra");

            migrationBuilder.DropTable(
                name: "CT_PHIEUMUON",
                schema: "muontra");

            migrationBuilder.DropTable(
                name: "CT_PHIEUTRA",
                schema: "muontra");

            migrationBuilder.DropTable(
                name: "DAUSACH_BANSACH",
                schema: "khosach");

            migrationBuilder.DropTable(
                name: "NHATKY_DANGNHAP",
                schema: "quantri");

            migrationBuilder.DropTable(
                name: "NHATKY_HOATDONG",
                schema: "quantri");

            migrationBuilder.DropTable(
                name: "PHANQUYEN",
                schema: "quantri");

            migrationBuilder.DropTable(
                name: "PHIEUDATCHO",
                schema: "muontra");

            migrationBuilder.DropTable(
                name: "SOCHUNGTU",
                schema: "muontra");

            migrationBuilder.DropTable(
                name: "TAIKHOAN_BANDOC",
                schema: "muontra");

            migrationBuilder.DropTable(
                name: "THONGBAO",
                schema: "muontra");

            migrationBuilder.DropTable(
                name: "YEUTHICH",
                schema: "muontra");

            migrationBuilder.DropTable(
                name: "BIENBAN",
                schema: "vipham");

            migrationBuilder.DropTable(
                name: "KHOANTHU",
                schema: "vipham");

            migrationBuilder.DropTable(
                name: "PHIEUGIAHAN",
                schema: "muontra");

            migrationBuilder.DropTable(
                name: "PHIEUTRA",
                schema: "muontra");

            migrationBuilder.DropTable(
                name: "QUYEN",
                schema: "quantri");

            migrationBuilder.DropTable(
                name: "NGUOIDUNG",
                schema: "quantri");

            migrationBuilder.DropTable(
                name: "SACH",
                schema: "khosach");

            migrationBuilder.DropTable(
                name: "DAUSACH",
                schema: "khosach");

            migrationBuilder.DropTable(
                name: "PHIEUMUON",
                schema: "muontra");

            migrationBuilder.DropTable(
                name: "NHOMNGUOIDUNG",
                schema: "quantri");

            migrationBuilder.DropTable(
                name: "KHUVUC",
                schema: "khosach");

            migrationBuilder.DropTable(
                name: "THELOAI",
                schema: "khosach");

            migrationBuilder.DropTable(
                name: "BANDOC",
                schema: "muontra");

            migrationBuilder.DropSequence(
                name: "SEQ_NHATKY_DANGNHAP",
                schema: "quantri");

            migrationBuilder.DropSequence(
                name: "SEQ_NHATKY_HOATDONG",
                schema: "quantri");
        }
    }
}
