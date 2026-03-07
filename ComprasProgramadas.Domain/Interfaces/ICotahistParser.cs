namespace ComprasProgramadas.Domain.Interfaces;

/// <summary>
/// Contrato para leitura e parse dos arquivos COTAHIST da B3.
///
/// Separa a responsabilidade: o Domain sabe que precisa de cotações,
/// mas não sabe que elas vêm de um arquivo TXT com layout fixo da B3.
/// Isso permite trocar a fonte (ex: API em tempo real) sem alterar o Domain ou Application.
/// </summary>
public interface ICotahistParser
{
    /// <summary>
    /// Lê e faz o parse de um arquivo COTAHIST da B3.
    /// Retorna tuplas com os dados brutos de cada linha de ativo.
    /// </summary>
    IEnumerable<CotahistRegistro> Parsear(string caminhoArquivo);
}

/// <summary>
/// Dados extraídos de uma linha do arquivo COTAHIST.
/// Corresponde ao layout fixo especificado pela B3.
/// </summary>
public record CotahistRegistro(
    string   Ticker,
    DateOnly DataPregao,
    decimal  PrecoAbertura,
    decimal  PrecoMaximo,
    decimal  PrecoMinimo,
    decimal  PrecoFechamento,
    decimal? PrecoMedioDia,
    decimal? VolumeNegociado
);
