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

    [Column(TypeName = "TEXT")]
    public string? OcrRawText { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual ICollection<ReceiptItem> ReceiptItems { get; set; } = new List<ReceiptItem>();
}
