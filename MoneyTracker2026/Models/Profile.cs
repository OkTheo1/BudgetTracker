using System.ComponentModel.DataAnnotations;

namespace MoneyTracker2026.Models
{
    public class Profile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string DataFilePath { get; set; } = string.Empty;

        public string? PinHash { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime LastAccessedAt { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;
    }
}
