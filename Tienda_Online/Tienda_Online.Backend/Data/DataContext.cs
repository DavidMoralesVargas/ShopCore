using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Tienda_Online.Shared.Entidades;

namespace Tienda_Online.Backend.Data
{
    public class DataContext : IdentityDbContext<Usuario>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        public DbSet<Producto> Productos { get; set; }
        public DbSet<PromocionProducto> PromocionesProducto { get; set; }
        public DbSet<InformeProducto> informesProducto { get; set; }
        public DbSet<CarritoDeCompra> CarritosDeCompra { get; set; }
        public DbSet<Factura> Facturas {  get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Producto>().HasIndex(p => p.Nombre).IsUnique();
        }
    }
}
