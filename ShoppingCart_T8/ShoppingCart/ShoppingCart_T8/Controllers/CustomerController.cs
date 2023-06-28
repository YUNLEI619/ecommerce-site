using Castle.Core.Resource;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ShoppingCart_T8.Data;
using ShoppingCart_T8.Models;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace ShoppingCart_T8.Controllers
{
    public class CustomerController : Controller
    {
        private readonly DataContext _data;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(DataContext data, ILogger<CustomerController> logger)
        {
            _data = data;
            _logger = logger;
        }

        //==================================================================================================================\\
        //==================================================================================================================\\

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string? status) //WIP from TempData instead
        {
            _logger.LogInformation($"[{DateTime.Now}] LOG_Information : at '/Customer/Login [GET]' starting...");

            if (!string.IsNullOrEmpty(Request.Cookies["COOKIE_SESSION"])) //has session
            {
                var sessions = await _data.TBL_SessionLog.ToListAsync(); //fetch latest state from Database
                LogSession? userSession = await _data.TBL_SessionLog.FirstOrDefaultAsync(session => session.SessionId == Request.Cookies["COOKIE_SESSION"]); //session is found active
                if (userSession != null && userSession.LogSessionCookieExpiry > DateTime.Now)
                {
                     return RedirectToAction("Gallery", "Product");
                }
            }
            ViewBag.Msg_Error_Login = TempData["Msg_Error_Login"]; //for error message display from POST Login Action, if any
            _logger.LogInformation($"[{DateTime.Now}] LOG_Information : at '/Customer/Login [GET]' completed");
            return View();
        }

        //==================================================================================================================\\
        [HttpPost]
        //[ValidateAntiForgeryToken] //prevents POSTman
        //[Consumes("application/json", "application/xml")] //restriction on posted media format type
        public async Task<IActionResult> Login(IFormCollection form)
        {
            _logger.LogInformation($"[{DateTime.Now}] LOG_Information : at '/Customer/Login [POST]' starting...");
            string cookieNameShoppingCart = "COOKIE_SHOPPINGCART";
            string UserName = form["UserName"];
            string Password = PasswordHash(form["Password"]);
            if (!string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password)) //in case Javascript not working as intent
            {
                Customer? validLogin = await _data.TBL_Customer.Where(c => c.UserName == UserName && c.Password == Password).FirstOrDefaultAsync();
                if (validLogin != null) //user login credentials match (found in database)
                {
                    _logger.LogInformation(DateTime.Now + " : Customer [ " + validLogin.CustomerId + " ] logging in...");

                    if (ModelState.IsValid)
                    {   //update successful login into database first

                        await using var transaction_Login = await _data.Database.BeginTransactionAsync(); // Begin a transaction for data integrity
                        try
                        {
                            List<LogSession> userExistingSessions = await _data.TBL_SessionLog.Where(session => session.CustomerId_FK == validLogin.CustomerId).ToListAsync();
                            if (userExistingSessions.Count > 0)
                            {
                                _data.TBL_SessionLog.RemoveRange(userExistingSessions); //ensures no existing session for same user in database before issueing a new session cookie
                                await _data.SaveChangesAsync(); //commits to database to clear any existing sessions, if login from elsewhere etc
                            }

                            await _data.TBL_SessionLog.AddAsync(new LogSession() // shorter term: LogSession
                            {
                                CustomerId_FK = validLogin.CustomerId,
                                SessionId = HttpContext.Session.Id,
                                LogSessionCookieStart = DateTime.Now,
                                LogSessionCookieExpiry = DateTime.Now.AddMinutes(10) //10 minutes with consideration to CA demo purpose, adjustable
                            });

                            await _data.TBL_UserLog.AddAsync(new LogUser() //longer term: LogUser
                            { //keeps a more permanent log of customer who had successfully logged in, along with other custoemr details, if any.
                                CustomerId_FK = validLogin.CustomerId,
                                Timestamp = DateTime.Now,
                                IPAddress = GetIPAddress(),
                                UserAgent = GetUserAgent(),
                                Actions = "LOGIN"
                            });

                            await _data.SaveChangesAsync(); //commits to database Customer login event

                            await transaction_Login.CommitAsync(); //ensures data integrity
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(DateTime.Now + " " + ex.Message + "\n\t" + ex.StackTrace);
                            _logger.LogError($"LOG_ERROR:  Failure Log Message from '/Customer/Login' POST action: saving login details to database.");
                            await transaction_Login.RollbackAsync(); //rollback on error
                            throw;
                        }


                        // Checks Customer cartItem in database, if not in db then update database with Guest Cart Id,
                        // if Login Cart empty, then update with Client Current (browser) Cart 
                        // if Login Cart has items, the noverride Guest Cart

                        // As per requirement gathered for CA, to REPLACE database cartItems with CURRENT customer Cart (client side visual),
                        // unless empty. i.e. Customer CURRENT cart from cookie takes higher priority.

                        if (!string.IsNullOrEmpty(Request.Cookies[cookieNameShoppingCart])) // client cookie cart not empty 
                        {
                            await using var transaction_CartLatest = await _data.Database.BeginTransactionAsync(); //to ensure data integrity
                            try
                            {
#if DEBUG
                                _logger.LogDebug($"LOG_Debug: at '/Customer/Login', login success, client cart cookie NOT empty.");
#endif
                                // checks and clears current login user cart data in database
                                List<CartItem> oldCart = await _data.TBL_CartItem.Where(ci => ci.CustomerId_FK == validLogin.CustomerId || ci.CartItemQuantity < 1).ToListAsync();
                                if (oldCart != null || oldCart.Count > 0)
                                {
                                    _data.TBL_CartItem.RemoveRange(oldCart);
                                    await _data.SaveChangesAsync(); //commits to database
                                }

                                //updates database cartItem with current login user cart
                                string[] cartParser = Request.Cookies[cookieNameShoppingCart]!.Split(",");
                                Dictionary<string, string> shoppingCartItem = new Dictionary<string, string>();
                                string[] str = new string[2];
                                foreach (string item in cartParser)
                                {
                                    str = item.Split(':');
                                    if (str[1] != "0") // if quantity != "0" then add to database
                                    {
                                        await _data.TBL_CartItem.AddAsync(new CartItem() //Add to database with Customer latest shopping cart
                                        {
                                            CustomerId_FK = validLogin.CustomerId,
                                            ProductId_FK = Int32.Parse(str[0]),
                                            CartItemQuantity = Int32.Parse(str[1])
                                        });
                                    }
                                }
                                await _data.SaveChangesAsync(); //commit to database updates with latest cartItem

                                await transaction_CartLatest.CommitAsync(); //ensures data integrity on Cart Update
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(DateTime.Now + " " + ex.Message + "\n\t" + ex.StackTrace);
                                _logger.LogError($"LOG_ERROR:  Failure Log Message from '/Customer/Login' POST action, saving cartItem updates to database.");
                                await transaction_CartLatest.RollbackAsync(); //rollback on error
                                throw;
                            }
                        }
                        else // client side no cartItem
                        {
#if DEBUG
                            _logger.LogDebug($"LOG_Debug: at '/Customer/Login', login success, client cart cookie empty.");
#endif
                            try
                            {
                                List<CartItem> cart = await _data.TBL_CartItem.Where(c => c.CustomerId_FK == validLogin.CustomerId).Include(p => p.CartItem_Product).ToListAsync();
                                if (cart.Count > 1) // and has cartItem in database
                                {
                                    StringBuilder builder = new StringBuilder();
                                    int count = cart.Count;
                                    foreach (var item in cart) //construct new cart string for client's browser
                                    {
                                        builder.Append(item.ProductId_FK); builder.Append(":"); builder.Append(item.CartItemQuantity);
                                        if (--count > 0) { builder.Append(","); }
                                    }
                                    Response.Cookies.Append(cookieNameShoppingCart, builder.ToString()); //updates cookie for client
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(DateTime.Now + " " + ex.Message + "\n\t" + ex.StackTrace);
                                _logger.LogError($"LOG_ERROR:  Failure Log Message from '/Customer/Login' POST action, update Client Cart cookie from database.");
                                throw;
                            }
                        }
                    }

                    //prepares cookies for Customer
                    _logger.LogInformation(DateTime.Now + " : Customer [ " + validLogin.CustomerId + " ] login database update success.");
                    Response.Cookies.Append("COOKIE_SESSION", HttpContext.Session.Id); //session cookie for Customer
                    Response.Cookies.Append("COOKIE_USER", validLogin.CustomerName); //update cookie for name display
                    _logger.LogInformation(DateTime.Now + " : Customer [ " + validLogin.CustomerId + " ] cookies updated, logged in completeds.");
                    return RedirectToAction("Gallery", "Product"); // redirect to Product Gallery with cookies issued to Customer

                }
                TempData["Msg_Error_Login"] = "Please enter a valid Username and Password."; //to be shift up
            }
            else
            {
                TempData["Msg_Error_Login"] = "Username and Password are required."; //to be shift up
            }
            return RedirectToAction("Login", "Customer"); // if without username or password for validation, display Login form for Csutomer
        }

        //==================================================================================================================\\
        //==================================================================================================================\\

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation($"[{DateTime.Now}] LOG_Information : at '/Customer/Logout' starting...");
            //Checks user session
            LogSession? userSession = await _data.TBL_SessionLog.Include(c => c.LogSession_Customer).FirstOrDefaultAsync(s => s.SessionId == Request.Cookies["COOKIE_SESSION"]);
            if (userSession != null) //if found record in LogSession table
            {

                #region PART1: USER_LOGOUT_DETAILS
                Customer customer = userSession.LogSession_Customer;
                _logger.LogInformation(DateTime.Now + " : Customer [ " + customer.CustomerId + " ] logging out...");

                await using var transaction_Logout = await _data.Database.BeginTransactionAsync();

                try
                {
                    List<LogSession> userLogout = await _data.TBL_SessionLog.Where(x => x.CustomerId_FK == customer.CustomerId).ToListAsync();


                    if (userLogout != null) { _data.TBL_SessionLog.RemoveRange(userLogout); } //Clear all related sessions records from session table when user logs out.

                    await _data.TBL_UserLog.AddAsync(new LogUser()
                    { //keeps a more permanent log of customer who had successfully logged out, along with other custoemr details, if any.
                        CustomerId_FK = customer.CustomerId,
                        Timestamp = DateTime.Now,
                        IPAddress = GetIPAddress(),
                        UserAgent = GetUserAgent(),
                        Actions = "LOGOUT"
                    });

                    await _data.SaveChangesAsync(); //commit to database 

                    await transaction_Logout.CommitAsync(); //ensures data integrity, if user logout properly
                }
                catch (Exception ex)
                {
                    _logger.LogError(DateTime.Now + " " + ex.Message + "\n\t" + ex.StackTrace);
                    _logger.LogError($"LOG_ERROR:  Failure Log Message from '/Customer/Logout' action, updating to database proper User Logout.");
                    await transaction_Logout.RollbackAsync(); //rollback on error
                    throw;
                }
                #endregion PART1: USER_LOGOUT_DETAILS

                #region PART2: USER_LOGOUT_CART_UPDATE
                if (!string.IsNullOrEmpty(Request.Cookies["COOKIE_SHOPPINGCART"])) //OPTIONAL: Updates Cart if there are items in Client shopping cart cookie
                {
                    //Updates database (Replace cart) to client's cookie latest as per CA requirement direction
                    List<CartItem> cartitems = await _data.TBL_CartItem.Where(ci => ci.CustomerId_FK == customer.CustomerId || ci.CartItemQuantity < 1).ToListAsync();

                    await using var transaction_Logout_CartUpdate = await _data.Database.BeginTransactionAsync(); //for data integrity
                    try
                    {
                        //Clear customer's all database cartItem records for latest recent cart items  when log out
                        if (cartitems != null) { _data.TBL_CartItem.RemoveRange(cartitems); }
                        await _data.SaveChangesAsync(); //updates database first


                        string[] cartParser = Request.Cookies["COOKIE_SHOPPINGCART"].Split(",");
                        Dictionary<string, string> shoppingCartItem = new Dictionary<string, string>();
                        string[] str = new string[2];
                        foreach (string item in cartParser)
                        {
                            str = item.Split(':'); //update to database the latest shopping cart items of customer
                            _data.TBL_CartItem.Add(new CartItem() { CartItemQuantity = Int32.Parse(str[1]), CustomerId_FK = customer.CustomerId, ProductId_FK = Int32.Parse(str[0]) });
                        }
                        await _data.SaveChangesAsync(); //updates database

                        await transaction_Logout_CartUpdate.CommitAsync(); //ensures data integrity
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(DateTime.Now + " " + ex.Message + "\n\t" + ex.StackTrace);
                        _logger.LogError($"LOG_ERROR:  Failure Log Message from '/Customer/Logout' action, update database cart items with customer latest.");
                        await transaction_Logout_CartUpdate.RollbackAsync(); //rollback on error
                        throw;
                    }
                }
                #endregion PART2: USER_LOGOUT_CART_UPDATE

                _logger.LogInformation(DateTime.Now + " : Customer [ " + customer.CustomerId + " ] logout database update success.");
                Response.Cookies.Delete("COOKIE_SESSION"); //Remove Session Cookie at client end
                Response.Cookies.Append("COOKIE_USER", "Guest");
                _logger.LogInformation(DateTime.Now + " : Customer [ " + customer.CustomerId + " ] cookies updated, logged out completed");
            }
            _logger.LogInformation($"[{DateTime.Now}] LOG_Information : at '/Customer/Logout' completed");
            return RedirectToAction("Login", "Customer");
        }

        //====================================== Private Methods ======================================\\

        private string? GetIPAddress() //gets IP Address
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            return Convert.ToString(ipHostInfo.AddressList.FirstOrDefault(addr => addr.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork));
        }

        private string GetUserAgent() //gets browser details
        {
            string userAgent = Request.Headers["User-Agent"].ToString();
            string browser = "";
            string version = "";

            if (userAgent.Contains("Edge"))
            {
                browser = "Microsoft Edge";
                version = userAgent.Substring(userAgent.IndexOf("Edge/") + 5);
            }
            else if (userAgent.Contains("Chrome"))
            {
                browser = "Google Chrome";
                version = userAgent.Substring(userAgent.IndexOf("Chrome/") + 7);
            }
            else if (userAgent.Contains("Safari") && userAgent.Contains("Version"))
            {
                browser = "Apple Safari";
                version = userAgent.Substring(userAgent.IndexOf("Version/") + 8, userAgent.IndexOf("Safari/") - userAgent.IndexOf("Version/") - 8);
            }
            else if (userAgent.Contains("Firefox"))
            {
                browser = "Mozilla Firefox";
                version = userAgent.Substring(userAgent.IndexOf("Firefox/") + 8);
            }
            else if (userAgent.Contains("Opera"))
            {
                browser = "Opera";
                version = userAgent.Substring(userAgent.IndexOf("Version/") + 8);
            }
            else { browser = "Unknown"; version = "NA"; }
            return browser + " " + version;
        }

        private static string PasswordHash(string password) //a simple hasher, to be saved in database mainly, else depends on HTTPS (HSTS) for over network security
        {
            using (SHA256 sha256 = SHA256.Create()) //sing the using statement to ensure that the SHA256 object is properly disposed of after use. 
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password)); // Encoding.UTF8 object a more modern and widely supported character encoding than Encoding.ASCII
                return Convert.ToBase64String(hashedBytes); // converting the hashed byte array to a Base64 string using the Convert.ToBase64String() as a printable string.
            }
        }
    }
}
