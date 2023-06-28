using ShoppingCart_T8.Data;
using ShoppingCart_T8.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

//COOKIES:
// 1. COOKIE_SESSION        = issue to user after logged in, deleted after user logged out
// 2. COOKIE_SHOPPINGCART   = retrieves cart details from database, to be 'stringify'
//                              for user's browser Local Storage in single string, of <ProductId>:<quantity> pairs with ', ' seperator
// 3. COOKIE_USER           = "Guest" or Models.Customer.CustomerName, mainly display purpose
// 4. COOKIE_GUEST_CARTID   = for guest, when visitors not login, related to each customer
// 5. COOKIE_GUEST_CART     = originally designed to update with database items with matching Guest Id, save as JSON string in database, UPDATE: implementation removed as Professors' direction is no multi-value group in database
//                              Currently no differentiation of customer
// Re-purposed to also redirect user to a "404" NotFound page (in shared folder) with a return to Product Gallery button on certain exceptions.

namespace ShoppingCart_T8.Middlewares
{
    public class MiddlewareCookies
    {
        private readonly RequestDelegate _requestDelegate;

        public MiddlewareCookies(RequestDelegate requestDelegate)
        {
            _requestDelegate = requestDelegate;
        }

        public async Task InvokeAsync(HttpContext context, ILogger<MiddlewareCookies> logger, DataContext _data)
        {
            try
            {
                if (!context.Request.Cookies.Any()) // has no cookies, likely new visitor
                {
                    //issue COOKIE_GUEST_CARTID to 1st time visitor, for visitor's Local Storage
                    context.Response.Cookies.Append("COOKIE_USER", "Guest");
                    //context.Response.Cookies.Append("COOKIE_GUEST_CARTID", Guid.NewGuid().ToString()); //mean as key to guest cart but remove due to being multi-value group as a string of products and quantities
                    context.Response.Cookies.Append("COOKIE_SHOPPINGCART", "");
                }
                else // has cookies(s)
                {
                    if (!string.IsNullOrEmpty(context.Request.Cookies["COOKIE_SESSION"])) // has session cookie
                    {
                        string? loginValid = context.Request.Cookies["COOKIE_SESSION"]; //check if cookies valid
                        LogSession? validSession = await _data.TBL_SessionLog.Where(s => s.SessionId == loginValid).FirstOrDefaultAsync();
                        if (validSession != null) //found in database TBL_LogSession table
                        {   //checks for expiry
                            if (validSession.LogSessionCookieExpiry < DateTime.Now) //if expired, SIDE: can be DataTime.UtcNow depends on requirement
                            {
                                context.Response.Cookies.Delete("COOKIE_SESSION"); // removes invalid Session cookie from client
                                context.Response.Cookies.Append("COOKIE_USER", "Guest"); //updates cookies accordingly
                            }
                        }
                    }
                }
                await _requestDelegate(context); //next middleware in pipeline
            }
            catch (Exception ex)
            {
                logger.LogError(DateTime.Now + " " + ex.Message + "\n\t" + ex.StackTrace);
                logger.LogError($"LOG_ERROR:  Failure Log Message from 'MiddlewareGuest.cs'.");
                context.Response.Redirect("/Stats/NotFound"); //for NotFound.cshtml
            }

        }
    }
}
