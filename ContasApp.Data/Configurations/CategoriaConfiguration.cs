using ContasApp.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContasApp.Data.Configurations
{
    public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
    {
        public void Configure(EntityTypeBuilder<Categoria> builder)
        {
            //nome da tabela no banco de dados
            builder.ToTable("CATEGORIA");

            //chave primária
            builder.HasKey(c => c.Id);

            //mapeando o campo 'Id'
            builder.Property(c => c.Id)
                .HasColumnName("ID");

            //mapeando o campo 'Nome'
            builder.Property(c => c.Nome)
                .HasColumnName("NOME")
                .HasMaxLength(150)
                .IsRequired();

            //mapeando o campo 'Tipo'
            builder.Property(c => c.Tipo)
                .HasColumnName("TIPO")
                .IsRequired();

            //mapeando o campo 'UsuarioId'
            builder.Property(c => c.UsuarioId)
                .HasColumnName("USUARIO_ID")
                .IsRequired();

            //mapeando o relacionamento OneToMany
            builder.HasOne(c => c.Usuario) //Categoria TEM 1 Usuário
                .WithMany(u => u.Categorias) //Usuário TEM MUITAS Categorias
                .HasForeignKey(c => c.UsuarioId) //Chave estrangeira
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}



