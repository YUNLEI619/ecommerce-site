using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ShoppingCart_T8.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }
        [Required, MinLength(3, ErrorMessage = "Enter minimally {1} characters for {0} please.")]
        public string ProductName { get; set; } = string.Empty;
        public string ProductDescription { get; set; } = string.Empty;
        public string ProductImage { get; set; } = "/images/NO_IMAGE.png";
        [Required, Range(0.01, double.MaxValue)]
        [Column(TypeName = "decimal(8,2)")]
        public decimal ProductPrice { get; set; }
        public string? ProductStatus { get; set; } //usage such as Deleted (not removal from database), disable Adding to Cart etc, in case the CA scope increases

        //Navigational Properties
        public virtual ICollection<Review> Product_Reviews { get; set; } //A Product may be reviewed by Zero or More Customer

        public virtual ICollection<CartItem> Product_CartItems { get; set; } //1 or more Customer may add same Product to each of their Cart
    }
}
