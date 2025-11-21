using Microsoft.EntityFrameworkCore;
using SingularSystemsTechnicalAssessment.Server.Domain_Layer.Entities;
using SingularSystemsTechnicalAssessment.Server.Infrastructure_Layer.Repository;
using SingularSystemsTechnicalAssessment.Server.Infrastructure_Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        // check if Get Filterd retruns a filterd result 
        [Fact]
        public async Task GetFilteredAsync_ReturnsFilteredSales()
        {
            var context = GetInMemoryDb();

            // add temp data
            context.Products.Add(new Product { Id = 1, Name = "Product A", Price = 10 });
            context.Sales.Add(new Sale { Id = 1, ProductId = 1, Quantity = 5, UnitPrice = 10, SaleDate = DateTime.UtcNow });
            await context.SaveChangesAsync();

            var repo = new SaleRepository(context);

            var result = await repo.GetFilteredAsync(productId: 1, startDate: null, endDate: null, page: 1, pageSize: 10);

            Assert.Single(result);
            Assert.Equal(1, result.First().ProductId);
        }

    }
}
