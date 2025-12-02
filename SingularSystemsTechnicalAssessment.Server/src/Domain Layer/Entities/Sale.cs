using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SingularSystemsTechnicalAssessment.Server.Domain_Layer.Entities
{
	

	public class Sale
	{
		public int Id { get; set; }
		public int ProductId { get; set; }

		
		public int SaleQty { get; set; }
		public decimal SalePrice { get; set; }
		public DateTime SaleDate { get; set; }

		// avoid EF column conflicts
		[NotMapped]
		public DateTime Date
		{
			get => SaleDate;
			set => SaleDate = value;
		}

		[NotMapped]
		public decimal Amount
		{
			get => SalePrice;
			set => SalePrice = value;
		}

		public Product? Product { get; set; }
	}
}
