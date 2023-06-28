namespace ShoppingCart_T8.Models
{
    public class ProductViewModel //for /Product/Gallery display
    {
        public int VM_ProductId { get; set; }
        public string VM_ProductName { get; set; }
        public string VM_ProductDescription { get; set; }
        public string VM_ProductImage { get; set; }

        public decimal VM_ProductPrice { get; set; }
        public string? VM_ProductStatus { get; set; }

        public int VM_ProductStars { get; set; }

    }
}
