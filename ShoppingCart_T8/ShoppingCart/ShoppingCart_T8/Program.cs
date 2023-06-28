using Microsoft.EntityFrameworkCore;
using ShoppingCart_T8.Data;
using ShoppingCart_T8.Middlewares;
using ShoppingCart_T8.Models;

//==================================================
//==================================================
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//for EF Core
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseLazyLoadingProxies().UseSqlServer(builder.Configuration["ConnectionStrings:DbConnection"]);
});

// For sessions
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.IsEssential = true; //If set to true, the cookie will be sent even if the user hasn't consented to it, required for the website's core functionality, such as a shopping cart or a user authentication.
    options.Cookie.Name = "SessionCookie";
    options.Cookie.MaxAge = TimeSpan.FromMinutes(10); // Set the cookie expiration time to 30 minutes from now, since this CA scope should not be more than 30 minutes
    options.IdleTimeout = TimeSpan.FromMinutes(5); //2 minutes for demo purpose, else 15 minutes for development testing
    options.Cookie.SameSite = SameSiteMode.None; //a configuration that prevents certain CSRF but may prevents SSO across different domains, SameSiteMode.Strict prevents cookies deletion
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; //enforces HTTPS
    //options.Cookie.HttpOnly = true; //prevents Client-side Javascript access, to prevent XSS.
});

//builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); //for quick access and testing of HttpRequest where needed

// CUSTOM SERVICES
builder.Services.AddSingleton<LogStats>(); // to be instantiated 'externally' for dependency injection where needed.

//==================================================
//==================================================
var app = builder.Build();
//DataInitializer.SeedDatabase(app.Services.CreateScope().ServiceProvider.GetRequiredService<DataContext>()); //Initial seeding of Database tables

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
    //app.UseDatabaseErrorPage();
}

app.UseHttpsRedirection();

app.UseMiddleware<MiddlewareStats>(); //modified from SA ASP .NET Workshop to capture user Views access count.

app.UseStaticFiles();

app.UseRouting();

app.UseMiddleware<MiddlewareCookies>(); //CUSTOM MIDDLEWARE: checks and handle cookies

//app.UseAuthentication(); //for Identity, removed as Admin, Roles, Areas not within SA course scope

app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Stats}/{action=Index}"); //so as to capture all user access pages where possible

/*InitDB(app.Services);*/ // Initialize our database before our web application starts

app.Run();

//void InitDB(IServiceProvider serviceProvider)
//{
//    using var scope = serviceProvider.CreateScope();
//    DataContext db = scope.ServiceProvider.GetRequiredService<DataContext>();

//    db.Database.EnsureDeleted(); //removal of old database (if there is one).
//    db.Database.EnsureCreated(); //creates a new database
//} 