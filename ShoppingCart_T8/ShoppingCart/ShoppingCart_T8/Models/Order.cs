using System.ComponentModel.DataAnnotations;

namespace ShoppingCart_T8.Models
{
    public class Order
    {
        //Properties
        [Key]
        public int OrderId { get; set; }
        public DateTime OrderPurchasedDateTime { get; set; } // normalised OrderPurchasedDateTime from OrderItem, for 'single source of truth'

        //Model Relationships
        public int CustomerId_FK { get; set; }

        //Navigational Properties
        public virtual Customer Order_Customer { get; set; }
        public virtual ICollection<OrderItem> Order_OrderItems { get; }
    }
}
