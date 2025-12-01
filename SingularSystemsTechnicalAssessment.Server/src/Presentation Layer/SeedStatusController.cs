using Microsoft.AspNetCore.Mvc;
using SingularSystemsTechnicalAssessment.Server.src.Infrastructure_Layer;
using SingularSystemsTechnicalAssessment.Server.Infrastructure_Layer;

namespace SingularSystemsTechnicalAssessment.Server.src.Presentation_Layer
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeedStatusController : ControllerBase
    {
        private readonly SeedDataService _seeder;
        private readonly AssessmentDbContext _db;

        public SeedStatusController(SeedDataService seeder, AssessmentDbContext db)
        {
            _seeder = seeder;
            _db = db;
        }

        // GET: api/seedstatus
        [HttpGet]
        public IActionResult GetStatus()
        {
            var status = new
            {
                lastSeededAt = _seeder.LastSeededAt,
                productsInDb = _db.Products.Count(),
                salesInDb = _db.Sales.Count(),
                lastProductCount = _seeder.LastProductCount,
                lastSaleCount = _seeder.LastSaleCount
            };

            return Ok(status);
        }
    }
}
