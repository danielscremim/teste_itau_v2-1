using ComprasProgramadas.Application.DTOs.Responses;
using ComprasProgramadas.Domain.Exceptions;
using ComprasProgramadas.Domain.Interfaces;
using ComprasProgramadas.Domain.Interfaces.Repositories;

namespace ComprasProgramadas.Application.UseCases.Clientes;

public class SairDoProdutoUseCase
{
    private readonly IClienteRepository _clienteRepo;
    private readonly IUnitOfWork        _uow;

    public SairDoProdutoUseCase(IClienteRepository clienteRepo, IUnitOfWork uow)
    { _clienteRepo = clienteRepo; _uow = uow; }

    public async Task<SaidaResponse> ExecutarAsync(long clienteId)
    {
        var cliente = await _clienteRepo.ObterPorIdAsync(clienteId)
            ?? throw new DomainException($"Cliente {clienteId} nao encontrado.");

        cliente.Desativar();
        _clienteRepo.Atualizar(cliente);
        await _uow.CommitAsync();

        return new SaidaResponse(
            cliente.Id, cliente.Nome, cliente.Ativo,
            cliente.DataSaida ?? DateTime.UtcNow,
            "Adesao cancelada com sucesso.");
    }
}
