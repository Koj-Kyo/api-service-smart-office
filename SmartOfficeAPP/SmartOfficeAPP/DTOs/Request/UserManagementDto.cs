using System.ComponentModel.DataAnnotations;

namespace SmartOfficeAPP.DTOs.Request;

public class CreateUserDto
{
    [Required(ErrorMessage = "Le nom d'utilisateur est requis.")]
    [StringLength(50, ErrorMessage = "Le nom d'utilisateur ne peut pas dépasser 50 caractères.")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'adresse e-mail est requise.")]
    [EmailAddress(ErrorMessage = "L'adresse e-mail n'est pas valide.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le nom complet est requis.")]
    [StringLength(100, ErrorMessage = "Le nom complet ne peut pas dépasser 100 caractères.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le mot de passe est requis.")]
    [StringLength(100, ErrorMessage = "Le mot de passe doit contenir au moins {2} caractères.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [StringLength(15, ErrorMessage = "Le numéro de téléphone ne peut pas dépasser 15 caractères.")]
    [Phone(ErrorMessage = "Le format du numéro de téléphone n'est pas valide.")]
    public string? PhoneNumber { get; set; }

    public bool EmailConfirmed { get; set; } = true;
    
    public IList<string> Roles { get; set; } = new List<string>();
}

public class UpdateUserDto
{
    [Required(ErrorMessage = "Le nom d'utilisateur est requis.")]
    [StringLength(50, ErrorMessage = "Le nom d'utilisateur ne peut pas dépasser 50 caractères.")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "L'adresse e-mail est requise.")]
    [EmailAddress(ErrorMessage = "L'adresse e-mail n'est pas valide.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le nom complet est requis.")]
    [StringLength(100, ErrorMessage = "Le nom complet ne peut pas dépasser 100 caractères.")]
    public string FullName { get; set; } = string.Empty;

    [StringLength(15, ErrorMessage = "Le numéro de téléphone ne peut pas dépasser 15 caractères.")]
    [Phone(ErrorMessage = "Le format du numéro de téléphone n'est pas valide.")]
    public string? PhoneNumber { get; set; }

    public bool EmailConfirmed { get; set; }
    public bool LockoutEnabled { get; set; }
}

public class CreateRoleDto
{
    [Required(ErrorMessage = "Le nom du rôle est requis.")]
    [StringLength(50, ErrorMessage = "Le nom du rôle ne peut pas dépasser 50 caractères.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "La description ne peut pas dépasser 200 caractères.")]
    public string? Description { get; set; }
}

public class UpdateRoleDto
{
    [Required(ErrorMessage = "Le nom du rôle est requis.")]
    [StringLength(50, ErrorMessage = "Le nom du rôle ne peut pas dépasser 50 caractères.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "La description ne peut pas dépasser 200 caractères.")]
    public string? Description { get; set; }
}

public class AssignRolesDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public IList<string> RoleNames { get; set; } = new List<string>();
}