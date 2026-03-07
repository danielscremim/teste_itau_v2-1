using ComprasProgramadas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ComprasProgramadas.Infrastructure.Data.Configurations;

// IEntityTypeConfiguration<T> = o "manual de instruções" da tabela Cliente
public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("clientes"); // nome da tabela no banco

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedOnAdd(); // AUTO_INCREMENT

        builder.Property(c => c.Nome)
            .IsRequired()
            .HasMaxLength(150);

        // CPF: char fixo de 11 dígitos + índice único (RN-002: não pode duplicar)
        builder.Property(c => c.Cpf)
            .IsRequired()
            .HasColumnType("char(11)");
        builder.HasIndex(c => c.Cpf).IsUnique();

        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(150);

        // decimal(15,2): até 999 trilhões com 2 casas decimais — suficiente para valores monetários
        builder.Property(c => c.ValorMensal)
            .IsRequired()
            .HasColumnType("decimal(15,2)");

        builder.Property(c => c.Ativo).IsRequired();
        builder.Property(c => c.DataAdesao).IsRequired();
        builder.Property(c => c.DataSaida);
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.UpdatedAt).IsRequired();

        // Relacionamentos — EF Core usa isso para montar os JOINs
        builder.HasOne(c => c.ContaGrafica)
            .WithOne(cg => cg.Cliente)
            .HasForeignKey<ContaGrafica>(cg => cg.ClienteId);

        builder.HasMany(c => c.CustodiasFilhote)
            .WithOne(cf => cf.Cliente)
            .HasForeignKey(cf => cf.ClienteId);

        builder.HasMany(c => c.HistoricoValorMensal)
            .WithOne(h => h.Cliente)
            .HasForeignKey(h => h.ClienteId);
    }
}
