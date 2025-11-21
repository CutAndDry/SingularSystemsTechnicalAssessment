using Microsoft.EntityFrameworkCore;
using SingularSystemsTechnicalAssessment.Server.Application_Layer.Interfaces.Repositories;
using SingularSystemsTechnicalAssessment.Server.Infrastructure_Layer;
using SingularSystemsTechnicalAssessment.Server.Infrastructure_Layer.Repository;
using SingularSystemsTechnicalAssessment.Server.src.Infrastructure_Layer;
using SingularSystemsTechnicalAssessment.Server.src.Presentation_Layer;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.



builder.Services.AddDbContext<AssessmentDbContext>(options =>
    options.UseInMemoryDatabase("ProductsDb"));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ISaleRepository, SaleRepository>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("VueCors", policy =>
    {
        policy
            .WithOrigins("https://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseDefaultFiles();
app.UseStaticFiles();

//Add DataToInMemDB
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AssessmentDbContext>();
    DefaultDBData.AddDefaultData(db);
}


app.UseCors("VueCors");


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
