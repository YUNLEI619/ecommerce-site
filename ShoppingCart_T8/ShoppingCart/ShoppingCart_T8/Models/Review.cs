using System.ComponentModel.DataAnnotations;

namespace ShoppingCart_T8.Models
{
    public class Review // to manage the many to many relationship on reviews between Customer and Product entities
    {
        //Properties
        [Required, Range(0, 5)] //0 means Not Reviewed, assumed to be not part of review score averages
        public int ReviewScore { get; set; } = 0; //average will be derived

        //Composite Keys
        public int CustomerId_FK { get; set; }
        public int ProductId_FK { get; set; } //for Group By with Average function on ReviewScore

        //Navigational Properties
        public virtual Customer Review_Customer { get; set; }
        public virtual Product Review_Product { get; set; }
    }
}
