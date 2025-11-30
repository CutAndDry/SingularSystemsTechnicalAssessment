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
                new Product { Id = 1, Name = "Product A", Price = 10 },
                new Product { Id = 2, Name = "Product B", Price = 20 }
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
            context.Products.Add(new Product { Id = 1, Name = "Product A", Price = 10 });
            await context.SaveChangesAsync();

            var repo = new ProductRepository(context);
            var result = await repo.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Product A", result.Name);
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
            var product = new Product { Id = 1, Name = "Product A", Price = 10 };
            context.Products.Add(product);
            context.Sales.Add(new Sale { Id = 1, ProductId = 1, Quantity = 5, UnitPrice = 10, SaleDate = DateTime.UtcNow });
            await context.SaveChangesAsync();

            var repo = new ProductRepository(context);
            var result = await repo.GetProductWithSalesAsync(1);

            Assert.NotNull(result);
            Assert.Single(result.Sales);
            Assert.Equal(5, result.Sales.First().Quantity);
        }

        [Fact]
        public async Task AddAsync_AddsProductToDatabase()
        {
            var context = GetInMemoryDb();
            var product = new Product { Id = 1, Name = "New Product", Price = 25 };

            var repo = new ProductRepository(context);
            await repo.AddAsync(product);
            await repo.SaveChangesAsync();

            var retrieved = await context.Products.FindAsync(1);
            Assert.NotNull(retrieved);
            Assert.Equal("New Product", retrieved.Name);
        }

        [Fact]
        public async Task Update_UpdatesProductInDatabase()
        {
            var context = GetInMemoryDb();
            var product = new Product { Id = 1, Name = "Original Name", Price = 10 };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var repo = new ProductRepository(context);
            product.Name = "Updated Name";
            repo.Update(product);
            await repo.SaveChangesAsync();

            var updated = await context.Products.FindAsync(1);
            Assert.Equal("Updated Name", updated.Name);
        }

        [Fact]
        public async Task Delete_RemovesProductFromDatabase()
        {
            var context = GetInMemoryDb();
            var product = new Product { Id = 1, Name = "Product to Delete", Price = 10 };
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
