using Microsoft.EntityFrameworkCore;
using UniEletronicsProjetoUninove.Models;

namespace UniEletronicsProjetoUninove.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Produto> Produtos { get; set; }
        public DbSet<ImagemProduto> ImagensProdutos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Gmail)
                .IsUnique();

            // Garante no próprio banco que só pode existir UM administrador
            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Role)
                .IsUnique()
                .HasFilter("\"Role\" = 'Administrador'")
                .HasDatabaseName("IX_Usuarios_Role_Administrador");
        }
    }
}