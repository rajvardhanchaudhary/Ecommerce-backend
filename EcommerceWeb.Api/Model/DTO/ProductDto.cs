using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceWeb.Api.Model.DTO
{
    public class ProductDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }

        public decimal Discount { get; set; }

        public decimal CurrentPrice { get; set; }
        public int StockQuantity { get; set; }
        public string SKU { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public class AssignProductsRequest
    {
        public List<Guid> ProductIds { get; set; } = new List<Guid>();
    }
}
