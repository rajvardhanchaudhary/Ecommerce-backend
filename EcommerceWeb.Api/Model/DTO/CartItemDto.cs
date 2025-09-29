using System;
using System.Collections.Generic;
using System.Linq;

namespace EcommerceWeb.Api.Model.DTO
{

    public class CartItemDto
    {
        public string Id { get; set; }
        public string ProductId { get; set; }
        public string ProductTitle { get; set; }
        public string Description { get; set; }
        public decimal PricePerUnit { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => PricePerUnit * Quantity;
        public string ImageUrl { get; set; }
        public string Sku { get; set; }
    }

    public class CartResponseDto
    {
        public List<CartItemDto> Items { get; set; } = new();
        public decimal GrandTotal => Items.Sum(i => i.TotalPrice);
    }
}
