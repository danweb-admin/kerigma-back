using System;
using System.Text;
using System.Text.Json;
using RccManager.Domain.Entities;
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
            try
            {
                var mensagem = string.Join("\n\n", new[]
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

                var payload = new
                {
                    number = m.Telefone,
                    textMessage = new
                    {
                        text = mensagem
                    }
                };


                var response = await _http.PostAsync(
                    $"/message/sendText/danweb",
                    new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
                );

                if (!response.IsSuccessStatusCode)
                    Console.WriteLine("response: " + await response.Content.ReadAsStringAsync());


                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro Enviar  Texto: " + ex.StackTrace);

                throw;
            }
            
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

            var response = await _http.PostAsync(
                $"/message/sendMedia/danweb",
                new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            );

            if (!response.IsSuccessStatusCode)
                Console.WriteLine("response: " + await response.Content.ReadAsStringAsync());

            response.EnsureSuccessStatusCode();
        }
    }
}

