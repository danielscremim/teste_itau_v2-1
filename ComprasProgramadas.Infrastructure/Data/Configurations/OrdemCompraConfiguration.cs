using ComprasProgramadas.Domain.Entities;
using ComprasProgramadas.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprasProgramadas.Infrastructure.Data.Configurations;

public class OrdemCompraConfiguration : IEntityTypeConfiguration<OrdemCompra>
{
    public void Configure(EntityTypeBuilder<OrdemCompra> builder)
    {
        builder.ToTable("ordens_compra");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedOnAdd();

        builder.Property(o => o.CestaId).IsRequired();
        builder.Property(o => o.DataExecucao).IsRequired();

        // DateOnly → MySQL salva como DATE (sem hora)
        builder.Property(o => o.DataReferencia)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(o => o.TotalConsolidado).IsRequired().HasColumnType("decimal(15,2)");

        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<string>(); // salva "Pendente", "Executada" ou "Erro" no banco

        builder.Property(o => o.ArquivoCotacao).HasMaxLength(100);
        builder.Property(o => o.CreatedAt).IsRequired();

        builder.HasOne(o => o.Cesta)
            .WithMany()
            .HasForeignKey(o => o.CestaId);

        builder.HasMany(o => o.Itens)
            .WithOne(i => i.Ordem)
            .HasForeignKey(i => i.OrdemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
