using System.ComponentModel.DataAnnotations;

namespace SmartOfficeAPP.DTOs;

public class NotificationDto
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string Titre { get; set; } = string.Empty;

    [Required]
    public string Message { get; set; } = string.Empty;

    public DateTime Horodatage { get; set; }
    public bool EstLue { get; set; }
}
