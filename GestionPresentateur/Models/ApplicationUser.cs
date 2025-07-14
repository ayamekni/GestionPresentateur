using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace GestionPresentateur.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [Display(Name = "Prénom")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Nom")]
        public string LastName { get; set; }

        [Display(Name = "Photo de profil")]
        public string? ProfilePictureUrl { get; set; } // Made nullable with ?

        public virtual ICollection<NumeroRegistration> RegisteredNumeros { get; set; }
    }
}