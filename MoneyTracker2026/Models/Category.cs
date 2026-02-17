using System.ComponentModel.DataAnnotations;

namespace MoneyTracker2026.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(10)]
        public string IconGlyph { get; set; } = string.Empty;

        [MaxLength(20)]
        public string ColorHex { get; set; } = "#00C4B4";

        public bool IsSystem { get; set; } = false;

        // Navigation property
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
