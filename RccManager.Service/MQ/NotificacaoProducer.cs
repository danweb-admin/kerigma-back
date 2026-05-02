using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RccManager.Domain.Responses;

namespace RccManager.Service.MQ
{
    public class NotificacaoProducer
    {
        private readonly RabbitMQConnection _rmq;
        private readonly ILogger<NotificacaoProducer> _logger;

        private const string EXCHANGE_NAME = "pagamento.confirmado";

        public NotificacaoProducer(
            RabbitMQConnection rmq,
            ILogger<NotificacaoProducer> logger
        )
        {
            _rmq = rmq;
            _logger = logger;
        }

        public async Task PublishConfirmacao(InscricaoMQResponse inscricao)
        {
            var channel = await _rmq.CreateChannelAsync();

            // 🔥 Exchange do tipo FANOUT (envia para todas as filas vinculadas)
            await channel.ExchangeDeclareAsync(
                exchange: EXCHANGE_NAME,
                type: ExchangeType.Fanout,
                durable: true
            );

            var props = new BasicProperties
            {
                DeliveryMode = DeliveryModes.Persistent // mensagem persistente
            };

            try
            {
                var json = JsonSerializer.Serialize(inscricao);
                var body = Encoding.UTF8.GetBytes(json);

                await channel.BasicPublishAsync(
                    exchange: EXCHANGE_NAME,
                    routingKey: "", // fanout ignora
                    mandatory: false,
                    basicProperties: props,
                    body: body
                );

                _logger.LogInformation(
                    "Mensagem publicada no exchange {Exchange} para inscrição {Codigo}",
                    EXCHANGE_NAME,
                    inscricao.CodigoInscricao
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao publicar mensagem no RabbitMQ para inscrição {Codigo}",
                    inscricao.CodigoInscricao
                );
                throw; // importante pra não engolir erro silencioso
            }
        }
    }
}