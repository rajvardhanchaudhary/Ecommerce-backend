using EcommerceWeb.Api.Model.Entities;

namespace EcommerceWeb.Api.Repositories.Interface
{
    public interface IReviewRepository
    {
        Task<Review> AddReviewAsync(Guid productId, Review review);

        Task<List<Review>> GetReviewsByProductIdAsync(Guid productId);
        Task<bool> DeleteReviewAsync(Guid reviewId);


    }
}
