using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace GestionPresentateur.Models.ViewModels
{
    public class ProfileViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Le prénom est obligatoire")]
        [Display(Name = "Prénom")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Le nom est obligatoire")]
        [Display(Name = "Nom")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "L'email est obligatoire")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Format de numéro de téléphone invalide")]
        [Display(Name = "Téléphone")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Photo de profil actuelle")]
        public string CurrentProfilePicture { get; set; }

        [Display(Name = "Nouvelle photo de profil")]
        public IFormFile ProfilePicture { get; set; }
    }
}