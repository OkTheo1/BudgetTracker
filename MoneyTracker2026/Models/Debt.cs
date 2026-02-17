using System.ComponentModel.DataAnnotations;

namespace MoneyTracker2026.Models
{
    public enum PayoffMethod
    {
        Snowball = 0, // Pay smallest balance first
        Avalanche = 1 // Pay highest interest rate first
    }

    public class Debt
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public decimal Balance { get; set; }

        public decimal InterestRatePercent { get; set; } // Annual interest rate

        public decimal MinPayment { get; set; }

        public PayoffMethod PayoffMethod { get; set; } = PayoffMethod.Avalanche;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsPaidOff { get; set; } = false;

        public DateTime? PaidOffAt { get; set; }
    }
}
