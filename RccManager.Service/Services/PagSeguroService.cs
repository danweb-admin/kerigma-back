using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using RccManager.Domain.Dtos.Evento;
using RccManager.Domain.Entities;
using RccManager.Domain.Interfaces.Repositories;
using RccManager.Domain.Interfaces.Services;
using RccManager.Domain.Responses;
using RccManager.Service.Helper;

namespace RccManager.Service.Services
{
	public class PagSeguroService : IPagSeguroService
	{
        private readonly IEventoRepository _eventoRepository;

        public PagSeguroService(IEventoRepository eventoRepository)
		{
            _eventoRepository = eventoRepository;

        }

        public async Task<PagSeguroResponse> GerarLinkPagamentoAsync(InscricaoDto inscricao)
        {
            string token = Environment.GetEnvironmentVariable("Token");
            string url = Environment.GetEnvironmentVariable("UrlPagSeguro");

            Evento evento = await _eventoRepository.GetById(inscricao.EventoId);

            using var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            string cpfSomenteNumeros = inscricao.Cpf
                .Replace(".", "")
                .Replace("-", "");

            var body = new
            {
                reference_id = inscricao.CodigoInscricao,

                customer = new
                {
                    name = inscricao.Nome,
                    email = inscricao.Email,
                    tax_id = Utils.SomenteNumeros( inscricao.Cpf), 
                    phones = new[]
                        {
                            new {
                                country = "55",
                                area = Utils.SomenteNumeros(inscricao.Telefone).Substring(0, 2),
                                number = Utils.SomenteNumeros(inscricao.Telefone).Substring(2),
                                type = "MOBILE"
                            }
                        }       
                },
                items = new[]
                    {
                        new {
                            reference_id = evento.Slug,
                            name = evento.Nome,
                            quantity = 1,
                            unit_amount = (int)(inscricao.ValorInscricao * 100) // em centavos
                        }
                    },

                qr_codes = new[]
                {
                    new {
                        amount = new { value = (int)(inscricao.ValorInscricao * 100) },
                        expiration_date = DateTime.Now.AddDays(1)
                            .ToString("yyyy-MM-ddTHH:mm:sszzz") // formato ISO
                    }
                },
                notification_urls = new[]
                {
                    "https://backend.kerigma-eventos.online/api/webhook"
                }
            };

            Console.WriteLine("request: " + body.ToString());
            var json = JsonSerializer.Serialize(body, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            Console.WriteLine("JSON: " + json);


            var response = await http.PostAsJsonAsync($"{url}/orders", body);

            if (!response.IsSuccessStatusCode)
                Console.WriteLine("response: " + await response.Content.ReadAsStringAsync());

            Console.WriteLine("response: " + await response.Content.ReadAsStringAsync());

            var result = await response.Content.ReadFromJsonAsync<PagSeguroResponse>();

            var payLink = result.Qr_Codes?.FirstOrDefault()?.Links?.FirstOrDefault(l => l.Rel == "QRCODE.BASE64")?.Href;

            var content = new StringContent("{}", Encoding.UTF8, "application/json");

            var payResponse = await http.GetAsync(payLink);

            var payResult = await payResponse.Content.ReadAsStringAsync();

            if (!payResponse.IsSuccessStatusCode)
                Console.WriteLine("response: " + await response.Content.ReadAsStringAsync());

            result.QrCodeBase64 = payResult;

            return result;
        }

        public async Task<PagSeguroResponse> GerarPagamentoCartaoAsync(InscricaoDto inscricao)
        {
            string token = Environment.GetEnvironmentVariable("Token");
            string url = Environment.GetEnvironmentVariable("UrlPagSeguro");

            Evento evento = await _eventoRepository.GetById(inscricao.EventoId);

            using var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            string cpfSomenteNumeros = inscricao.Cpf
                .Replace(".", "")
                .Replace("-", "");

            var body = new
            {
                reference_id = inscricao.CodigoInscricao,

                customer = new
                {
                    name = inscricao.Nome,
                    email = inscricao.Email,
                    tax_id = cpfSomenteNumeros
                },

                items = new[]
                {
                    new {
                        reference_id = evento.Slug,
                        name = evento.Nome,
                        quantity = 1,
                        unit_amount = (int)(inscricao.ValorInscricao * 100)
                    }
                },

                notification_urls = new[]
                {
                    "https://backend.kerigma-eventos.online/api/webhook"
                },

                payment_methods = new[]
                {
                    new {
                        type = "CREDIT_CARD"
                    }
                },
                payment_methods_configs = new[]
                {
                    new
                    {
                        type = "CREDIT_CARD",
                        config_options = new[]
                        {
                            new
                            {
                                option = "INSTALLMENTS_LIMIT",
                                value = $"{evento.QtdParcelas}"
                            }
                        }
                    }
                },
            };

            Console.WriteLine("request: " + body.ToString());
            var json = JsonSerializer.Serialize(body, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            Console.WriteLine("JSON: " + json);

            var response = await http.PostAsJsonAsync($"{url}/checkouts", body);

            if (!response.IsSuccessStatusCode)
            {
                var erro = await response.Content.ReadAsStringAsync();
                throw new Exception(erro);
            }

             Console.WriteLine("response: " + await response.Content.ReadAsStringAsync());

            var result = await response.Content.ReadFromJsonAsync<PagSeguroResponse>();

            return result;
        }

        
    }
}



