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
        /// Configures the model for the database.
        /// This method is called by Entity Framework Core to configure entity mappings,
        /// constraints, and indexes.
        /// </summary>
        /// <param name="modelBuilder">The model builder used to configure the model.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the Order entity
            modelBuilder.Entity<Order>(entity =>
            {
                // Configure the primary key
                entity.HasKey(e => e.Id);

                // Configure the OrderId property with unique constraint
                entity.Property(e => e.OrderId)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.HasIndex(e => e.OrderId)
                    .IsUnique()
                    .HasDatabaseName("UQ_Orders_OrderId");

                // Configure the CustomerId property with index
                entity.Property(e => e.CustomerId)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.HasIndex(e => e.CustomerId)
                    .HasDatabaseName("IX_Orders_CustomerId");

                // Configure the Total property
                entity.Property(e => e.Total)
                    .HasPrecision(10, 2)
                    .IsRequired();

                // Configure the Description property (optional)
                entity.Property(e => e.Description)
                    .HasMaxLength(255)
                    .IsRequired(false);

                // Configure the OrderDate property with index
                entity.Property(e => e.OrderDate)
                    .IsRequired();
                entity.HasIndex(e => e.OrderDate)
                    .HasDatabaseName("IX_Orders_OrderDate");

                // Configure the CreatedAt property with default value
                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()");

                // Configure table name
                entity.ToTable("Orders");
            });
        }
    }
}
