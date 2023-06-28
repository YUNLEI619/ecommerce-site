using Castle.Core.Resource;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ShoppingCart_T8.Data;
using ShoppingCart_T8.Models;

namespace ShoppingCart_T8.Controllers
{
    public class ProductController : Controller
    {
        private readonly DataContext _data;
        private readonly ILogger<ProductController> _logger;

        public ProductController(DataContext data, ILogger<ProductController> logger)
        {
            _data = data;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Gallery() //Display products to visitors, regardless of login status
        {
            _logger.LogInformation($"[{DateTime.Now}] LOG_Information : at '/Product/Gallery' starting...");

            //Guest user can still browse and add Product to ShoppingCart
            ViewBag.Visitor = string.IsNullOrEmpty(Request.Cookies["COOKIE_USER"]) ? "Guest" : Request.Cookies["COOKIE_USER"]; //for View's display purpose
            ViewBag.Session = string.IsNullOrEmpty(Request.Cookies["COOKIE_SESSION"]) ? null : Request.Cookies["COOKIE_SESSION"]; //for form hidden field at View
            ViewBag.Cart = Request.Cookies["COOKIE_SHOPPINGCART"];

            try
            {
                HashSet<Product> products = new HashSet<Product>();
                products = (await _data.TBL_Product.Where(p => p.ProductStatus == "O").Include(p => p.Product_Reviews).ToListAsync()).ToHashSet(); //ensuring unique entries with HashSet()
                if (products == null || products.Count == 0) { ViewBag.Msg_Error_ProductGallery = "No Product available in Gallery."; return View(null); }

                HashSet<ProductViewModel> vm_products = new HashSet<ProductViewModel>();
                //if (products != null || products.Count > 0)

                foreach (var product in products)
                {
                    vm_products.Add(new ProductViewModel()
                    {
                        VM_ProductId = product.ProductId,
                        VM_ProductName = product.ProductName,
                        VM_ProductDescription = product.ProductDescription,
                        VM_ProductImage = product.ProductImage,
                        VM_ProductPrice = product.ProductPrice,
                        VM_ProductStars = (int)Math.Round(product.Product_Reviews.DefaultIfEmpty().Average(r => r?.ReviewScore ?? 0))
                        //the Product_Reviews property can be null when there are no reviews for a product.
                        //Average() on the DefaultIfEmpty() throw NullReferenceException cuz Review object = null.
                        //Trick: if r is null, the ?. operator will return null, and the ?? operator will replace it with 0.
                    });
                }
                _logger.LogInformation($"[{DateTime.Now}] LOG_Information : at '/Product/GAllery' completed.");
                return View(vm_products);
            }
            catch (Exception ex)
            {
                _logger.LogError(DateTime.Now + " " + ex.Message + "\n\t" + ex.StackTrace);
                _logger.LogError($"LOG_ERROR:  Failure Log Message from '/Product/Gallery' action.");
                throw;
            }
        }

        //==================================================================================================================\\
        //==================================================================================================================\\

        [HttpPost]
        public async Task<IActionResult> ReviewRatings(int CustomerId, int ProductId, int Ratings)
        {
            _logger.LogInformation($"[{DateTime.Now}] LOG_Information : at '/Product/ReviewRatings' starting...");
            try
            {
                Review? review = await _data.TBL_Review.Where(r => r.CustomerId_FK == CustomerId && r.ProductId_FK == ProductId).FirstOrDefaultAsync();
                if (review == null)
                {   //if first time review by customer on this product, add to database
                    _data.TBL_Review.Add(new Review() { CustomerId_FK = CustomerId, ProductId_FK = ProductId, ReviewScore = Ratings });
                }
                else
                {   //update new score of previously reviewed products
                    review.ReviewScore = Ratings;
                    _data.TBL_Review.Update(review);
                }
                await _data.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(DateTime.Now + " " + ex.Message + "\n\t" + ex.StackTrace);
                _logger.LogError($"LOG_ERROR:  Failure Log Message from '/Product/ReviewRatings' action.");
                return Json(new { success = false });
                throw;
            }
            _logger.LogInformation($"[{DateTime.Now}] LOG_Information : at '/Product/ReviewRatings' completed.");
            return Json(new { success = true });
        }
    }
}
