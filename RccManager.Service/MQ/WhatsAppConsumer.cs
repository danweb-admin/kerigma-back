using System;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RccManager.Domain.Interfaces.Services;
using RccManager.Domain.Responses;
using RccManager.Service.Helper;

namespace RccManager.Service.MQ
{
    public class WhatsAppConsumer : BackgroundService
    {
        private readonly RabbitMQConnection _rmq;
        private readonly IWhatsAppService _whatsAppService;
        private IChannel _channel;

        private const int MAX_RETRY = 5;

        public WhatsAppConsumer(RabbitMQConnection rmq, IWhatsAppService whatsAppService)
        {
            _rmq = rmq;
            _whatsAppService = whatsAppService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    Console.WriteLine("🔌 Conectando ao RabbitMQ...");
                    
                    _channel = await _rmq.CreateChannelAsync();

                    await _channel.QueueDeclareAsync("whatsapp_queue", true, false, false);
                    await _channel.QueueDeclareAsync("whatsapp_retry_queue", true, false, false);
                    await _channel.QueueDeclareAsync("whatsapp_error_queue", true, false, false);

                    await _channel.BasicQosAsync(0, 1, false);

                    var consumer = new AsyncEventingBasicConsumer(_channel);

                    consumer.ReceivedAsync += async (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var json = Encoding.UTF8.GetString(body);

                        int retry = GetRetryCount(ea);

                        try
                        {
                            var message = JsonSerializer.Deserialize<InscricaoMQResponse>(json);

                            Console.WriteLine($"📩 Processando: {message?.Telefone} | Retry: {retry}");

                            if (message == null)
                            {
                                await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                                return;
                            }

                            message.Telefone = "55" + Utils.SomenteNumeros(message.Telefone);

                            await _whatsAppService.EnviarTexto(message);

                            await Task.Delay(Random.Shared.Next(2000, 5000));

                            await _whatsAppService.EnviarQrCode(message);

                            await _channel.BasicAckAsync(ea.DeliveryTag, false);

                            Console.WriteLine($"✅ Sucesso: {message.Telefone}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"❌ Erro: {ex.Message}");

                            if (retry < MAX_RETRY)
                            {
                                Console.WriteLine($"🔁 Retry {retry + 1}/{MAX_RETRY}");

                                PublishWithRetry(body, retry + 1);

                                await _channel.BasicAckAsync(ea.DeliveryTag, false);
                            }
                            else
                            {
                                Console.WriteLine("💀 Mensagem enviada para fila de erro");

                                await _channel.BasicPublishAsync(
                                    exchange: "",
                                    routingKey: "whatsapp_error_queue",
                                    body: body
                                );

                                await _channel.BasicAckAsync(ea.DeliveryTag, false);
                            }
                        }
                    };

                    await _channel.BasicConsumeAsync("whatsapp_queue", false, consumer);

                    Console.WriteLine("👂 Aguardando mensagens...");
                    await Task.Delay(Timeout.Infinite, stoppingToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro conexão: {ex.Message}");
                    await Task.Delay(5000, stoppingToken);
                }
            }
        }

        private int GetRetryCount(BasicDeliverEventArgs ea)
        {
            if (ea.BasicProperties?.Headers != null &&
                ea.BasicProperties.Headers.TryGetValue("x-retry", out var value))
            {
                return Convert.ToInt32(Encoding.UTF8.GetString((byte[])value));
            }

            return 0;
        }

        private async Task PublishWithRetry(byte[] body, int retry)
        {
            var props = new BasicProperties
            {
                Persistent = true,
                Headers = new Dictionary<string, object>
                {
                    { "x-retry", Encoding.UTF8.GetBytes(retry.ToString()) }
                }
            };
        
            await _channel.BasicPublishAsync(
                exchange: "",
                routingKey: "whatsapp_retry_queue",
                mandatory: false,
                basicProperties: props,
                body: body
            );    
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_channel != null)
            {
                await _channel.CloseAsync();
                _channel.Dispose();
            }

            await base.StopAsync(cancellationToken);
        }
    }
}
