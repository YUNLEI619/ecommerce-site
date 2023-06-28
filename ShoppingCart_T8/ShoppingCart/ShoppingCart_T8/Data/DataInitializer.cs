using Microsoft.EntityFrameworkCore;
using ShoppingCart_T8.Models;
using System.Security.Cryptography;
using System.Text;

namespace ShoppingCart_T8.Data
{
    public class DataInitializer
    {
        public static void SeedDatabase(DataContext context)
        {
            context.Database.Migrate();

            if (!context.TBL_Customer.Any())
            {
                context.TBL_Customer.AddRange
                (
                    new Customer { CustomerId = 1, UserName = "ruth", Password = PasswordHash("ruth"), CustomerName = "Ruth", CustomerStatus = "O", CustomerGuestCartId = new Guid() },
                    new Customer { CustomerId = 2, UserName = "vincent", Password = PasswordHash("vincent"), CustomerName = "Vincent", CustomerStatus = "O", CustomerGuestCartId = new Guid() },
                    new Customer { CustomerId = 3, UserName = "rainne", Password = PasswordHash("rainne"), CustomerName = "Rainne", CustomerStatus = "O", CustomerGuestCartId = new Guid() },
                    new Customer { CustomerId = 4, UserName = "steven", Password = PasswordHash("steven"), CustomerName = "Steven", CustomerStatus = "O", CustomerGuestCartId = new Guid() },
                    new Customer { CustomerId = 5, UserName = "snow", Password = PasswordHash("snow"), CustomerName = "Snow", CustomerStatus = "O", CustomerGuestCartId = new Guid() },
                    new Customer { CustomerId = 6, UserName = "annabelle", Password = PasswordHash("annabelle"), CustomerName = "Annabelle", CustomerStatus = "O", CustomerGuestCartId = new Guid() },
                    new Customer { CustomerId = 7, UserName = "ethan", Password = PasswordHash("ethan"), CustomerName = "Ethan", CustomerStatus = "O", CustomerGuestCartId = new Guid() }
                );
            }
            context.SaveChanges();

            if (!context.TBL_Product.Any())
            {
                context.TBL_Product.AddRange
                (
                    new Product { ProductName = ".NET Charts", ProductStatus = "O", ProductPrice = 99.0M, ProductDescription = "Brings powerful charting capabilities to your .NET applications.", ProductImage = "/images/Chart.png" },
                    new Product { ProductName = ".NET PayPal", ProductStatus = "O", ProductPrice = 69.0M, ProductDescription = "Integrate your .NET applications with PayPal the easy way!", ProductImage = "/images/Paypal_logo.png" },
                    new Product { ProductName = ".NET ML", ProductStatus = "O", ProductPrice = 299.0M, ProductDescription = "Supercharged .NET Machine Learning libraries", ProductImage = "/images/ML_NET.png" },
                    new Product { ProductName = ".NET Analytics", ProductStatus = "O", ProductPrice = 299.0M, ProductDescription = "Perforns data mining and analytics easily in .NET", ProductImage = "/images/Analytics.png" },
                    new Product { ProductName = ".NET Logger", ProductStatus = "O", ProductPrice = 49.0M, ProductDescription = "Logs and aggregates events easily in your .NET apps", ProductImage = "/images/Logger.png" },
                    new Product { ProductName = ".NET Numberics", ProductStatus = "O", ProductPrice = 199.0M, ProductDescription = "Powerful numerical methods for your .NET simulations", ProductImage = "/images/Numerics.png" },
                    new Product { ProductName = ".NET Gamer", ProductStatus = "O", ProductPrice = 963.69M, ProductDescription = "Realise your dream Game Application with latest Unity Libraries!", ProductImage = "/images/SSFF.png" },
                    new Product { ProductName = ".NET Web Form", ProductStatus = "X", ProductPrice = 6.66M, ProductDescription = "Welcome to the where it all begins :^)", ProductImage = "https://as1.ftcdn.net/v2/jpg/02/48/42/64/1000_F_248426448_NVKLywWqArG2ADUxDq6QprtIzsF82dMF.jpg" }
                );
            }
            context.SaveChanges();

        }

        private static string PasswordHash(string password) // a simple hash on password before being saved to database
        {
            using (SHA256 sha256 = SHA256.Create()) //sing the using statement to ensure that the SHA256 object is properly disposed of after use. 
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password)); // Encoding.UTF8 object a more modern and widely supported character encoding than Encoding.ASCII
                return Convert.ToBase64String(hashedBytes); // converting the hashed byte array to a Base64 string using the Convert.ToBase64String() as a printable string.
            }
        }
    }
}
