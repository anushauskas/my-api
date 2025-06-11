using CleanArchitecture.Application.Common.Interfaces;

namespace CleanArchitecture.Service.Services;
internal class CurrentUser : IUser
{
    public string? Id => "ServiceId";
}
