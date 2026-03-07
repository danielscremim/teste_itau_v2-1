namespace ComprasProgramadas.Domain.Interfaces;

/// <summary>
/// Contrato para publicação de mensagens no Kafka.
///
/// Por que uma interface aqui no Domain?
/// O Domain precisa saber que "existe a necessidade de publicar IR no Kafka",
/// mas NÃO precisa saber que é Kafka especificamente — poderia ser RabbitMQ, SNS, etc.
/// Isso é o princípio da Inversão de Dependência (SOLID-D).
///
/// A implementação concreta com Confluent.Kafka fica em Infrastructure.
/// </summary>
public interface IKafkaPublisher
{
    /// <summary>
    /// Publica uma mensagem em um tópico Kafka.
    /// </summary>
    /// <param name="topico">Nome do tópico (ex: "ir-dedo-duro", "ir-venda")</param>
    /// <param name="chave">Chave de particionamento (ex: clienteId como string)</param>
    /// <param name="payloadJson">Payload serializado em JSON</param>
    Task PublicarAsync(string topico, string chave, string payloadJson);
}
