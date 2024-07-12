using System.ComponentModel.DataAnnotations;

namespace MySecretServices.Models
{
	public class Settings
	{
		[Required]
        public string LabelTitle { get; set; }

        [Required]
        public string ReclameString { get; set; }

        [Required]
        public string ShopName { get; set; }
    }
}
