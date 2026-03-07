using ComprasProgramadas.Application.DTOs.Requests;
using ComprasProgramadas.Application.UseCases.Admin;
using ComprasProgramadas.Domain.Exceptions;
using ComprasProgramadas.Domain.Interfaces;
using ComprasProgramadas.Domain.Interfaces.Repositories;
using FluentAssertions;
using Moq;

namespace ComprasProgramadas.Tests.UseCases;

/// <summary>
/// Testes do ImportarCotacoesUseCase.
///
/// O Use Case lê um arquivo COTAHIST da B3 (do disco) e importa as cotações
/// para o banco de dados.
///
/// Testamos dois cenários:
///   1. Arquivo não existe → lança DomainException
///   2. Arquivo existe + parser retorna 0 cotações → importação vazia, sucesso
/// </summary>
public class ImportarCotacoesTests
{
    private readonly Mock<ICotahistParser>             _parserMock      = new();
    private readonly Mock<ICotacaoHistoricaRepository> _cotacaoRepoMock = new();
    private readonly Mock<IUnitOfWork>                 _uowMock         = new();

    private ImportarCotacoesUseCase CriarUseCase(string pasta) =>
        new(_parserMock.Object, _cotacaoRepoMock.Object, _uowMock.Object, pasta);

    [Fact(DisplayName = "ExecutarAsync com arquivo inexistente deve lançar DomainException")]
    public async Task ExecutarAsync_ArquivoNaoExiste_LancaDomainException()
    {
        // Arrange: pasta temporária que existe mas o arquivo não
        var pasta   = Path.GetTempPath();
        var request = new ImportarCotacoesRequest("COTAHIST_D99991231.TXT");
        var useCase = CriarUseCase(pasta);

        // Act
        Func<Task> act = () => useCase.ExecutarAsync(request);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*COTAHIST_D99991231.TXT*");
    }

    [Fact(DisplayName = "ExecutarAsync com arquivo vazio deve retornar 0 importações")]
    public async Task ExecutarAsync_ArquivoVazio_Retorna0Importacoes()
    {
        // Arrange: cria um arquivo real temporário para File.Exists retornar true
        var pasta    = Path.GetTempPath();
        var nomeArq  = "COTAHIST_D01012020.TXT";
        var caminho  = Path.Combine(pasta, nomeArq);
        await File.WriteAllTextAsync(caminho, ""); // arquivo vazio

        try
        {
            // Mock do parser: retorna nenhum registro (arquivo vazio)
            _parserMock
                .Setup(p => p.Parsear(caminho))
                .Returns([]);

            _uowMock
                .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var request = new ImportarCotacoesRequest(nomeArq);
            var useCase = CriarUseCase(pasta);

            // Act
            var resultado = await useCase.ExecutarAsync(request);

            // Assert
            resultado.NomeArquivo.Should().Be(nomeArq);
            resultado.TotalRegistros.Should().Be(0);
            resultado.Status.Should().Contain("concluida");
        }
        finally
        {
            // Limpa o arquivo temporário
            if (File.Exists(caminho)) File.Delete(caminho);
        }
    }

    [Fact(DisplayName = "ExecutarAsync com 3 cotações deve chamar AdicionarRangeAsync e CommitAsync")]
    public async Task ExecutarAsync_Com3Cotacoes_PersisteCotacoes()
    {
        // Arrange: arquivo real temporário
        var pasta   = Path.GetTempPath();
        var nomeArq = "COTAHIST_D01022020.TXT";
        var caminho = Path.Combine(pasta, nomeArq);
        await File.WriteAllTextAsync(caminho, "conteudo-qualquer");

        try
        {
            // 3 registros simulados de cotação PETR4
            var registros = Enumerable.Range(1, 3).Select(i =>
                new ComprasProgramadas.Domain.Interfaces.CotahistRegistro(
                    "PETR4",
                    DateOnly.FromDateTime(DateTime.Today),
                    30m, 31m, 29m, 30.5m,
                    null, null
                ));

            _parserMock.Setup(p => p.Parsear(caminho)).Returns(registros);
            _cotacaoRepoMock
                .Setup(r => r.AdicionarRangeAsync(It.IsAny<IEnumerable<ComprasProgramadas.Domain.Entities.CotacaoHistorica>>()))
                .Returns(Task.CompletedTask);
            _uowMock
                .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var useCase   = CriarUseCase(pasta);
            var request   = new ImportarCotacoesRequest(nomeArq);

            // Act
            var resultado = await useCase.ExecutarAsync(request);

            // Assert
            resultado.TotalRegistros.Should().Be(3);
            _cotacaoRepoMock.Verify(r => r.AdicionarRangeAsync(It.IsAny<IEnumerable<ComprasProgramadas.Domain.Entities.CotacaoHistorica>>()), Times.Once);
        }
        finally
        {
            if (File.Exists(caminho)) File.Delete(caminho);
        }
    }
}
