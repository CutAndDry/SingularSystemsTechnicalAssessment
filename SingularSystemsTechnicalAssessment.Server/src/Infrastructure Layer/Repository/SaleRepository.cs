using Microsoft.EntityFrameworkCore;
using SingularSystemsTechnicalAssessment.Server.Application_Layer.Interfaces.Repositories;
using SingularSystemsTechnicalAssessment.Server.Domain_Layer.Entities;

namespace SingularSystemsTechnicalAssessment.Server.Infrastructure_Layer.Repository
{
    public class SaleRepository : Repository<Sale>, ISaleRepository
    {
        public SaleRepository(AssessmentDbContext context) : base(context) { }

    
        public override async Task<IEnumerable<Sale>> GetAllAsync()
        {
            return await _dbSet
                .Include(s => s.Product)
                .ToListAsync();
        }

     
        public override async Task<Sale?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(s => s.Product)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

   
        public async Task<IEnumerable<Sale>> GetFilteredAsync(
            int? productId, DateTime? startDate, DateTime? endDate,
            int page, int pageSize)
        {
            var query = _dbSet
                .Include(s => s.Product) 
                .AsQueryable();

            if (productId != null)
                query = query.Where(s => s.ProductId == productId);

            if (startDate != null)
                query = query.Where(s => s.SaleDate >= startDate);

            if (endDate != null)
                query = query.Where(s => s.SaleDate <= endDate);

            return await query
                .OrderByDescending(s => s.SaleDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
