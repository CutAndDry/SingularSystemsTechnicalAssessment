using Microsoft.AspNetCore.Mvc;
using SingularSystemsTechnicalAssessment.Server.src.Infrastructure_Layer;

namespace SingularSystemsTechnicalAssessment.Server.src.Presentation_Layer
{
    [ApiController]
    [Route("api/seed")]
    public class SeedController : ControllerBase
    {
        private readonly SeedDataService _seedService;
        private readonly ILogger<SeedController> _logger;

        public SeedController(SeedDataService seedService, ILogger<SeedController> logger)
        {
            _seedService = seedService;
            _logger = logger;
        }

        [HttpPost("run")]
        public async Task<IActionResult> Run()
        {
            try
            {
                await _seedService.SeedAsync();
                return Ok(new { success = true, message = "Seed completed" });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Manual seeding failed");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
