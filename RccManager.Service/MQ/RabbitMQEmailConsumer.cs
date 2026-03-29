using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RccManager.Domain.Entities;
using RccManager.Domain.Responses;
using RccManager.Service.Helper;

namespace RccManager.Service.MQ
{
    public class RabbitMQEmailConsumer : BackgroundService
    {
        private readonly RabbitMQConnection _rmq;
        private IChannel _channel;

        public RabbitMQEmailConsumer(RabbitMQConnection rmq)
        {
            _rmq = rmq;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Retry com delay em caso de falha
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    Console.WriteLine("🔌 Tentando conectar ao RabbitMQ...");
                    
                    _channel = await _rmq.CreateChannelAsync();

                    Console.WriteLine("✅ Conectado ao RabbitMQ com sucesso!");

                    await _channel.QueueDeclareAsync(
                        queue: "email_queue",
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

                            Console.WriteLine($"📩 Processando email para: {message?.Email}");

                            if (message != null)
                            {
                                await EnviarEmailPagamentoConfirmado(message);
                                await _channel.BasicAckAsync(ea.DeliveryTag, false);
                                Console.WriteLine($"✅ Email enviado com sucesso para: {message.Email}");
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
                        queue: "email_queue",
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
                        Console.WriteLine("⏳ Aguardando 55 segundos antes de reconectar...");
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

        private async Task EnviarEmailPagamentoConfirmado(InscricaoMQResponse inscricao)
        {
            try
            {
                var smtpServer = Environment.GetEnvironmentVariable("SmtpServer");
                var porta = Environment.GetEnvironmentVariable("Port");
                var senderEmail = Environment.GetEnvironmentVariable("SenderEmail");
                var senderPassword = Environment.GetEnvironmentVariable("SenderPassword");

                // Validar variáveis de ambiente
                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(porta) ||
                    string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword))
                {
                    throw new InvalidOperationException("Variáveis de ambiente SMTP não configuradas corretamente");
                }

                var nomeOrganizacao = "Kerigma Eventos";
                var logoUrl = "https://res.cloudinary.com/dgcpvxvcj/image/upload/v1774819269/Fotos%20Eventos/Logo_Kerigma_Eventos.png";

                string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates", "email-confirmacao.html");
                
                if (!File.Exists(templatePath))
                {
                    throw new FileNotFoundException($"Template não encontrado: {templatePath}");
                }

                var valorTexto = inscricao.Status == "isento" ? "Isento" : $"R$ {inscricao.ValorInscricao:F2}".Replace(".", ",");

                Utils.GerarQrCodePNG(inscricao.CodigoInscricao);

                var urlQrCode = $"https://backend.auxiliadoraonline.com.br/qrcodes/{inscricao.CodigoInscricao}.png";

                string html = await File.ReadAllTextAsync(templatePath);

                html = html
                    .Replace("{{NOME}}", inscricao.Nome)
                    .Replace("{{EMAIL}}", inscricao.Email)
                    .Replace("{{CPF}}", inscricao.Cpf)
                    .Replace("{{CODIGO_INSCRICAO}}", inscricao.CodigoInscricao)
                    .Replace("{{VALOR}}", valorTexto)
                    .Replace("{{NOME_EVENTO}}", inscricao.NomeEvento)
                    .Replace("{{DATA_INICIAL}}", inscricao.DataInicio.ToString("dd/MM/yyyy"))
                    .Replace("{{DATA_FINAL}}", inscricao.DataFim.ToString("dd/MM/yyyy"))
                    .Replace("{{LOCAL_EVENTO}}", inscricao.Local)
                    .Replace("{{ORGANIZADOR}}", inscricao.OrganizadorNome)
                    .Replace("{{LOGO_URL}}", logoUrl)
                    .Replace("{{NOME_ORGANIZACAO}}", nomeOrganizacao)
                    .Replace("{{QRCODE_URL}}", urlQrCode);;
                    

                using var smtp = new SmtpClient(smtpServer, int.Parse(porta))
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    Timeout = 30000 // 30 segundos
                };

                var message = new MailMessage
                {
                    From = new MailAddress(senderEmail,"Paróquia Nossa Senhora Auxiliadora - Eventos"),
                    Subject = $"Pagamento confirmado! Seu ingresso está disponível - {inscricao.NomeEvento} - {inscricao.CodigoInscricao}",
                    Body = html,
                    IsBodyHtml = true
                };

                message.To.Add(inscricao.Email);

                await smtp.SendMailAsync(message);
                
                Console.WriteLine($"📧 Email enviado com sucesso para {inscricao.Email}, {inscricao.Nome}");
            }
            catch (SmtpException smtpEx)
            {
                Console.WriteLine($"❌ Erro SMTP: {smtpEx.Message}");
                Console.WriteLine($"Status Code: {smtpEx.StatusCode}");
                throw; // Re-throw para que a mensagem seja reprocessada
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao enviar email: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw; // Re-throw para que a mensagem seja reprocessada
            }
        }
    }
}
