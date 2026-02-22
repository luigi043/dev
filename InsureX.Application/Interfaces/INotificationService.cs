using InsureX.Domain.Interfaces;      // for repositories
using InsureX.Application.Interfaces; // for Notification/User/Tenant services

namespace InsureX.Application.Interfaces
{
    
    public interface INotificationService 
    {
        Task SendEmailAsync(string email, string subject, string body);
    }
}