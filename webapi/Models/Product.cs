using System.ComponentModel.DataAnnotations;

namespace webapi.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
   
        public string Name { get; set; }

        [Required(ErrorMessage = "Price is required.")]
      
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Description is required.")]

        public string Description { get; set; }

        [Required(ErrorMessage = "Product Image is required.")]
        public string ImageUrl { get; set; }

        public int RegisteredId { get; set; }
    }
}
