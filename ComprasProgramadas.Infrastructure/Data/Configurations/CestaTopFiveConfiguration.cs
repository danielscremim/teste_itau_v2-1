using ComprasProgramadas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprasProgramadas.Infrastructure.Data.Configurations;

public class CestaTopFiveConfiguration : IEntityTypeConfiguration<CestaTopFive>
{
    public void Configure(EntityTypeBuilder<CestaTopFive> builder)
    {
        builder.ToTable("cestas_top_five");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd();

        builder.Property(c => c.Ativa).IsRequired();
        builder.Property(c => c.DataAtivacao).IsRequired();
        builder.Property(c => c.DataDesativacao); // nullable
        builder.Property(c => c.CriadoPor).HasMaxLength(100);
        builder.Property(c => c.CreatedAt).IsRequired();

        // Uma cesta tem muitos itens (os 5 ativos)
        builder.HasMany(c => c.Itens)
            .WithOne(i => i.Cesta)
            .HasForeignKey(i => i.CestaId)
            .OnDelete(DeleteBehavior.Cascade); // se deletar cesta, deleta os itens junto
    }
}
