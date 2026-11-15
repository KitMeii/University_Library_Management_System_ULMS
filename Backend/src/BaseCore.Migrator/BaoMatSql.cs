using System.Text;
using BaseCore.Entities;
using BaseCore.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BaseCore.Migrator;

/// <summary>
/// Ép ranh giới service bằng quyền SQL (GIA_DINH D10, D13, D14):
/// - Mỗi service một login, toàn quyền DML trên schema của mình.
/// - Đọc (SELECT) được schema khác, trừ cột mật khẩu (DENY).
/// - INSERT vào NHATKY_HOATDONG, THONGBAO; dùng SEQUENCE nhật ký.
/// - vipham được EXECUTE thủ tục cấp số chứng từ.
/// Login có SID cố định để sau khi phục hồi CSDL không bị "orphan user".
/// </summary>
public static class BaoMatSql
{
    private sealed record DichVu(string Ten, string Schema, string Sid);

    private static readonly DichVu[] DanhSach =
    [
        new("svc_quantri", LuocDo.QuanTri, "0x554C4D53000000000000000000000001"),
        new("svc_khosach", LuocDo.KhoSach, "0x554C4D53000000000000000000000002"),
        new("svc_muontra", LuocDo.MuonTra, "0x554C4D53000000000000000000000003"),
        new("svc_vipham", LuocDo.ViPham, "0x554C4D53000000000000000000000004"),
    ];

    public static async Task ApDungAsync(ThuVienDbContext db, IConfiguration config)
    {
        var sql = new StringBuilder();
        foreach (var dv in DanhSach)
        {
            var bien = "MatKhauDichVu:" + dv.Schema;
            var mk = config[bien];
            if (string.IsNullOrWhiteSpace(mk))
                throw new InvalidOperationException($"Thiếu mật khẩu login {dv.Ten} (biến môi trường MatKhauDichVu__{dv.Schema}).");
            var mkSql = mk.Replace("'", "''");

            sql.AppendLine($"""
                IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'{dv.Ten}')
                    CREATE LOGIN [{dv.Ten}] WITH PASSWORD = N'{mkSql}', SID = {dv.Sid}, DEFAULT_DATABASE = [ThuVien], CHECK_POLICY = ON;
                ELSE
                    ALTER LOGIN [{dv.Ten}] WITH PASSWORD = N'{mkSql}';
                IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'{dv.Ten}')
                    CREATE USER [{dv.Ten}] FOR LOGIN [{dv.Ten}];
                ELSE
                    ALTER USER [{dv.Ten}] WITH LOGIN = [{dv.Ten}];
                GRANT SELECT, INSERT, UPDATE, DELETE, EXECUTE ON SCHEMA::[{dv.Schema}] TO [{dv.Ten}];
                """);
            foreach (var khac in LuocDo.TatCa.Where(s => s != dv.Schema))
                sql.AppendLine($"GRANT SELECT ON SCHEMA::[{khac}] TO [{dv.Ten}];");

            if (dv.Schema != LuocDo.QuanTri)
            {
                sql.AppendLine($"GRANT INSERT ON [{LuocDo.QuanTri}].[NHATKY_HOATDONG] TO [{dv.Ten}];");
                sql.AppendLine($"GRANT UPDATE ON OBJECT::[{LuocDo.QuanTri}].[{ThuVienDbContext.SeqNhatKyHoatDong}] TO [{dv.Ten}];");
                sql.AppendLine($"DENY SELECT ON [{LuocDo.QuanTri}].[NGUOIDUNG] ([matkhau]) TO [{dv.Ten}];");
            }
            if (dv.Schema != LuocDo.MuonTra)
            {
                sql.AppendLine($"GRANT INSERT ON [{LuocDo.MuonTra}].[THONGBAO] TO [{dv.Ten}];");
                sql.AppendLine($"DENY SELECT ON [{LuocDo.MuonTra}].[TAIKHOAN_BANDOC] ([matkhau]) TO [{dv.Ten}];");
            }
        }
        sql.AppendLine($"GRANT EXECUTE ON [{LuocDo.MuonTra}].[usp_CapSoChungTu] TO [svc_vipham];");
        // Sao lưu 04.00.09 (SPEC 6.3.5). Phục hồi dùng kết nối quản trị riêng (Pha 2).
        sql.AppendLine("GRANT BACKUP DATABASE TO [svc_quantri];");

        await db.Database.ExecuteSqlRawAsync(sql.ToString());
    }
}
