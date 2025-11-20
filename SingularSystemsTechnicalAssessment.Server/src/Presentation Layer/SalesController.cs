using Microsoft.AspNetCore.Mvc;
using SingularSystemsTechnicalAssessment.Server.Application_Layer.Interfaces.Repositories;
using SingularSystemsTechnicalAssessment.Server.Domain_Layer.Entities;
using SingularSystemsTechnicalAssessment.Server.Infrastructure_Layer.Repository;
using SingularSystemsTechnicalAssessment.Server.src.Application_Layer.DTO_s;

namespace SingularSystemsTechnicalAssessment.Server.src.Presentation_Layer
{
        [ApiController]
        [Route("api/[controller]")]
        public class SalesController : ControllerBase
        {

        private readonly IRepository<Sale> _saleRepository;
        private readonly IRepository<Product> _productRepository;

        public SalesController(IRepository<Sale> saleRepository, IRepository<Product> productRepository)
        {
            _saleRepository = saleRepository;
            _productRepository = productRepository;
        }

        // GET: api/Products page 1 => page 10
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var allProducts = await _productRepository.GetAllAsync();

            var pagedProducts = allProducts
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = pagedProducts.Select(p => new ProductDto
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


        // GET: api/Sales/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var sale = await _saleRepository.GetByIdAsync(id);
            if (sale == null) return NotFound();

            var dto = new SaleDto
            {
                Id = sale.Id,
                ProductId = sale.ProductId,
                ProductName = sale.Product.Name,
                SaleDate = sale.SaleDate,
                Quantity = sale.Quantity,
                UnitPrice = sale.UnitPrice
            };

            return Ok(dto);
        }

        // POST: api/Sales
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaleDto dto)
        {
            var product = await _productRepository.GetByIdAsync(dto.ProductId);
            if (product == null) return BadRequest("Invalid ProductId.");

            var sale = new Sale
            {
                ProductId = dto.ProductId,
                SaleDate = dto.SaleDate,
                Quantity = dto.Quantity,
                UnitPrice = dto.UnitPrice
            };

            await _saleRepository.AddAsync(sale);
            await _saleRepository.SaveChangesAsync();

            dto.Id = sale.Id;
            dto.ProductName = product.Name;

            return CreatedAtAction(nameof(GetById), new { id = sale.Id }, dto);
        }

        // PUT: api/Sales/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] SaleDto dto)
        {
            var sale = await _saleRepository.GetByIdAsync(id);
            if (sale == null) return NotFound();

            var product = await _productRepository.GetByIdAsync(dto.ProductId);
            if (product == null) return BadRequest("Invalid ProductId.");

            sale.ProductId = dto.ProductId;
            sale.SaleDate = dto.SaleDate;
            sale.Quantity = dto.Quantity;
            sale.UnitPrice = dto.UnitPrice;

            _saleRepository.Update(sale);
            await _saleRepository.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Sales/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var sale = await _saleRepository.GetByIdAsync(id);
            if (sale == null) return NotFound();

            _saleRepository.Delete(sale);
            await _saleRepository.SaveChangesAsync();

            return NoContent();
        }
    }

}
