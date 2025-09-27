namespace ShopApiPostgreSQL.Data;

using Microsoft.EntityFrameworkCore;
using ShopApiPostgreSQL.Models;

public class ShopDbContext(DbContextOptions<ShopDbContext> opt) : DbContext(opt)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }
    protected override void OnModelCreating(ModelBuilder b)
    {
        // Categories
        b.Entity<Category>(e =>
        {
            e.ToTable("categories");
            e.HasKey(x => x.CategoryId).HasName("pk_categories");
            e.Property(x => x.CategoryId).HasColumnName("category_id");
            e.Property(x => x.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
            e.Property(x => x.Slug).HasColumnName("slug");
            e.HasIndex(x => x.Name).IsUnique().HasDatabaseName("ux_categories_name");
            e.HasIndex(x => x.Slug).IsUnique().HasDatabaseName("ux_categories_slug");
        });

        // Products
        b.Entity<Product>(e =>
        {
            e.ToTable("products");
            e.HasKey(x => x.ProductId).HasName("pk_products");
            e.Property(x => x.ProductId).HasColumnName("product_id");
            e.Property(x => x.Name).HasColumnName("name").IsRequired().HasMaxLength(150);
            e.Property(x => x.Price).HasColumnName("price").HasPrecision(18, 2);
            e.Property(x => x.CategoryId).HasColumnName("category_id");
            e.HasOne(x => x.Category).WithMany(c => c.Products).HasForeignKey(x => x.CategoryId)
             .HasConstraintName("fk_products_categories_category_id").OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.CategoryId).HasDatabaseName("ix_products_category_id");
            e.HasCheckConstraint("ck_products_price_nonnegative", "price >= 0");
        });

        // Customers
        b.Entity<Customer>(e =>
        {
            e.ToTable("customers");
            e.HasKey(x => x.CustomerId).HasName("pk_customers");
            e.Property(x => x.CustomerId).HasColumnName("customer_id");
            e.Property(x => x.FullName).HasColumnName("full_name").IsRequired().HasMaxLength(150);
            e.Property(x => x.Email).HasColumnName("email").IsRequired().HasMaxLength(150);
            e.HasIndex(x => x.Email).IsUnique().HasDatabaseName("ux_customers_email");
        });

        // Orders
        b.Entity<Order>(e =>
        {
            e.ToTable("orders");
            e.HasKey(x => x.OrderId).HasName("pk_orders");
            e.Property(x => x.OrderId).HasColumnName("order_id");
            e.Property(x => x.CustomerId).HasColumnName("customer_id");
            e.Property(x => x.OrderDate).HasColumnName("order_date")
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("now()");
            e.Property(x => x.Status).HasColumnName("status")
                .HasConversion<string>().HasMaxLength(20);
            e.HasOne(x => x.Customer).WithMany(c => c.Orders).HasForeignKey(x => x.CustomerId)
                .HasConstraintName("fk_orders_customers_customer_id").OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.CustomerId).HasDatabaseName("ix_orders_customer_id");
            e.HasIndex(x => x.OrderDate).HasDatabaseName("ix_orders_order_date");
            e.HasIndex(x => x.Status).HasDatabaseName("ix_orders_status");
            e.HasCheckConstraint("ck_orders_status_valid",
                "status IN ('Pending','Paid','Shipped','Cancelled')");
        });

        // OrderItems
        b.Entity<OrderItem>(e =>
        {
            e.ToTable("order_items");
            e.HasKey(x => x.OrderItemId).HasName("pk_order_items");
            e.Property(x => x.OrderItemId).HasColumnName("order_item_id");
            e.Property(x => x.OrderId).HasColumnName("order_id");
            e.Property(x => x.ProductId).HasColumnName("product_id");
            e.Property(x => x.Qty).HasColumnName("qty");
            e.Property(x => x.UnitPrice).HasColumnName("unit_price").HasPrecision(18, 2);

            e.HasOne(x => x.Order).WithMany(o => o.Items).HasForeignKey(x => x.OrderId)
                .HasConstraintName("fk_order_items_orders_order_id").OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Product).WithMany(p => p.OrderItems).HasForeignKey(x => x.ProductId)
                .HasConstraintName("fk_order_items_products_product_id").OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => x.OrderId).HasDatabaseName("ix_order_items_order_id");
            e.HasIndex(x => x.ProductId).HasDatabaseName("ix_order_items_product_id");
            e.HasCheckConstraint("ck_order_items_qty_positive", "qty > 0");
            e.HasCheckConstraint("ck_order_items_unit_price_nonnegative", "unit_price >= 0");
        });
    }
}
