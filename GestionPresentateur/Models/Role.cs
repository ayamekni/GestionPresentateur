using System.ComponentModel.DataAnnotations;

namespace GestionPresentateur.Models
{
    public class Role
    {
        [Key]
        public string CodeR { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le libellé du rôle est obligatoire")]
        [Display(Name = "Libellé du rôle")]
        public string Libelle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prix est obligatoire")]
        [Display(Name = "Prix")]
        [DataType(DataType.Currency)]
        public decimal Prix { get; set; }

        public virtual ICollection<Presentateur> Presentateurs { get; set; } = new List<Presentateur>();
    }
}