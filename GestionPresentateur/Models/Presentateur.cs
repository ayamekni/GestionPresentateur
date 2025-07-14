using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionPresentateur.Models
{
    public class Presentateur
    {
        [Key]
        public string CodeP { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom du présentateur est obligatoire")]
        [Display(Name = "Nom du présentateur")]
        public string NomP { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le rôle est obligatoire")]
        [Display(Name = "Rôle")]
        public string CodeR { get; set; } = string.Empty;

        [ForeignKey("CodeR")]
        public virtual Role? Role { get; set; }

        public virtual ICollection<Numero> Numeros { get; set; } = new List<Numero>();
    }
}