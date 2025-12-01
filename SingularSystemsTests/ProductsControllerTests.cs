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
using Xunit;

namespace SingularSystemsTests
{
    public class ProductsControllerTests
    {
        [Fact]
        public async Task GetAll_ReturnsOkWithProducts()
        {
            var mockRepo = new Mock<IProductRepository>();
            mockRepo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Product>
                {
                    new Product { Id = 1, Description = "Product A", SalePrice = 10 },
                    new Product { Id = 2, Description = "Product B", SalePrice = 20 }
                });

            var controller = new ProductsController(mockRepo.Object);
            var result = await controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenProductDoesNotExist()
        {
            var mockRepo = new Mock<IProductRepository>();
            mockRepo.Setup(r => r.GetProductWithSalesAsync(It.IsAny<int>()))
                .ReturnsAsync((Product)null);

            var controller = new ProductsController(mockRepo.Object);
            var result = await controller.GetById(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetById_ReturnsOkWithProduct_WhenProductExists()
        {
            var product = new Product { Id = 1, Description = "Product A", SalePrice = 10, Sales = new List<Sale>() };
            var mockRepo = new Mock<IProductRepository>();
            mockRepo.Setup(r => r.GetProductWithSalesAsync(1))
                .ReturnsAsync(product);

            var controller = new ProductsController(mockRepo.Object);
            var result = await controller.GetById(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtAction()
        {
            var mockRepo = new Mock<IProductRepository>();
            mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Product)null);
            mockRepo.Setup(r => r.AddAsync(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);
            mockRepo.Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var controller = new ProductsController(mockRepo.Object);
            var dto = new ProductCreateDto { Description = "New Product", SalePrice = 15 };
            var result = await controller.CreateJson(dto);

            Assert.IsType<CreatedAtActionResult>(result);
            mockRepo.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
            mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Update_ReturnsNoContent_WhenSuccessful()
        {
            var product = new Product { Id = 1, Description = "Original", SalePrice = 10 };
            var mockRepo = new Mock<IProductRepository>();
            mockRepo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(product);
            mockRepo.Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var controller = new ProductsController(mockRepo.Object);
            var dto = new ProductUpdateDto { Description = "Updated", SalePrice = 20 };
            var result = await controller.Update(1, dto);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenProductNotFound()
        {
            var mockRepo = new Mock<IProductRepository>();
            mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Product)null);

            var controller = new ProductsController(mockRepo.Object);
            var dto = new ProductUpdateDto { Description = "Updated", SalePrice = 20 };
            var result = await controller.Update(999, dto);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenSuccessful()
        {
            var product = new Product { Id = 1, Description = "Product", SalePrice = 10 };
            var mockRepo = new Mock<IProductRepository>();
            mockRepo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(product);
            mockRepo.Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var controller = new ProductsController(mockRepo.Object);
            var result = await controller.Delete(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenProductNotFound()
        {
            var mockRepo = new Mock<IProductRepository>();
            mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Product)null);

            var controller = new ProductsController(mockRepo.Object);
            var result = await controller.Delete(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetAllPagination_ReturnsPaginatedProducts()
        {
            var products = new List<Product>
            {
                new Product { Id = 1, Description = "Product A", SalePrice = 10, Sales = new List<Sale>() },
                new Product { Id = 2, Description = "Product B", SalePrice = 20, Sales = new List<Sale>() }
            };
            var mockRepo = new Mock<IProductRepository>();
            mockRepo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(products);

            var controller = new ProductsController(mockRepo.Object);
            var result = await controller.GetAllPagination(1, 10);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var pagedResult = Assert.IsType<ProductPagedResult<ProductListDto>>(okResult.Value);
            Assert.NotEmpty(pagedResult.Items);
        }
    }
}
