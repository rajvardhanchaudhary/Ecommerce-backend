namespace EcommerceWeb.Api.Model.DTO
{
    public class CheckoutRequestDto
    {
        public string ShippingAddress { get; set; }
        public string PaymentMethod { get; set; } // e.g., "CreditCard", "PayPal"
    }

    public class CheckoutResponseDto
    {
        public Guid OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
    }
}
