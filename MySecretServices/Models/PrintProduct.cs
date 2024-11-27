using System.ComponentModel.DataAnnotations;

namespace MySecretServices.Models
{
	public class PrintProduct
	{
		[Required]
		public string Id { get; set; }

		[Required]
		public double Price { get; set; }

		[Required]
		public string Name { get; set; }

		[Required]
		public string Barcode { get; set; }

		[Required]
		public int Copy { get; set; }

		[Required]
		public string Path { get; set; }
	}
}
