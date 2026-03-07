using ComprasProgramadas.Application.DTOs.Requests;
using ComprasProgramadas.Application.DTOs.Responses;
using ComprasProgramadas.Domain.Entities;
using ComprasProgramadas.Domain.Exceptions;
using ComprasProgramadas.Domain.Interfaces;
using ComprasProgramadas.Domain.Interfaces.Repositories;

namespace ComprasProgramadas.Application.UseCases.Admin;

public class ImportarCotacoesUseCase
{
    private readonly ICotahistParser             _parser;
    private readonly ICotacaoHistoricaRepository _cotacaoRepo;
    private readonly IUnitOfWork                 _uow;
    private readonly string                      _pastaCotacoes;

    public ImportarCotacoesUseCase(
        ICotahistParser parser,
        ICotacaoHistoricaRepository cotacaoRepo,
        IUnitOfWork uow,
        string pastaCotacoes)
    { _parser = parser; _cotacaoRepo = cotacaoRepo; _uow = uow; _pastaCotacoes = pastaCotacoes; }

    public async Task<ImportacaoCotacoesResponse> ExecutarAsync(ImportarCotacoesRequest request)
    {
        var caminho = Path.Combine(_pastaCotacoes, request.NomeArquivo);
        if (!File.Exists(caminho))
            throw new DomainException($"Arquivo '{request.NomeArquivo}' nao encontrado na pasta de cotacoes.");

        var registros = _parser.Parsear(caminho).ToList();

        var cotacoes = registros.Select(r => CotacaoHistorica.Criar(
            r.Ticker,
            r.DataPregao,
            r.PrecoAbertura,
            r.PrecoMaximo,
            r.PrecoMinimo,
            r.PrecoFechamento,
            request.NomeArquivo,
            r.PrecoMedioDia,
            r.VolumeNegociado
        )).ToList();

        // Insere em lotes de 500 para nao sobrecarregar o banco
        const int tamLote = 500;
        for (int i = 0; i < cotacoes.Count; i += tamLote)
        {
            var lote = cotacoes.Skip(i).Take(tamLote);
            await _cotacaoRepo.AdicionarRangeAsync(lote);
            await _uow.CommitAsync();
        }

        return new ImportacaoCotacoesResponse(request.NomeArquivo, cotacoes.Count, "Importacao concluida.");
    }
}
