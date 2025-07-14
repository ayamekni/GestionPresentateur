namespace GestionPresentateur.Models.ViewModels
{
    public class HomeViewModel
    {
        public List<Numero> UpcomingNumeros { get; set; } = new List<Numero>();
        public List<Numero> PastNumeros { get; set; } = new List<Numero>();
        public Dictionary<string, bool> UserRegistrations { get; set; } = new Dictionary<string, bool>();
    }
}