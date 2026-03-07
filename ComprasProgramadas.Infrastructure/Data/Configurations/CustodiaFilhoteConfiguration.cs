using ComprasProgramadas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprasProgramadas.Infrastructure.Data.Configurations;

public class CustodiaFilhoteConfiguration : IEntityTypeConfiguration<CustodiaFilhote>
{
    public void Configure(EntityTypeBuilder<CustodiaFilhote> builder)
    {
        builder.ToTable("custodia_filhote");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();

        builder.Property(c => c.ClienteId).IsRequired();
        builder.Property(c => c.ContaGraficaId).IsRequired();
        builder.Property(c => c.Ticker).IsRequired().HasMaxLength(10);
        builder.Property(c => c.Quantidade).IsRequired();

        // decimal(15,6): preço médio com 6 casas decimais para não perder precisão
        builder.Property(c => c.PrecoMedio).IsRequired().HasColumnType("decimal(15,6)");
        builder.Property(c => c.DataUltimaAtualizacao).IsRequired();
        builder.Property(c => c.CreatedAt).IsRequired();

        // Um cliente não pode ter o mesmo ticker duas vezes na custódia
        builder.HasIndex(c => new { c.ClienteId, c.Ticker }).IsUnique();

        builder.HasOne(c => c.Cliente)
            .WithMany(cl => cl.CustodiasFilhote)
            .HasForeignKey(c => c.ClienteId);

        builder.HasOne(c => c.ContaGrafica)
            .WithMany(cg => cg.CustodiasFilhote)
            .HasForeignKey(c => c.ContaGraficaId);
    }
}
