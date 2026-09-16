using Microsoft.EntityFrameworkCore;
using OrderFunctionApp.Models;

namespace OrderFunctionApp.Data
{
    /// <summary>
    /// OrderIntegrationContext is the Entity Framework Core DbContext for the Order Integration POC.
    /// It provides access to the Orders table and manages database operations.
    /// </summary>
    public class OrderIntegrationContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the OrderIntegrationContext class.
        /// </summary>
        /// <param name="options">The DbContextOptions for this context.</param>
        public OrderIntegrationContext(DbContextOptions<OrderIntegrationContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the Orders DbSet for database operations.
        /// </summary>
        public DbSet<Order> Orders { get; set; } = null!;

        /// <summary>
        /// Gets or sets the Customers DbSet for database operations.
        /// </summary>
        public DbSet<Customer> Customers { get; set; } = null!;

        /// <summary>
        /// Gets or sets the OrderLines DbSet for database operations.
        /// </summary>
        public DbSet<OrderLine> OrderLines { get; set; } = null!;

        /// <summary>
        /// Configures the model for the database.
        /// This method is called by Entity Framework Core to configure entity mappings,
        /// constraints, and indexes.
        /// </summary>
        /// <param name="modelBuilder">The model builder used to configure the model.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the Customer entity
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.CustomerId)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.HasIndex(e => e.CustomerId)
                    .IsUnique()
                    .HasDatabaseName("UQ_Customers_CustomerId");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Phone)
                    .HasMaxLength(20);

                entity.Property(e => e.RegistrationNumber)
                    .HasMaxLength(50);

                entity.Property(e => e.BillingAddress)
                    .HasMaxLength(255);

                entity.Property(e => e.ShippingAddress)
                    .HasMaxLength(255);

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true);

                entity.HasMany(c => c.Orders)
                    .WithOne(o => o.Customer)
                    .HasForeignKey(o => o.CustomerDatabaseId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.ToTable("Customers");
            });

            // Configure the Order entity
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.OrderId)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.HasIndex(e => e.OrderId)
                    .IsUnique()
                    .HasDatabaseName("UQ_Orders_OrderId");

                entity.Property(e => e.CustomerId)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.HasIndex(e => e.CustomerId)
                    .HasDatabaseName("IX_Orders_CustomerId");

                entity.Property(e => e.Total)
                    .HasPrecision(10, 2)
                    .IsRequired();

                entity.Property(e => e.Description)
                    .HasMaxLength(255);

                entity.Property(e => e.OrderDate)
                    .IsRequired();
                entity.HasIndex(e => e.OrderDate)
                    .HasDatabaseName("IX_Orders_OrderDate");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasDefaultValue(OrderStatus.Pending);

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.UpdatedAt);

                entity.HasMany(o => o.OrderLines)
                    .WithOne(ol => ol.Order)
                    .HasForeignKey(ol => ol.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(o => o.Customer)
                    .WithMany(c => c.Orders)
                    .HasForeignKey(o => o.CustomerDatabaseId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.ToTable("Orders");
            });

            // Configure the OrderLine entity
            modelBuilder.Entity<OrderLine>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.OrderId)
                    .IsRequired();

                entity.Property(e => e.LineNumber)
                    .IsRequired();

                entity.Property(e => e.ProductId)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Quantity)
                    .HasPrecision(10, 2)
                    .IsRequired();

                entity.Property(e => e.Unit)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasDefaultValue("pcs");

                entity.Property(e => e.UnitPrice)
                    .HasPrecision(10, 2)
                    .IsRequired();

                entity.Property(e => e.LineTotal)
                    .HasPrecision(10, 2)
                    .IsRequired();

                entity.Property(e => e.DiscountPercentage)
                    .HasPrecision(5, 2);

                entity.Property(e => e.Notes)
                    .HasMaxLength(255);

                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(e => e.OrderId)
                    .HasDatabaseName("IX_OrderLines_OrderId");

                entity.HasOne(ol => ol.Order)
                    .WithMany(o => o.OrderLines)
                    .HasForeignKey(ol => ol.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.ToTable("OrderLines");
            });
        }
    }
}
