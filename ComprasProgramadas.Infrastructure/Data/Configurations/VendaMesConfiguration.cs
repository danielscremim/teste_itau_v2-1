using ComprasProgramadas.Domain.Entities;
using ComprasProgramadas.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprasProgramadas.Infrastructure.Data.Configurations;

public class VendaMesConfiguration : IEntityTypeConfiguration<VendaMes>
{
    public void Configure(EntityTypeBuilder<VendaMes> builder)
    {
        builder.ToTable("vendas_mes");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).ValueGeneratedOnAdd();

        builder.Property(v => v.ClienteId).IsRequired();
        builder.Property(v => v.MesReferencia).IsRequired().HasColumnType("char(7)"); // "2026-03"
        builder.Property(v => v.Ticker).IsRequired().HasMaxLength(10);
        builder.Property(v => v.Quantidade).IsRequired();
        builder.Property(v => v.PrecoVenda).IsRequired().HasColumnType("decimal(15,6)");
        builder.Property(v => v.PrecoMedio).IsRequired().HasColumnType("decimal(15,6)");
        builder.Property(v => v.ValorTotalVenda).IsRequired().HasColumnType("decimal(15,2)");
        builder.Property(v => v.Lucro).IsRequired().HasColumnType("decimal(15,2)");
        builder.Property(v => v.Origem).IsRequired().HasConversion<string>();
        builder.Property(v => v.DataVenda).IsRequired();
        builder.Property(v => v.CreatedAt).IsRequired();

        // Índice composto para buscar rapidamente todas as vendas de um cliente num mês
        builder.HasIndex(v => new { v.ClienteId, v.MesReferencia });

        builder.HasOne(v => v.Cliente)
            .WithMany()
            .HasForeignKey(v => v.ClienteId);
    }
}
