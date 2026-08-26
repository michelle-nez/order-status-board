using Microsoft.EntityFrameworkCore;
using OrderStatus.Data.Models;

namespace OrderStatus.Data;

public class BoardDbContext : DbContext
{
    public BoardDbContext(DbContextOptions<BoardDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<OrderState> OrderStates => Set<OrderState>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Money needs an exact SQL type, or SQL Server picks a float-like default.
        modelBuilder.Entity<Order>()
            .Property(o => o.Total)
            .HasColumnType("decimal(18,2)");

        // The database refuses duplicate order numbers.
        modelBuilder.Entity<Order>()
            .HasIndex(o => o.OrderNumber)
            .IsUnique();

        // Foreign key one - the related second table.
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // Foreign key two - the lookup. Restrict means a status still in use
        // cannot be deleted out from under the orders sitting in it.
        modelBuilder.Entity<Order>()
            .HasOne(o => o.OrderState)
            .WithMany(s => s.Orders)
            .HasForeignKey(o => o.OrderStateId)
            .OnDelete(DeleteBehavior.Restrict);

        // The lookup list is seeded, so the board always has its columns.
        modelBuilder.Entity<OrderState>().HasData(
            new OrderState { Id = 1, Name = "New",      SortOrder = 1, Accent = "#5b8cff" },
            new OrderState { Id = 2, Name = "Picking",  SortOrder = 2, Accent = "#a78bfa" },
            new OrderState { Id = 3, Name = "Packed",   SortOrder = 3, Accent = "#f59e0b" },
            new OrderState { Id = 4, Name = "Shipped",  SortOrder = 4, Accent = "#0ea5e9" },
            new OrderState { Id = 5, Name = "Delivered",SortOrder = 5, Accent = "#34d399" });

        // A couple of customers, so the dropdown is never empty on first run.
        modelBuilder.Entity<Customer>().HasData(
            new Customer { Id = 1, Name = "RiteAV",           Email = "orders@riteav.example" },
            new Customer { Id = 2, Name = "Ultra Spec Cables", Email = "purchasing@ultraspec.example" },
            new Customer { Id = 3, Name = "Wallplate City",    Email = "buyer@wallplatecity.example" });

        base.OnModelCreating(modelBuilder);
    }
}
