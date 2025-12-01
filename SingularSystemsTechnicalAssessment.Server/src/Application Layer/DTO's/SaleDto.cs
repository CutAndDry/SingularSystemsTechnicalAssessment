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
        public int ProductId { get; set; }
        public int SaleQty { get; set; }
        public decimal? SalePrice { get; set; }
    }

    public class SaleUpdateDto
    {
        public int ProductId { get; set; }
        public int SaleQty { get; set; }
        public decimal SalePrice { get; set; }
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
