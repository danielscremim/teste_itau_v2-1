namespace ComprasProgramadas.Domain.Exceptions;

/// <summary>
/// Exceção lançada quando uma regra de negócio do Domain é violada.
///
/// Por que uma exceção própria?
/// A camada de API pode capturá-la especificamente e retornar HTTP 400 (Bad Request),
/// enquanto exceções inesperadas (ex: NullReferenceException) viram HTTP 500.
/// Isso separa "erro de negócio" de "bug do sistema".
/// </summary>
public class DomainException : Exception
{
    public DomainException(string mensagem) : base(mensagem) { }
}
