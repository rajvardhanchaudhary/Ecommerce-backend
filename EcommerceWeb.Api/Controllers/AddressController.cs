using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EcommerceWeb.Api.Data;
using EcommerceWeb.Api.Model.Entities;
using System.Security.Claims;
using System.Linq;

namespace EcommerceWeb.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AddressController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public AddressController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Get all addresses for current user
        [HttpGet("my-addresses")]
        public IActionResult GetAddresses()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var addresses = _dbContext.Addresses
                .Where(a => a.UserId == userId)
                .ToList();

            return Ok(new { success = true, data = addresses });
        }

        // Add new address
        [HttpPost]
        public IActionResult AddAddress([FromBody] Address address)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            address.UserId = userId;

            // If this is the first address, set it as default
            if (!_dbContext.Addresses.Any(a => a.UserId == userId))
            {
                address.IsDefault = true;
            }

            _dbContext.Addresses.Add(address);
            _dbContext.SaveChanges();

            return Ok(new { success = true, data = address });
        }

        // Update address
        [HttpPut("{id}")]
        public IActionResult UpdateAddress(int id, [FromBody] Address updatedAddress)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var address = _dbContext.Addresses.FirstOrDefault(a => a.Id == id && a.UserId == userId);
            if (address == null) return NotFound(new { success = false, message = "Address not found" });

            address.Street = updatedAddress.Street;
            address.City = updatedAddress.City;
            address.State = updatedAddress.State;
            address.ZipCode = updatedAddress.ZipCode;
            address.Country = updatedAddress.Country;

            _dbContext.SaveChanges();
            return Ok(new { success = true, data = address });
        }

        // Delete address
        [HttpDelete("{id}")]
        public IActionResult DeleteAddress(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var address = _dbContext.Addresses.FirstOrDefault(a => a.Id == id && a.UserId == userId);
            if (address == null) return NotFound(new { success = false, message = "Address not found" });

            _dbContext.Addresses.Remove(address);
            _dbContext.SaveChanges();

            // If deleted address was default, set another as default
            if (address.IsDefault)
            {
                var another = _dbContext.Addresses.FirstOrDefault(a => a.UserId == userId);
                if (another != null)
                {
                    another.IsDefault = true;
                    _dbContext.SaveChanges();
                }
            }

            return Ok(new { success = true });
        }

        // Set default address
        [HttpPut("set-default/{id}")]
        public IActionResult SetDefault(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var address = _dbContext.Addresses.FirstOrDefault(a => a.Id == id && a.UserId == userId);
            if (address == null) return BadRequest(new { success = false, message = "Unable to set default." });

            // Reset all other addresses
            var userAddresses = _dbContext.Addresses.Where(a => a.UserId == userId).ToList();
            foreach (var addr in userAddresses)
            {
                addr.IsDefault = false;
            }

            // Set selected as default
            address.IsDefault = true;
            _dbContext.SaveChanges();

            return Ok(new { success = true });
        }
    }
}
