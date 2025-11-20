using SingularSystemsTechnicalAssessment.Server.Infrastructure_Layer.Models;
using SingularSystemsTechnicalAssessment.Server.Infrastructure_Layer.Repository;

namespace SingularSystemsTechnicalAssessment.Server.Application_Layer.Interfaces.Repositories
{
        public interface ISaleRepository : IRepository<Sale>
        {
            Task<IEnumerable<Sale>> GetFilteredAsync(
                int? productId, DateTime? startDate, DateTime? endDate, int page, int pageSize);
        }
}
