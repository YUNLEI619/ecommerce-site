namespace ShoppingCart_T8.Models
{
    public class OrderItemViewModel //for View display at /Product/PurchaseHistory
    {
        public int VM_OrderItemId { get; set; }
        public int VM_OrderItemProductId { get; set; }
        public DateTime VM_OrderItemPurchasedDateTime { get; set; }
        public string VM_OrderItemName { get; set; }
        public string VM_OrderItemDescription { get; set; }
        public string VM_OrderItemImage { get; set; }
        public decimal VM_OrderItemPrice { get; set; }
        public int VM_OrderItemReviewScore { get; set; }
        public int VM_OrderItemQuantity { get; set; }
        public List<OrderItemCode> VM_OrderItem_ActivationCode { get; set; }

    }
}
