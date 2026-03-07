namespace ComprasProgramadas.Domain.Entities;

/// <summary>
/// Classe base para todas as entidades do sistema.
/// Todo objeto de negócio tem um Id único e auditoria de criação.
/// Herdar dessa classe evita repetir esses campos em cada entidade.
/// </summary>
public abstract class EntidadeBase
{
    public long     Id        { get; protected set; }
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
}
