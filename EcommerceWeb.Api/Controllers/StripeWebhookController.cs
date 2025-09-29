using Microsoft.AspNetCore.Mvc;
using EcommerceWeb.Api.Data;
using EcommerceWeb.Api.Model.Entities;
using Stripe;
using System.IO;

[ApiController]
[Route("api/webhooks/stripe")]
public class StripeWebhookController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _config;

    public StripeWebhookController(ApplicationDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public static class OrderStatus
    {
        public const string Paid = "Paid";
        public const string Failed = "Failed";
    }

    [HttpPost]
    public async Task<IActionResult> Index()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                Request.Headers["Stripe-Signature"],
                _config["Stripe:WebhookSecret"]
            );

            var eventType = stripeEvent.Type;
            Console.WriteLine($"Stripe webhook received: {eventType}");

            if (eventType == "payment_intent.succeeded" || eventType == "payment_intent.payment_failed")
            {
                var pi = stripeEvent.Data.Object as PaymentIntent;
                var orderIdStr = pi?.Metadata?["orderId"];

                if (!string.IsNullOrEmpty(orderIdStr) && Guid.TryParse(orderIdStr, out var orderId))
                {
                    var order = await _db.Orders.FindAsync(orderId);
                    if (order != null)
                    {
                        if (eventType == "payment_intent.succeeded" && order.Status != OrderStatus.Paid)
                        {
                            order.Status = OrderStatus.Paid;
                            order.TransactionId = pi.Id;
                        }
                        else if (eventType == "payment_intent.payment_failed" && order.Status != OrderStatus.Failed)
                        {
                            order.Status = OrderStatus.Failed;
                        }

                        await _db.SaveChangesAsync();
                    }
                }
            }

            return Ok();
        }
        catch (StripeException ex)
        {
            Console.WriteLine($"Stripe webhook error: {ex.Message}");
            return BadRequest(new { error = ex.Message });
        }
    }
}
