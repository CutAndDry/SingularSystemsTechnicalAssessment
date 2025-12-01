using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SingularSystemsTechnicalAssessment.Server.Domain_Layer.Entities
{
	// Minimal Sale entity to satisfy Product navigation property.
	// Remove or expand this if you already have a full Sale entity.
	public class Sale
	{
		public int Id { get; set; }
		public int ProductId { get; set; }

		// New property names expected by SaleController
		public int SaleQty { get; set; }
		public decimal SalePrice { get; set; }
		public DateTime SaleDate { get; set; }

		// Backward-compatible aliases (not mapped to avoid EF column conflicts)
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
