using EcommerceWeb.Api.Data;
using EcommerceWeb.Api.Model.Entities;
using EcommerceWeb.Api.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceWeb.Api.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext dbContext;

        public ProductRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<List<Product>> GetAllAsync(string? searchQuery = null)
        {
            var products = dbContext.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                products = products.Where(x =>
                    x.Title.ToLower().Contains(searchQuery.ToLower()) ||
                    x.Description.ToLower().Contains(searchQuery.ToLower())
                );
            }

            return await products.ToListAsync();
        }

        public async Task<Product> CreateAsync(Product product)
        {
            product.CurrentPrice = product.Price - (product.Price * (product.Discount / 100));
            await dbContext.Products.AddAsync(product);
            await dbContext.SaveChangesAsync();
            return product;
        }

        public async Task<Product?> DeleteAsync(Guid id)
        {
            var existingProduct = await dbContext.Products.FirstOrDefaultAsync(x => x.Id == id);

            if (existingProduct == null)
            {
                return null;
            }

            dbContext.Products.Remove(existingProduct);
            await dbContext.SaveChangesAsync();

            return existingProduct;
        }


        public async Task<Product?> GetByIdAsync(Guid id)
        {
            return await dbContext.Products
                .FirstOrDefaultAsync(p => p.Id == id);
        }


        public async Task<Product?> UpdateAsync(Guid id, Product product)
        {
            var existingProduct = await dbContext.Products.FirstOrDefaultAsync(x => x.Id == id);

            if (existingProduct == null)
            {
                return null;
            }

            existingProduct.Title = product.Title;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.Discount = product.Discount;
            existingProduct.CurrentPrice = (product.Price - (product.Price * (product.Discount / 100)));
            existingProduct.StockQuantity = product.StockQuantity;
            existingProduct.SKU = product.SKU;
            existingProduct.ImageUrl = product.ImageUrl;
            existingProduct.IsActive = product.IsActive;

            await dbContext.SaveChangesAsync();

            return existingProduct;

        }
    }
}