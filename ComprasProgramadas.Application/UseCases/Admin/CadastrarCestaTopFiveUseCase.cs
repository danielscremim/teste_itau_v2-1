using ComprasProgramadas.Application.DTOs.Requests;
using ComprasProgramadas.Application.DTOs.Responses;
using ComprasProgramadas.Domain.Entities;
using ComprasProgramadas.Domain.Interfaces;
using ComprasProgramadas.Domain.Interfaces.Repositories;

namespace ComprasProgramadas.Application.UseCases.Admin;

public class CadastrarCestaTopFiveUseCase
{
    private readonly ICestaTopFiveRepository    _cestaRepo;
    private readonly IClienteRepository         _clienteRepo;
    private readonly IRebalanceamentoRepository _rebalRepo;
    private readonly IUnitOfWork                _uow;

    public CadastrarCestaTopFiveUseCase(
        ICestaTopFiveRepository    cestaRepo,
        IClienteRepository         clienteRepo,
        IRebalanceamentoRepository rebalRepo,
        IUnitOfWork                uow)
    { _cestaRepo = cestaRepo; _clienteRepo = clienteRepo; _rebalRepo = rebalRepo; _uow = uow; }

    public async Task<CestaResponse> ExecutarAsync(CadastrarCestaRequest request)
    {
        // 1. Desativar cesta atual
        var cestaAtual = await _cestaRepo.ObterAtivaAsync();
        if (cestaAtual is not null)
        {
            cestaAtual.Desativar();
            _cestaRepo.Atualizar(cestaAtual);
        }

        // 2. Criar nova cesta  factory valida 5 itens + soma 100%
        var tuples = request.Itens
            .Select(i => (i.Ticker.ToUpper(), i.Percentual))
            .ToList();

        var novaCesta = CestaTopFive.Criar(tuples, request.CriadoPor);
        await _cestaRepo.AdicionarAsync(novaCesta);
        await _uow.CommitAsync(); // commit para obter novaCesta.Id

        // 3. Criar rebalanceamento para todos os clientes ativos
        var clientes = await _clienteRepo.ListarAtivosAsync();
        if (clientes.Any())
        {
            var reba = Rebalanceamento.CriarPorMudancaCesta(novaCesta.Id);
            await _rebalRepo.AdicionarAsync(reba);
            await _uow.CommitAsync(); // commit para obter reba.Id

            foreach (var cliente in clientes)
                reba.Clientes.Add(RebalanceamentoCliente.Criar(reba.Id, cliente.Id));

            _rebalRepo.Atualizar(reba);
            await _uow.CommitAsync();
        }

        return new CestaResponse(
            novaCesta.Id, novaCesta.Ativa, novaCesta.DataAtivacao,
            novaCesta.DataDesativacao, novaCesta.CriadoPor,
            novaCesta.Itens.Select(i => new ItemCestaResponse(i.Ticker, i.Percentual)).ToList());
    }
}
