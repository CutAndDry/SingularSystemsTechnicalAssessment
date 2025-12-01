using System.ComponentModel.DataAnnotations;

namespace SingularSystemsTechnicalAssessment.Server.src.Application_Layer.DTO_s
{
  

    public class SaleListDto
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = null!;
        public int SaleQty { get; set; }
        public decimal SalePrice { get; set; }
        public DateTime SaleDate { get; set; }
    }

    public class SaleDetailDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public DateTime SaleDate { get; set; }
    }


    public class SaleCreateDto
    {
        [Required(ErrorMessage = "Product ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Product ID must be a valid positive integer")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Sale quantity is required")]
        [Range(1, 100000, ErrorMessage = "Sale quantity must be between 1 and 100000")]
        public int SaleQty { get; set; }

        [Range(0.01, 999999.99, ErrorMessage = "Sale price must be between 0.01 and 999999.99")]
        public decimal? SalePrice { get; set; }
    }

    public class SaleUpdateDto
    {
        [Required(ErrorMessage = "Product ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Product ID must be a valid positive integer")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Sale quantity is required")]
        [Range(1, 100000, ErrorMessage = "Sale quantity must be between 1 and 100000")]
        public int SaleQty { get; set; }

        [Required(ErrorMessage = "Sale price is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Sale price must be between 0.01 and 999999.99")]
        public decimal SalePrice { get; set; }

        [Required(ErrorMessage = "Sale date is required")]
        public DateTime SaleDate { get; set; }
    }

    public class SalesPagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }


}
