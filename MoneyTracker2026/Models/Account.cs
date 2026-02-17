using System.ComponentModel.DataAnnotations;

namespace MoneyTracker2026.Models
{
    public class Account
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public decimal Balance { get; set; } = 0;

        [MaxLength(50)]
        public string Type { get; set; } = "Checking"; // Checking, Savings, Credit, Investment

        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "GBP";

        // Navigation property
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
