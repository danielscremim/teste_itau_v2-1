using ComprasProgramadas.Domain.Interfaces;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ComprasProgramadas.Infrastructure.Messaging;

/// <summary>
/// O Kafka Publisher é como um carteiro digital.
///
/// Você entrega a carta (payloadJson) com o endereço (topico)
/// e ele garante a entrega. Se o destinatário não estiver em casa agora,
/// o Kafka guarda a mensagem e entrega quando ele conectar.
///
/// Por que usar Kafka e não chamar diretamente outro serviço?
/// Imagine que a Receita Federal está fora do ar às 3h da manhã
/// quando o motor roda. Com Kafka, a mensagem fica guardada e é
/// processada quando o serviço voltar — sem perder dados.
/// </summary>
public class KafkaPublisher : IKafkaPublisher, IAsyncDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaPublisher> _logger;

    public KafkaPublisher(IConfiguration configuration, ILogger<KafkaPublisher> logger)
    {
        _logger = logger;

        // Configuração do producer Kafka
        var config = new ProducerConfig
        {
            // Endereço do Kafka (vem do appsettings.json)
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",

            // Aguarda confirmação de ao menos 1 broker antes de considerar entregue
            Acks = Acks.Leader,

            // Se falhar, tenta novamente até 3 vezes automaticamente
            MessageSendMaxRetries = 3
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    /// <summary>
    /// Publica uma mensagem no tópico Kafka.
    /// A chave (clienteId) garante que todas as mensagens do mesmo cliente
    /// vão para a mesma partição — mantendo a ordem de chegada.
    /// </summary>
    public async Task PublicarAsync(string topico, string chave, string payloadJson)
    {
        try
        {
            var mensagem = new Message<string, string>
            {
                Key   = chave,
                Value = payloadJson
            };

            var result = await _producer.ProduceAsync(topico, mensagem);

            _logger.LogInformation(
                "Mensagem publicada no Kafka. Tópico: {Topico} | Partição: {Particao} | Offset: {Offset}",
                topico, result.Partition.Value, result.Offset.Value);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex,
                "Erro ao publicar no Kafka. Tópico: {Topico} | Erro: {Erro}",
                topico, ex.Error.Reason);
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        // Garante que mensagens pendentes sejam enviadas antes de fechar
        _producer.Flush(TimeSpan.FromSeconds(10));
        _producer.Dispose();
        await ValueTask.CompletedTask;
    }
}
