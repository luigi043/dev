using InsureX.Application.Interfaces;

namespace InsureX.Application.Services.Helpers
{
    public class CurrentUserService : ICurrentUserService
    {
        public string? GetCurrentUserId() => "system";
        public string? GetCurrentUserEmail() => "system@example.com";
    }
}