using System.ComponentModel.DataAnnotations;

namespace MoneyTracker2026.Models
{
    public class Currency
    {
        [Key]
        [MaxLength(3)]
        public string Code { get; set; } = "GBP";

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = "British Pound";

        [MaxLength(5)]
        public string Symbol { get; set; } = "£";

        public decimal ExchangeRateToBase { get; set; } = 1.0m;

        public bool IsBase { get; set; } = false;

        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}
