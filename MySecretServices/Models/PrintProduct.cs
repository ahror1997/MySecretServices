using System.ComponentModel.DataAnnotations;

namespace MySecretServices.Models
{
	public class PrintProduct
	{
		[Required]
		public string Id { get; set; }

		[Required]
		public string Price { get; set; }

		[Required]
		public string Name { get; set; }

		[Required]
		public string Barcode { get; set; }

		[Required]
		public int Copy { get; set; }
	}
}
