using System.ComponentModel.DataAnnotations;

namespace ShoppingCart_T8.Models
{
    public class LogSession
    {
        //Properties
        [Key]
        public long Id { get; set; }
        public string SessionId { get; set; }
        public DateTime LogSessionCookieStart { get; set; }
        public DateTime LogSessionCookieExpiry { get; set; }

        //Model Relationships
        public int CustomerId_FK { get; set; } //each LogSession record refers to a Customer

        //Navigational Properties
        public virtual Customer LogSession_Customer { get; set; }
    }
}
