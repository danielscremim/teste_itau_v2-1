using ComprasProgramadas.Application.DTOs.Requests;
using ComprasProgramadas.Application.DTOs.Responses;
using ComprasProgramadas.Domain.Entities;
using ComprasProgramadas.Domain.Exceptions;
using ComprasProgramadas.Domain.Interfaces;
using ComprasProgramadas.Domain.Interfaces.Repositories;

namespace ComprasProgramadas.Application.UseCases.Clientes;

public class AderirAoProdutoUseCase
{
    private readonly IClienteRepository      _clienteRepo;
    private readonly IContaGraficaRepository _contaRepo;
    private readonly IUnitOfWork             _uow;

    public AderirAoProdutoUseCase(
        IClienteRepository      clienteRepo,
        IContaGraficaRepository contaRepo,
        IUnitOfWork             uow)
    {
        _clienteRepo = clienteRepo;
        _contaRepo   = contaRepo;
        _uow         = uow;
    }

    public async Task<AdesaoResponse> ExecutarAsync(AdesaoRequest request)
    {
        var existente = await _clienteRepo.ObterPorCpfAsync(request.Cpf);
        if (existente is not null)
            throw new DomainException($"CPF {request.Cpf} ja esta cadastrado no sistema.");

        var cliente = Cliente.Criar(request.Nome, request.Cpf, request.Email, request.ValorMensal);
        await _clienteRepo.AdicionarAsync(cliente);
        await _uow.CommitAsync();

        var totalContas = await _contaRepo.ContarFilhotesAsync();
        var numeroConta = $"FLH-{(totalContas + 1):D6}";
        var conta = ContaGrafica.CriarFilhote(cliente.Id, numeroConta);
        await _contaRepo.AdicionarAsync(conta);
        await _uow.CommitAsync();

        return new AdesaoResponse(
            cliente.Id, cliente.Nome, cliente.Cpf, cliente.Email,
            cliente.ValorMensal, cliente.Ativo, cliente.DataAdesao,
            new ContaGraficaResponse(conta.Id, conta.NumeroConta, conta.Tipo.ToString(), conta.DataCriacao)
        );
    }
}
