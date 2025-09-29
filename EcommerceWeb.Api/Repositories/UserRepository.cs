// UserRepository.cs
using EcommerceWeb.Api.Data;
using EcommerceWeb.Api.Repositories.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext dbContext;

    public UserRepository(ApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IdentityUser?> GetCurrentUserAsync(string userId)
    {
        return await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<IdentityUser?> UpdateCurrentUserAsync(string userId, string? username, string? email)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return null;

        user.UserName = username ?? user.UserName;
        user.Email = email ?? user.Email;

        await dbContext.SaveChangesAsync();
        return user;
    }
}


