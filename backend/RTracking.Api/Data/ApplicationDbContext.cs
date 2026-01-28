using Microsoft.EntityFrameworkCore;
using RTracking.Api.Models;

namespace RTracking.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Receipt> Receipts { get; set; }
    public DbSet<ReceiptItem> ReceiptItems { get; set; }
    public DbSet<Category> Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Receipt entity
        modelBuilder.Entity<Receipt>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Index on PurchasedAt for date range queries
            entity.HasIndex(e => e.PurchasedAt);
            
            // Configure decimal precision for money
            entity.Property(e => e.Total)
                .HasPrecision(18, 2);
            
            // Configure timestamps
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .ValueGeneratedOnAddOrUpdate();
        });

        // Configure ReceiptItem entity
        modelBuilder.Entity<ReceiptItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Index on ReceiptId for efficient lookups
            entity.HasIndex(e => e.ReceiptId);
            
            // Index on CategoryId for category-based queries
            entity.HasIndex(e => e.CategoryId);
            
            // Configure decimal precision for money
            entity.Property(e => e.PriceTotal)
                .HasPrecision(18, 2);
            
            // Configure decimal precision for weights
            entity.Property(e => e.WeightKg)
                .HasPrecision(10, 3);
            
            entity.Property(e => e.Quantity)
                .HasPrecision(10, 3);
            
            // Configure decimal precision for edible fraction
            entity.Property(e => e.EdibleFraction)
                .HasPrecision(5, 3)
                .HasDefaultValue(1.0m);
            
            // Configure relationships
            entity.HasOne(e => e.Receipt)
                .WithMany(r => r.ReceiptItems)
                .HasForeignKey(e => e.ReceiptId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Category)
                .WithMany(c => c.ReceiptItems)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent category deletion if items exist
        });

        // Configure Category entity
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Unique index on Name
            entity.HasIndex(e => e.Name)
                .IsUnique();
        });

        // Seed categories
        SeedCategories(modelBuilder);
    }

    private void SeedCategories(ModelBuilder modelBuilder)
    {
        var categories = new[]
        {
            new Category { Id = 1, Name = "Meat & Fish", Description = "Meat and fish products (bone-in/out handled via EdibleFraction on item)" },
            new Category { Id = 2, Name = "Dairy", Description = "Dairy products" },
            new Category { Id = 3, Name = "Bread & Flour", Description = "Bread, flour, and bakery products" },
            new Category { Id = 4, Name = "Fruit & Vegetables", Description = "Fresh fruits and vegetables" },
            new Category { Id = 5, Name = "Sweets & Snacks", Description = "Chocolate, crisps, and other snacks" },
            new Category { Id = 6, Name = "Spices & Cooking Extras", Description = "Spices, condiments, and cooking ingredients" },
            new Category { Id = 7, Name = "Kitchen Tools", Description = "Kitchen utensils and tools" },
            new Category { Id = 8, Name = "Cleaning & Personal Hygiene", Description = "Cleaning supplies and personal care products" },
            new Category { Id = 9, Name = "Other", Description = "Other items" }
        };

        modelBuilder.Entity<Category>().HasData(categories);
    }
}
