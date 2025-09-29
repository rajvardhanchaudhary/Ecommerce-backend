using EcommerceWeb.Api.Data;
using EcommerceWeb.Api.Model.DTO;
using EcommerceWeb.Api.Model.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceWeb.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductCategoryController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public ProductCategoryController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        // PUT: api/productcategory/{categoryId}/assign
        [HttpPut("{categoryId:Guid}/assign")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignProductsToCategory(
            [FromRoute] Guid categoryId,
            [FromBody] AssignProductsRequest request)
        {
            var category = await dbContext.Categories
                .Include(c => c.ProductCategories)
                .FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category == null)
                return NotFound(new { success = false, message = "Category not found." });

            // Clear existing associations
            category.ProductCategories.Clear();

            // Assign new products
            foreach (var productId in request.ProductIds)
            {
                var product = await dbContext.Products.FindAsync(productId);
                if (product != null)
                {
                    category.ProductCategories.Add(new ProductCategory
                    {
                        ProductId = productId,
                        CategoryId = categoryId
                    });
                }
            }

            await dbContext.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Products assigned to category successfully.",
                data = new
                {
                    CategoryId = category.Id,
                    CategoryName = category.Name,
                    AssignedProductIds = request.ProductIds
                }
            });
        }


        // GET: api/productcategory/category/{categoryId}/products
        [HttpGet("category/{categoryId:Guid}/products")]
        public async Task<IActionResult> GetProductsByCategory([FromRoute] Guid categoryId)
        {
            var products = await dbContext.ProductCategories
                .Where(pc => pc.CategoryId == categoryId)
                .Select(pc => pc.Product)
                .ToListAsync();

            if (!products.Any())
                return NotFound(new { success = false, message = "No products found for this category." });

            // Manual mapping
            var productDtos = products.Select(p => new ProductDto
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Price = p.Price,
                Discount = p.Discount,
                CurrentPrice = p.CurrentPrice,
                StockQuantity = p.StockQuantity,
                SKU = p.SKU,
                ImageUrl = p.ImageUrl,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            }).ToList();

            return Ok(new
            {
                success = true,
                message = "Products retrieved successfully.",
                data = productDtos
            });
        }
    }
}
