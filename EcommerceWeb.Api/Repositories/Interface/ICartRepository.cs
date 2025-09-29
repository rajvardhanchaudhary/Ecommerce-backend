namespace EcommerceWeb.Api.Repositories.Interface
{
    public interface ICartRepository
    {
        Task<List<CartItem>> GetCartByUserIdAsync(string userId);
        Task<CartItem> AddOrUpdateItemAsync(string userId, Guid productId, int quantity);
        Task<CartItem?> UpdateItemQuantityAsync(string userId, Guid itemId, int quantity);
        Task<bool> RemoveItemAsync(string userId, Guid itemId);

        Task<bool> ClearCartAsync(string userId);


    }
}
