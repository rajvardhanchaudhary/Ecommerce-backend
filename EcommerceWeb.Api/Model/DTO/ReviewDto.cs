namespace EcommerceWeb.Api.Model.DTO
{
    public class ReviewDto
    {
        public Guid Id { get; set; }

        public required string Title { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ImageUrl { get; set; }
    }
    public class ReviewsResponse
    {
        public Guid ProductId { get; set; }
        public List<ReviewDto> Reviews { get; set; }
    }
}
