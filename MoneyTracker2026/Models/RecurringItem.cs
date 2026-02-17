using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneyTracker2026.Models
{
    public enum Frequency
    {
        Daily = 0,
        Weekly = 1,
        Monthly = 2,
        Yearly = 3
    }

    public class RecurringItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public decimal Amount { get; set; }

        public Frequency Frequency { get; set; } = Frequency.Monthly;

        [Required]
        public DateTime NextDate { get; set; }

        public int? CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        public int? AccountId { get; set; }

        [ForeignKey("AccountId")]
        public virtual Account? Account { get; set; }

        public TransactionType Type { get; set; }

        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "GBP";

        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
    }
}
