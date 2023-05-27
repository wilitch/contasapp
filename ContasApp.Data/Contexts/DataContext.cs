using ContasApp.Data.Configurations;
using ContasApp.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContasApp.Data.Contexts
{
    //Regra 1: Herdar a classe DbContext
    public class DataContext : DbContext
    {
        //Regra 2: Sobrescrever o método OnConfiguring
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //definindo o tipo de banco de dados do projeto

            //Banco de dados em memória
            //optionsBuilder.UseInMemoryDatabase(databaseName: "BDContasApp");

            //Banco de dados físico
            //Banco de dados local
            //optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=BDContasApp;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
            //Banco de dados no servidor
            optionsBuilder.UseSqlServer("Data Source=SQL5110.site4now.net;Initial Catalog=db_a99efa_bdcontasapp;User Id=db_a99efa_bdcontasapp_admin;Password=@Adm123!");
        }

        //Regra 3: Sobrescrever o método OnModelCreating
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //adicionar cada classe de configuração criada no projeto
            modelBuilder.ApplyConfiguration(new CategoriaConfiguration());
            modelBuilder.ApplyConfiguration(new ContaConfiguration());
            modelBuilder.ApplyConfiguration(new UsuarioConfiguration());
        }

        //Regra 4: Mapear cada entidade do projeto através do DbSet
        public DbSet<Categoria>? Categoria { get; set; }
        public DbSet<Conta>? Conta { get; set; }
        public DbSet<Usuario>? Usuario { get; set; }
    }
}



