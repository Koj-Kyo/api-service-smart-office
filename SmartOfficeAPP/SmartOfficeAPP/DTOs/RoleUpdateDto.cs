using System.ComponentModel.DataAnnotations;

namespace SmartOfficeAPP.DTOs;

public class RoleUpdateDto
{
    [Required(ErrorMessage = "Le nom du rôle est requis.")]
    public string RoleName { get; set; }
}
