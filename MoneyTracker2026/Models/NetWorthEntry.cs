using System.ComponentModel.DataAnnotations;

namespace MoneyTracker2026.Models
{
    public class NetWorthEntry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.Now;

        [Required]
        public decimal TotalAssets { get; set; }

        [Required]
        public decimal TotalLiabilities { get; set; }

        // Computed
        public decimal NetWorth => TotalAssets - TotalLiabilities;
    }
}
