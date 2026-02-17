using System.ComponentModel.DataAnnotations;

namespace MoneyTracker2026.Models;

public class Category
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(10)]
    public string IconGlyph { get; set; } = "📁";
    
    [MaxLength(10)]
    public string ColorHex { get; set; } = "#5BD787";
    
    public bool IsSystem { get; set; } = false;
    
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();
    public virtual ICollection<RecurringItem> RecurringItems { get; set; } = new List<RecurringItem>();
}
