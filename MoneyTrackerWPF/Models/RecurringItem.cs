using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneyTracker2026.Models;

public enum Frequency
{
    Daily,
    Weekly,
    Monthly,
    Yearly
}

public class RecurringItem
{
    [Key]
    public int Id { get; set; }
    
    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    
    public Frequency Frequency { get; set; } = Frequency.Monthly;
    
    public DateTime NextDate { get; set; } = DateTime.Now;
    
    public int CategoryId { get; set; }
    public virtual Category? Category { get; set; }
    
    public int AccountId { get; set; }
    public virtual Account? Account { get; set; }
    
    public TransactionType Type { get; set; }
    
    [MaxLength(3)]
    public string CurrencyCode { get; set; } = "GBP";
    
    public bool IsActive { get; set; } = true;
}
