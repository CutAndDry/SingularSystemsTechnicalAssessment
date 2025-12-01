namespace SingularSystemsTechnicalAssessment.Server.Domain_Layer.Entities
{
    public class Sale
    {
        public int Id { get; set; }               
        public int ProductId { get; set; }         
        public DateTime SaleDate { get; set; }
        public int SaleQty { get; set; }
        public decimal SalePrice { get; set; }     
   

     
        public Product Product { get; set; } = null!;
    }
}
