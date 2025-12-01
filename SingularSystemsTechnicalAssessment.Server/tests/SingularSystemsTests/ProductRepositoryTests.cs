using Microsoft.EntityFrameworkCore;
using SingularSystemsTechnicalAssessment.Server.Domain_Layer.Entities;
using SingularSystemsTechnicalAssessment.Server.Infrastructure_Layer;
using SingularSystemsTechnicalAssessment.Server.Infrastructure_Layer.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SingularSystemsTests
{
    public class ProductRepositoryTests
    {
        private AssessmentDbContext GetInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<AssessmentDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AssessmentDbContext(options);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllProducts()
        {
            var context = GetInMemoryDb();
            context.Products.AddRange(
                new Product { Id = 1, Description = "Product A", SalePrice = 10 },
                new Product { Id = 2, Description = "Product B", SalePrice = 20 }
            );
            await context.SaveChangesAsync();

            var repo = new ProductRepository(context);
            var result = await repo.GetAllAsync();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsProductWhenExists()
        {
            var context = GetInMemoryDb();
            context.Products.Add(new Product { Id = 1, Description = "Product A", SalePrice = 10 });
            await context.SaveChangesAsync();

            var repo = new ProductRepository(context);
            var result = await repo.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Product A", result.Description);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNullWhenNotExists()
        {
            var context = GetInMemoryDb();
            var repo = new ProductRepository(context);
            var result = await repo.GetByIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetProductWithSalesAsync_ReturnsProductWithSales()
        {
            var context = GetInMemoryDb();
            var product = new Product { Id = 1, Description = "Product A", SalePrice = 10 };
            context.Products.Add(product);
            context.Sales.Add(new Sale { Id = 1, ProductId = 1, SaleQty = 5, SalePrice = 10, SaleDate = DateTime.UtcNow });
            await context.SaveChangesAsync();

            var repo = new ProductRepository(context);
            var result = await repo.GetProductWithSalesAsync(1);

            Assert.NotNull(result);
            Assert.Single(result.Sales);
            Assert.Equal(5, result.Sales.First().SaleQty);
        }

        [Fact]
        public async Task AddAsync_AddsProductToDatabase()
        {
            var context = GetInMemoryDb();
            var product = new Product { Id = 1, Description = "New Product", SalePrice = 25 };

            var repo = new ProductRepository(context);
            await repo.AddAsync(product);
            await repo.SaveChangesAsync();

            var retrieved = await context.Products.FindAsync(1);
            Assert.NotNull(retrieved);
            Assert.Equal("New Product", retrieved.Description);
        }

        [Fact]
        public async Task Update_UpdatesProductInDatabase()
        {
            var context = GetInMemoryDb();
            var product = new Product { Id = 1, Description = "Original Desc", SalePrice = 10 };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var repo = new ProductRepository(context);
            product.Description = "Updated Desc";
            repo.Update(product);
            await repo.SaveChangesAsync();

            var updated = await context.Products.FindAsync(1);
            Assert.Equal("Updated Desc", updated.Description);
        }

        [Fact]
        public async Task Delete_RemovesProductFromDatabase()
        {
            var context = GetInMemoryDb();
            var product = new Product { Id = 1, Description = "Product to Delete", SalePrice = 10 };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var repo = new ProductRepository(context);
            repo.Delete(product);
            await repo.SaveChangesAsync();

            var deleted = await context.Products.FindAsync(1);
            Assert.Null(deleted);
        }
    }
}
