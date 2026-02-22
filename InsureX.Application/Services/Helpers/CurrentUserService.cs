using InsureX.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace InsureX.Application.Services.Helpers
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetCurrentUserId()
        {
            return _httpContextAccessor.HttpContext?.User?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public string? GetCurrentUserEmail()
        {
            return _httpContextAccessor.HttpContext?.User?
                .FindFirst(ClaimTypes.Email)?.Value;
        }

        public bool IsAuthenticated()
        {
            return _httpContextAccessor.HttpContext?.User?
                .Identity?.IsAuthenticated == true;
        }
    }
}