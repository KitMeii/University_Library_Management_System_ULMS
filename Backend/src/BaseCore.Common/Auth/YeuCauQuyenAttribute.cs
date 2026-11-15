using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace BaseCore.Common.Auth;

/// <summary>
/// Yêu cầu tài khoản nhân viên có mã quyền (PHANQUYEN), ví dụ <c>[YeuCauQuyen("TT025")]</c>, <c>[YeuCauQuyen("SACH_E")]</c>.
/// Nhiều mã trong một attribute = chỉ cần có một trong các mã.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class YeuCauQuyenAttribute : AuthorizeAttribute
{
    public YeuCauQuyenAttribute(params string[] maQuyen)
    {
        if (maQuyen.Length == 0) throw new ArgumentException("Cần ít nhất một mã quyền.", nameof(maQuyen));
        MaQuyen = maQuyen;
        Policy = ChinhSach.TienToQuyen + string.Join('|', maQuyen);
    }

    public string[] MaQuyen { get; }
}

public sealed class CoQuyenRequirement(string[] maQuyen) : IAuthorizationRequirement
{
    public string[] MaQuyen { get; } = maQuyen;
}

public sealed class CoQuyenHandler : AuthorizationHandler<CoQuyenRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CoQuyenRequirement requirement)
    {
        var u = context.User;
        if (u.HasClaim(ThuVienClaims.Loai, LoaiTaiKhoan.NhanVien) &&
            requirement.MaQuyen.Any(m => u.HasClaim(ThuVienClaims.Quyen, m)))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}

/// <summary>Tạo policy động cho tiền tố "Quyen:".</summary>
public sealed class QuyenPolicyProvider(IOptions<AuthorizationOptions> options) : DefaultAuthorizationPolicyProvider(options)
{
    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(ChinhSach.TienToQuyen, StringComparison.Ordinal))
        {
            var ma = policyName[ChinhSach.TienToQuyen.Length..].Split('|', StringSplitOptions.RemoveEmptyEntries);
            return new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new CoQuyenRequirement(ma))
                .Build();
        }
        return await base.GetPolicyAsync(policyName);
    }
}
