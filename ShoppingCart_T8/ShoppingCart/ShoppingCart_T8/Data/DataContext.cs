using Microsoft.EntityFrameworkCore;
using ShoppingCart_T8.Models;

namespace ShoppingCart_T8.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { } //intentionally empty, to pass optiosn to Parent 'DbContext'

        protected override void OnModelCreating(ModelBuilder modelBuilder) //overriding for composite keys
        {
            //Set up Model one-to-many relationships
            modelBuilder.Entity<LogSession>().HasOne(session => session.LogSession_Customer).WithMany(c => c.Customer_SessionLogs).HasForeignKey(session => session.CustomerId_FK);
            modelBuilder.Entity<LogUser>().HasOne(login => login.LogUser_Customer).WithMany(c => c.Customer_UserLogs).HasForeignKey(login => login.CustomerId_FK);

            modelBuilder.Entity<Order>().HasOne(o => o.Order_Customer).WithMany(c => c.Customer_Orders).HasForeignKey(o => o.CustomerId_FK);
            modelBuilder.Entity<OrderItem>().HasOne(oi => oi.OrderItem_Order).WithMany(o => o.Order_OrderItems).HasForeignKey(oi => oi.OrderId_FK);
            modelBuilder.Entity<OrderItemCode>().HasOne(oic => oic.OrderItemCode_OrderItem).WithMany(oi => oi.OrderItemCodes).HasForeignKey(oic => oic.OrderItemId_FK);

            //Configuring composite keys on Models

            //Review Entity
            modelBuilder.Entity<Review>().HasKey(cp => new
            {
                cp.ProductId_FK,
                cp.CustomerId_FK
            });

            modelBuilder.Entity<Review>().HasOne(cp => cp.Review_Product).WithMany(p => p.Product_Reviews).HasForeignKey(cp => cp.ProductId_FK);
            modelBuilder.Entity<Review>().HasOne(cp => cp.Review_Customer).WithMany(c => c.Customer_Reviews).HasForeignKey(cp => cp.CustomerId_FK);

            modelBuilder.Entity<CartItem>().HasKey(ci => new
            {
                ci.ProductId_FK,
                ci.CustomerId_FK
            });

            modelBuilder.Entity<CartItem>().HasOne(ci => ci.CartItem_Product).WithMany(p => p.Product_CartItems).HasForeignKey(ci => ci.ProductId_FK);
            modelBuilder.Entity<CartItem>().HasOne(ci => ci.CartItem_Customer).WithMany(c => c.Customer_CartItems).HasForeignKey(ci => ci.CustomerId_FK);

            modelBuilder.Entity<OrderItemCode>().HasKey(oic => new
            {
                oic.OrderItemCodeCustomerId,
                oic.OrderItem_ActivationCode //Guid collision rate is low but not zero
            });

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Customer> TBL_Customer { get; set; }
        public DbSet<Review> TBL_Review { get; set; }
        public DbSet<Product> TBL_Product { get; set; }
        public DbSet<CartItem> TBL_CartItem { get; set; }
        public DbSet<Order> TBL_Order { get; set; }
        public DbSet<OrderItem> TBL_OrderItem { get; set; }
        public DbSet<OrderItemCode> TBL_OrderItemCode { get; set; }
        public DbSet<LogUser> TBL_UserLog { get; set; }
        public DbSet<LogSession> TBL_SessionLog { get; set; }
    }
}
