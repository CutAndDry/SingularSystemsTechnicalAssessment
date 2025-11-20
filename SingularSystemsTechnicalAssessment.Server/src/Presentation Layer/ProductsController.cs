using Microsoft.AspNetCore.Mvc;
using SingularSystemsTechnicalAssessment.Server.Application_Layer.Interfaces.Repositories;
using SingularSystemsTechnicalAssessment.Server.Domain_Layer.Entities;
using SingularSystemsTechnicalAssessment.Server.src.Application_Layer.DTO_s;

namespace SingularSystemsTechnicalAssessment.Server.src.Presentation_Layer
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;

        public ProductsController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _productRepository.GetAllAsync();

            var result = products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Description = p.Description,
                TotalSales = p.Sales.Sum(s => s.Quantity),
                TotalRevenue = p.Sales.Sum(s => s.Quantity * s.UnitPrice)
            });

            return Ok(result);
        }

        // GET: api/Products/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productRepository.GetProductWithSalesAsync(id);
            if (product == null) return NotFound();

            var dto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                TotalSales = product.Sales.Sum(s => s.Quantity),
                TotalRevenue = product.Sales.Sum(s => s.Quantity * s.UnitPrice)
            };

            return Ok(dto);
        }

        // POST: api/Products
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductDto dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Price = dto.Price,
                Description = dto.Description
            };

            await _productRepository.AddAsync(product);
            await _productRepository.SaveChangesAsync();

            dto.Id = product.Id;
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, dto);
        }


        // PUT: api/Products/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductDto dto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();

            product.Name = dto.Name;
            product.Price = dto.Price;
            product.Description = dto.Description;

            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Products/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();

            _productRepository.Delete(product);
            await _productRepository.SaveChangesAsync();

            return NoContent();
        }




    }
}
