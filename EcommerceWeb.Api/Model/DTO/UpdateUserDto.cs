using System.ComponentModel.DataAnnotations;

namespace EcommerceWeb.Api.Model.DTO
{
    public class UpdateUserDto
    {
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Address { get; set; }
    }
}
