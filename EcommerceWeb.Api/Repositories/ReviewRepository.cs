using EcommerceWeb.Api.Data;
using EcommerceWeb.Api.Model.Entities;
using EcommerceWeb.Api.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace EcommerceWeb.Api.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly ApplicationDbContext dbContext;

        public ReviewRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<Review> AddReviewAsync(Guid productId, Review review)
        {
            var product = await dbContext.Products.FindAsync(productId);
            if (product == null) return null;

            review.ProductId = productId;

            await dbContext.Reviews.AddAsync(review);
            await dbContext.SaveChangesAsync();

            return review;
        }

        public async Task<List<Review>> GetReviewsByProductIdAsync(Guid productId)
        {
            var product = await dbContext.Products.FindAsync(productId);
            if (product == null) return null;

            return await dbContext.Reviews
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> DeleteReviewAsync(Guid reviewId)
        {
            var review = await dbContext.Reviews.FindAsync(reviewId);
            if (review == null)
                return false;

            dbContext.Reviews.Remove(review);
            await dbContext.SaveChangesAsync();

            return true;
        }

    }
}
