using ComprasProgramadas.Domain.Entities;
using ComprasProgramadas.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprasProgramadas.Infrastructure.Data.Configurations;

public class RebalanceamentoClienteConfiguration : IEntityTypeConfiguration<RebalanceamentoCliente>
{
    public void Configure(EntityTypeBuilder<RebalanceamentoCliente> builder)
    {
        builder.ToTable("rebalanceamento_clientes");
        builder.HasKey(rc => rc.Id);
        builder.Property(rc => rc.Id).ValueGeneratedOnAdd();

        builder.Property(rc => rc.RebalanceamentoId).IsRequired();
        builder.Property(rc => rc.ClienteId).IsRequired();
        builder.Property(rc => rc.Status).IsRequired().HasConversion<string>();
        builder.Property(rc => rc.TotalVendas).IsRequired().HasColumnType("decimal(15,2)");
        builder.Property(rc => rc.TotalCompras).IsRequired().HasColumnType("decimal(15,2)");
        builder.Property(rc => rc.IrDevido).IsRequired().HasColumnType("decimal(15,2)");
        builder.Property(rc => rc.KafkaIrPublicado).IsRequired();
        builder.Property(rc => rc.DataExecucao); // nullable
        builder.Property(rc => rc.CreatedAt).IsRequired();

        builder.HasOne(rc => rc.Rebalanceamento)
            .WithMany(r => r.Clientes)
            .HasForeignKey(rc => rc.RebalanceamentoId);

        builder.HasOne(rc => rc.Cliente)
            .WithMany()
            .HasForeignKey(rc => rc.ClienteId);
    }
}
