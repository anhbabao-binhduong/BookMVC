using System.Data.Entity;
using System.Security.Policy;

namespace Thuc_hanh_WEB.Models
{
    public class BookStoreDBContext : DbContext
    {
        public BookStoreDBContext() : base("BookStoreDBConnection") { }

        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Publisher> Publishers { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<EmailVerification> EmailVerifications { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<Coupon> Coupons { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Coupon>()
                .Property(c => c.DiscountValue)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Coupon>()
                .Property(c => c.MinOrderAmount)
                .HasPrecision(18, 2);

            base.OnModelCreating(modelBuilder);
        }
    }
}
