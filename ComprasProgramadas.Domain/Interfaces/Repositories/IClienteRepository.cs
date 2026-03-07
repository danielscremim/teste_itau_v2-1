using ComprasProgramadas.Domain.Entities;

namespace ComprasProgramadas.Domain.Interfaces.Repositories;

/// <summary>
/// Contrato de acesso a dados para a entidade Cliente.
/// A implementação (EF Core + MySQL) fica em Infrastructure.
/// O Application depende desta interface, não da implementação — princípio da Inversão de Dependência (SOLID-D).
/// </summary>
public interface IClienteRepository
{
    Task<Cliente?> ObterPorIdAsync(long id);
    Task<Cliente?> ObterPorCpfAsync(string cpf);
    Task<IEnumerable<Cliente>> ListarAtivosAsync();
    Task AdicionarAsync(Cliente cliente);
    void Atualizar(Cliente cliente);
}
