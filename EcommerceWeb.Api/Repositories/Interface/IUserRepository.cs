using EcommerceWeb.Api.Model.Entities;
using Microsoft.AspNetCore.Identity;

namespace EcommerceWeb.Api.Repositories.Interface
{
    public interface IUserRepository
    {
        Task<IdentityUser?> GetCurrentUserAsync(string userId);
        Task<IdentityUser?> UpdateCurrentUserAsync(string userId, string? username, string? email);
    }

}
