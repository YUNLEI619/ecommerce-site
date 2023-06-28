using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ShoppingCart_T8.Models
{
    public class OrderItem
    {
        //Properties
        [Key]
        public int OrderItemId { get; set; }
        public int OrderItemQuantity { get; set; } //determines number of Activation Codes to be generated for this purchased Product in this order

        //To save as an 'ex' reference from Product, because of price difference in ScreenShot 2, assumed promotional price considerations
        //Assumed requirement to seperate Product and OrderItem as 2 different entities, for Use Case where original Product changes not affect purchased items (i.e. OrderItem) of Customer
        public int OrderItemProductId { get; set; } //not a true foreign key to ProductId, but a mere reference to Product data, immutable string
        public string OrderItemName { get; set; } = string.Empty;
        public string OrderItemDescription { get; set; } = string.Empty;
        public string OrderItemImage { get; set; } = "/images/NO_IMAGE.png";
        [Required, Range(0.01, double.MaxValue)]
        [Column(TypeName = "decimal(8,2)")]
        public decimal OrderItemPrice { get; set; }
        public string? OrderItemtStatus { get; set; } //For various function manipulation such as disable Adding to Cart, Invisible settings etc, in case the CA scope increases

        //Model Relationships
        public int OrderId_FK { get; set; }

        //Navigational Properties
        public virtual Order OrderItem_Order { get; set; }

        public virtual ICollection<OrderItemCode> OrderItemCodes { get; set; }


    }
}
