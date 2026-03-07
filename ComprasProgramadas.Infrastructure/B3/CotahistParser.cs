using ComprasProgramadas.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ComprasProgramadas.Infrastructure.B3;

/// <summary>
/// Parser do arquivo COTAHIST da B3.
///
/// O arquivo COTAHIST é como uma planilha de largura fixa:
/// cada linha tem exatamente X caracteres e cada informação
/// está sempre na mesma posição (coluna), sem separadores.
///
/// Exemplo visual de uma linha real:
/// Posição:  01234567890123456789...
/// Conteúdo: 02202602260200012PETR4 ...00000003850...
///
/// É como se cada gaveta de uma cômoda tivesse um tamanho fixo:
/// gaveta 1 (2cm) = tipo de registro
/// gaveta 2 (8cm) = data
/// gaveta 3 (12cm) = ticker
/// e assim por diante.
///
/// Especificação oficial do layout: https://www.b3.com.br (COTAHIST)
/// Posições abaixo são 0-indexed (C# Substring).
/// </summary>
public class CotahistParser : ICotahistParser
{
    private readonly ILogger<CotahistParser> _logger;

    public CotahistParser(ILogger<CotahistParser> logger) => _logger = logger;

    public IEnumerable<CotahistRegistro> Parsear(string caminhoArquivo)
    {
        if (!File.Exists(caminhoArquivo))
            throw new FileNotFoundException($"Arquivo COTAHIST não encontrado: {caminhoArquivo}");

        var nomeArquivo = Path.GetFileName(caminhoArquivo);
        var registros = new List<CotahistRegistro>();

        foreach (var linha in File.ReadLines(caminhoArquivo))
        {
            // Linha muito curta para ser um registro de ativo? Ignora.
            if (linha.Length < 245) continue;

            // Posição 0-1 = tipo do registro
            // "01" = cabeçalho, "02" = dado de ativo, "99" = rodapé
            var tipoRegistro = linha.Substring(0, 2);

            // Só processa linhas de dados de ativos
            if (tipoRegistro != "02") continue;

            try
            {
                var registro = ParsearLinha(linha, nomeArquivo);
                if (registro != null)
                    registros.Add(registro);
            }
            catch (Exception ex)
            {
                // Linha com problema? Loga e pula — não quebra o processamento todo
                _logger.LogWarning(ex, "Erro ao parsear linha do arquivo {Arquivo}: {Linha}",
                    nomeArquivo, linha[..Math.Min(50, linha.Length)]);
            }
        }

        _logger.LogInformation("Arquivo {Arquivo} parseado: {Total} cotações importadas.",
            nomeArquivo, registros.Count);

        return registros;
    }

    private static CotahistRegistro? ParsearLinha(string linha, string nomeArquivo)
    {
        // --- Layout fixo do COTAHIST da B3 (posições 0-indexed) ---
        // Pos 10-11 (2 chars): CODBDI - código do mercado
        // "02" = lote padrão, "12" = ETF, "14" = opções de compra, etc.
        // Para nosso sistema, tratamos lote padrão (02) e fracionário (10)
        var codbdi = linha.Substring(10, 2).Trim();
        if (codbdi != "02" && codbdi != "10") return null; // ignora opções, ETFs, etc.

        // Pos 12-23 (12 chars): CODNEG - ticker do ativo (ex: "PETR4       ")
        var ticker = linha.Substring(12, 12).Trim();
        if (string.IsNullOrEmpty(ticker)) return null;

        // Pos 24-26 (3 chars): TPMERC - tipo de mercado
        // "010" = mercado à vista (lote padrão)
        // "070" = mercado fracionário
        var tipoMercado = linha.Substring(24, 3).Trim();
        if (tipoMercado != "010" && tipoMercado != "070") return null;

        // Pos 2-9 (8 chars): DATPRE - data do pregão (YYYYMMDD)
        var dataStr = linha.Substring(2, 8);
        if (!DateOnly.TryParseExact(dataStr, "yyyyMMdd", out var dataPregao))
            return null;

        // Preços: 13 dígitos onde os ÚLTIMOS 2 são os centavos
        // Ex: "0000000003850" = R$ 38,50 (divide por 100)
        var precoAbertura   = ParsearPreco(linha.Substring(56, 13));
        var precoMaximo     = ParsearPreco(linha.Substring(69, 13));
        var precoMinimo     = ParsearPreco(linha.Substring(82, 13));
        var precoMedio      = ParsearPreco(linha.Substring(95, 13));
        var precoFechamento = ParsearPreco(linha.Substring(108, 13));

        // Volume: 18 dígitos, últimos 2 são centavos
        decimal? volume = null;
        if (long.TryParse(linha.Substring(170, 18).Trim(), out var volBruto))
            volume = volBruto / 100m;

        return new CotahistRegistro(
            Ticker:           ticker,
            DataPregao:       dataPregao,
            PrecoAbertura:    precoAbertura,
            PrecoMaximo:      precoMaximo,
            PrecoMinimo:      precoMinimo,
            PrecoFechamento:  precoFechamento,
            PrecoMedioDia:    precoMedio,
            VolumeNegociado:  volume
        );
    }

    /// <summary>
    /// Converte os 13 dígitos do campo de preço do COTAHIST em decimal.
    /// Os 2 últimos dígitos são os centavos.
    /// Ex: "0000000003850" → 3850 / 100 = R$ 38,50
    /// </summary>
    private static decimal ParsearPreco(string campo)
    {
        if (long.TryParse(campo.Trim(), out var valorBruto))
            return valorBruto / 100m;
        return 0m;
    }
}
