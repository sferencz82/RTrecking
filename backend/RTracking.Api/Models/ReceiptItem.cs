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
