using ComprasProgramadas.Domain.Entities;
using ComprasProgramadas.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprasProgramadas.Infrastructure.Data.Configurations;

public class ContaGraficaConfiguration : IEntityTypeConfiguration<ContaGrafica>
{
    public void Configure(EntityTypeBuilder<ContaGrafica> builder)
    {
        builder.ToTable("contas_graficas");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();

        builder.Property(c => c.NumeroConta).IsRequired().HasMaxLength(20);
        builder.HasIndex(c => c.NumeroConta).IsUnique();

        // Enum salvo como string no banco (mais legível que número)
        // Ex: "Filhote" em vez de "2"
        builder.Property(c => c.Tipo)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(c => c.ClienteId); // nullable (conta master não tem cliente)
        builder.Property(c => c.DataCriacao).IsRequired();
        builder.Property(c => c.CreatedAt).IsRequired();

        builder.HasMany(c => c.CustodiasFilhote)
            .WithOne(cf => cf.ContaGrafica)
            .HasForeignKey(cf => cf.ContaGraficaId);
    }
}
