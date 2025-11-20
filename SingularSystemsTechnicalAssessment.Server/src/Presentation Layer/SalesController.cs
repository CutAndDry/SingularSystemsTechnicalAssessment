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

        // GET: api/Sales
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? productId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int page = 1)
        {
            const int pageSize = 50;

            var salesQuery = (await _saleRepository.GetAllAsync()).AsQueryable();

            if (productId.HasValue)
                salesQuery = salesQuery.Where(s => s.ProductId == productId.Value);

            if (startDate.HasValue)
                salesQuery = salesQuery.Where(s => s.SaleDate >= startDate.Value);

            if (endDate.HasValue)
                salesQuery = salesQuery.Where(s => s.SaleDate <= endDate.Value);

            var pagedSales = salesQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = pagedSales.Select(s => new SaleDto
            {
                Id = s.Id,
                ProductId = s.ProductId,
                ProductName = s.Product.Name,
                SaleDate = s.SaleDate,
                Quantity = s.Quantity,
                UnitPrice = s.UnitPrice
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
