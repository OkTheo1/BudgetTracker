using System.ComponentModel.DataAnnotations;

namespace MoneyTracker2026.Models
{
    public class SavingsGoal
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public decimal TargetAmount { get; set; }

        public decimal CurrentAmount { get; set; } = 0;

        public DateTime? TargetDate { get; set; }

        [MaxLength(10)]
        public string Icon { get; set; } = "🏆";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsAchieved { get; set; } = false;

        public DateTime? AchievedAt { get; set; }
    }
}
