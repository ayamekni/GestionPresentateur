using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionPresentateur.Models
{
    public class Numero
    {
        [Key]
        public string CodeN { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le titre du numéro est obligatoire")]
        [Display(Name = "Titre du numéro")]
        public string Titre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La durée est obligatoire")]
        [Display(Name = "Durée (minutes)")]
        [Range(1, 120, ErrorMessage = "La durée doit être entre 1 et 120 minutes")]
        public int Duree { get; set; }

        [Required(ErrorMessage = "Le présentateur est obligatoire")]
        [Display(Name = "Présentateur")]
        public string CodeP { get; set; } = string.Empty;

        [ForeignKey("CodeP")]
        public virtual Presentateur? Presentateur { get; set; }

        [Required]
        [Display(Name = "Date de représentation")]
        [DataType(DataType.DateTime)]
        public DateTime DateRepresentation { get; set; }

        public virtual ICollection<NumeroRegistration> Registrations { get; set; } = new List<NumeroRegistration>();

        [NotMapped]
        public bool IsUpcoming => DateRepresentation > DateTime.Now;
    }
}