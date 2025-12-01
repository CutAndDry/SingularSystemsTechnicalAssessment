namespace SingularSystemsTechnicalAssessment.Server.src.Application_Layer.DTO_s
{


    public class ProductListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public string? Category { get; set; }
        public string? Image { get; set; }

        public int TotalSales { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class ProductDetailDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? Category { get; set; }
        public string? Image { get; set; }

        public int TotalSales { get; set; }
        public decimal TotalRevenue { get; set; }

        public List<SaleDetailDto> Sales { get; set; } = new();
    }

    public class ProductCreateDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? Category { get; set; }
        public string? Image { get; set; }
    }
    public class ProductUpdateDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? Category { get; set; }
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
