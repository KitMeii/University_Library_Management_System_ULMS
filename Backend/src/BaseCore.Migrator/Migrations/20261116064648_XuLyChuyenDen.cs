using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseCore.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class XuLyChuyenDen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "XULY_CHUYENDEN",
                schema: "khosach",
                columns: table => new
                {
                    ma_bansach = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    so_chungtu = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    tientrinh = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    tinhtrang_moi = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ma_nd = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    thoigian_xuly = table.Column<DateTime>(type: "datetime2(0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XULY_CHUYENDEN", x => new { x.ma_bansach, x.so_chungtu, x.tientrinh });
                    table.CheckConstraint("CK_XULY_CHUYENDEN_tinhtrang_moi", "[tinhtrang_moi] IN (N'Sẵn sàng', N'Đang được mượn', N'Mất', N'Hư hỏng')");
                    table.ForeignKey(
                        name: "FK_XULY_CHUYENDEN_NGUOIDUNG_ma_nd",
                        column: x => x.ma_nd,
                        principalSchema: "quantri",
                        principalTable: "NGUOIDUNG",
                        principalColumn: "ma_nd",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_XULY_CHUYENDEN_SACH_ma_bansach",
                        column: x => x.ma_bansach,
                        principalSchema: "khosach",
                        principalTable: "SACH",
                        principalColumn: "ma_bansach",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_XULY_CHUYENDEN_ma_nd",
                schema: "khosach",
                table: "XULY_CHUYENDEN",
                column: "ma_nd");

            // GIA_DINH D47: ghi nhận lùi các lần trả Mất/Hư hỏng mà Bộ phận kho sách đã cập nhật SACH tương ứng
            // (trước khi có bảng này, “đã xử lý” suy ra từ SACH.tinhtrang = tinhtrang_tra).
            migrationBuilder.Sql("""
                INSERT INTO [khosach].[XULY_CHUYENDEN] (ma_bansach, so_chungtu, tientrinh, tinhtrang_moi, ma_nd, thoigian_xuly)
                SELECT ct.ma_bansach, ct.so_ptra, N'03.03.02', s.tinhtrang, pt.ma_nd, CAST(SYSDATETIME() AS datetime2(0))
                FROM [muontra].[CT_PHIEUTRA] ct
                JOIN [muontra].[PHIEUTRA] pt ON pt.so_ptra = ct.so_ptra
                JOIN [khosach].[SACH] s ON s.ma_bansach = ct.ma_bansach
                WHERE ct.tinhtrang_tra IN (N'Mất', N'Hư hỏng') AND s.tinhtrang = ct.tinhtrang_tra;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "XULY_CHUYENDEN",
                schema: "khosach");
        }
    }
}
