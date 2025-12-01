using Microsoft.AspNetCore.Mvc;
using SingularSystemsTechnicalAssessment.Server.Application_Layer.Interfaces.Repositories;
using SingularSystemsTechnicalAssessment.Server.Domain_Layer.Entities;
using SingularSystemsTechnicalAssessment.Server.src.Application_Layer.DTO_s;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using System.Linq;
using System.Threading.Tasks;

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
                Description = p.Description,
                SalePrice = p.SalePrice,
                Category = p.Category,
                Image = p.Image,
                TotalSales = p.Sales.Sum(s => s.SaleQty),
                TotalRevenue = p.Sales.Sum(s => s.SaleQty * s.SalePrice)
            });

            return Ok(result);
        }

        // GET: api/Products?pageNumber=1&pageSize=10
        [HttpGet("GetAllPagination")]
        public async Task<IActionResult> GetAllPagination([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
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
                    Description = p.Description,
                    SalePrice = p.SalePrice,
                    Category = p.Category,
                    Image = p.Image,
                    TotalSales = p.Sales.Sum(s => s.SaleQty),
                    TotalRevenue = p.Sales.Sum(s => s.SaleQty * s.SalePrice)
                })
                .ToList();

            var response = new ProductPagedResult<ProductListDto>
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
                Description = product.Description,
                SalePrice = product.SalePrice,
                Category = product.Category,
                Image = product.Image,
                TotalSales = product.Sales.Sum(s => s.SaleQty),
                TotalRevenue = product.Sales.Sum(s => s.SaleQty * s.SalePrice),

                Sales = product.Sales.Select(s => new SaleDetailDto
                {
                    Id = s.Id,
                    SaleDate = s.SaleDate,
                    Quantity = s.SaleQty,
                    UnitPrice = s.SalePrice,
                    ProductId = s.ProductId
                }).ToList()
            };

            return Ok(dto);
        }

        // PUT: api/Products/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductUpdateDto dto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();

            product.Description = dto.Description;
            product.SalePrice = dto.SalePrice;
            product.Category = dto.Category;
            product.Image = dto.Image;

            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Products (multipart/form-data with file)
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] ProductCreateFormDto dto)
        {
            var product = new Product
            {
                Description = dto.Description,
                SalePrice = dto.SalePrice,
                Category = dto.Category,
                Image = null
            };

            if (dto.Image != null && dto.Image.Length > 0)
            {
                using var ms = new MemoryStream();
                await dto.Image.CopyToAsync(ms);
                var base64 = Convert.ToBase64String(ms.ToArray());
                var contentType = string.IsNullOrWhiteSpace(dto.Image.ContentType) ? "application/octet-stream" : dto.Image.ContentType;
                product.Image = $"data:{contentType};base64,{base64}";
            }

            await _productRepository.AddAsync(product);
            await _productRepository.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = product.Id }, null);
        }

        // POST: api/Products/json (application/json)
        [HttpPost("json")]
        [Consumes("application/json")]
        public async Task<IActionResult> CreateJson([FromBody] ProductCreateDto dto)
        {
            var product = new Product
            {
                Description = dto.Description,
                SalePrice = dto.SalePrice,
                Category = dto.Category,
                Image = dto.Image
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
