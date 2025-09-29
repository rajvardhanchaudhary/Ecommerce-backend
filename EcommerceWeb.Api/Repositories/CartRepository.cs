using EcommerceWeb.Api.Data;
using EcommerceWeb.Api.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace EcommerceWeb.Api.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext dbContext;

        public CartRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<CartItem?> AddOrUpdateItemAsync(string userId, Guid productId, int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

            // Check if product exists
            var productExists = await dbContext.Products.AnyAsync(p => p.Id == productId);
            if (!productExists)
            {
                // Product doesn't exist, return null or handle as you want
                return null;
            }

            var existingItem = await dbContext.CartItems
                .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                existingItem = new CartItem
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ProductId = productId,
                    Quantity = quantity
                };

                await dbContext.CartItems.AddAsync(existingItem);
            }

            await dbContext.SaveChangesAsync();
            return existingItem;
        }


        public async Task<List<CartItem>> GetCartByUserIdAsync(string userId)
        {
            return await dbContext.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.UserId == userId)
                .ToListAsync();
        }

        public async Task<bool> RemoveItemAsync(string userId, Guid itemId)
        {
            var item = await dbContext.CartItems
                .FirstOrDefaultAsync(ci => ci.Id == itemId && ci.UserId == userId);

            if (item == null) return false;

            dbContext.CartItems.Remove(item);
            await dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<CartItem?> UpdateItemQuantityAsync(string userId, Guid itemId, int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

            var item = await dbContext.CartItems
                .FirstOrDefaultAsync(ci => ci.Id == itemId && ci.UserId == userId);

            if (item == null) return null;

            item.Quantity = quantity;
            await dbContext.SaveChangesAsync();

            return item;
        }

        public async Task<bool> ClearCartAsync(string userId)
        {
            var items = await dbContext.CartItems
                .Where(ci => ci.UserId == userId)
                .ToListAsync();

            if (!items.Any())
                return false;

            dbContext.CartItems.RemoveRange(items);
            await dbContext.SaveChangesAsync();
            return true;
        }
    }
}
