using System.ComponentModel.DataAnnotations;

namespace ShoppingCart_T8.Models
{
    public class LogUser
    {
        //Properties
        [Key]
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }

        public string Actions { get; set; } //can be "LOGIN", "LOGOUT", "PURCHASE MADE: "OrderId" or even ILogger content etc

        //Model Relationships
        public int CustomerId_FK { get; set; } //each LogLogin record refers to a Customer

        //Navigational Properties
        public virtual Customer LogUser_Customer { get; set; }
    }
}
