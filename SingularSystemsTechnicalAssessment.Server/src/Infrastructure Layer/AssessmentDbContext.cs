using Microsoft.EntityFrameworkCore;
using SingularSystemsTechnicalAssessment.Server.Domain_Layer.Entities;
using System;

namespace SingularSystemsTechnicalAssessment.Server.Infrastructure_Layer
{
    public class AssessmentDbContext : DbContext
    {
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Sale> Sales => Set<Sale>();

        public AssessmentDbContext(DbContextOptions<AssessmentDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .HasMany(p => p.Sales)
                .WithOne(s => s.Product)
                .HasForeignKey(s => s.ProductId);
        }

    }
}
