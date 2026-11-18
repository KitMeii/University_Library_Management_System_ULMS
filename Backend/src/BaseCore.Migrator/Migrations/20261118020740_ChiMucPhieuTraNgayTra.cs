using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseCore.Migrator.Migrations
{
    /// <inheritdoc />
    public partial class ChiMucPhieuTraNgayTra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PHIEUTRA_so_pmuon",
                schema: "muontra",
                table: "PHIEUTRA");

            migrationBuilder.CreateIndex(
                name: "IX_PHIEUTRA_so_pmuon",
                schema: "muontra",
                table: "PHIEUTRA",
                column: "so_pmuon",
                unique: true)
                .Annotation("SqlServer:Include", new[] { "ngaytra" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PHIEUTRA_so_pmuon",
                schema: "muontra",
                table: "PHIEUTRA");

            migrationBuilder.CreateIndex(
                name: "IX_PHIEUTRA_so_pmuon",
                schema: "muontra",
                table: "PHIEUTRA",
                column: "so_pmuon",
                unique: true);
        }
    }
}
