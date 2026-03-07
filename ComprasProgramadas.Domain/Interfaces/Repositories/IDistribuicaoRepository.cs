using ComprasProgramadas.Domain.Entities;

namespace ComprasProgramadas.Domain.Interfaces.Repositories;

public interface IDistribuicaoRepository
{
    Task AdicionarAsync(Distribuicao distribuicao);
    Task<IEnumerable<Distribuicao>> ListarNaoPublicadasKafkaAsync();
    /// <summary>
    /// Retorna o histórico de distribuições de um cliente, ordenado por data.
    /// Usado pelo endpoint de rentabilidade para construir a evolução da carteira.
    /// </summary>
    Task<IEnumerable<Distribuicao>> ListarPorClienteAsync(long clienteId);
    void Atualizar(Distribuicao distribuicao);
}
