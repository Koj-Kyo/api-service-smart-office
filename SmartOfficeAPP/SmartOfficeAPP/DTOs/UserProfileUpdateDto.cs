using System.ComponentModel.DataAnnotations;

namespace SmartOfficeAPP.DTOs;

public class UserProfileUpdateDto
{
    [Required(ErrorMessage = "L'adresse e-mail est requise.")]
    [EmailAddress(ErrorMessage = "L'adresse e-mail n'est pas valide.")]
    public string Email { get; set; }

    // Validation optionnelle pour le nom complet
    [MaxLength(100)]
    public string FullName { get; set; }
}
