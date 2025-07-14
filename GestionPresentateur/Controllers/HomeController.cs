using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using GestionPresentateur.Models;
using GestionPresentateur.Models.ViewModels;
using GestionPresentateur.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GestionPresentateur.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<HomeController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Home Index accessed");

                var viewModel = new HomeViewModel();

                // Get upcoming shows - SAFE query
                try
                {
                    viewModel.UpcomingNumeros = await _context.Numeros
                        .Include(n => n.Presentateur)
                        .ThenInclude(p => p!.Role)
                        .Where(n => n.DateRepresentation > DateTime.Now &&
                                   n.Presentateur != null &&
                                   n.Presentateur.Role != null)
                        .OrderBy(n => n.DateRepresentation)
                        .ToListAsync();

                    _logger.LogInformation($"Found {viewModel.UpcomingNumeros.Count} upcoming numeros");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading upcoming numeros");
                    viewModel.UpcomingNumeros = new List<Numero>();
                }

                // Get past shows - SAFE query
                try
                {
                    viewModel.PastNumeros = await _context.Numeros
                        .Include(n => n.Presentateur)
                        .ThenInclude(p => p!.Role)
                        .Where(n => n.DateRepresentation <= DateTime.Now &&
                                   n.Presentateur != null &&
                                   n.Presentateur.Role != null)
                        .OrderByDescending(n => n.DateRepresentation)
                        .ToListAsync();

                    _logger.LogInformation($"Found {viewModel.PastNumeros.Count} past numeros");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading past numeros");
                    viewModel.PastNumeros = new List<Numero>();
                }

                // Get user registrations ONLY if user is authenticated
                if (User.Identity != null && User.Identity.IsAuthenticated)
                {
                    try
                    {
                        var user = await _userManager.GetUserAsync(User);

                        if (user != null && !string.IsNullOrEmpty(user.Id))
                        {
                            _logger.LogInformation($"Loading registrations for user: {user.Id}");

                            // SAFE way to get user registrations
                            var userId = user.Id; // Store in variable to avoid closure issues
                            var registrations = await _context.NumeroRegistrations
                                .Where(r => r.UserId == userId)
                                .Select(r => r.NumeroId)
                                .ToListAsync();

                            _logger.LogInformation($"Found {registrations.Count} registrations for user");

                            // Build registration dictionary safely
                            foreach (var numero in viewModel.UpcomingNumeros)
                            {
                                if (!string.IsNullOrEmpty(numero.CodeN))
                                {
                                    viewModel.UserRegistrations[numero.CodeN] = registrations.Contains(numero.CodeN);
                                }
                            }
                        }
                        else
                        {
                            _logger.LogWarning("User object is null or has no ID");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error loading user registrations");
                        // Continue without registrations rather than failing
                    }
                }
                else
                {
                    _logger.LogInformation("User not authenticated, skipping registration check");
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Home Index");
            
                return View(new HomeViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterForShow(string numeroId)
        {
            try
            {
                _logger.LogInformation($"RegisterForShow called with numeroId: {numeroId}");

                if (!User.Identity?.IsAuthenticated ?? true)
                {
                    _logger.LogWarning("User not authenticated for registration");
                    return RedirectToAction("Login", "Account");
                }

                if (string.IsNullOrEmpty(numeroId))
                {
                    _logger.LogWarning("NumeroId is null or empty");
                    TempData["ErrorMessage"] = "Numéro de spectacle invalide";
                    return RedirectToAction(nameof(Index));
                }

                var user = await _userManager.GetUserAsync(User);

                if (user == null || string.IsNullOrEmpty(user.Id))
                {
                    _logger.LogWarning("User object is null or has no ID");
                    return RedirectToAction("Login", "Account");
                }

                _logger.LogInformation($"User {user.Id} attempting to register for show {numeroId}");

                
                var userId = user.Id; // Store in variable
                var existingRegistration = await _context.NumeroRegistrations
                    .Where(r => r.UserId == userId && r.NumeroId == numeroId)
                    .FirstOrDefaultAsync();

                if (existingRegistration == null)
                {
                    
                    var numeroExists = await _context.Numeros
                        .AnyAsync(n => n.CodeN == numeroId);

                    if (!numeroExists)
                    {
                        _logger.LogWarning($"Numero {numeroId} does not exist");
                        TempData["ErrorMessage"] = "Le spectacle sélectionné n'existe pas";
                        return RedirectToAction(nameof(Index));
                    }

                    // Register for the show
                    var registration = new NumeroRegistration
                    {
                        UserId = userId,
                        NumeroId = numeroId,
                        RegistrationDate = DateTime.Now
                    };

                    _context.NumeroRegistrations.Add(registration);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"User {userId} successfully registered for show {numeroId}");
                    TempData["SuccessMessage"] = "Inscription réussie!";
                }
                else
                {
                    _logger.LogInformation($"User {userId} already registered for show {numeroId}");
                    TempData["InfoMessage"] = "Vous êtes déjà inscrit à ce spectacle";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RegisterForShow");
                TempData["ErrorMessage"] = "Erreur lors de l'inscription";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelRegistration(string numeroId)
        {
            try
            {
                _logger.LogInformation($"CancelRegistration called with numeroId: {numeroId}");

                if (!User.Identity?.IsAuthenticated ?? true)
                {
                    _logger.LogWarning("User not authenticated for cancellation");
                    return RedirectToAction("Login", "Account");
                }

                if (string.IsNullOrEmpty(numeroId))
                {
                    _logger.LogWarning("NumeroId is null or empty");
                    TempData["ErrorMessage"] = "Numéro de spectacle invalide";
                    return RedirectToAction(nameof(Index));
                }

                var user = await _userManager.GetUserAsync(User);

                if (user == null || string.IsNullOrEmpty(user.Id))
                {
                    _logger.LogWarning("User object is null or has no ID");
                    return RedirectToAction("Login", "Account");
                }

                _logger.LogInformation($"User {user.Id} attempting to cancel registration for show {numeroId}");

                // Find the registration - SAFE query
                var userId = user.Id; // Store in variable
                var registration = await _context.NumeroRegistrations
                    .Where(r => r.UserId == userId && r.NumeroId == numeroId)
                    .FirstOrDefaultAsync();

                if (registration != null)
                {
                    _context.NumeroRegistrations.Remove(registration);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation($"User {userId} successfully cancelled registration for show {numeroId}");
                    TempData["SuccessMessage"] = "Inscription annulée!";
                }
                else
                {
                    _logger.LogWarning($"No registration found for user {userId} and show {numeroId}");
                    TempData["InfoMessage"] = "Aucune inscription trouvée pour ce spectacle";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CancelRegistration");
                TempData["ErrorMessage"] = "Erreur lors de l'annulation";
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}