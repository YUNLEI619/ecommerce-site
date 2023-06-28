using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ShoppingCart_T8.Data;
using ShoppingCart_T8.Models;

namespace ShoppingCart_T8.Controllers
{
    public class OrderController : Controller
    {
        private readonly DataContext _data;
        private readonly ILogger<OrderController> _logger;

        public OrderController(DataContext data, ILogger<OrderController> logger) //dependency injected
        {
            _data = data;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> CheckOut()
        {
            _logger.LogInformation($"[{DateTime.Now}] LOG_Information : at '/Order/CheckOut' starting...");

            if (string.IsNullOrEmpty(Request.Cookies["COOKIE_SESSION"]))
            {
                TempData["REDIRECT"] = "/Cart/CartView"; //for redirection back
                return RedirectToAction("Login", "Customer"); 
            }
            string? loginValid = Request.Cookies["COOKIE_SESSION"];
            LogSession? validSession = await _data.TBL_SessionLog.Where(s => s.SessionId == loginValid).FirstOrDefaultAsync();
            if (validSession == null) { return RedirectToAction("Login", "Customer", new { returnUrl = "/Cart/Checkout" }); } //absence of valid session cookie
            if (validSession.LogSessionCookieExpiry < DateTime.Now) //can be DataTime.UtcNow depends on requirement
            {
                Response.Cookies.Delete("COOKIE_SESSION"); // removes invalid Session cookie from client
                Response.Cookies.Append("COOKIE_USER", "Guest"); //updates cookies accordingly
                TempData["REDIRECT"] = "/Cart/CartView";
                return RedirectToAction("Login", "Customer"); //if no login session, redirects to login
            }
            else //validSession indeed ;^)
            {
                await using var transaction = await _data.Database.BeginTransactionAsync(); //prepares a transaction for data integrity

                try
                {
                    IList<CartItem> cartItems = await _data.TBL_CartItem.Where(ci => ci.CustomerId_FK == validSession.CustomerId_FK).Include(c => c.CartItem_Product).ThenInclude(r => r.Product_Reviews).ToListAsync();
                    //translate to Order, OrderItem, OrderItemCode

                    Order? order = new Order() { CustomerId_FK = validSession.CustomerId_FK, OrderPurchasedDateTime = DateTime.Now };
                    OrderItem? orderItem; //OrderItemCode? orderItemCode;
                    _data.TBL_Order.Add(order);
                    await _data.SaveChangesAsync();
                    order = await _data.TBL_Order.Where(o => o.OrderPurchasedDateTime == order.OrderPurchasedDateTime && o.CustomerId_FK == order.CustomerId_FK).FirstOrDefaultAsync();
                    string temp;
                    if (order != null)
                    {
                        foreach (var item in cartItems)
                        {
                            _logger.LogInformation($"[{DateTime.Now}] LOG_Information : at '/Order/CheckOut' saving to TBL_OrderItem...");
                            orderItem = new OrderItem()
                            {
                                OrderItemProductId = item.CartItem_Product.ProductId,
                                OrderItemName = item.CartItem_Product.ProductName,
                                OrderItemDescription = item.CartItem_Product.ProductDescription,
                                OrderItemImage = item.CartItem_Product.ProductImage,
                                OrderItemPrice = item.CartItem_Product.ProductPrice,
                                OrderItemQuantity = item.CartItemQuantity,
                                OrderId_FK = order.OrderId
                            };
                            _data.TBL_OrderItem.Add(orderItem);
                            await _data.SaveChangesAsync();

                            orderItem = await _data.TBL_OrderItem.Where(oi => oi.OrderItemId == orderItem.OrderItemId).FirstOrDefaultAsync();
                            for (int i = 0; i < orderItem.OrderItemQuantity; i++)
                            {
                                _logger.LogInformation($"[{DateTime.Now}] LOG_Information : at '/Order/CheckOut' saving to TBL_OrderItemCode...");
                                _data.TBL_OrderItemCode.Add(new OrderItemCode()
                                {
                                    OrderItem_ActivationCode = Guid.NewGuid(), //simulator
                                    OrderItemCodeCustomerId = validSession.CustomerId_FK,
                                    OrderItemId_FK = orderItem.OrderItemId
                                });
                                await _data.SaveChangesAsync();
                            }
                        }
                    }
                    _logger.LogInformation($"[{DateTime.Now}] LOG_Information : at '/Order/CheckOut' deleting from TBL_CartItem...");
                    _data.TBL_CartItem.RemoveRange(cartItems); //clears related items
                    await _data.SaveChangesAsync();

                    await transaction.CommitAsync();

                    _logger.LogInformation($"[{DateTime.Now}] LOG_Information : at '/Order/CheckOut' updating cookies...");
                    Response.Cookies.Append("COOKIE_SHOPPINGCART", ""); //clears shopping cart cookie after database confirmation, else can reuse as backup?
                    HttpContext.Session.SetString("ORDEROWNER", validSession.CustomerId_FK.ToString());
                }
                catch (Exception ex)
                {
                    _logger.LogError(DateTime.Now + " " + ex.Message + "\n\t" + ex.StackTrace);
                    _logger.LogError($"LOG_ERROR:  Failure Log Message from '/Order/CheckOut' action: saving checkout data to database.");
                    await transaction.RollbackAsync();
                    return View("NotFound");
                }
            }
            _logger.LogInformation($"[{DateTime.Now}] LOG_Information : at '/Order/CheckOut' completed, redirects to /PurchaseHistory.");
            return RedirectToAction("PurchaseHistory", "Order");
        }

        //==================================================================================================================\\
        //==================================================================================================================\\

        [HttpGet]
        public async Task<IActionResult> PurchaseHistory()
        {
            _logger.LogInformation($"[{DateTime.Now}] LOG_Information : at '/Order/PurchaseHistory' starting...");
            int? cid = 0;
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("ORDEROWNER")))
            {
                _logger.LogInformation($"[{DateTime.Now}] LOG_Information : at '/Order/PurchaseHistory', updating cid from Session...");
                cid = Int32.Parse(HttpContext.Session.GetString("ORDEROWNER"));
            }
            else
            {
                _logger.LogInformation($"[{DateTime.Now}] LOG_Information : at '/Order/PurchaseHistory', Session is NOT available...");
                LogSession custSession = await _data.TBL_SessionLog.Where(s => s.SessionId == Request.Cookies["COOKIE_SESSION"]).FirstOrDefaultAsync();
                if (custSession == null) 
                {
                    _logger.LogInformation($"[{DateTime.Now}] LOG_Information : at '/Order/PurchaseHistory', LogSession NOT found... redirect to Login.");
                    return RedirectToAction("Login", "Customer");
                }
                cid = custSession.CustomerId_FK;
            }
            if (cid == null || cid == 0) //to handle unexpect corner-case event
            {
                _logger.LogInformation($"[{DateTime.Now}] LOG_Information : at '/Order/PurchaseHistory', cid not available... redirect to Login.");
                return RedirectToAction("Login", "Customer");
            }
            ViewBag.CustomerId = cid.ToString();
            IList<OrderItemViewModel> vm_orderItems = new List<OrderItemViewModel>();
            IList<Order> orders = await _data.TBL_Order.Where(o => o.CustomerId_FK == cid).OrderByDescending(o => o.OrderPurchasedDateTime).ToListAsync();
            if (orders != null || orders.Count > 0)
            {
                ICollection<OrderItem> orderItems;
                Review? r_score = new Review();
                foreach (Order order in orders)
                {
                    orderItems = await _data.TBL_OrderItem.Where(oi => oi.OrderId_FK == order.OrderId).OrderBy(n => n.OrderItemId).ToListAsync();
                    foreach (OrderItem item in orderItems)
                    {
                        r_score = await _data.TBL_Review.Where(r => r.CustomerId_FK == cid && r.ProductId_FK == item.OrderItemProductId).FirstOrDefaultAsync();
                        vm_orderItems.Add(new OrderItemViewModel
                        {
                            VM_OrderItemId = item.OrderItemId,
                            VM_OrderItemProductId = item.OrderItemProductId,
                            VM_OrderItemPurchasedDateTime = order.OrderPurchasedDateTime,
                            VM_OrderItemName = item.OrderItemName,
                            VM_OrderItemDescription = item.OrderItemDescription,
                            VM_OrderItemImage = item.OrderItemImage,
                            VM_OrderItemPrice = item.OrderItemPrice,
                            VM_OrderItemQuantity = item.OrderItemQuantity,
                            VM_OrderItem_ActivationCode = await _data.TBL_OrderItemCode.Where(oic => oic.OrderItemId_FK == item.OrderItemId).ToListAsync(),
                            VM_OrderItemReviewScore = r_score?.ReviewScore ?? 0
                        });
                    }
                }
            }
            _logger.LogInformation($"[{DateTime.Now}] LOG_Information : at '/Order/PurchaseHistory' completed.");
            return View(vm_orderItems);
        }
    }
}
