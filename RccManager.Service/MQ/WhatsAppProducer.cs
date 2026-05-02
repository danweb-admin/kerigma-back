using System;
using RabbitMQ.Client;
using RccManager.Domain.Responses;
using System.Text;
using System.Text.Json;

namespace RccManager.Service.MQ
{
    public class WhatsAppProducer
    {
        private readonly RabbitMQConnection _rmq;

        public WhatsAppProducer(RabbitMQConnection rmq)
        {
            _rmq = rmq;
        }

        public async Task PublishWhatsAppMessage(InscricaoMQResponse inscricao)
        {
            var channel = await _rmq.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: "whatsapp_queue",
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            var props = new BasicProperties
            {
                DeliveryMode = DeliveryModes.Persistent
            };

            try
            {
                var json = JsonSerializer.Serialize(inscricao);
                var body = Encoding.UTF8.GetBytes(json);

                await channel.BasicPublishAsync(
                    exchange: "",
                    routingKey: "whatsapp_queue",
                    mandatory: false,
                    basicProperties: props,
                    body: body
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }
    }
}

