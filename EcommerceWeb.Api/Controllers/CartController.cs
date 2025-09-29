using AutoMapper;
using EcommerceWeb.Api.Model.DTO;
using EcommerceWeb.Api.Repositories.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartRepository cartRepository;
    private readonly IMapper mapper;

    public CartController(ICartRepository cartRepository, IMapper mapper)
    {
        this.cartRepository = cartRepository;
        this.mapper = mapper;
    }

    // GET: api/cart
    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized(new ApiResponse { Success = false, Message = "User not authenticated." });

        var cartItems = await cartRepository.GetCartByUserIdAsync(userId);
        var itemDtos = mapper.Map<List<CartItemDto>>(cartItems);

        return Ok(new ApiResponse
        {
            Success = true,
            Message = "Cart fetched successfully.",
            Data = new CartResponseDto { Items = itemDtos }
        });
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(new ApiResponse { Success = false, Message = string.Join(" | ", errors) });
        }

        if (dto.Quantity == null || dto.Quantity <= 0)
        {
            return BadRequest(new ApiResponse { Success = false, Message = "Quantity must be greater than zero." });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized(new ApiResponse { Success = false, Message = "User not authenticated." });

        var cartItem = await cartRepository.AddOrUpdateItemAsync(userId, dto.ProductId, dto.Quantity.Value);

        if (cartItem == null)
        {
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = "Product does not exist."
            });
        }

        return Ok(new ApiResponse
        {
            Success = true,
            Message = "Item added to cart.",
            Data = new
            {
                cartItem.Id,
                cartItem.ProductId,
                cartItem.Quantity
            }
        });
    }

    // PUT: api/cart/items/{itemId}
    [HttpPut("items/{itemId}")]
    public async Task<IActionResult> UpdateCartItemQuantity(Guid itemId, [FromBody] UpdateCartItemDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(new ApiResponse { Success = false, Message = string.Join(" | ", errors) });
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized(new ApiResponse { Success = false, Message = "User not authenticated." });

        var updatedItem = await cartRepository.UpdateItemQuantityAsync(userId, itemId, dto.Quantity);

        if (updatedItem == null)
            return NotFound(new ApiResponse { Success = false, Message = "Cart item not found." });

        return Ok(new ApiResponse
        {
            Success = true,
            Message = "Cart item updated.",
            Data = new
            {
                updatedItem.Id,
                updatedItem.ProductId,
                updatedItem.Quantity
            }
        });
    }

    // DELETE: api/cart/items/{itemId}
    [HttpDelete("items/{itemId}")]
    public async Task<IActionResult> RemoveCartItem(Guid itemId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized(new ApiResponse { Success = false, Message = "User not authenticated." });

        var success = await cartRepository.RemoveItemAsync(userId, itemId);

        if (!success)
            return NotFound(new ApiResponse { Success = false, Message = "Cart item not found." });

        return Ok(new ApiResponse { Success = true, Message = "Cart item removed." });
    }

    // DELETE: api/cart
    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized(new ApiResponse { Success = false, Message = "User not authenticated." });

        var cleared = await cartRepository.ClearCartAsync(userId);

        if (!cleared)
            return NotFound(new ApiResponse { Success = false, Message = "Cart is already empty." });

        return Ok(new ApiResponse { Success = true, Message = "Cart cleared." });
    }
}
