using Microsoft.AspNetCore.Mvc;
using SingularSystemsTechnicalAssessment.Server.Domain_Layer.Entities;
using SingularSystemsTechnicalAssessment.Server.Infrastructure_Layer;
using System;

namespace SingularSystemsTechnicalAssessment.Server.src.Infrastructure_Layer
{
    public static class DefaultDBData
    {
        // No-op: seeding moved to SeedDataService which fetches external API data.
        public static Task AddDefaultData(AssessmentDbContext context, SeedDataService seedDataService)
        {
            return Task.CompletedTask;
        }
    }
}
