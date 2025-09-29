using EcommerceWeb.Api.Model.Entities;

namespace EcommerceWeb.Api.Service
{
    public interface IPaymentService
    {
        Task<Order> CreateOrUpdatePaymentIntentAsync(Order order);
    }
}
