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

        public WhatsAppConsumer(RabbitMQConnection rmq, IWhatsAppService whatsAppService)
        {
            _rmq = rmq;
            _whatsAppService = whatsAppService;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Retry com delay em caso de falha
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    Console.WriteLine("🔌 Tentando conectar ao RabbitMQ. ..");
                    
                    _channel = await _rmq.CreateChannelAsync();

                    Console.WriteLine("✅ Conectado ao RabbitMQ - WhatsApp com sucesso !");

                    await _channel.QueueDeclareAsync(
                        queue: "whatsapp_queue",
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null
                    );

                    // Configurar prefetch para processar 1 mensagem por vez
                    await _channel.BasicQosAsync(0, 1, false);

                    var consumer = new AsyncEventingBasicConsumer(_channel);

                    consumer.ReceivedAsync += async (model, ea) =>
                    {
                        try
                        {
                            var body = ea.Body.ToArray();
                            var json = Encoding.UTF8.GetString(body);
                            var message = JsonSerializer.Deserialize<InscricaoMQResponse>(json);

                            Console.WriteLine($"📩 Processando mensagem  para: {message?.Telefone}");

                            if (message != null)
                            {
                                message.Telefone =  "55" + Utils.SomenteNumeros(message.Telefone);

                                await _whatsAppService.EnviarTexto(message);

                                // ⏱ Delay anti-ban
                                await Task.Delay(Random.Shared.Next(2000, 5000));

                                await _whatsAppService.EnviarQrCode(message);

                                await _channel.BasicAckAsync(ea.DeliveryTag, false);
                                Console.WriteLine($"✅ Mensagem enviada com sucesso para: {message.Telefone}");
                            }
                            else
                            {
                                Console.WriteLine("⚠️ Mensagem deserializada é nula");
                                await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                            }
                        }
                        catch (JsonException jsonEx)
                        {
                            Console.WriteLine($"❌ Erro ao deserializar mensagem: {jsonEx.Message}");
                            // Rejeitar mensagem sem requeue (mensagem inválida)
                            await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"❌ Erro ao processar mensagem: {ex.Message}");
                            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                            // Rejeitar com requeue para tentar novamente
                            await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
                        }
                    };

                    await _channel.BasicConsumeAsync(
                        queue: "whatsapp_queue",
                        autoAck: false,
                        consumer: consumer
                    );

                    Console.WriteLine("👂 Aguardando mensagens...");

                    // Manter o consumer ativo até o cancellation
                    await Task.Delay(Timeout.Infinite, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("🛑 Consumer está sendo encerrado...");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("-*-*-*- ERRO CONSUMER -*-*-*-");
                    Console.WriteLine($"Mensagem: {ex.Message}");
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                    // Aguardar antes de tentar reconectar
                    if (!stoppingToken.IsCancellationRequested)
                    {
                        Console.WriteLine("⏳ Aguardando 5 segundos antes de reconectar - WhatsApp...");
                        await Task.Delay(5000, stoppingToken);
                    }
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("🛑 Encerrando consumer...");
            
            if (_channel != null)
            {
                await _channel.CloseAsync();
                _channel.Dispose();
            }

            await base.StopAsync(cancellationToken);
        }

        
    }
}

