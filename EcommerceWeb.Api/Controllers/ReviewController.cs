using AutoMapper;
using EcommerceWeb.Api.Data;
using EcommerceWeb.Api.Model.DTO;
using EcommerceWeb.Api.Model.Entities;
using EcommerceWeb.Api.Repositories.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EcommerceWeb.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IMapper mapper;
        private readonly IReviewRepository reviewRepository;

        public ReviewController(ApplicationDbContext dbContext, IMapper mapper, IReviewRepository reviewRepository)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
            this.reviewRepository = reviewRepository;
        }

        // POST: api/review/{productId}/reviews
        [HttpPost("{id:Guid}/reviews")]
        public async Task<IActionResult> AddReview([FromRoute] Guid id, [FromBody] CreateReviewDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid review data.",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "User not authorized."
                });
            }

            var review = mapper.Map<Review>(dto);
            review.UserId = userId;

            var result = await reviewRepository.AddReviewAsync(id, review);
            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Product not found."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Review added successfully.",
                data = new { reviewId = result.Id }
            });
        }

        // GET: api/review/{productId}/reviews
        [HttpGet("{id:Guid}/reviews")]
        public async Task<IActionResult> GetReviews([FromRoute] Guid id)
        {
            var reviews = await reviewRepository.GetReviewsByProductIdAsync(id);

            if (reviews == null || !reviews.Any())
            {
                return NotFound(new
                {
                    success = false,
                    message = "Product not found or no reviews available."
                });
            }

            var reviewsDto = mapper.Map<List<ReviewDto>>(reviews);

            return Ok(new
            {
                success = true,
                message = "Reviews retrieved successfully.",
                data = new
                {
                    productId = id,
                    reviews = reviewsDto
                }
            });
        }

        // DELETE: api/review/{reviewId}
        [HttpDelete("{id:Guid}")]
        [Authorize]
        public async Task<IActionResult> DeleteReview([FromRoute] Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "User not authorized."
                });
            }

            var review = await dbContext.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Review not found."
                });
            }

            if (review.UserId != userId)
            {
                return StatusCode(403, new
                {
                    success = false,
                    message = "You are not allowed to delete this review."
                });
            }


            var result = await reviewRepository.DeleteReviewAsync(id);
            if (!result)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to delete review."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Review deleted successfully."
            });
        }
    }
}
