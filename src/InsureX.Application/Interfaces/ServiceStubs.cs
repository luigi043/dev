namespace InsureX.Application.Interfaces;

// Minimal interfaces so services compile
public interface ITenantContext
{
    int TenantId { get; }
}

public interface ICurrentUserService
{
    int UserId { get; }
}

public interface INotificationService
{
    Task NotifyAsync(string message);
}