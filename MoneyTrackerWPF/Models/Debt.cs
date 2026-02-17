using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneyTracker2026.Models;

public enum PayoffMethod
{
    Snowball,
    Avalanche
}

public class Debt
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Balance { get; set; }
    
    [Column(TypeName = "decimal(5,2)")]
    public decimal InterestRatePercent { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal MinPayment { get; set; }
    
    public PayoffMethod PayoffMethod { get; set; } = PayoffMethod.Avalanche;
}
