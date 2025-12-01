namespace SingularSystemsTechnicalAssessment.Server.Domain_Layer.Entities
{
    public class Product
    {
        public int Id { get; set; }           
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? Category { get; set; }
        public string? Image { get; set; }

        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }
}
