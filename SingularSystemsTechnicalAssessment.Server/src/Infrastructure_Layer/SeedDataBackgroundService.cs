using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SingularSystemsTechnicalAssessment.Server.src.Infrastructure_Layer
{
    public class SeedDataBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<SeedDataBackgroundService> _logger;

        public SeedDataBackgroundService(IServiceProvider provider, ILogger<SeedDataBackgroundService> logger)
        {
            _provider = provider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Run once at startup and respect shutdown token.
            try
            {
                using var scope = _provider.CreateScope();
                var seeder = scope.ServiceProvider.GetRequiredService<SeedDataService>();
                if (seeder == null)
                {
                    _logger.LogWarning("SeedDataService not registered; skipping seeding.");
                    return;
                }

                // Call existing SeedAsync() and wait with cancellation. If SeedAsync accepts a CancellationToken,
                // you can update this call, otherwise WaitAsync will allow cancellation while waiting.
                var seedTask = seeder.SeedAsync();
                await seedTask.WaitAsync(stoppingToken);

                _logger.LogInformation("Seed data completed successfully.");
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Seed data cancelled due to shutdown.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Background seeding failed (fallbacks should apply).");
            }
        }
    }
}
