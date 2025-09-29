using System.ComponentModel.DataAnnotations;

namespace EcommerceWeb.Api.Model.Entities
{
    public class Category
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        // Navigation property for many-to-many
        public ICollection<ProductCategory>? ProductCategories { get; set; }
    }
}
