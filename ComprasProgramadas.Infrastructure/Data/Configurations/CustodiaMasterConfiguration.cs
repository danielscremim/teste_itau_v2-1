using ComprasProgramadas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprasProgramadas.Infrastructure.Data.Configurations;

public class CustodiaMasterConfiguration : IEntityTypeConfiguration<CustodiaMaster>
{
    public void Configure(EntityTypeBuilder<CustodiaMaster> builder)
    {
        builder.ToTable("custodia_master");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();

        // Cada ticker aparece uma única vez na master (uma linha por ativo)
        builder.Property(c => c.Ticker).IsRequired().HasMaxLength(10);
        builder.HasIndex(c => c.Ticker).IsUnique();

        builder.Property(c => c.Quantidade).IsRequired();
        builder.Property(c => c.PrecoMedio).IsRequired().HasColumnType("decimal(15,6)");
        builder.Property(c => c.DataUltimaAtualizacao).IsRequired();
        builder.Property(c => c.CreatedAt).IsRequired();
    }
}
