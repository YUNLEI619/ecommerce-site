using System.ComponentModel.DataAnnotations;

namespace ShoppingCart_T8.Models
{
    public class CartItem //for CA scope directions, to avoid multi-value group strings
    {
        [Required, Range(1, int.MaxValue)] //no negatives, 0 included for special corner case handling
        public int CartItemQuantity { get; set; }

        //Composite Keys
        public int ProductId_FK { get; set; }
        public int CustomerId_FK { get; set; }

        //Navigational Properties
        public virtual Customer CartItem_Customer { get; set; }
        public virtual Product CartItem_Product { get; set; }
    }
}
