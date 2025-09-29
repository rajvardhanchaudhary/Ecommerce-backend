using EcommerceWeb.Api.Data;
using EcommerceWeb.Api.Model.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceWeb.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public PaymentsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // Constants for order statuses
        public static class OrderStatus
        {
            public const string Pending = "Pending";
            public const string CodPending = "COD_Pending";
            public const string Paid = "Paid";
        }

        [HttpPost("cod/{orderId}")]
        public async Task<IActionResult> PlaceCod(Guid orderId)
        {
            var order = await _db.Orders.FindAsync(orderId);
            if (order == null)
                return NotFound(new { success = false, message = "Order not found" });

            if (order.Status != OrderStatus.Pending)
                return BadRequest(new { success = false, message = "Order cannot be set to COD" });

            order.PaymentMethod = "COD";
            order.Status = OrderStatus.CodPending;

            try
            {
                await _db.SaveChangesAsync();
                return Ok(new
                {
                    success = true,
                    orderId = order.Id,
                    status = order.Status,
                    paymentMethod = order.PaymentMethod
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to place COD order",
                    details = ex.Message
                });
            }
        }
    }
}
