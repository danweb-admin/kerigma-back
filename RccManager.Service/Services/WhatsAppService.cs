using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RccManager.Domain.Interfaces.Services;
using RccManager.Domain.Responses;

namespace RccManager.Service.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly HttpClient _http;

        public WhatsAppService(HttpClient http)
        {
            _http = http;
        }

        public async Task EnviarTexto(InscricaoMQResponse m)
        {
            var mensagem = MontarMensagemTexto(m);

            var payload = new
            {
                number = m.Telefone,
                textMessage = new
                {
                    text = mensagem
                }
            };

            await PostAsync("/message/sendText/danweb", payload);
        }

        public async Task EnviarQrCode(InscricaoMQResponse m)
        {
            var urlQrCode = $"https://backend.rcc-londrina.online/qrcodes/{m.CodigoInscricao}.png";

            var payload = new
            {
                number = m.Telefone,
                mediaMessage = new
                {
                    mediatype = "image",
                    media = urlQrCode,
                    caption = "Seu QR Code para check-in 🎟️\nApresente na entrada do evento."
                }
            };

            await PostAsync("/message/sendMedia/danweb", payload);
        }

        // 🔥 Centraliza chamada HTTP + tratamento de erro
        private async Task PostAsync(string url, object payload)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _http.PostAsync(url, content);

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // 🔥 ESSENCIAL: lançar erro pra retry no RabbitMQ
                throw new Exception($"Erro ao enviar WhatsApp ({url}): {response.StatusCode} - {responseBody}");
            }
        }

        // 🔥 Separado pra organização/teste
        private string MontarMensagemTexto(InscricaoMQResponse m)
        {
            return string.Join("\n\n", new[]
            {
                $"Olá, {m.Nome}! 😊",
                $"Seu pagamento para o evento *{m.NomeEvento}* foi confirmado ✅",
                $"📅 {m.DataInicio:dd/MM/yyyy}",
                $"📍 {m.Local}",
                $"🆔 Código: {m.CodigoInscricao}",
                "Apresente seu CPF ou código na entrada.",
                "Em seguida enviarei seu QR Code 🎟️",

                "⚠️ Esta é uma mensagem automática de confirmação de pagamento.",
                "Para outras informações, entre em contato com o organizador do evento."
            });
        }
    }
}
