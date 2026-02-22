using System.Threading.Tasks;
using InsureX.Application.Interfaces;

namespace InsureX.Application.Services.Helpers
{
    public class NotificationService : INotificationService
    {
        public Task SendEmailAsync(string email, string subject, string body)
        {
            // Placeholder implementation, replace with real email sending logic
            return Task.CompletedTask;
        }
    }
}