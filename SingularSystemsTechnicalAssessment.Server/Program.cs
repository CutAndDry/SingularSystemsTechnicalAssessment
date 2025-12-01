using Microsoft.EntityFrameworkCore;
using SingularSystemsTechnicalAssessment.Server.Application_Layer.Interfaces.Repositories;
using SingularSystemsTechnicalAssessment.Server.Infrastructure_Layer;
using SingularSystemsTechnicalAssessment.Server.Infrastructure_Layer.Repository;
using SingularSystemsTechnicalAssessment.Server.src.Infrastructure_Layer;
using SingularSystemsTechnicalAssessment.Server.src.Presentation_Layer;
using System;
using System.Security.Authentication;
using System.Net.Http.Headers;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AssessmentDbContext>(options =>
    options.UseInMemoryDatabase("AssessmentDb"));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ISaleRepository, SaleRepository>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("VueCors", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173", "https://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Register SeedDataService HttpClient. In Development allow insecure certs to tolerate TLS/cert issues from external API.
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpClient<SeedDataService>(client =>
    {
        // Some test endpoints expect text/plain in Accept (matches the curl command). Also accept JSON.
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    })
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        // Dev-only: force TLS 1.2/1.3 and accept any server certificate (useful for local/legacy endpoints during development)
        var handler = new HttpClientHandler
        {
            SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        // If a proxy is configured via SEED_PROXY or HTTP_PROXY, use it for the seeder.
        var proxyEnv = Environment.GetEnvironmentVariable("SEED_PROXY") ?? Environment.GetEnvironmentVariable("HTTP_PROXY");
        if (!string.IsNullOrWhiteSpace(proxyEnv))
        {
            try
            {
                var proxyUri = new Uri(proxyEnv);
                handler.Proxy = new WebProxy(proxyUri);
                handler.UseProxy = true;
            }
            catch (Exception ex)
            {
                // If proxy parsing fails, log to console — don't throw here as this is dev convenience only.
                Console.WriteLine($"Warning: invalid SEED_PROXY value '{proxyEnv}': {ex.Message}");
            }
        }

        return handler;
    });
}
else
{
    builder.Services.AddHttpClient<SeedDataService>();
}
builder.Services.AddScoped<SeedDataService>();

var app = builder.Build();

    // Start background seeding so the app will seed on boot. When external fetch fails the seeder will use the local fallback files.
        var provider = app.Services;
        _ = Task.Run(async () =>
        {
            using var scope = provider.CreateScope();
            var seedDataService = scope.ServiceProvider.GetRequiredService<SeedDataService>();
            try
            {
                await seedDataService.SeedAsync();
            }
            catch (Exception ex)
            {
                app.Logger.LogWarning(ex, "Background seeding failed (this should be rare because fallback is available).");
            }
        });

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors("VueCors");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("/index.html");

app.Run();
