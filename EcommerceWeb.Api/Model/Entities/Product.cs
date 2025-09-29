using EcommerceWeb.Api.Model.Entities;
using System.ComponentModel.DataAnnotations;

public class Product
{
    [Key]
    public Guid Id { get; set; }  

    public string Title { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }

    public decimal Discount { get; set; } = 0;

    public decimal CurrentPrice { get; set; }
    public int StockQuantity { get; set; }
    public string SKU { get; set; }
    public string ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<Review> Reviews { get; set; }

    public ICollection<ProductCategory>? ProductCategories { get; set; }

}
