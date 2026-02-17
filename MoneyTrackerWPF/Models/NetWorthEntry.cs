using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneyTracker2026.Models;

public class NetWorthEntry
{
    [Key]
    public int Id { get; set; }
    
    public DateTime Date { get; set; } = DateTime.Now;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAssets { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalLiabilities { get; set; }
    
    [NotMapped]
    public decimal NetWorth => TotalAssets - TotalLiabilities;
}
