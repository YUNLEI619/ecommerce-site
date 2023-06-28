namespace ShoppingCart_T8.Models
{
    public class OrderItemCode
    {
        //public string OrderItemCodeStatus { get; set; } //usage such as 'FRESH', 'ACTIVATED', 'EXPIRED' etc, not in CA Scope

        //Composite Keys
        public Guid OrderItem_ActivationCode { get; set; }
        public int OrderItemCodeCustomerId { get; set; } //Not a true foreign key to Customer Table

        //Model Relationships
        public int OrderItemId_FK { get; set; }

        //Navigational Properties
        public virtual OrderItem OrderItemCode_OrderItem { get; set; }
    }
}
