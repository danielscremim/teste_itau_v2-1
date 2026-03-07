using ComprasProgramadas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprasProgramadas.Infrastructure.Data.Configurations;

public class DistribuicaoConfiguration : IEntityTypeConfiguration<Distribuicao>
{
    public void Configure(EntityTypeBuilder<Distribuicao> builder)
    {
        builder.ToTable("distribuicoes");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedOnAdd();

        builder.Property(d => d.OrdemId).IsRequired();
        builder.Property(d => d.ClienteId).IsRequired();
        builder.Property(d => d.Ticker).IsRequired().HasMaxLength(10);
        builder.Property(d => d.Quantidade).IsRequired();
        builder.Property(d => d.PrecoUnitario).IsRequired().HasColumnType("decimal(15,6)");
        builder.Property(d => d.ValorOperacao).IsRequired().HasColumnType("decimal(15,2)");

        // decimal(8,6): proporção entre 0 e 1, ex: 0.333333
        builder.Property(d => d.ProporcaoCliente).IsRequired().HasColumnType("decimal(8,6)");
        builder.Property(d => d.ValorIrDedoDuro).IsRequired().HasColumnType("decimal(15,6)");
        builder.Property(d => d.KafkaPublicado).IsRequired();
        builder.Property(d => d.DataDistribuicao).IsRequired();
        builder.Property(d => d.CreatedAt).IsRequired();

        builder.HasOne(d => d.Ordem)
            .WithMany()
            .HasForeignKey(d => d.OrdemId);

        builder.HasOne(d => d.Cliente)
            .WithMany()
            .HasForeignKey(d => d.ClienteId);
    }
}
