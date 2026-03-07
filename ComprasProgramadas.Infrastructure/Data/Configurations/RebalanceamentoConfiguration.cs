using ComprasProgramadas.Domain.Entities;
using ComprasProgramadas.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprasProgramadas.Infrastructure.Data.Configurations;

public class RebalanceamentoConfiguration : IEntityTypeConfiguration<Rebalanceamento>
{
    public void Configure(EntityTypeBuilder<Rebalanceamento> builder)
    {
        builder.ToTable("rebalanceamentos");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedOnAdd();

        builder.Property(r => r.Tipo).IsRequired().HasConversion<string>();
        builder.Property(r => r.CestaNovaId); // nullable (só preenchido em MudancaCesta)
        builder.Property(r => r.Status).IsRequired().HasConversion<string>();
        builder.Property(r => r.DataInicio).IsRequired();
        builder.Property(r => r.DataFim); // nullable
        builder.Property(r => r.CreatedAt).IsRequired();

        builder.HasOne(r => r.CestaNova)
            .WithMany()
            .HasForeignKey(r => r.CestaNovaId)
            .IsRequired(false);

        builder.HasMany(r => r.Clientes)
            .WithOne(rc => rc.Rebalanceamento)
            .HasForeignKey(rc => rc.RebalanceamentoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
