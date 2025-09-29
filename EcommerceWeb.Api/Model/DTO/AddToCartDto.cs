using System.ComponentModel.DataAnnotations;

namespace EcommerceWeb.Api.Model.DTO
{
    public class AddToCartDto
    {
        [Required(ErrorMessage = "ProductId is required.")]
        public Guid ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public int? Quantity { get; set; }
    }
}
