using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using SingularSystemsTechnicalAssessment.Server.Domain_Layer.Entities;
using SingularSystemsTechnicalAssessment.Server.Infrastructure_Layer;

namespace SingularSystemsTechnicalAssessment.Server.src.Infrastructure_Layer
{
    public class SeedDataService
    {
        private readonly HttpClient _http;
        private readonly AssessmentDbContext _db;
        private readonly ILogger<SeedDataService> _logger;
        private readonly IHostEnvironment _env;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        private const string ProductsEndpoint = "https://singularsystems-tech-assessment-sales-api2.azurewebsites.net/products";
        private const string SalesEndpoint = "https://singularsystems-tech-assessment-sales-api2.azurewebsites.net/product-sales";

        public DateTime? LastSeededAt { get; private set; }
        public int LastProductCount { get; private set; }
        public int LastSaleCount { get; private set; }

        public SeedDataService(HttpClient http, AssessmentDbContext db, ILogger<SeedDataService> logger, IHostEnvironment env)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        // Public method to seed both products and sales into the provided in-memory DB.
        public async Task SeedAsync()
        {
            _logger.LogInformation("Starting external seed process...");

            var products = await FetchProductsWithRetryAsync();
            var sales = new List<Sale>();

            // Fetch sales for each product
            if (products.Any())
            {
                foreach (var product in products)
                {
                    var productSales = await FetchSalesForProductWithRetryAsync(product.Id);
                    sales.AddRange(productSales);
                }
            }

            // Only add if not already present to avoid duplicates on restart.
            if (products.Any() && !_db.Products.Any())
            {
                _db.Products.AddRange(products);
                await _db.SaveChangesAsync();
                _logger.LogInformation("Saved {Count} products to DB", products.Count);
            }

            if (sales.Any() && !_db.Sales.Any())
            {
                _db.Sales.AddRange(sales);
                await _db.SaveChangesAsync();
                _logger.LogInformation("Saved {Count} sales to DB", sales.Count);
            }

            LastSeededAt = DateTime.UtcNow;
            LastProductCount = _db.Products.Count();
            LastSaleCount = _db.Sales.Count();

            _logger.LogInformation("Seeding complete. Products: {P}, Sales: {S}", LastProductCount, LastSaleCount);
        }

        private async Task<List<Product>> FetchProductsWithRetryAsync(int maxAttempts = 3)
        {
            int attempt = 0;
            Exception? lastEx = null;

            while (attempt < maxAttempts)
            {
                attempt++;
                try
                {
                    var resp = await _http.GetAsync(ProductsEndpoint);
                    if (!resp.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Products endpoint returned {Status}. Attempt {Attempt}", resp.StatusCode, attempt);
                        lastEx = new Exception($"Status {resp.StatusCode}");
                    }
                    else
                    {
                        var json = await resp.Content.ReadAsStringAsync();
                        var ext = JsonSerializer.Deserialize<List<ExternalProduct>>(json, _jsonOptions) ?? new List<ExternalProduct>();
                        var mapped = ext.Select(p => new Product
                        {
                            Id = p.id,
                            Description = p.description,
                            SalePrice = Convert.ToDecimal(p.salePrice),
                            Category = p.category,
                            Image = p.image
                        }).ToList();

                        return mapped;
                    }
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                    _logger.LogWarning(ex, "Error fetching products attempt {Attempt}", attempt);
                }

                // simple backoff
                await Task.Delay(500 * attempt);
            }

            _logger.LogWarning(lastEx, "Failed to fetch products after {Attempts} attempts", maxAttempts);

            if (_env.IsDevelopment())
            {
                _logger.LogInformation("Environment is Development; attempting local fallback for products.");
                try
                {
                    var local = LoadProductsFromLocalFile();
                    if (local != null && local.Any())
                    {
                        _logger.LogInformation("Loaded {Count} products from local fallback file.", local.Count);
                        return local;
                    }
                    else
                    {
                        _logger.LogWarning("No local fallback products found. Returning empty list.");
                        return new List<Product>();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading local fallback products; returning empty list.");
                    return new List<Product>();
                }
            }
            else
            {
                _logger.LogWarning("Not in Development environment; skipping local fallback for products and returning empty list.");
                return new List<Product>();
            }
        }

        private async Task<List<Sale>> FetchSalesForProductWithRetryAsync(int productId, int maxAttempts = 3)
        {
            int attempt = 0;
            Exception? lastEx = null;

            while (attempt < maxAttempts)
            {
                attempt++;
                try
                {
                    var url = $"{SalesEndpoint}?Id={productId}";
                    var resp = await _http.GetAsync(url);
                    if (!resp.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Sales endpoint returned {Status} for product {ProductId}. Attempt {Attempt}", resp.StatusCode, productId, attempt);
                        lastEx = new Exception($"Status {resp.StatusCode}");
                    }
                    else
                    {
                        var json = await resp.Content.ReadAsStringAsync();
                        var ext = JsonSerializer.Deserialize<List<ExternalSale>>(json, _jsonOptions) ?? new List<ExternalSale>();

                        var list = new List<Sale>();
                        foreach (var s in ext)
                        {
                            DateTime parsed = DateTime.UtcNow;
                            if (!string.IsNullOrWhiteSpace(s.saleDate))
                            {
                                DateTime.TryParse(s.saleDate, null, System.Globalization.DateTimeStyles.RoundtripKind, out parsed);
                            }

                            var sale = new Sale
                            {
                                Id = s.saleId,
                                ProductId = s.productId,
                                SaleQty = s.saleQty,
                                SalePrice = Convert.ToDecimal(s.salePrice),
                                SaleDate = parsed
                            };
                            list.Add(sale);
                        }

                        return list;
                    }
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                    _logger.LogWarning(ex, "Error fetching sales for product {ProductId} attempt {Attempt}", productId, attempt);
                }

                await Task.Delay(500 * attempt);
            }

            _logger.LogWarning(lastEx, "Failed to fetch sales for product {ProductId} after {Attempts} attempts", productId, maxAttempts);

            if (_env.IsDevelopment())
            {
                _logger.LogInformation("Environment is Development; attempting local fallback for sales.");
                try
                {
                    var local = LoadSalesFromLocalFile();
                    if (local != null && local.Any())
                    {
                        _logger.LogInformation("Loaded {Count} sales from local fallback file.", local.Count);
                        return local;
                    }
                    else
                    {
                        _logger.LogWarning("No local fallback sales found. Returning empty list.");
                        return new List<Sale>();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading local fallback sales; returning empty list.");
                    return new List<Sale>();
                }
            }
            else
            {
                _logger.LogWarning("Not in Development environment; skipping local fallback for sales and returning empty list.");
                return new List<Sale>();
            }
        }

        private async Task<List<Sale>> FetchSalesWithRetryAsync(int maxAttempts = 3)
        {
            int attempt = 0;
            Exception? lastEx = null;

            while (attempt < maxAttempts)
            {
                attempt++;
                try
                {
                    var resp = await _http.GetAsync(SalesEndpoint);
                    if (!resp.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Sales endpoint returned {Status}. Attempt {Attempt}", resp.StatusCode, attempt);
                        lastEx = new Exception($"Status {resp.StatusCode}");
                    }
                    else
                    {
                        var json = await resp.Content.ReadAsStringAsync();
                        var ext = JsonSerializer.Deserialize<List<ExternalSale>>(json, _jsonOptions) ?? new List<ExternalSale>();

                        var list = new List<Sale>();
                        foreach (var s in ext)
                        {
                            DateTime parsed = DateTime.UtcNow;
                            if (!string.IsNullOrWhiteSpace(s.saleDate))
                            {
                                DateTime.TryParse(s.saleDate, null, System.Globalization.DateTimeStyles.RoundtripKind, out parsed);
                            }

                            var sale = new Sale
                            {
                                Id = s.saleId,
                                ProductId = s.productId,
                                SaleQty = s.saleQty,
                                SalePrice = Convert.ToDecimal(s.salePrice),
                                SaleDate = parsed
                            };
                            list.Add(sale);
                        }

                        return list;
                    }
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                    _logger.LogWarning(ex, "Error fetching sales attempt {Attempt}", attempt);
                }

                await Task.Delay(500 * attempt);
            }

            _logger.LogWarning(lastEx, "Failed to fetch sales after {Attempts} attempts", maxAttempts);

            if (_env.IsDevelopment())
            {
                _logger.LogInformation("Environment is Development; attempting local fallback for sales.");
                try
                {
                    var local = LoadSalesFromLocalFile();
                    if (local != null && local.Any())
                    {
                        _logger.LogInformation("Loaded {Count} sales from local fallback file.", local.Count);
                        return local;
                    }
                    else
                    {
                        _logger.LogWarning("No local fallback sales found. Returning empty list.");
                        return new List<Sale>();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading local fallback sales; returning empty list.");
                    return new List<Sale>();
                }
            }
            else
            {
                _logger.LogWarning("Not in Development environment; skipping local fallback for sales and returning empty list.");
                return new List<Sale>();
            }
        }

        private List<Product>? LoadProductsFromLocalFile()
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), "SeedData", "products.json");
            if (!File.Exists(file)) return null;

            var json = File.ReadAllText(file);
            var ext = JsonSerializer.Deserialize<List<ExternalProduct>>(json, _jsonOptions) ?? new List<ExternalProduct>();
            var mapped = ext.Select(p => new Product
            {
                Id = p.id,
                Description = p.description,
                SalePrice = Convert.ToDecimal(p.salePrice),
                Category = p.category,
                Image = p.image
            }).ToList();

            return mapped;
        }

        private List<Sale>? LoadSalesFromLocalFile()
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(), "SeedData", "product-sales.json");
            if (!File.Exists(file)) return null;

            var json = File.ReadAllText(file);
            var ext = JsonSerializer.Deserialize<List<ExternalSale>>(json, _jsonOptions) ?? new List<ExternalSale>();
            var list = new List<Sale>();
            foreach (var s in ext)
            {
                DateTime parsed = DateTime.UtcNow;
                if (!string.IsNullOrWhiteSpace(s.saleDate))
                {
                    DateTime.TryParse(s.saleDate, null, System.Globalization.DateTimeStyles.RoundtripKind, out parsed);
                }

                var sale = new Sale
                {
                    Id = s.saleId,
                    ProductId = s.productId,
                    SaleQty = s.saleQty,
                    SalePrice = Convert.ToDecimal(s.salePrice),
                    SaleDate = parsed
                };
                list.Add(sale);
            }

            return list;
        }

        private class ExternalProduct
        {
            public int id { get; set; }
            public string? description { get; set; }
            public decimal salePrice { get; set; }
            public string? category { get; set; }
            public string? image { get; set; }
        }

        private class ExternalSale
        {
            public int saleId { get; set; }
            public int productId { get; set; }
            public decimal salePrice { get; set; }
            public int saleQty { get; set; }
            public string? saleDate { get; set; }
        }
    }
}
