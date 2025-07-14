using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionPresentateur.Models
{
    public class NumeroRegistration
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string NumeroId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [ForeignKey("NumeroId")]
        public virtual Numero Numero { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.Now;
    }
}