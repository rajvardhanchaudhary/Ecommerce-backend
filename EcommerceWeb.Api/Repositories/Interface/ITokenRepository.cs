using Microsoft.AspNetCore.Identity;

namespace EcommerceWeb.Api.Repositories.Interface
{
    public interface ITokenRepository
    {
        string CreateJWTToken(IdentityUser user, List<string> roles);
    }
}
