using Microsoft.AspNetCore.Mvc;
using Moq;
using SingularSystemsTechnicalAssessment.Server.Application_Layer.Interfaces.Repositories;
using SingularSystemsTechnicalAssessment.Server.Domain_Layer.Entities;
using SingularSystemsTechnicalAssessment.Server.src.Application_Layer.DTO_s;
using SingularSystemsTechnicalAssessment.Server.src.Presentation_Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SingularSystemsTests
{
    public class SalesControllerTests
    {
        [Fact]
        public async Task GetById_ReturnsNotFound_ForInvalidId()
        {
            var saleRepo = new Mock<ISaleRepository>();
            var productRepo = new Mock<IRepository<Product>>();
            saleRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Sale)null);

            var controller = new SalesController(saleRepo.Object, productRepo.Object);

            var result = await controller.GetById(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetById_ReturnsOkWithSale_WhenSaleExists()
        {
            var sale = new Sale 
            { 
                Id = 1, 
                ProductId = 1, 
                Quantity = 5, 
                UnitPrice = 10, 
                SaleDate = DateTime.UtcNow,
                Product = new Product { Id = 1, Name = "Product A", Price = 10 }
            };
            var saleRepo = new Mock<ISaleRepository>();
            var productRepo = new Mock<IRepository<Product>>();
            saleRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(sale);

            var controller = new SalesController(saleRepo.Object, productRepo.Object);

            var result = await controller.GetById(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetAll_ReturnsAllSales()
        {
            var product = new Product { Id = 1, Name = "Product A", Price = 10 };
            var sales = new List<Sale>
            {
                new Sale { Id = 1, ProductId = 1, Quantity = 5, UnitPrice = 10, SaleDate = DateTime.UtcNow, Product = product },
                new Sale { Id = 2, ProductId = 1, Quantity = 3, UnitPrice = 10, SaleDate = DateTime.UtcNow, Product = product }
            };
            var saleRepo = new Mock<ISaleRepository>();
            var productRepo = new Mock<IRepository<Product>>();
            saleRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(sales);

            var controller = new SalesController(saleRepo.Object, productRepo.Object);

            var result = await controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtAction_WithValidData()
        {
            var product = new Product { Id = 1, Name = "Product A", Price = 10 };
            var saleRepo = new Mock<ISaleRepository>();
            var productRepo = new Mock<IRepository<Product>>();
            productRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);
            saleRepo.Setup(r => r.AddAsync(It.IsAny<Sale>())).Returns(Task.CompletedTask);
            saleRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var controller = new SalesController(saleRepo.Object, productRepo.Object);
            var dto = new SaleCreateDto { ProductId = 1, Quantity = 5, UnitPrice = null };

            var result = await controller.Create(dto);

            Assert.IsType<CreatedAtActionResult>(result);
            saleRepo.Verify(r => r.AddAsync(It.IsAny<Sale>()), Times.Once);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WithInvalidProductId()
        {
            var saleRepo = new Mock<ISaleRepository>();
            var productRepo = new Mock<IRepository<Product>>();
            productRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Product)null);

            var controller = new SalesController(saleRepo.Object, productRepo.Object);
            var dto = new SaleCreateDto { ProductId = 999, Quantity = 5 };

            var result = await controller.Create(dto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsNoContent_WhenSuccessful()
        {
            var sale = new Sale 
            { 
                Id = 1, 
                ProductId = 1, 
                Quantity = 5, 
                UnitPrice = 10, 
                SaleDate = DateTime.UtcNow 
            };
            var product = new Product { Id = 1, Name = "Product A", Price = 10 };
            var saleRepo = new Mock<ISaleRepository>();
            var productRepo = new Mock<IRepository<Product>>();
            saleRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(sale);
            productRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);
            saleRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var controller = new SalesController(saleRepo.Object, productRepo.Object);
            var dto = new SaleUpdateDto { ProductId = 1, Quantity = 10, UnitPrice = 15, SaleDate = DateTime.UtcNow };

            var result = await controller.Update(1, dto);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenSaleNotFound()
        {
            var saleRepo = new Mock<ISaleRepository>();
            var productRepo = new Mock<IRepository<Product>>();
            saleRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Sale)null);

            var controller = new SalesController(saleRepo.Object, productRepo.Object);
            var dto = new SaleUpdateDto { ProductId = 1, Quantity = 5, UnitPrice = 10, SaleDate = DateTime.UtcNow };

            var result = await controller.Update(999, dto);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenSuccessful()
        {
            var sale = new Sale { Id = 1, ProductId = 1, Quantity = 5, UnitPrice = 10, SaleDate = DateTime.UtcNow };
            var saleRepo = new Mock<ISaleRepository>();
            var productRepo = new Mock<IRepository<Product>>();
            saleRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(sale);
            saleRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var controller = new SalesController(saleRepo.Object, productRepo.Object);

            var result = await controller.Delete(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenSaleNotFound()
        {
            var saleRepo = new Mock<ISaleRepository>();
            var productRepo = new Mock<IRepository<Product>>();
            saleRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Sale)null);

            var controller = new SalesController(saleRepo.Object, productRepo.Object);

            var result = await controller.Delete(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetAllPagination_ReturnsPaginatedSales()
        {
            var product = new Product { Id = 1, Name = "Product A", Price = 10 };
            var sales = new List<Sale>
            {
                new Sale { Id = 1, ProductId = 1, Quantity = 5, UnitPrice = 10, SaleDate = DateTime.UtcNow, Product = product }
            };
            var saleRepo = new Mock<ISaleRepository>();
            var productRepo = new Mock<IRepository<Product>>();
            saleRepo.Setup(r => r.GetFilteredAsync(It.IsAny<int?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(sales);

            var controller = new SalesController(saleRepo.Object, productRepo.Object);

            var result = await controller.GetAllPagination(1, 10);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var pagedResult = Assert.IsType<SalesPagedResult<SaleListDto>>(okResult.Value);
            Assert.NotEmpty(pagedResult.Items);
        }
    }
}
