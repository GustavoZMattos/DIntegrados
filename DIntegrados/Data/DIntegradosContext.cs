using DIntegrados.Data.Configuracao;
using DIntegrados.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DIntegrados.Data
{
    public class DIntegradosContext : DbContext
    {
        public DIntegradosContext(DbContextOptions options) : base(options)
        {

        }

        protected DIntegradosContext()
        { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Imagem> Imagens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetAssembly(typeof(UsuarioConfiguration)));
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetAssembly(typeof(ImagemConfiguration)));
            base.OnModelCreating(modelBuilder);
        }
    }
}
