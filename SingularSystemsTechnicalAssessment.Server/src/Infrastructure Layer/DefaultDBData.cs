using Microsoft.AspNetCore.Mvc;
using SingularSystemsTechnicalAssessment.Server.Domain_Layer.Entities;
using SingularSystemsTechnicalAssessment.Server.Infrastructure_Layer;
using System;

namespace SingularSystemsTechnicalAssessment.Server.src.Infrastructure_Layer
{
    public static class DefaultDBData
    {
        public static void AddDefaultData(AssessmentDbContext context)
        {
           
            if (!context.Products.Any())
            {
                var products = new List<Product>
                {
                    new Product { Id = 1, Name = "Apple", Price = 10.00m, Description = "A Apple" },
                    new Product { Id = 2, Name = "Banana", Price = 15.50m, Description = "A Banana" },
                    new Product { Id = 3, Name = "Peach", Price = 20.00m, Description = "A Peach" }
                };

                context.Products.AddRange(products);
                context.SaveChanges();

                var sales = new List<Sale>
                {
                    new Sale { Id = 1, ProductId = 1, SaleDate = DateTime.UtcNow.AddDays(-10), Quantity = 2, UnitPrice = 1000.00m },
                    new Sale { Id = 2, ProductId = 2, SaleDate = DateTime.UtcNow.AddDays(-5), Quantity = 5, UnitPrice = 15.50m },
                    new Sale { Id = 3, ProductId = 3, SaleDate = DateTime.UtcNow.AddDays(-2), Quantity = 3, UnitPrice = 45.00m }
                };

                context.Sales.AddRange(sales);
                context.SaveChanges();
            }
        }
    }
}
