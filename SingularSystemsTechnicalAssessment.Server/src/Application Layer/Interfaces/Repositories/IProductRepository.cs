using SingularSystemsTechnicalAssessment.Server.Infrastructure_Layer.Models;
using SingularSystemsTechnicalAssessment.Server.Infrastructure_Layer.Repository;

namespace SingularSystemsTechnicalAssessment.Server.Application_Layer.Interfaces.Repositories
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<Product?> GetProductWithSalesAsync(int productId);
    }
}
