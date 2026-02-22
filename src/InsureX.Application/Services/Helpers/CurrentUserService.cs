using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using InsureX.Application.Interfaces;

namespace InsureX.Application.Services.Helpers;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // Properties
    public string? UserId => _httpContextAccessor.HttpContext?.User?
        .FindFirstValue(ClaimTypes.NameIdentifier);

    public string? UserName => _httpContextAccessor.HttpContext?.User?
        .FindFirstValue(ClaimTypes.Name);

    public string? Email => _httpContextAccessor.HttpContext?.User?
        .FindFirstValue(ClaimTypes.Email);

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?
        .Identity?.IsAuthenticated ?? false;

    public IEnumerable<string> Roles => _httpContextAccessor.HttpContext?.User?
        .FindAll(ClaimTypes.Role).Select(c => c.Value) ?? Array.Empty<string>();

    // Methods
    public string? GetCurrentUserId() => UserId;

    public string? GetCurrentUserEmail() => Email;

    public string? GetCurrentUserName() => UserName;

    public bool IsInRole(string role) => _httpContextAccessor.HttpContext?.User?
        .IsInRole(role) ?? false;

    public string? GetClaimValue(string claimType) => _httpContextAccessor.HttpContext?.User?
        .FindFirstValue(claimType);
}