using AutoMapper;
using EcommerceWeb.Api.Data;
using EcommerceWeb.Api.Model.DTO;
using EcommerceWeb.Api.Model.Entities;
using EcommerceWeb.Api.Repositories.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceWeb.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IProductRepository productRepository;
        private readonly IMapper mapper;

        public ProductController(ApplicationDbContext dbContext, IProductRepository productRepository, IMapper mapper)
        {
            this.dbContext = dbContext;
            this.productRepository = productRepository;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<IEnumerable<Product>> GetAllAsync(string? searchQuery)
        {
            var query = dbContext.Products.AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.ToLower();
                query = query.Where(p => p.Title.ToLower().Contains(searchQuery)
                                      || p.Description.ToLower().Contains(searchQuery));
            }

            return await query.ToListAsync();
        }

        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var product = await productRepository.GetByIdAsync(id);

            if (product == null)
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
                message = "Product retrieved successfully.",
                data = mapper.Map<ProductDto>(product)
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateProductDto productDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid product data.",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var productDomainModel = mapper.Map<Product>(productDto);

            productDomainModel = await productRepository.CreateAsync(productDomainModel);

            var createdProductDto = mapper.Map<ProductDto>(productDomainModel);

            return CreatedAtAction(nameof(GetById), new { id = productDomainModel.Id }, new
            {
                success = true,
                message = "Product created successfully.",
                data = createdProductDto
            });
        }

        [HttpPut("{id:Guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateProductDto updateProductDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid product update data.",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var productDomainModel = mapper.Map<Product>(updateProductDto);
            productDomainModel = await productRepository.UpdateAsync(id, productDomainModel);

            if (productDomainModel == null)
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
                message = "Product updated successfully.",
                data = mapper.Map<ProductDto>(productDomainModel)
            });
        }

        [HttpDelete("{id:Guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            var deletedProduct = await productRepository.DeleteAsync(id);
            if (deletedProduct == null)
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
                message = "Product deleted successfully.",
                data = mapper.Map<ProductDto>(deletedProduct)
            });
        }
    }
}