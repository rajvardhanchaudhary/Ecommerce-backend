using EcommerceWeb.Api.Model.Entities;
using Stripe;

namespace EcommerceWeb.Api.Service
{
    public class PaymentService : IPaymentService
    {
        public async Task<Order> CreateOrUpdatePaymentIntentAsync(Order order)
        {
            var service = new PaymentIntentService();
            var amount = (long)(order.TotalAmount * 100); // cents

            if (string.IsNullOrEmpty(order.PaymentIntentId))
            {
                var createOptions = new PaymentIntentCreateOptions
                {
                    Amount = amount,
                    Currency = order.Currency ?? "usd",
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true },
                    Metadata = new Dictionary<string, string>
                {
                    { "orderId", order.Id.ToString() }
                }
                };
                var intent = await service.CreateAsync(createOptions);
                order.PaymentIntentId = intent.Id;
                order.ClientSecret = intent.ClientSecret;
                order.Status = "AwaitingPayment";
            }
            else
            {
                var updateOptions = new PaymentIntentUpdateOptions { Amount = amount };
                await service.UpdateAsync(order.PaymentIntentId, updateOptions);
            }

            return order;
        }
    }
}
