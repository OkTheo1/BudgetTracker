using System.ComponentModel.DataAnnotations;

namespace MoneyTracker2026.Models;

public class Profile
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string DataFilePath { get; set; } = string.Empty;
    
    [MaxLength(64)]
    public string? PinHash { get; set; }
    
    public bool IsActive { get; set; } = true;
}
