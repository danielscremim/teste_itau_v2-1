using ComprasProgramadas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprasProgramadas.Infrastructure.Data.Configurations;

public class HistoricoValorMensalConfiguration : IEntityTypeConfiguration<HistoricoValorMensal>
{
    public void Configure(EntityTypeBuilder<HistoricoValorMensal> builder)
    {
        builder.ToTable("historico_valor_mensal");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).ValueGeneratedOnAdd();

        builder.Property(h => h.ClienteId).IsRequired();
        builder.Property(h => h.ValorAnterior).IsRequired().HasColumnType("decimal(15,2)");
        builder.Property(h => h.ValorNovo).IsRequired().HasColumnType("decimal(15,2)");
        builder.Property(h => h.DataAlteracao).IsRequired();
        builder.Property(h => h.CreatedAt).IsRequired();

        builder.HasOne(h => h.Cliente)
            .WithMany(c => c.HistoricoValorMensal)
            .HasForeignKey(h => h.ClienteId);
    }
}
