using Microsoft.EntityFrameworkCore;
using SingularSystemsTechnicalAssessment.Server.Domain_Layer.Entities;
using SingularSystemsTechnicalAssessment.Server.Infrastructure_Layer.Repository;
using SingularSystemsTechnicalAssessment.Server.Infrastructure_Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace SingularSystemsTests
{
    public class SaleRepositoryTests
    {
        private AssessmentDbContext GetInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<AssessmentDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AssessmentDbContext(options);
        }

        [Fact]
        public async Task GetFilteredAsync_ReturnsFilteredSales()
        {
            var context = GetInMemoryDb();
            context.Products.Add(new Product { Id = 1, Description = "Product A", SalePrice = 10 });
            context.Sales.Add(new Sale { Id = 1, ProductId = 1, SaleQty = 5, SalePrice = 10, SaleDate = DateTime.UtcNow });
            await context.SaveChangesAsync();

            var repo = new SaleRepository(context);
            var result = await repo.GetFilteredAsync(productId: 1, startDate: null, endDate: null, page: 1, pageSize: 10);

            Assert.Single(result);
            Assert.Equal(1, result.First().ProductId);
        }

        [Fact]
        public async Task GetFilteredAsync_ReturnsEmptyWhenNoMatch()
        {
            var context = GetInMemoryDb();
            context.Products.Add(new Product { Id = 1, Description = "Product A", SalePrice = 10 });
            context.Sales.Add(new Sale { Id = 1, ProductId = 1, SaleQty = 5, SalePrice = 10, SaleDate = DateTime.UtcNow });
            await context.SaveChangesAsync();

            var repo = new SaleRepository(context);
            var result = await repo.GetFilteredAsync(productId: 2, startDate: null, endDate: null, page: 1, pageSize: 10);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetFilteredAsync_FiltersByDateRange()
        {
            var context = GetInMemoryDb();
            var now = DateTime.UtcNow;
            context.Products.Add(new Product { Id = 1, Description = "Product A", SalePrice = 10 });
            context.Sales.AddRange(
                new Sale { Id = 1, ProductId = 1, SaleQty = 5, SalePrice = 10, SaleDate = now.AddDays(-5) },
                new Sale { Id = 2, ProductId = 1, SaleQty = 3, SalePrice = 10, SaleDate = now }
            );
            await context.SaveChangesAsync();

            var repo = new SaleRepository(context);
            var result = await repo.GetFilteredAsync(productId: null, startDate: now.AddDays(-2), endDate: now, page: 1, pageSize: 10);

            Assert.Single(result);
            Assert.Equal(2, result.First().Id);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllSalesWithProducts()
        {
            var context = GetInMemoryDb();
            context.Products.Add(new Product { Id = 1, Description = "Product A", SalePrice = 10 });
            context.Sales.AddRange(
                new Sale { Id = 1, ProductId = 1, SaleQty = 5, SalePrice = 10, SaleDate = DateTime.UtcNow },
                new Sale { Id = 2, ProductId = 1, SaleQty = 3, SalePrice = 10, SaleDate = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();

            var repo = new SaleRepository(context);
            var result = await repo.GetAllAsync();

            Assert.Equal(2, result.Count());
            Assert.All(result, s => Assert.NotNull(s.Product));
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsSaleWithProduct()
        {
            var context = GetInMemoryDb();
            context.Products.Add(new Product { Id = 1, Description = "Product A", SalePrice = 10 });
            context.Sales.Add(new Sale { Id = 1, ProductId = 1, SaleQty = 5, SalePrice = 10, SaleDate = DateTime.UtcNow });
            await context.SaveChangesAsync();

            var repo = new SaleRepository(context);
            var result = await repo.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.NotNull(result.Product);
            Assert.Equal("Product A", result.Product.Description);
        }

        [Fact]
        public async Task AddAsync_AddsSaleToDatabase()
        {
            var context = GetInMemoryDb();
            context.Products.Add(new Product { Id = 1, Description = "Product A", SalePrice = 10 });
            await context.SaveChangesAsync();

            var sale = new Sale { Id = 1, ProductId = 1, SaleQty = 5, SalePrice = 10, SaleDate = DateTime.UtcNow };
            var repo = new SaleRepository(context);
            await repo.AddAsync(sale);
            await repo.SaveChangesAsync();

            var retrieved = await context.Sales.FindAsync(1);
            Assert.NotNull(retrieved);
            Assert.Equal(5, retrieved.SaleQty);
        }

        [Fact]
        public async Task GetFilteredAsync_PaginatesCorrectly()
        {
            var context = GetInMemoryDb();
            context.Products.Add(new Product { Id = 1, Description = "Product A", SalePrice = 10 });
            for (int i = 1; i <= 25; i++)
            {
                context.Sales.Add(new Sale { Id = i, ProductId = 1, SaleQty = i, SalePrice = 10, SaleDate = DateTime.UtcNow });
            }
            await context.SaveChangesAsync();

            var repo = new SaleRepository(context);
            var page1 = await repo.GetFilteredAsync(null, null, null, 1, 10);
            var page2 = await repo.GetFilteredAsync(null, null, null, 2, 10);

            Assert.Equal(10, page1.Count());
            Assert.Equal(10, page2.Count());
            Assert.NotEqual(page1.First().Id, page2.First().Id);
        }
    }
}
