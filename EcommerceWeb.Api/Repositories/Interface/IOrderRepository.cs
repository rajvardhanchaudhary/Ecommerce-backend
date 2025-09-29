using EcommerceWeb.Api.Model.Entities;

namespace EcommerceWeb.Api.Repositories.Interface
{
    public interface IOrderRepository
    {
        Task<Order> CreateOrderAsync(Order order);
    }
}
