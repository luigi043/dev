using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using InsureX.Domain.Interfaces;
using System.Linq;

namespace InsureX.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // Helper properties (not part of domain interface but useful)
    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    public string? UserName => _httpContextAccessor.HttpContext?.User?.Identity?.Name;
    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

    public IEnumerable<string> Roles => _httpContextAccessor.HttpContext?.User?
        .FindAll(ClaimTypes.Role)
        .Select(c => c.Value) ?? Enumerable.Empty<string>();

    // Domain interface implementations
    public string? GetCurrentUserId() => UserId;
    public string? GetCurrentUserEmail() => Email;
    public bool IsAuthenticated() => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    // Convenience
    public bool IsInRole(string role) => Roles.Contains(role);
}
