using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EcommerceWeb.Api.Model.Entities;

namespace EcommerceWeb.Api.Repositories.Interface
{
    public interface IProductRepository
    {
        Task<List<Product>> GetAllAsync(string? searchQuery = null);
        Task<Product?> GetByIdAsync(Guid id);

        Task<Product> CreateAsync(Product product);

        Task<Product?> UpdateAsync(Guid id, Product product);

        Task<Product?> DeleteAsync(Guid id);
    }
}