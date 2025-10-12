using System.ComponentModel.DataAnnotations;

namespace SmartOfficeAPP.DTOs.Request;

public class RegisterDto
{
    [Required(ErrorMessage = "Le nom d'utilisateur est requis.")]
    [StringLength(50, ErrorMessage = "Le nom d'utilisateur ne peut pas dépasser 50 caractères.")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "L'adresse e-mail est requise.")]
    [EmailAddress(ErrorMessage = "L'adresse e-mail n'est pas valide.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Le nom complet est requis.")]
    [StringLength(100, ErrorMessage = "Le nom complet ne peut pas dépasser 100 caractères.")]
    public string FullName { get; set; }

    [Required(ErrorMessage = "Le mot de passe est requis.")]
    [StringLength(100, ErrorMessage = "Le mot de passe doit contenir au moins {2} caractères.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirmer le mot de passe")]
    [Compare("Password", ErrorMessage = "Le mot de passe et le mot de passe de confirmation ne correspondent pas.")]
    public string ConfirmPassword { get; set; }
}
