using ComprasProgramadas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprasProgramadas.Infrastructure.Data.Configurations;

public class CotacaoHistoricaConfiguration : IEntityTypeConfiguration<CotacaoHistorica>
{
    public void Configure(EntityTypeBuilder<CotacaoHistorica> builder)
    {
        builder.ToTable("cotacoes_historicas");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();

        builder.Property(c => c.Ticker).IsRequired().HasMaxLength(12);
        builder.Property(c => c.DataPregao).IsRequired().HasColumnType("date");
        builder.Property(c => c.PrecoAbertura).IsRequired().HasColumnType("decimal(15,6)");
        builder.Property(c => c.PrecoMaximo).IsRequired().HasColumnType("decimal(15,6)");
        builder.Property(c => c.PrecoMinimo).IsRequired().HasColumnType("decimal(15,6)");
        builder.Property(c => c.PrecoFechamento).IsRequired().HasColumnType("decimal(15,6)");
        builder.Property(c => c.PrecoMedioDia).HasColumnType("decimal(15,6)");
        builder.Property(c => c.VolumeNegociado).HasColumnType("decimal(20,2)");
        builder.Property(c => c.ArquivoOrigem).IsRequired().HasMaxLength(100);
        builder.Property(c => c.CreatedAt).IsRequired();

        // Índice único: mesma ação não pode ter duas cotações no mesmo dia
        builder.HasIndex(c => new { c.Ticker, c.DataPregao }).IsUnique();
        builder.HasIndex(c => c.DataPregao);
        builder.HasIndex(c => c.Ticker);
    }
}
