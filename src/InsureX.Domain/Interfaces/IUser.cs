using InsureX.Domain.Interfaces;
namespace InsureX.Domain.Interfaces;

public interface IUser
{
    string Id { get; }
    string UserName { get; }
    string Email { get; }
}