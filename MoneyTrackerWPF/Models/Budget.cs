using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneyTracker2026.Models;

public class Budget
{
    [Key]
    public int Id { get; set; }
    
    public int CategoryId { get; set; }
    public virtual Category? Category { get; set; }
    
    [MaxLength(7)]
    public string MonthYear { get; set; } = DateTime.Now.ToString("yyyy-MM"); // Format: yyyy-MM
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; } // Budget amount in base currency (GBP)
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Spent { get; set; } // Amount spent in base currency (GBP)
}
