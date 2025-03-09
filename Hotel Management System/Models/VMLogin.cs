namespace Hotel_Management_System.Models;
using System.ComponentModel.DataAnnotations;

public class VMLogin
{
    [Required, EmailAddress]
    public required string Email { get; set; }

    [Required, DataType(DataType.Password)]
    public required string Password { get; set; }

    [Display(Name = "Remember Me")]
    public bool RememberMe { get; set; }
}