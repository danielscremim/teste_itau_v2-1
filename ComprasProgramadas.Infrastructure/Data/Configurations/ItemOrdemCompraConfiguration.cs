using ComprasProgramadas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprasProgramadas.Infrastructure.Data.Configurations;

public class ItemOrdemCompraConfiguration : IEntityTypeConfiguration<ItemOrdemCompra>
{
    public void Configure(EntityTypeBuilder<ItemOrdemCompra> builder)
    {
        builder.ToTable("itens_ordem_compra");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedOnAdd();

        builder.Property(i => i.OrdemId).IsRequired();
        builder.Property(i => i.Ticker).IsRequired().HasMaxLength(10);
        builder.Property(i => i.ValorAlvo).IsRequired().HasColumnType("decimal(15,2)");
        builder.Property(i => i.CotacaoFechamento).IsRequired().HasColumnType("decimal(15,6)");
        builder.Property(i => i.QuantidadeCalculada).IsRequired();
        builder.Property(i => i.SaldoMasterDescontado).IsRequired();
        builder.Property(i => i.QuantidadeAComprar).IsRequired();
        builder.Property(i => i.QtdLotePadrao).IsRequired();
        builder.Property(i => i.QtdFracionario).IsRequired();
        builder.Property(i => i.TickerFracionario).HasMaxLength(11); // nullable
        builder.Property(i => i.CreatedAt).IsRequired();

        builder.HasOne(i => i.Ordem)
            .WithMany(o => o.Itens)
            .HasForeignKey(i => i.OrdemId);
    }
}
