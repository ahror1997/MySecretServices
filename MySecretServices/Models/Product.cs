using System.ComponentModel.DataAnnotations;

namespace MySecretServices.Models
{
	public class Product
	{
		[Required]
		public int Id { get; set; }

		[Required]
		public double Price { get; set; }

		[Required]
		public string Name { get; set; }

		[Required]
		public int GroupCode { get; set; }

		[Required]
		public int GoodType { get; set; }

		[Required]
		public int PLUNumber { get; set; }
	}
}