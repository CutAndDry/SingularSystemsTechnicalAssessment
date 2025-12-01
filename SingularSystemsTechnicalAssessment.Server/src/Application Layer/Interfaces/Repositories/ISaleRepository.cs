using SingularSystemsTechnicalAssessment.Server.Domain_Layer.Entities;
using SingularSystemsTechnicalAssessment.Server.Infrastructure_Layer.Repository;

namespace SingularSystemsTechnicalAssessment.Server.Application_Layer.Interfaces.Repositories
{
        public interface ISaleRepository : IRepository<Sale>
        {
        Task<IEnumerable<Sale>> GetFilteredAsync(
                int? productId, DateTime? startDate, DateTime? endDate, int page, int pageSize);
        Task<(IEnumerable<Sale> items, int totalCount)> GetFilteredWithCountAsync(
                int? productId, DateTime? startDate, DateTime? endDate, int page, int pageSize);
        Task<Sale?> GetByIdAsync(int id);
        Task AddAsync(Sale sale);
        Task SaveChangesAsync();
        void Update(Sale sale);
        void Delete(Sale sale);
        }
}
