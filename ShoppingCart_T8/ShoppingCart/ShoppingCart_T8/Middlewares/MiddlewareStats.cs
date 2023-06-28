using ShoppingCart_T8.Models;

namespace ShoppingCart_T8.Middlewares
{
    public class MiddlewareStats //Largely taken from SA ASP .NET Workshop, modified slightly, as a simple page access counter
    {
        private readonly RequestDelegate next;
        private readonly LogStats stats;

        public MiddlewareStats(RequestDelegate next, LogStats stats)
        {
            this.next = next;
            this.stats = stats; //Singleton
        }

        public async Task Invoke(HttpContext context)
        {
            string path = context.Request.Path.ToString();
            //to whitelist the selected paths reletive to this CA scope, not all paths
            if (path.Contains("/Customer/Login") || path.Contains("/Product/Gallery") || path.Contains("/Cart/CartView") || path.Contains("/Order/PurchaseHistory") || path.Contains("/NotFound"))
            {
                stats.Add(path); //Easter Egg serves as an analytics of user requests (decided not saved to database as not within this CA core scope)
            }

            await next(context);
        }
    }
}
