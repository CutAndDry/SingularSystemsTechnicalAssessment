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
                    Name = p.Name,
                    Price = p.Price,
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
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
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

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.Category = dto.Category;
            product.Image = dto.Image;

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
                Price = dto.Price,
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
