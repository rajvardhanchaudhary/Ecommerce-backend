using System.ComponentModel.DataAnnotations;

namespace EcommerceWeb.Api.Model.DTO
{
    public class CreateReviewDto
    {
        public required string Title { get; set; }

        [Required(ErrorMessage = "Comment is required.")]
        [StringLength(500, MinimumLength = 3, ErrorMessage = "Comment must be between 3 and 500 characters.")]
        public string Comment { get; set; }

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }

        public string? ImageUrl { get; set; }
    }
}
