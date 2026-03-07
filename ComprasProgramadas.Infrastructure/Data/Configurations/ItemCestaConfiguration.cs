using ComprasProgramadas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprasProgramadas.Infrastructure.Data.Configurations;

public class ItemCestaConfiguration : IEntityTypeConfiguration<ItemCesta>
{
    public void Configure(EntityTypeBuilder<ItemCesta> builder)
    {
        builder.ToTable("itens_cesta");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedOnAdd();

        builder.Property(i => i.CestaId).IsRequired();
        builder.Property(i => i.Ticker).IsRequired().HasMaxLength(10);

        // decimal(5,2): ex: 30.00 = 30%, máximo 999.99%
        builder.Property(i => i.Percentual).IsRequired().HasColumnType("decimal(5,2)");
        builder.Property(i => i.CreatedAt).IsRequired();

        // Mesmo ticker não pode aparecer duas vezes na mesma cesta
        builder.HasIndex(i => new { i.CestaId, i.Ticker }).IsUnique();

        builder.HasOne(i => i.Cesta)
            .WithMany(c => c.Itens)
            .HasForeignKey(i => i.CestaId);
    }
}
