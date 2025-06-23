using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using web_app_template.Domain.Models.Entities;

namespace web_app_template.Data
{
    public class WebAppTemplateDbContext : IdentityDbContext<CustomIdentityUser>
    {
        public WebAppTemplateDbContext(DbContextOptions<WebAppTemplateDbContext> options) : base(options) { }

        public DbSet<Pokemon> Pokemons { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
