// File: ApplicationUser.cs
using Microsoft.AspNetCore.Identity;

namespace EcommerceWeb.Api.Model.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string? Address { get; set; }

        public string Phone { get; set; }
    }
}
