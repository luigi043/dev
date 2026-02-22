namespace InsureX.Domain.Interfaces;

public interface ICurrentUserService
{
    string? GetCurrentUserId();
    string? GetCurrentUserEmail();
    bool IsAuthenticated();
}