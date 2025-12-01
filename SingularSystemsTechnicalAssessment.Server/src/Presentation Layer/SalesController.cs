using Microsoft.AspNetCore.Mvc;
using SingularSystemsTechnicalAssessment.Server.Application_Layer.Interfaces.Repositories;
using SingularSystemsTechnicalAssessment.Server.Domain_Layer.Entities;
using SingularSystemsTechnicalAssessment.Server.Infrastructure_Layer.Repository;
using SingularSystemsTechnicalAssessment.Server.src.Application_Layer.DTO_s;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SingularSystemsTechnicalAssessment.Server.src.Presentation_Layer
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesController : ControllerBase
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IRepository<Product> _productRepository;

        public SalesController(ISaleRepository saleRepository, IRepository<Product> productRepository)
        {
            _saleRepository = saleRepository;
            _productRepository = productRepository;
        }

        // GET: api/Sales
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var sales = await _saleRepository.GetAllAsync();

            var result = sales.Select(s => new SaleListDto
            {
                Id = s.Id,
                ProductName = s.Product.Description,
                SaleQty = s.SaleQty,
                SalePrice = s.SalePrice,
                SaleDate = s.SaleDate
            });

            return Ok(result);
        }

        // filtered paginated result
        // GET: api/Sales?pageNumber=1&pageSize=10
        [HttpGet("GetAllPagination")]
        public async Task<IActionResult> GetAllPagination(
           [FromQuery] int pageNumber = 1,
           [FromQuery] int pageSize = 10,
           [FromQuery] int? productId = null,
           [FromQuery] DateTime? startDate = null,
           [FromQuery] DateTime? endDate = null)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var sales = await _saleRepository.GetFilteredAsync(
                productId,
                startDate,
                endDate,
                pageNumber,
                pageSize
            );

            var totalCount = (await _saleRepository.GetFilteredAsync(
                productId,
                startDate,
                endDate,
                1,
                int.MaxValue
            )).Count();

            var items = sales.Select(s => new SaleListDto
            {
                Id = s.Id,
                ProductName = s.Product.Description,
                SaleQty = s.SaleQty,
                SalePrice = s.SalePrice,
                SaleDate = s.SaleDate
            }).ToList();

            var response = new SalesPagedResult<SaleListDto>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return Ok(response);
        }


        // GET: api/Sales/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var sale = await _saleRepository.GetByIdAsync(id);
            if (sale == null) return NotFound();

            var dto = new SaleDetailDto
            {
                Id = sale.Id,
                ProductId = sale.ProductId,
                ProductName = sale.Product.Description,
                Quantity = sale.SaleQty,
                UnitPrice = sale.SalePrice,
                SaleDate = sale.SaleDate
            };

            return Ok(dto);
        }

        // POST: api/Sales
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SaleCreateDto dto)
        {
            var product = await _productRepository.GetByIdAsync(dto.ProductId);
            if (product == null) return BadRequest("Invalid ProductId.");

            var sale = new Sale
            {
                ProductId = dto.ProductId,
                SaleQty = dto.SaleQty,
                SalePrice = dto.SalePrice ?? product.SalePrice,
                SaleDate = DateTime.UtcNow
            };

            await _saleRepository.AddAsync(sale);
            await _saleRepository.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = sale.Id }, null);
        }

        // PUT: api/Sales/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] SaleUpdateDto dto)
        {
            var sale = await _saleRepository.GetByIdAsync(id);
            if (sale == null) return NotFound();

            var product = await _productRepository.GetByIdAsync(dto.ProductId);
            if (product == null) return BadRequest("Invalid ProductId.");

            sale.ProductId = dto.ProductId;
            sale.SaleQty = dto.SaleQty;
            sale.SalePrice = dto.SalePrice;
            sale.SaleDate = dto.SaleDate;

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
