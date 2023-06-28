using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ShoppingCart_T8.Models
{
    public class Customer
    {
        //Login Details
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CustomerId { get; set; }
        [DisplayName("User Name")]
        [MinLength(3), Required]
        public string UserName { get; set; }
        [DataType(DataType.Password), Required, MinLength(4, ErrorMessage = "Minimum Length for {0} is {1}")]
        public string Password { get; set; }

        //Customer Particulars
        [DisplayName("Customer Name")]
        [Required]
        public string CustomerName { get; set; }
        public string CustomerStatus { get; set; } //for easier coding purpose, Login, Guest, Locked, Deactivated etc
        public Guid CustomerGuestCartId { get; set; } = new Guid(); //00000000-0000-0000-0000-000000000000
        // OPTIONAL: Id and cart in localstorage, if mismatch with guest after logged in, triggers Javascript to clear Guest selection
        // Originally intended to save Guest Cart as a string of items, but due to Professors' direction to avoid Multi-Group Values in database, removed such features.

        //NOTE: Other 'usual' business properties not included as not within this CA scope, e.g.
        //[Phone]
        //public string CustomerPhone { get; set; }
        //[EmailAddress]
        //public string CustomerEmail { get; set; }

        //Navigation properties
        public virtual ICollection<CartItem> Customer_CartItems { get; set; } //One Customer may add Zero or More Product to the Shopping Cart
        public virtual ICollection<Order> Customer_Orders { get; set; } //One Customer may add Zero or More Product to the Shopping Cart
        public virtual ICollection<Review> Customer_Reviews { get; set; } //A Customer may review Zero or More Product
        public virtual ICollection<LogUser> Customer_UserLogs { get; set; } //each Customer may have more than one entries in LogLogin entity, for extra learning purpose
        public virtual ICollection<LogSession> Customer_SessionLogs { get; set; } //each Customer may have more than one entries in LogSession entity
    }
}
