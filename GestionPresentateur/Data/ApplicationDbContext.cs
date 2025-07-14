using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GestionPresentateur.Models;

namespace GestionPresentateur.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Presentateur> Presentateurs { get; set; }
        public new DbSet<Role> Roles { get; set; }
        public DbSet<Numero> Numeros { get; set; }
        public DbSet<NumeroRegistration> NumeroRegistrations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Presentateur>()
                .HasOne(p => p.Role)
                .WithMany(r => r.Presentateurs)
                .HasForeignKey(p => p.CodeR)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Numero>()
                .HasOne(n => n.Presentateur)
                .WithMany(p => p.Numeros)
                .HasForeignKey(n => n.CodeP)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<NumeroRegistration>()
                .HasOne(nr => nr.User)
                .WithMany(u => u.RegisteredNumeros)
                .HasForeignKey(nr => nr.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<NumeroRegistration>()
                .HasOne(nr => nr.Numero)
                .WithMany(n => n.Registrations)
                .HasForeignKey(nr => nr.NumeroId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}