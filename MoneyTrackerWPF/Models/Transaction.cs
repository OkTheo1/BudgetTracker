using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneyTracker2026.Models;

public class Transaction
{
    [Key]
    public int Id { get; set; }
    
    public DateTime Date { get; set; } = DateTime.Now;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public int CategoryId { get; set; }
    public virtual Category? Category { get; set; }
    
    public int AccountId { get; set; }
    public virtual Account? Account { get; set; }
    
    public TransactionType Type { get; set; }
    
    public int? RecurringId { get; set; }
    
    [MaxLength(3)]
    public string CurrencyCode { get; set; } = "GBP";
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal ConvertedAmount { get; set; } // Amount in base currency (GBP)
}
