using Microsoft.EntityFrameworkCore;
using SingularSystemsTechnicalAssessment.Server.Application_Layer.Interfaces.Repositories;
using SingularSystemsTechnicalAssessment.Server.Domain_Layer.Entities;
using System;

namespace SingularSystemsTechnicalAssessment.Server.Infrastructure_Layer.Repository
{
 

    public class ProductRepository : Repository<Product>, IProductRepository
    {
        public ProductRepository(AssessmentDbContext context) : base(context) { }

        public async Task<Product?> GetProductWithSalesAsync(int productId)
        {
            return await _dbSet
                .Include(p => p.Sales)
                .FirstOrDefaultAsync(p => p.Id == productId);
        }
    }
}
