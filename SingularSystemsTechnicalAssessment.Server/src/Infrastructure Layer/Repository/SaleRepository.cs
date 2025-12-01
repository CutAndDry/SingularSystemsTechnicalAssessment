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

        /// <summary>
        /// Gets filtered sales with pagination metadata using a single database query for the count.
        /// </summary>
        public async Task<(IEnumerable<Sale> items, int totalCount)> GetFilteredWithCountAsync(
            int? productId, DateTime? startDate, DateTime? endDate,
            int page, int pageSize)
        {
            var baseQuery = _dbSet.AsQueryable();

            if (productId != null)
                baseQuery = baseQuery.Where(s => s.ProductId == productId);

            if (startDate != null)
                baseQuery = baseQuery.Where(s => s.SaleDate >= startDate);

            if (endDate != null)
                baseQuery = baseQuery.Where(s => s.SaleDate <= endDate);

            // Get total count at database level
            var totalCount = await baseQuery.CountAsync();

            // Get paginated items with Product included
            var items = await baseQuery
                .Include(s => s.Product)
                .OrderByDescending(s => s.SaleDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
