using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GestionPresentateur.Models;
using GestionPresentateur.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace GestionPresentateur.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger<AdminController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        #region Main Pages

        public IActionResult Index()
        {
            _logger.LogInformation("Admin Index page accessed");
            return View();
        }

        public async Task<IActionResult> Dashboard()
        {
            try
            {
                _logger.LogInformation("Admin Dashboard accessed");

                ViewBag.NumeroCount = await _context.Numeros.CountAsync();
                ViewBag.PresentateurCount = await _context.Presentateurs.CountAsync();
                ViewBag.RoleCount = await _context.Roles.CountAsync();
                ViewBag.UserCount = await _userManager.Users.CountAsync();

                ViewBag.UpcomingNumeros = await _context.Numeros
                    .Include(n => n.Presentateur)
                    .ThenInclude(p => p!.Role)
                    .Where(n => n.DateRepresentation > DateTime.Now &&
                               n.Presentateur != null &&
                               n.Presentateur.Role != null)
                    .OrderBy(n => n.DateRepresentation)
                    .Take(5)
                    .ToListAsync();

                ViewBag.RecentRegistrations = await _context.NumeroRegistrations
                    .Include(r => r.User)
                    .Include(r => r.Numero)
                    .Where(r => r.User != null && r.Numero != null)
                    .OrderByDescending(r => r.RegistrationDate)
                    .Take(10)
                    .ToListAsync();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                return View("Error");
            }
        }

        #endregion

        #region Presentateur Management

        public async Task<IActionResult> Presentateurs()
        {
            try
            {
                var presentateurs = await _context.Presentateurs
                    .Include(p => p.Role)
                    .ToListAsync();
                return View(presentateurs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading presentateurs");
                return View(new List<Presentateur>());
            }
        }

        public async Task<IActionResult> PresentateurDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var presentateur = await _context.Presentateurs
                    .Include(p => p.Role)
                    .Include(p => p.Numeros)
                    .FirstOrDefaultAsync(p => p.CodeP == id);

                if (presentateur == null)
                {
                    return NotFound();
                }

                return View(presentateur);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading presentateur details for {Id}", id);
                return View("Error");
            }
        }

        public async Task<IActionResult> CreatePresentateur()
        {
            try
            {
                ViewBag.Roles = await _context.Roles.ToListAsync();
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading roles for CreatePresentateur");
                ViewBag.Roles = new List<Role>();
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePresentateur(Presentateur presentateur)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if CodeP already exists
                    var existingPresentateur = await _context.Presentateurs
                        .FirstOrDefaultAsync(p => p.CodeP == presentateur.CodeP);

                    if (existingPresentateur != null)
                    {
                        ModelState.AddModelError("CodeP", "Ce code de présentateur existe déjà");
                    }
                    else
                    {
                        // Verify the role exists
                        var roleExists = await _context.Roles
                            .AnyAsync(r => r.CodeR == presentateur.CodeR);

                        if (!roleExists)
                        {
                            ModelState.AddModelError("CodeR", "Le rôle sélectionné n'existe pas");
                        }
                        else
                        {
                            _context.Add(presentateur);
                            await _context.SaveChangesAsync();
                            TempData["SuccessMessage"] = $"Le présentateur '{presentateur.NomP}' a été créé avec succès!";
                            return RedirectToAction(nameof(Presentateurs));
                        }
                    }
                }

                ViewBag.Roles = await _context.Roles.ToListAsync();
                return View(presentateur);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating presentateur");
                ModelState.AddModelError("", "Une erreur s'est produite lors de la création");
                ViewBag.Roles = await _context.Roles.ToListAsync();
                return View(presentateur);
            }
        }

        public async Task<IActionResult> EditPresentateur(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var presentateur = await _context.Presentateurs.FindAsync(id);

                if (presentateur == null)
                {
                    return NotFound();
                }

                ViewBag.Roles = await _context.Roles.ToListAsync();
                return View(presentateur);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading presentateur for edit {Id}", id);
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPresentateur(string id, Presentateur presentateur)
        {
            if (id != presentateur.CodeP)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    _context.Update(presentateur);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Le présentateur '{presentateur.NomP}' a été modifié avec succès!";
                    return RedirectToAction(nameof(Presentateurs));
                }

                ViewBag.Roles = await _context.Roles.ToListAsync();
                return View(presentateur);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PresentateurExists(presentateur.CodeP))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating presentateur {Id}", id);
                ModelState.AddModelError("", "Une erreur s'est produite lors de la modification");
                ViewBag.Roles = await _context.Roles.ToListAsync();
                return View(presentateur);
            }
        }

        public async Task<IActionResult> DeletePresentateur(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var presentateur = await _context.Presentateurs
                    .Include(p => p.Role)
                    .FirstOrDefaultAsync(p => p.CodeP == id);

                if (presentateur == null)
                {
                    return NotFound();
                }

                return View(presentateur);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading presentateur for delete {Id}", id);
                return View("Error");
            }
        }

        [HttpPost, ActionName("DeletePresentateur")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePresentateurConfirmed(string id)
        {
            try
            {
                var presentateur = await _context.Presentateurs.FindAsync(id);
                if (presentateur != null)
                {
                    _context.Presentateurs.Remove(presentateur);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Le présentateur a été supprimé avec succès!";
                }
                return RedirectToAction(nameof(Presentateurs));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting presentateur {Id}", id);
                TempData["ErrorMessage"] = "Impossible de supprimer ce présentateur (il est peut-être utilisé dans des numéros)";
                return RedirectToAction(nameof(Presentateurs));
            }
        }

        private bool PresentateurExists(string id)
        {
            return _context.Presentateurs.Any(e => e.CodeP == id);
        }

        #endregion

        #region Role Management

        public async Task<IActionResult> Roles()
        {
            try
            {
                var roles = await _context.Roles
                    .Include(r => r.Presentateurs)
                    .ToListAsync();
                return View(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading roles");
                return View(new List<Role>());
            }
        }

        public async Task<IActionResult> RoleDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var role = await _context.Roles
                    .Include(r => r.Presentateurs)
                    .FirstOrDefaultAsync(r => r.CodeR == id);

                if (role == null)
                {
                    return NotFound();
                }

                return View(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading role details for {Id}", id);
                return View("Error");
            }
        }

        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(Role role)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if CodeR already exists
                    var existingRole = await _context.Roles
                        .FirstOrDefaultAsync(r => r.CodeR == role.CodeR);

                    if (existingRole != null)
                    {
                        ModelState.AddModelError("CodeR", "Ce code de rôle existe déjà");
                        return View(role);
                    }

                    _context.Add(role);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Le rôle '{role.Libelle}' a été créé avec succès!";
                    return RedirectToAction(nameof(Roles));
                }
                return View(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role");
                ModelState.AddModelError("", "Une erreur s'est produite lors de la création");
                return View(role);
            }
        }

        public async Task<IActionResult> EditRole(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var role = await _context.Roles.FindAsync(id);

                if (role == null)
                {
                    return NotFound();
                }

                return View(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading role for edit {Id}", id);
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRole(string id, Role role)
        {
            if (id != role.CodeR)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    _context.Update(role);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Le rôle '{role.Libelle}' a été modifié avec succès!";
                    return RedirectToAction(nameof(Roles));
                }
                return View(role);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoleExists(role.CodeR))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating role {Id}", id);
                ModelState.AddModelError("", "Une erreur s'est produite lors de la modification");
                return View(role);
            }
        }

        public async Task<IActionResult> DeleteRole(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var role = await _context.Roles
                    .Include(r => r.Presentateurs)
                    .FirstOrDefaultAsync(r => r.CodeR == id);

                if (role == null)
                {
                    return NotFound();
                }

                return View(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading role for delete {Id}", id);
                return View("Error");
            }
        }

        [HttpPost, ActionName("DeleteRole")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRoleConfirmed(string id)
        {
            try
            {
                var role = await _context.Roles.FindAsync(id);
                if (role != null)
                {
                    _context.Roles.Remove(role);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Le rôle a été supprimé avec succès!";
                }
                return RedirectToAction(nameof(Roles));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting role {Id}", id);
                TempData["ErrorMessage"] = "Impossible de supprimer ce rôle (il est peut-être utilisé par des présentateurs)";
                return RedirectToAction(nameof(Roles));
            }
        }

        private bool RoleExists(string id)
        {
            return _context.Roles.Any(e => e.CodeR == id);
        }

        #endregion

        #region Numero Management

        public async Task<IActionResult> Numeros()
        {
            try
            {
                var numeros = await _context.Numeros
                    .Include(n => n.Presentateur)
                    .ThenInclude(p => p!.Role)
                    .Where(n => n.Presentateur != null && n.Presentateur.Role != null)
                    .OrderBy(n => n.DateRepresentation)
                    .ToListAsync();

                return View(numeros);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading numeros");
                return View(new List<Numero>());
            }
        }

        public async Task<IActionResult> NumeroDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var numero = await _context.Numeros
                    .Include(n => n.Presentateur)
                    .ThenInclude(p => p!.Role)
                    .Include(n => n.Registrations)
                        .ThenInclude(r => r.User)
                    .FirstOrDefaultAsync(n => n.CodeN == id);

                if (numero == null)
                {
                    return NotFound();
                }

                return View(numero);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading numero details for {Id}", id);
                return View("Error");
            }
        }

        public async Task<IActionResult> CreateNumero()
        {
            try
            {
                var presentateurs = await _context.Presentateurs
                    .Include(p => p.Role)
                    .Where(p => p.Role != null)
                    .ToListAsync();

                if (!presentateurs.Any())
                {
                    TempData["ErrorMessage"] = "Aucun présentateur avec un rôle valide trouvé. Veuillez d'abord créer des présentateurs.";
                }

                ViewBag.Presentateurs = presentateurs;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading presentateurs for CreateNumero");
                ViewBag.Presentateurs = new List<Presentateur>();
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateNumero(Numero numero)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Check if CodeN already exists
                    var existingNumero = await _context.Numeros
                        .FirstOrDefaultAsync(n => n.CodeN == numero.CodeN);

                    if (existingNumero != null)
                    {
                        ModelState.AddModelError("CodeN", "Ce code de numéro existe déjà");
                    }
                    else
                    {
                        // Verify the presenter exists
                        var presenter = await _context.Presentateurs
                            .Include(p => p.Role)
                            .FirstOrDefaultAsync(p => p.CodeP == numero.CodeP);

                        if (presenter == null)
                        {
                            ModelState.AddModelError("CodeP", "Le présentateur sélectionné n'existe pas");
                        }
                        else
                        {
                            _context.Numeros.Add(numero);
                            await _context.SaveChangesAsync();
                            TempData["SuccessMessage"] = $"Le numéro '{numero.Titre}' a été créé avec succès!";
                            return RedirectToAction(nameof(Numeros));
                        }
                    }
                }

                ViewBag.Presentateurs = await _context.Presentateurs
                    .Include(p => p.Role)
                    .Where(p => p.Role != null)
                    .ToListAsync();

                return View(numero);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating numero");
                ModelState.AddModelError("", "Une erreur s'est produite lors de la création");
                ViewBag.Presentateurs = await _context.Presentateurs
                    .Include(p => p.Role)
                    .Where(p => p.Role != null)
                    .ToListAsync();
                return View(numero);
            }
        }

        public async Task<IActionResult> EditNumero(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var numero = await _context.Numeros.FindAsync(id);

                if (numero == null)
                {
                    return NotFound();
                }

                ViewBag.Presentateurs = await _context.Presentateurs
                    .Include(p => p.Role)
                    .Where(p => p.Role != null)
                    .ToListAsync();

                return View(numero);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading numero for edit {Id}", id);
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditNumero(string id, Numero numero)
        {
            if (id != numero.CodeN)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    _context.Update(numero);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Le numéro '{numero.Titre}' a été modifié avec succès!";
                    return RedirectToAction(nameof(Numeros));
                }

                ViewBag.Presentateurs = await _context.Presentateurs
                    .Include(p => p.Role)
                    .Where(p => p.Role != null)
                    .ToListAsync();

                return View(numero);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NumeroExists(numero.CodeN))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating numero {Id}", id);
                ModelState.AddModelError("", "Une erreur s'est produite lors de la modification");
                ViewBag.Presentateurs = await _context.Presentateurs
                    .Include(p => p.Role)
                    .Where(p => p.Role != null)
                    .ToListAsync();
                return View(numero);
            }
        }

        public async Task<IActionResult> DeleteNumero(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var numero = await _context.Numeros
                    .Include(n => n.Presentateur)
                    .ThenInclude(p => p!.Role)
                    .FirstOrDefaultAsync(n => n.CodeN == id);

                if (numero == null)
                {
                    return NotFound();
                }

                return View(numero);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading numero for delete {Id}", id);
                return View("Error");
            }
        }

        [HttpPost, ActionName("DeleteNumero")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteNumeroConfirmed(string id)
        {
            try
            {
                var numero = await _context.Numeros.FindAsync(id);
                if (numero != null)
                {
                    _context.Numeros.Remove(numero);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Le numéro a été supprimé avec succès!";
                }
                return RedirectToAction(nameof(Numeros));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting numero {Id}", id);
                TempData["ErrorMessage"] = "Impossible de supprimer ce numéro (il y a peut-être des réservations)";
                return RedirectToAction(nameof(Numeros));
            }
        }

        private bool NumeroExists(string id)
        {
            return _context.Numeros.Any(e => e.CodeN == id);
        }

        #endregion

        #region User Management

        public async Task<IActionResult> Users()
        {
            try
            {
                var users = await _userManager.Users.ToListAsync();
                return View(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users");
                return View(new List<ApplicationUser>());
            }
        }

        public async Task<IActionResult> UserDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id);

                if (user == null)
                {
                    return NotFound();
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                ViewBag.UserRoles = userRoles;

                var registrations = await _context.NumeroRegistrations
                    .Where(r => r.UserId == id)
                    .Include(r => r.Numero)
                    .ThenInclude(n => n!.Presentateur)
                    .ThenInclude(p => p!.Role)
                    .ToListAsync();

                ViewBag.Registrations = registrations;

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user details for {Id}", id);
                return View("Error");
            }
        }

        #endregion
    }
}