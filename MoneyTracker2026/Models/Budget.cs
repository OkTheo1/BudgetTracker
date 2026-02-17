using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneyTracker2026.Models
{
    public class Budget
    {
        [Key]
        public int Id { get; set; }

        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        [Required]
        public int Month { get; set; } // 1-12

        [Required]
        public int Year { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public decimal Spent { get; set; } = 0;

        // Computed property
        [NotMapped]
        public decimal Remaining => Amount - Spent;

        [NotMapped]
        public double PercentUsed => Amount > 0 ? (double)(Spent / Amount) * 100 : 0;
    }
}
