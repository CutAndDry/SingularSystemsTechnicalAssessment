using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SingularSystemsTechnicalAssessment.Server.src.Application_Layer.DTO_s
{


    public class ProductListDto
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public decimal SalePrice { get; set; }
        public string? Category { get; set; }
        public string? Image { get; set; }

        public int TotalSales { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class ProductDetailDto
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public decimal SalePrice { get; set; }
        public string? Category { get; set; }
        public string? Image { get; set; }

        public int TotalSales { get; set; }
        public decimal TotalRevenue { get; set; }

        public List<SaleDetailDto> Sales { get; set; } = new();
    }

    public class ProductCreateDto
    {
        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Description must be between 1 and 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Sale price is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Sale price must be between 0.01 and 999999.99")]
        public decimal SalePrice { get; set; }

        [StringLength(100, ErrorMessage = "Category must not exceed 100 characters")]
        public string? Category { get; set; }

        [StringLength(100000, ErrorMessage = "Image data exceeds maximum size")]
        public string? Image { get; set; }
    }

    public class ProductCreateFormDto
    {
        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Description must be between 1 and 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Sale price is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Sale price must be between 0.01 and 999999.99")]
        public decimal SalePrice { get; set; }

        [StringLength(100, ErrorMessage = "Category must not exceed 100 characters")]
        public string? Category { get; set; }

        [FileExtensions(Extensions = "jpg,jpeg,png,gif,webp", ErrorMessage = "Only image files are allowed")]
        public IFormFile? Image { get; set; }
    }

    public class ProductUpdateDto
    {
        [Required(ErrorMessage = "Description is required")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Description must be between 1 and 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Sale price is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Sale price must be between 0.01 and 999999.99")]
        public decimal SalePrice { get; set; }

        [StringLength(100, ErrorMessage = "Category must not exceed 100 characters")]
        public string? Category { get; set; }

        [StringLength(100000, ErrorMessage = "Image data exceeds maximum size")]
        public string? Image { get; set; }
    }


    public class ProductPagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public int TotalCount { get; set; }
        public int TotalPages { get; set; }

        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

}
