using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ShoppingCart_T8.Data;
using ShoppingCart_T8.Models;
using System.Text;

namespace ShoppingCart_T8.Controllers
{
    public class CartController : Controller
    {
        private readonly DataContext _data;
        private readonly ILogger<CartController> _logger;

        public CartController(DataContext data, ILogger<CartController> logger) //dependency injected
        {
            _data = data;
            _logger = logger;
        }

        //==================================================================================================================\\
        //==================================================================================================================\\

        [HttpGet]
        public async Task<IActionResult> CartView()
        {
            _logger.LogInformation($"[{DateTime.Now}] LOG_Information : at '/Product/CartView' starts...");
            Dictionary<string, CartItemViewModel> viewCartItems = new Dictionary<string, CartItemViewModel>(); //string key for uniqueness of cart item view
            IList<Product> products = await _data.TBL_Product.ToListAsync();
            if (!string.IsNullOrEmpty(Request.Cookies["COOKIE_SHOPPINGCART"])) //check cart item cookie not null or empty
            {
                //processing customer shopping cart items
                string[] cartParser = Request.Cookies["COOKIE_SHOPPINGCART"].Split(",");
                string[] str = new string[2];
                foreach (string item in cartParser)
                {
                    str = item.Split(':'); //str[0] = product id, str[1] = quantity

                    Product? product = products.Where(p => p.ProductId == Int32.Parse(str[0])).FirstOrDefault();
                    if (product != null && str[1] != "0") //(str[1] != "0") as cookies can be edited at client browser
                    {
                        if (viewCartItems.ContainsKey(str[0]))
                        {   //if similar product repeated then aggregate, in case client edit cookies
                            viewCartItems[str[0]].VM_CartItem_ProductQuantity += Int32.Parse(str[1]);
                        }
                        else
                        {   //Added for View
                            viewCartItems[str[0]] = (new CartItemViewModel
                            {
                                VM_CartItem_ProductId = product.ProductId,
                                VM_CartItem_ProducName = product.ProductName,
                                VM_CartItem_ProductDescription = product.ProductDescription,
                                VM_CartItem_ProductImage = product.ProductImage,
                                VM_CartItem_ProductPrice = product.ProductPrice,
                                VM_CartItem_ProductQuantity = Int32.Parse(str[1])
                            });
                        }
                    }
                }
                _logger.LogInformation($"[{DateTime.Now}] LOG_Information : at '/Product/CartView' completed, ViewModel updated.");
                return View(viewCartItems);
            }
            _logger.LogWarning($"[{DateTime.Now}] LOG_Warning : at '/Product/CartView' completed, cookie empty.");
            return View(viewCartItems); //would be null
        }

        //==================================================================================================================\\
        //==================================================================================================================\\

        [HttpPost]
        public async Task<IActionResult> Add(int ProductId, string? SessionId)
        {
            _logger.LogInformation($"[{DateTime.Now}] LOG_Information : at '/Cart/Add' starting...");
            Product? product = await _data.TBL_Product.FirstOrDefaultAsync(p => p.ProductId == ProductId);
            int currentCartItemTotal = 0;
            if (product == null) //Possible Database Error
            {
                _logger.LogError($"[{DateTime.Now}] LOG_Error : at '/Cart/Add' NO PRODUCT FOUND IN DB, currentCartItemTotal = 0.");
                return View("NotFound"); //a View in /Views/Shared folder to handle exceptions
            } 
            else //an existing Product found
            {
                int quantity = 0;
                if (string.IsNullOrEmpty(Request.Cookies["COOKIE_SHOPPINGCART"]))
                { Response.Cookies.Append("COOKIE_SHOPPINGCART", ProductId.ToString() + ":1"); } //initialized an empty shopping cart
                else 
                {   //updates Customer cart since cookie not null
                    string[] cartParser = Request.Cookies["COOKIE_SHOPPINGCART"].Split(",");
                    Dictionary<string, string> shoppingCartItem = new Dictionary<string, string>();
                    string[] str = new string[2];
                    foreach (string item in cartParser)
                    {
                        str = item.Split(':');
                        //if (str[1] != "0")
                        shoppingCartItem.Add(str[0], str[1]);
                    }
                    string p = ProductId.ToString(); // temp variable
                    if (shoppingCartItem.ContainsKey(p)) //if Product exists in current Shopping Cart then + 1
                    {
                        Int32.TryParse(shoppingCartItem[p], out quantity);
                        shoppingCartItem[p] = (Int32.Parse(shoppingCartItem[p]) + 1).ToString();
                    }
                    else { shoppingCartItem.Add(p, "1"); } //new product to shopping cart

                    StringBuilder builder = new StringBuilder();
                    int count = shoppingCartItem.Count; //temp variable
                    foreach (KeyValuePair<string, string> item in shoppingCartItem)
                    {
                        currentCartItemTotal += Int32.Parse(item.Value); //new shopping cart string
                        builder.Append(item.Key); builder.Append(":"); builder.Append(item.Value);
                        if (--count > 0) { builder.Append(","); }
                    }
                    Response.Cookies.Append("COOKIE_SHOPPINGCART", builder.ToString()); //updates cookie
                }

                //OPTIONAL: if visitor logged in then updates database
                _logger.LogInformation($"[{DateTime.Now}] LOG_Information : at '/Cart/Add', has session cookie, updating database...");
                string? sessionId = Request.Cookies["COOKIE_SESSION"];
                if (!String.IsNullOrEmpty(sessionId)) //session cookie not unavailable
                {

                    await using var transaction_CartAdd = await _data.Database.BeginTransactionAsync(); // Begin a transaction for data integrity
                    try
                    {
                        LogSession? validLogin = await _data.TBL_SessionLog.Where(s => s.SessionId == sessionId).FirstOrDefaultAsync();
                        if (validLogin != null) //session found
                        {
                            Customer? owner = await _data.TBL_Customer.Where(c => c.CustomerId == validLogin.CustomerId_FK).FirstOrDefaultAsync();
                            CartItem? cartitem = await _data.TBL_CartItem.Where(ci => ci.ProductId_FK == ProductId && ci.CustomerId_FK == owner.CustomerId).FirstOrDefaultAsync();
                            if (cartitem != null) //existing item for this customer cart
                            {
                                cartitem.CartItemQuantity = cartitem.CartItemQuantity + 1;
                                _data.TBL_CartItem.Update(cartitem);
                                await _data.SaveChangesAsync();
                            }
                            else //new item for this customer cart 
                            {
                                _data.TBL_CartItem.Add(new CartItem() { CartItemQuantity = 1, CustomerId_FK = owner.CustomerId, ProductId_FK = ProductId });
                                await _data.SaveChangesAsync();
                            }
                        }

                        await transaction_CartAdd.CommitAsync(); //ensures data integrity on Cart Update
                        _logger.LogInformation($"[{DateTime.Now}] LOG_Information : at '/Cart/Add', has session cookie, update database completed");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(DateTime.Now + " " + ex.Message + "\n\t" + ex.StackTrace);
                        _logger.LogError($"LOG_ERROR:  Failure Log Message from '/Cart/Add' action.");
                        await transaction_CartAdd.RollbackAsync(); //rollback on error
                        throw;
                    }
                }
            }
            _logger.LogInformation($"[{DateTime.Now}] LOG_Information : at '/Cart/Add' completed.");
            return Json(new { CartItemTotal = currentCartItemTotal });
        }

        //==================================================================================================================\\
        //==================================================================================================================\\

        [HttpPost]
        public async Task<IActionResult> UpdateCartCount() //for guest and customer
        {
            _logger.LogInformation($"[{DateTime.Now}] LOG_Information : at '/Cart/UpdateCartCount' starting...");
            int currentCartItemTotal = 0;

            //Because user can update from cookie direct to update cart - technical users consideration
            if (!string.IsNullOrEmpty(Request.Cookies["COOKIE_SHOPPINGCART"])) //resembles JSON format, Key Pair format, where key = <ProductId> : value = <Quantity>
            {
                string[] cartParser = Request.Cookies["COOKIE_SHOPPINGCART"].Split(",");
                string[] str = new string[2];
                foreach (string item in cartParser)
                {
                    str = item.Split(':');
                    if (str[1] != "0")
                    {
                        currentCartItemTotal += Int32.Parse(str[1]); //aggregating the quantity
                    }
                }
            }

            _logger.LogInformation($"[{DateTime.Now}] LOG_Information : at '/Cart/UpdateCartCount' completed");
            return Json(new { success = currentCartItemTotal }); //AJAX success attribute
        }

        //==================================================================================================================\\
        //==================================================================================================================\\

        [HttpPost]
        public async Task<IActionResult> UpdateCart(int ProductId, int quantity) //from CartView so quantity range is > 1
        {
            _logger.LogInformation($"[{DateTime.Now}] LOG_Information : at '/Cart/UpdateCart' starting...");
            Product? product = await _data.TBL_Product.FirstOrDefaultAsync(p => p.ProductId == ProductId);
            if (product == null) //Possible Database Error
            {
                _logger.LogError($"[{DateTime.Now}] LOG_Error : at '/Cart/UpdateCart' NO PRODUCT FOUND IN DB");
                return View("NotFound"); //a View in /Views/Shared folder to handle exceptions
            }
            else //an existing Product found
            {
                if (!string.IsNullOrEmpty(Request.Cookies["COOKIE_SHOPPINGCART"])) //resembles JSON format, Key Pair format, where key = <ProductId> : value = <Quantity>
                {
                    string[] cartParser = Request.Cookies["COOKIE_SHOPPINGCART"].Split(",");
                    Dictionary<string, string> shoppingCartItem = new Dictionary<string, string>();
                    string[] str = new string[2];
                    foreach (string item in cartParser)
                    {
                        str = item.Split(':');
                        if (str[1] != "0") //should not have 0, if so do not include such item. Customer can add again
                        {
                            shoppingCartItem.Add(str[0], str[1]);
                        }
                    }
                    string p = ProductId.ToString(); // temp variable
                    if (shoppingCartItem.ContainsKey(p)) //if Product exists in current Shopping Cart then updates quantity
                    {
                        _logger.LogCritical($"[{DateTime.Now}] LOG_Critical : at '/Cart/UpdateCart' Product [ {p} ] updated.");
                        shoppingCartItem[p] = (quantity).ToString();
                    }
                    StringBuilder builder = new StringBuilder();
                    int count = shoppingCartItem.Count; //temp variable
                    foreach (KeyValuePair<string, string> item in shoppingCartItem)
                    {
                        builder.Append(item.Key); builder.Append(":"); builder.Append(item.Value);
                        if (--count > 0) { builder.Append(","); }
                    }
                    Response.Cookies.Append("COOKIE_SHOPPINGCART", builder.ToString()); //updates cookie
                }

                //if logged in then updates database
                string? sessionId = Request.Cookies["COOKIE_SESSION"];
                if (!String.IsNullOrEmpty(sessionId)) //session cookie not unavailable
                {
                    try
                    {
                        LogSession? owner = await _data.TBL_SessionLog.Where(c => c.SessionId == sessionId).FirstOrDefaultAsync();
                        CartItem? cartitem = await _data.TBL_CartItem.Where(ci => ci.ProductId_FK == ProductId && ci.CustomerId_FK == owner.CustomerId_FK).FirstOrDefaultAsync();
                        if (cartitem != null) //existing item for this customer cart
                        {
                            cartitem.CartItemQuantity = quantity;
                            _data.TBL_CartItem.Update(cartitem);
                            await _data.SaveChangesAsync();
                        }
                        else //new item for this customer cart 
                        {
                            _data.TBL_CartItem.Add(new CartItem() { CartItemQuantity = quantity, CustomerId_FK = owner.CustomerId_FK, ProductId_FK = ProductId });
                            await _data.SaveChangesAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(DateTime.Now + " " + ex.Message + "\n\t" + ex.StackTrace);
                        _logger.LogError($"LOG_ERROR:  Failure Log Message from '/Cart/UpdateCartRemove' action, saving cartItem removal to database.");
                        throw;
                    }

                }
            }
            _logger.LogInformation($"[{DateTime.Now}] LOG_Information : at '/Cart/UpdateCart' completed");
            return Json(new { success = quantity }); //AJAX success attribute
        }

        //==================================================================================================================\\
        //==================================================================================================================\\

        [HttpPost]
        public async Task<IActionResult> UpdateCartRemove(int ProductId)
        {
            _logger.LogInformation($"[{DateTime.Now}] LOG_Information : at '/Cart/UpdateCartRemove' starting...");
            Product? product = await _data.TBL_Product.FirstOrDefaultAsync(p => p.ProductId == ProductId);
            if (product == null) //Possible Database Error
            {
                _logger.LogError($"[{DateTime.Now}] LOG_Error : at '/Cart/UpdateCartRemove' NO PRODUCT FOUND IN DB");
                return View("NotFound"); //a View in /Views/Shared folder to handle exceptions
            }
            else //an existing Product found
            {
                if (!string.IsNullOrEmpty(Request.Cookies["COOKIE_SHOPPINGCART"])) //check if cookie cart null else do nothing since already cleared
                {
                    string[] cartParser = Request.Cookies["COOKIE_SHOPPINGCART"].Split(",");
                    Dictionary<string, string> shoppingCartItem = new Dictionary<string, string>();
                    string[] str = new string[2];
                    foreach (string item in cartParser)
                    {
                        str = item.Split(':');
                        shoppingCartItem.Add(str[0], str[1]);
                    }
                    string p = ProductId.ToString(); // temp variable
                    if (shoppingCartItem.ContainsKey(p)) //if Product exists in current Shopping Cart then updates quantity
                    {
                        _logger.LogCritical($"[{DateTime.Now}] LOG_Critical : at '/Cart/UpdateCartRemove' Product [ {p} ] Remove from Cart");
                        shoppingCartItem.Remove(p);
                    }
                    StringBuilder builder = new StringBuilder();
                    int count = shoppingCartItem.Count; //temp variable
                    foreach (KeyValuePair<string, string> item in shoppingCartItem)
                    {
                        builder.Append(item.Key); builder.Append(":"); builder.Append(item.Value);
                        if (--count > 0) { builder.Append(","); }
                    }
                    Response.Cookies.Append("COOKIE_SHOPPINGCART", builder.ToString()); //updates cookie
                }

                //if logged in then updates database
                string? sessionId = Request.Cookies["COOKIE_SESSION"];
                if (!String.IsNullOrEmpty(sessionId)) //session cookie not unavailable
                {
                    try
                    {
                        LogSession? owner = await _data.TBL_SessionLog.Where(c => c.SessionId == sessionId).FirstOrDefaultAsync();
                        CartItem? cartitem = await _data.TBL_CartItem.Where(ci => ci.ProductId_FK == ProductId && ci.CustomerId_FK == owner.CustomerId_FK).FirstOrDefaultAsync();
                        if (cartitem != null) //existing item for this customer cart
                        {
                            _data.TBL_CartItem.Remove(cartitem);
                            await _data.SaveChangesAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(DateTime.Now + " " + ex.Message + "\n\t" + ex.StackTrace);
                        _logger.LogError($"LOG_ERROR:  Failure Log Message from '/Cart/UpdateCartRemove' action, saving cartItem removal to database.");
                        throw;
                    }

                }
            }
            _logger.LogInformation($"[{DateTime.Now}] LOG_Information : at '/Cart/UpdateCartRemove' completed");
            return Json(new { success = 0 }); //AJAX success attribute
        }
    }
}
