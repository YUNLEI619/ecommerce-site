namespace ShoppingCart_T8.Models
{
    public class CartItemViewModel //For CartView display generation 
    {
        public int VM_CartItem_ProductId { get; set; }

        public string VM_CartItem_ProducName { get; set; }

        public string VM_CartItem_ProductDescription { get; set; }

        public string VM_CartItem_ProductImage { get; set; }

        public decimal VM_CartItem_ProductPrice { get; set; }

        public int VM_CartItem_ProductQuantity { get; set; }
    }
}
