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

// Changed: compute allowedOrigins once so we can reuse and log it after build
var allowedOrigins = builder.Configuration["ALLOWED_ORIGINS"]?.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? new[] { "http://localhost:5173", "https://localhost:5173" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("VueCors", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
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

// New: register hosted background seeder (runs at startup, uses SeedDataService)
builder.Services.AddHostedService<SeedDataBackgroundService>();

// New: health checks for readiness/basic liveness
builder.Services.AddHealthChecks();

var app = builder.Build();

// Log configured CORS origins at startup
app.Logger.LogInformation("Configured CORS allowed origins: {Origins}", string.Join(", ", allowedOrigins));

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // production security: HSTS
    app.UseHsts();
}

// Ensure redirection to HTTPS early in the pipeline
app.UseHttpsRedirection();

// New: add a small security headers middleware before static files
app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors("VueCors");
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("/index.html");

// New: health endpoint
app.MapHealthChecks("/health");

app.Run();
