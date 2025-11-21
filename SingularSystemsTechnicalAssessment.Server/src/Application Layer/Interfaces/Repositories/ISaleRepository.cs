using SingularSystemsTechnicalAssessment.Server.Domain_Layer.Entities;
using SingularSystemsTechnicalAssessment.Server.Infrastructure_Layer.Repository;

namespace SingularSystemsTechnicalAssessment.Server.Application_Layer.Interfaces.Repositories
{
        public interface ISaleRepository : IRepository<Sale>
        {
        Task<IEnumerable<Sale>> GetAllAsync();
        Task<IEnumerable<Sale>> GetFilteredAsync(
                int? productId, DateTime? startDate, DateTime? endDate, int page, int pageSize);
        }
}
