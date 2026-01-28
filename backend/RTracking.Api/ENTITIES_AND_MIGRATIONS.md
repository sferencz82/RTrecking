# Entities and Migrations Documentation

## Entity Models

### Receipt.cs
```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RTracking.Api.Models;

public class Receipt
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public DateTime PurchasedAt { get; set; }

    [Required]
    [MaxLength(500)]
    public string MerchantName { get; set; } = string.Empty;

    [MaxLength(10)]
    public string Currency { get; set; } = "CHF";

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Total { get; set; }

    [Required]
    [MaxLength(1000)]
    public string ImagePath { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? LocationName { get; set; }

    [Column(TypeName = "double")]
    public double? Latitude { get; set; }

    [Column(TypeName = "double")]
    public double? Longitude { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual ICollection<ReceiptItem> ReceiptItems { get; set; } = new List<ReceiptItem>();
}
```

### ReceiptItem.cs
```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RTracking.Api.Models;

public class ReceiptItem
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ReceiptId { get; set; }

    [Required]
    [MaxLength(500)]
    public string RawName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? NormalizedName { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [Column(TypeName = "decimal(10,3)")]
    public decimal? Quantity { get; set; } = 1;

    [MaxLength(20)]
    public string? Unit { get; set; }

    [Column(TypeName = "decimal(10,3)")]
    public decimal? WeightKg { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PriceTotal { get; set; }

    [Column(TypeName = "decimal(5,3)")]
    public decimal EdibleFraction { get; set; } = 1.0m;

    [MaxLength(1000)]
    public string? Notes { get; set; }

    // Navigation properties
    [ForeignKey(nameof(ReceiptId))]
    public virtual Receipt Receipt { get; set; } = null!;

    [ForeignKey(nameof(CategoryId))]
    public virtual Category Category { get; set; } = null!;
}
```

### Category.cs
```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RTracking.Api.Models;

public class Category
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    // Navigation property
    public virtual ICollection<ReceiptItem> ReceiptItems { get; set; } = new List<ReceiptItem>();
}
```

## ApplicationDbContext.cs

```csharp
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
```

## Database Schema Features

### Decimal Precision
- **Money fields** (Total, PriceTotal): `decimal(18,2)` - Suitable for CHF amounts
- **Weight fields** (WeightKg): `decimal(10,3)` - Supports grams precision
- **Quantity**: `decimal(10,3)` - Supports fractional quantities
- **EdibleFraction**: `decimal(5,3)` - Supports values like 0.700 for 70%

### Indexes
- `Receipt.PurchasedAt` - For date range queries
- `ReceiptItem.ReceiptId` - For efficient receipt item lookups
- `ReceiptItem.CategoryId` - For category-based queries
- `Category.Name` - Unique index for category names

### Relationships
- `ReceiptItem` → `Receipt`: Cascade delete (deleting receipt deletes items)
- `ReceiptItem` → `Category`: Restrict delete (prevents category deletion if items exist)

## Creating Migrations

### Initial Migration
```bash
cd backend/RTracking.Api
dotnet ef migrations add InitialCreate --output-dir Migrations
```

### Apply Migration to Database
```bash
dotnet ef database update
```

### Remove Last Migration (if needed)
```bash
dotnet ef migrations remove
```

## Seeded Categories

The following categories are automatically seeded:

1. **Meat & Fish** - Meat and fish products (bone-in/out handled via EdibleFraction on item)
2. **Dairy** - Dairy products
3. **Bread & Flour** - Bread, flour, and bakery products
4. **Fruit & Vegetables** - Fresh fruits and vegetables
5. **Sweets & Snacks** - Chocolate, crisps, and other snacks
6. **Spices & Cooking Extras** - Spices, condiments, and cooking ingredients
7. **Kitchen Tools** - Kitchen utensils and tools
8. **Cleaning & Personal Hygiene** - Cleaning supplies and personal care products
9. **Other** - Other items
