using BaseCore.Repository;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BaseCore.Migrator.Migrations
{
    /// <summary>
    /// Thủ tục cấp số chứng từ (file 02 X08, GIA_DINH D4): khóa dòng bộ đếm bằng UPDLOCK, HOLDLOCK trong
    /// transaction của người gọi → hai người lập phiếu cùng lúc không nhận trùng số.
    /// PM/GH/PT/BB đánh số theo tháng (ngay = ngày đầu tháng); DC (đặt chỗ) theo ngày.
    /// </summary>
    [DbContext(typeof(ThuVienDbContext))]
    [Migration("20261115180000_ThuTucCapSoChungTu")]
    public partial class ThuTucCapSoChungTu : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Bọc EXEC(N'...') để kịch bản idempotent (Database/01_CSDL_LuocDo.sql) chạy được: CREATE PROCEDURE phải
            // đứng đầu batch nên không thể nằm trực tiếp trong khối IF NOT EXISTS của EF (GIA_DINH D44).
            migrationBuilder.Sql("EXEC(N'" + ThuTuc.Replace("'", "''") + "');");
        }

        private const string ThuTuc = """
                CREATE OR ALTER PROCEDURE [muontra].[usp_CapSoChungTu]
                    @loai nvarchar(4),
                    @ngay date,
                    @so int OUTPUT
                AS
                BEGIN
                    SET NOCOUNT ON;
                    IF @@TRANCOUNT = 0
                        THROW 50001, N'usp_CapSoChungTu phải được gọi trong transaction của nghiệp vụ lập chứng từ.', 1;

                    DECLARE @khoa date = CASE WHEN @loai = N'DC' THEN @ngay
                                              ELSE DATEFROMPARTS(YEAR(@ngay), MONTH(@ngay), 1) END;

                    UPDATE [muontra].[SOCHUNGTU] WITH (UPDLOCK, HOLDLOCK)
                       SET @so = [so_cuoi] = [so_cuoi] + 1
                     WHERE [loai] = @loai AND [ngay] = @khoa;

                    IF @@ROWCOUNT = 0
                    BEGIN
                        INSERT INTO [muontra].[SOCHUNGTU] ([loai], [ngay], [so_cuoi]) VALUES (@loai, @khoa, 1);
                        SET @so = 1;
                    END

                    IF @loai <> N'DC' AND @so > 9999
                        THROW 50002, N'Đã hết số chứng từ trong tháng (tối đa 9999).', 1;
                END
                """;

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS [muontra].[usp_CapSoChungTu];");
        }
    }
}
