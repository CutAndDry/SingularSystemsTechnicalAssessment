namespace SingularSystemsTechnicalAssessment.Server.Infrastructure_Layer.Models
{
    public class Product
    {
        public int Id { get; set; }           
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }

   
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }
}
