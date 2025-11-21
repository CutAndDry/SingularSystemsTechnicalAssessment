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

            var result = products.Select(p => new ProductListDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                TotalSales = p.Sales.Sum(s => s.Quantity),
                TotalRevenue = p.Sales.Sum(s => s.Quantity * s.UnitPrice)
            });

            return Ok(result);
        }

        // GET: api/Products?pageNumber=1&pageSize=10
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var products = await _productRepository.GetAllAsync();

            var totalCount = products.Count();
            var items = products
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductListDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    TotalSales = p.Sales.Sum(s => s.Quantity),
                    TotalRevenue = p.Sales.Sum(s => s.Quantity * s.UnitPrice)
                })
                .ToList();

            var response = new PagedResult<ProductListDto>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return Ok(response);
        }



        // GET: api/Products/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _productRepository.GetProductWithSalesAsync(id);
            if (product == null) return NotFound();

            var dto = new ProductDetailDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                TotalSales = product.Sales.Sum(s => s.Quantity),
                TotalRevenue = product.Sales.Sum(s => s.Quantity * s.UnitPrice),

                Sales = product.Sales.Select(s => new SaleDto
                {
                    Id = s.Id,
                    SaleDate = s.SaleDate,
                    Quantity = s.Quantity,
                    UnitPrice = s.UnitPrice,
                    ProductId = s.ProductId
                }).ToList()
            };

            return Ok(dto);
        }

        // GET: api/Products/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductUpdateDto dto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;

            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Products
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductCreateDto dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price
            };

            await _productRepository.AddAsync(product);
            await _productRepository.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = product.Id }, null);
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
