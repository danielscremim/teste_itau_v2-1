using ComprasProgramadas.Application.DTOs.Requests;
using ComprasProgramadas.Application.DTOs.Responses;
using ComprasProgramadas.Domain.Entities;
using ComprasProgramadas.Domain.Exceptions;
using ComprasProgramadas.Domain.Interfaces;
using ComprasProgramadas.Domain.Interfaces.Repositories;

namespace ComprasProgramadas.Application.UseCases.Clientes;

public class AlterarValorMensalUseCase
{
    private readonly IClienteRepository _clienteRepo;
    private readonly IUnitOfWork        _uow;

    public AlterarValorMensalUseCase(IClienteRepository clienteRepo, IUnitOfWork uow)
    { _clienteRepo = clienteRepo; _uow = uow; }

    public async Task<AlterarValorMensalResponse> ExecutarAsync(long clienteId, AlterarValorMensalRequest request)
    {
        var cliente = await _clienteRepo.ObterPorIdAsync(clienteId)
            ?? throw new DomainException($"Cliente {clienteId} nao encontrado.");

        if (!cliente.Ativo)
            throw new DomainException("Nao e possivel alterar valor de cliente inativo.");

        var valorAnterior = cliente.AlterarValorMensal(request.NovoValorMensal);

        var historico = HistoricoValorMensal.Registrar(cliente.Id, valorAnterior, request.NovoValorMensal);
        cliente.HistoricoValorMensal.Add(historico);

        _clienteRepo.Atualizar(cliente);
        await _uow.CommitAsync();

        return new AlterarValorMensalResponse(
            cliente.Id, cliente.Nome, valorAnterior, cliente.ValorMensal, historico.DataAlteracao);
    }
}
