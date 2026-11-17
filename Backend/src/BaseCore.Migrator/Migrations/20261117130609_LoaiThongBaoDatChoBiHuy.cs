using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseCore.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class LoaiThongBaoDatChoBiHuy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_THONGBAO_loai",
                schema: "muontra",
                table: "THONGBAO");

            migrationBuilder.AddCheckConstraint(
                name: "CK_THONGBAO_loai",
                schema: "muontra",
                table: "THONGBAO",
                sql: "[loai] IN (N'Đặt chỗ thành công', N'Sắp hết hạn giữ', N'Hết hạn giữ', N'Đã nhận sách', N'Sắp đến hạn trả', N'Quá hạn trả', N'Biên bản vi phạm', N'Đặt chỗ bị hủy')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_THONGBAO_loai",
                schema: "muontra",
                table: "THONGBAO");

            migrationBuilder.AddCheckConstraint(
                name: "CK_THONGBAO_loai",
                schema: "muontra",
                table: "THONGBAO",
                sql: "[loai] IN (N'Đặt chỗ thành công', N'Sắp hết hạn giữ', N'Hết hạn giữ', N'Đã nhận sách', N'Sắp đến hạn trả', N'Quá hạn trả', N'Biên bản vi phạm')");
        }
    }
}
