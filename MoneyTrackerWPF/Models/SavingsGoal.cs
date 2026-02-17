using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneyTracker2026.Models;

public class SavingsGoal
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal TargetAmount { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal CurrentAmount { get; set; }
    
    public DateTime? TargetDate { get; set; }
    
    [MaxLength(10)]
    public string Icon { get; set; } = "🎯";
}
