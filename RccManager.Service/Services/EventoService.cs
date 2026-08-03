using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Drawing;
using System.Net;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AutoMapper;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RccManager.Domain.Dtos.Evento;
using RccManager.Domain.Entities;
using RccManager.Domain.Interfaces.Repositories;
using RccManager.Domain.Interfaces.Services;
using RccManager.Domain.Responses;
using RccManager.Service.Helper;
using RccManager.Service.Hubs;
using RccManager.Service.MQ;
using RccManager.Service.Services;
using static QRCoder.PayloadGenerator;

namespace RccManager.Domain.Services
{
  public class EventoService : IEventoService
    {
        private readonly IEventoRepository _eventoRepository;
        private readonly IInscricaoRepository _inscricaoRepository;
        private readonly IGrupoOracaoRepository _grupoOracaoRepository;
        private readonly IDecanatoSetorRepository _decanatoRepository;
        private readonly IEventoUsuariosRepository _eventoUsuariosRepository;
        private readonly IPagSeguroService _pagSeguroService;
        private readonly IPagamentoAsaasService _pagamentoAsaasService;
        private readonly IServoRepository _servoRepository;
        private readonly EmailQueueProducer _producer;
        private readonly IHubContext<CheckinHub> _hub;
        private readonly IEmailService _emailService;
        private readonly WhatsAppProducer _whatsAppProducer;
        private readonly IFinanceiroService _financeiroService;
        private readonly IWalletService _walletService;

        private readonly IMapper _mapper;


        public EventoService(IEventoRepository eventoRepository, IInscricaoRepository inscricaoRepository, IGrupoOracaoRepository grupoOracaoRepository, IDecanatoSetorRepository decanatoRepository, IPagSeguroService pagSeguroService, EmailQueueProducer producer, IMapper mapper, IPagamentoAsaasService pagamentoAsaasService, IHubContext<CheckinHub> hub, IEventoUsuariosRepository eventoUsuariosRepository, IEmailService emailService, IServoRepository servoRepository, WhatsAppProducer whatsAppProducer, IFinanceiroService financeiroService, IWalletService walletService)
        {
            _eventoRepository = eventoRepository;
            _inscricaoRepository = inscricaoRepository;
            _decanatoRepository = decanatoRepository;
            _grupoOracaoRepository = grupoOracaoRepository;
            _pagSeguroService = pagSeguroService;
            _mapper = mapper;
            _producer = producer;
            _pagamentoAsaasService = pagamentoAsaasService;
            _hub = hub;
            _eventoUsuariosRepository = eventoUsuariosRepository;
            _emailService = emailService;
            _servoRepository = servoRepository;
            _whatsAppProducer = whatsAppProducer;
            _financeiroService = financeiroService;
            _walletService = walletService;
        }

        public async Task<HttpResponse> Create(EventoDto dto, Guid userId)
        {
            if (!dto.TaxaServico.HasValue)
                dto.TaxaServico = 0;

            if (!dto.LimiteParticipantes.HasValue)
                dto.LimiteParticipantes = 0;

            var evento = _mapper.Map<Evento>(dto);



            // Garantir mapeamento correto das filhas
            
            if (dto.Local != null)
                evento.Local = _mapper.Map<Local>(dto.Local);

            if (dto.Sobre != null)
                evento.Sobre = _mapper.Map<Sobre>(dto.Sobre);

            if (dto.InformacoesAdicionais != null)
                evento.InformacoesAdicionais = _mapper.Map<InformacoesAdicionais>(dto.InformacoesAdicionais);

            if (dto.Participacoes?.Any() == true)
                evento.Participacoes = dto.Participacoes.Select(p => _mapper.Map<Participacao>(p)).ToList();

            if (dto.Programacao?.Any() == true)
                evento.Programacao = dto.Programacao.Select(p => _mapper.Map<Programacao>(p)).ToList();

            if (dto.LotesInscricoes?.Any() == true)
                evento.LotesInscricoes = dto.LotesInscricoes.Select(l => _mapper.Map<LoteInscricao>(l)).ToList();

            if (dto.Inscricoes?.Any() == true)
                evento.Inscricoes = dto.Inscricoes.Select(i => _mapper.Map<Inscricao>(i)).ToList();

            try
            {
                var result = await _eventoRepository.Insert(evento);

                if (result == null)
                    return new HttpResponse { Message = "Houve um problema para adicionar o evento", StatusCode = (int)HttpStatusCode.BadRequest };

                var eventoUsuario = new EventoUsuarios
                {
                    Active = true,
                    CreatedAt = DateTime.Now,
                    EventoId = result.Id,
                    UserId = userId
                };
                var result1 = await _eventoUsuariosRepository.Insert(eventoUsuario);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex);
            }

            return new HttpResponse { Message = "Evento adicionado com sucesso.", StatusCode = (int)HttpStatusCode.OK };

        }

        public async Task<IEnumerable<EventoDtoResult>> GetAll(Guid userId)
        {
            var eventos = await _eventoRepository.GetAll(userId);

            return _mapper.Map<IEnumerable<EventoDtoResult>>(eventos);
        }

        public async Task<IEnumerable<CamposFormularioDto>> GetCamposByEvento(Guid eventoId)
        {
            var campos = await _eventoRepository.GetCamposByEvento(eventoId);

            foreach (var campo in campos)
            {
                campo.NomeCampo = GerarNomeCampo(campo.Label);
            }

            return _mapper.Map<IEnumerable<CamposFormularioDto>>(campos);
        }

        public async Task<IEnumerable<EventoDtoResult>> GetAllHome()
        {
            return _mapper.Map<IEnumerable<EventoDtoResult>>(await _eventoRepository.GetAllHome());
        }

        public async Task<EventoDto> GetById(Guid id)
        {
            return _mapper.Map<EventoDto>(await _eventoRepository.GetById(id));
        }

        public async Task<EventoDto> GetSlug(string slug)
        {
            return _mapper.Map<EventoDto>(await _eventoRepository.GetSlug(slug));
        }

        public async Task<string> VerificaStatus(string codigoInscricao)
        {
            var inscricao = await _inscricaoRepository.GetByInscricao(codigoInscricao);

            if (inscricao == null)
                return "PENDENTE";

            if (inscricao.Status == "pagamento_confirmado")
                return "PAGO";
            return "PENDENTE";
        }

        public async Task<IEnumerable<EventoDtoResult>> GetEventsByEmail(string email)
        {
            return _mapper.Map<IEnumerable<EventoDtoResult>>(await _eventoRepository.GetEventsByEmail(email));
        }

        public async Task<InscricaoDto> VerificarCPF(string cpf, Guid eventoId)
        {
            cpf = cpf.Replace(".","").Replace("-","");

            var verificaCPF = await _inscricaoRepository.CheckByCpf(eventoId,cpf);

            if (verificaCPF != null)
                return _mapper.Map<InscricaoDto>(verificaCPF);

            var servo = await _servoRepository.GetServoByCPF(cpf);

            if (servo != null)
            {
                var valorInscricao = await LoteInscricao(eventoId);

                var inscricao = new Inscricao
                {
                    Active = true,
                    Cpf = servo.CpfPlain,
                    Nome = servo.NamePlain,
                    DecanatoId = servo.GrupoOracao.ParoquiaCapela.DecanatoId,
                    GrupoOracaoId = servo.GrupoOracaoId,
                    ServoId = servo.Id,
                    Email = servo.EmailPlain,
                    Telefone = servo.CellphonePlain,
                    EventoId = eventoId,
                    ValorInscricao = valorInscricao

                };

                return _mapper.Map<InscricaoDto>(inscricao);
            }
            

            throw new WebException("Servo não encontrado");
        }

        public async Task<InscricaoDto> Inscricao(InscricaoDto inscricao)
        {

            var verificaCPF = await _inscricaoRepository.CheckByCpf(inscricao.EventoId, inscricao.Cpf);
            var financeira = Environment.GetEnvironmentVariable("Financeira");

            if (verificaCPF != null && verificaCPF.Status == "pagamento_confirmado")
                throw new WebException("CPF já está cadastrado no Evento!");

            //if (verificaCPF != null && verificaCPF.Status == "pendente")
            //{
            //    return _mapper.Map<InscricaoDto>(verificaCPF);
            //}

            inscricao.Email = inscricao.Email.Trim();

            if (!EmailValido(inscricao.Email))
            {
                throw new WebException("Email inválido");
            }

            if (inscricao.Status == null)
                inscricao.Status = "pendente";

            if (string.IsNullOrEmpty(inscricao.CodigoInscricao))
            {
                inscricao.CodigoInscricao = GerarCodigoInscricao();
            }
            inscricao.Nome = inscricao.Nome.ToUpper();    


            if (inscricao.Cpf.Length == 11)
                inscricao.Cpf = FormatarCpf(inscricao.Cpf);

            if (inscricao.Telefone.Length == 10 || inscricao.Telefone.Length == 11)
                inscricao.Telefone = FormatarTelefone(inscricao.Telefone);

            if (inscricao.DecanatoId.HasValue)
            {
                var decanato = await _decanatoRepository.GetById(inscricao.DecanatoId.Value);
                inscricao.Decanato = decanato.Name;
            }

            if (inscricao.GrupoOracaoId.HasValue)
            {
                var grupoOracao = await _grupoOracaoRepository.GetById(inscricao.GrupoOracaoId.Value);
                inscricao.GrupoOracao = grupoOracao.Name;
            }

            var slug = await _eventoRepository.GetSlug(inscricao.EventoId);

            var inscricao_ = _mapper.Map<Inscricao>(inscricao);

            inscricao_.CreatedAt = DateTime.Now;
            inscricao_.Status = "pendente";

            if (financeira == "PagSeguro")
            {
                if (inscricao.TipoPagamento == "gratuito")
                {
                    inscricao_.Status = "pagamento_confirmado";
                }

                if (inscricao.TipoPagamento == "pix")
                {
                    var qrCode = await _pagSeguroService.GerarLinkPagamentoAsync(inscricao);

                    if (qrCode == null)
                        throw new WebException("Houve um problema para gerar QRCode do pagamento da inscrição!");

                    inscricao_.QRCodeCopiaCola = qrCode.Qr_Codes?.FirstOrDefault()?.Text;
                    inscricao_.LinkQrCodePNG = qrCode.Qr_Codes?.FirstOrDefault()?.Links?.FirstOrDefault(l => l.Rel == "QRCODE.PNG")?.Href;
                    inscricao_.LinkQrCodeBase64 = qrCode.QrCodeBase64;
                }

                if (inscricao.TipoPagamento == "cartao")
                {
                    var cobranca = await _pagSeguroService.GerarPagamentoCartaoAsync(inscricao);

                    inscricao_.LinkPgtoCartao = cobranca.Links.First(l => l.Rel == "PAY").Href;
                }
            }

            if (financeira == "Asaas")
            {
                // ✅ PIX ASAAS
                if (inscricao.TipoPagamento == "pix" || inscricao.TipoPagamento == "dinheiro")
                {

                    var cobranca = await _pagamentoAsaasService.CreatePixAsync(inscricao_,inscricao.ValorInscricao,$"{inscricao.CodigoInscricao} | {slug}" );

                    if (cobranca == null)
                        throw new WebException("Erro ao gerar cobrança PIX no Asaas!");

                    inscricao_.QRCodeCopiaCola = cobranca.PixPayload;
                    inscricao_.LinkQrCodePNG = cobranca.BillingType;
                    inscricao_.LinkQrCodeBase64 = cobranca.PixQrBase64;
                    inscricao_.NumeroFatura = cobranca.InvoiceNumber;

                    if (inscricao.TipoPagamento == "dinheiro")
                    {
                        var cobranc = await _pagamentoAsaasService.ConfirmarRecebimentoDinheiro(inscricao_.NumeroFatura,inscricao.ValorInscricao,DateTime.Now );

                    }
                }

                if (inscricao.TipoPagamento == "cartao")
                {
                    var cobranca = await _pagamentoAsaasService.CreateCartaoCreditoAsync(inscricao,inscricao.ValorInscricao,$"{inscricao.CodigoInscricao} | {slug}" );

                    inscricao_.LinkPgtoCartao = cobranca.InvoiceUrl;
                }
            }

            var result = await _inscricaoRepository.Insert(inscricao_);

            foreach (var item in inscricao.CamposDinamicos )
            {
                item.InscricaoId = result.Id;
                item.Id = Guid.NewGuid();

                item.Valor = string.IsNullOrWhiteSpace(item.Valor) ? "." : item.Valor;
                var camposValores = _mapper.Map<InscricaoCampoValores>(item);
                await _inscricaoRepository.InsertCamposDinamicos(camposValores);
                
            }

            if (result == null)
                throw new WebException("Houve um problema para efetuar a Inscrição!");

            if (inscricao.TipoPagamento == "gratuito")
            {
                var insc = await _inscricaoRepository.GetByCodigo(inscricao.CodigoInscricao);

                var inscricaoMQ = ConvertInscricaoMQ(insc);

                inscricaoMQ.Email = insc.Email;

                await _producer.PublishEmail(inscricaoMQ);

                await _whatsAppProducer.PublishWhatsAppMessage(inscricaoMQ);
            }

            return _mapper.Map<InscricaoDto>(result);

        }

        public async Task<decimal> LoteInscricao(Guid id)
        {
            var timeZoneBrasil = TimeZoneInfo.FindSystemTimeZoneById(
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "E. South America Standard Time"
                    : "America/Sao_Paulo"
            );

            var hoje = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneBrasil);

            var evento = await _eventoRepository.GetById(id);

            var valorInscricao = evento.LotesInscricoes.FirstOrDefault(x => hoje.Date >= x.DataInicio.Date  && hoje.Date <=  x.DataFim.Date);

            if (valorInscricao == null)
            {
                return 0;
                throw new KeyNotFoundException("Valor da Inscrição não encontrada.");
            }
                

            return valorInscricao.Valor;
        }

        public async Task<HttpResponse> Update(EventoDto dto, Guid id)
        {
            var evento = await _eventoRepository.GetById(dto.Id.Value);

            if (evento == null)
                throw new KeyNotFoundException("Evento não encontrado.");

            //_mapper.Map(dto, evento);

            evento.BannerImagem = dto.BannerImagem;
            evento.CapaImagem = dto.CapaImagem;
            evento.Nome = dto.Nome;
            evento.Slug = dto.Slug;
            evento.DataInicio = dto.DataInicio;
            evento.DataFim = dto.DataFim;
            evento.OrganizadorNome = dto.OrganizadorNome;
            evento.OrganizadorEmail = dto.OrganizadorEmail;
            evento.OrganizadorContato = dto.OrganizadorContato;
            evento.Status = dto.Status;
            evento.ExibirPregadores = dto.ExibirPregadores;
            evento.ExibirProgramacao = dto.ExibirProgramacao;
            evento.ExibirInformacoesAdicionais = dto.ExibirInformacoesAdicionais;
            evento.HabilitarPix = dto.HabilitarPix;
            evento.HabilitarDinheiro = dto.HabilitarDinheiro;
            evento.HabilitarCartao = dto.HabilitarCartao;
            evento.QtdParcelas = dto.QtdParcelas;
            evento.LimiteParticipantes = dto.LimiteParticipantes.Value;
            evento.TaxaServico = dto.TaxaServico.Value;


            // =============== ENTIDADES 1:1 =================
            AtualizarOuCriarFilha(dto.Local, evento.Local, v => evento.Local = v);
            AtualizarOuCriarFilha(dto.Sobre, evento.Sobre, v => evento.Sobre = v);
            AtualizarOuCriarFilha(dto.InformacoesAdicionais, evento.InformacoesAdicionais, v => evento.InformacoesAdicionais = v);

            // =============== ENTIDADES 1:N (MERGE) =================
            MergeColecao(dto.Participacoes, evento.Participacoes, (dtoItem, entityItem) => dtoItem.Id == entityItem.Id);
            MergeColecao(dto.Programacao, evento.Programacao, (dtoItem, entityItem) => dtoItem.Id == entityItem.Id);
            MergeColecao(dto.LotesInscricoes, evento.LotesInscricoes, (dtoItem, entityItem) => dtoItem.Id == entityItem.Id);
            MergeColecao(dto.EventoCampos, evento.EventoCampos, (dtoItem, entityItem) => dtoItem.Id == entityItem.Id);

            var result = await _eventoRepository.Update(evento);

            if (result == null)
                return new HttpResponse { Message = "Houve um problema para atualizar o evento", StatusCode = (int)HttpStatusCode.BadRequest };

            return new HttpResponse { Message = "Evento atualizado com sucesso.", StatusCode = (int)HttpStatusCode.OK };

        }

        public async Task<ValidationResult> ReenvioComprovante(string codigoInscricao, string email)
        {
            var inscricao = await _inscricaoRepository.GetByCodigo(codigoInscricao);

            if (inscricao == null)
                throw new WebException("Inscrição não encontrado!");

            var inscricaoMQ = ConvertInscricaoMQ(inscricao);

            inscricaoMQ.Email = email;

            //await _emailService.EnviarEmailPagamentoConfirmado(inscricaoMQ);

            await _producer.PublishEmail(inscricaoMQ);

            await _whatsAppProducer.PublishWhatsAppMessage(inscricaoMQ);

            return ValidationResult.Success;
        }

        public async Task<ValidationResult> IsentarInscricao(string codigoInscricao)
        {
            var inscricao = await _inscricaoRepository.GetByCodigo(codigoInscricao);

            if (inscricao == null)
                throw new WebException("Inscrição não encontrado!");

            var inscricaoMQ = ConvertInscricaoMQ(inscricao);

            inscricao.Status = "isento";

            var result = await _inscricaoRepository.Update(inscricao);

            if (result == null)
                throw new WebException("Houve um problema para isentar a Inscrição!");

            await _producer.PublishEmail(inscricaoMQ);

            await _whatsAppProducer.PublishWhatsAppMessage(inscricaoMQ);


            return ValidationResult.Success;
        }

        public async Task<ValidationResult> RemoverInscricao(string codigoInscricao)
        {
            var inscricao = await _inscricaoRepository.GetByCodigo(codigoInscricao);

            if (inscricao == null)
                throw new WebException("Inscrição não encontrado!");

            var result = await _inscricaoRepository.Delete(inscricao.Id);

            if (!result)
                throw new WebException("Houve um problema para isentar a Inscrição!");

            return ValidationResult.Success;
        }

        public async Task<IEnumerable<InscricaoDto>> GetAllInscricoesByEvento(Guid eventoId)
        {
            var lista = await _eventoRepository.GetAllInscricoesByEvento(eventoId);

            return _mapper.Map<IEnumerable<InscricaoDto>>(lista);

        }

        public async Task<HttpResponse> FazerCheckin(string codigoInscricao)
        {
            var inscricao = await _inscricaoRepository.GetByCodigo(codigoInscricao);

            if (inscricao == null)
                throw new WebException("Inscrição não encontrado!");

            if (inscricao.CheckIn.HasValue)
                throw new WebException("Esse QR Code já foi utilizado!");

            inscricao.CheckIn = true;
            inscricao.DataCheckIn = DateTime.Now;

            var result = await _inscricaoRepository.Update(inscricao);

            if (result == null)
                return new HttpResponse { Message = "Houve um problema para fazer o checkin", StatusCode = (int)HttpStatusCode.BadRequest };

            await _hub.Clients.All.SendAsync("checkin-realizado", codigoInscricao);


            return new HttpResponse { Message = "Checkin efetuado com sucesso.", StatusCode = (int)HttpStatusCode.OK };


        }

        public async Task VerificaInscricoesPendentes()
        {
            Console.WriteLine("******** INICIO - VerificaInscricoesPendentes() ********");
            var dataBase = DateTime.Now.AddHours(-3);

            Console.WriteLine("DataBase: " + dataBase.ToString("dd/MM/yyyy HH:mm:ss"));


            var  pendentes = await _inscricaoRepository.GetAllPending(dataBase);

            if (pendentes.Any())
            {
                Console.WriteLine($"******** {pendentes.Count()} INSCRICOES ENCONTRADAS ********");
                foreach (var item in pendentes)
                {
                    Console.WriteLine($"Inscricao: {item.CodigoInscricao}, Nome: {item.Nome}, Email: {item.Email}, Valor: {item.ValorInscricao}");
                    await _inscricaoRepository.Delete(item.Id);
                }
                
                
            }else
            {
                Console.WriteLine("******** NENHUMA INSCRICAO PENDENTE ********");
            }



            
            Console.WriteLine("******** FIM - VerificaInscricoesPendentes() ********");
            await Task.CompletedTask;
        }

        //  WEBHOOK
        public async Task<ValidationResult> EventosWebhook(string response)
        {
            var financeira = Environment.GetEnvironmentVariable("Financeira");
            PagSeguroWebhook webhookPagSeguro = new PagSeguroWebhook();

            Inscricao inscricao = new Inscricao();

            if (financeira == "Asaas")
            {
                var webhookResponse = JsonConvert.DeserializeObject<AsaasWebhookPayload>(response);

                var split = webhookResponse.payment.description.Split("|");

                var codigoInscricao = split[0].Trim();

                inscricao = await _inscricaoRepository.GetByCodigo(codigoInscricao);

                if (inscricao == null)
                    return new ValidationResult("❌ Erro no processamento do webhook.");

                if  (webhookResponse.@event == "PAYMENT_OVERDUE")
                {
                    inscricao.Status = "pagamento_expirado";
                    await _inscricaoRepository.Update(inscricao);

                    Console.WriteLine($"❌ Pagamento expirado: {inscricao.CodigoInscricao} - {inscricao.Nome}");

                    return ValidationResult.Success;
                }
              

                if (webhookResponse.@event != "PAYMENT_RECEIVED" && webhookResponse.@event != "PAYMENT_CONFIRMED")
                    return new ValidationResult("❌ Erro no processamento do pagamento.");

                // Se já estava paga, ignore
                if (inscricao.Status == "pagamento_confirmado")
                    return ValidationResult.Success;

                // Atualizar status para pago
                inscricao.Status = "pagamento_confirmado";
                inscricao.DataPagamento = webhookResponse.dateCreated ?? DateTime.Now;

            }


            if (financeira == "PagSeguro")
            {
                webhookPagSeguro = JsonConvert.DeserializeObject<PagSeguroWebhook>(response);

                string codigoInscricao = webhookPagSeguro.Reference_Id;

                inscricao = await _inscricaoRepository.GetByCodigo(codigoInscricao);

                if (inscricao == null)
                    return new ValidationResult("❌ Erro no processamento do webhook.");

                // Se já estava paga, ignore
                if (inscricao.Status == "pagamento_confirmado")
                    return ValidationResult.Success;

                var charge = webhookPagSeguro.Charges.First();

                if (charge.Status != "PAID")
                    return new ValidationResult("❌ Erro no processamento do pagamento.");

                // Atualizar status para pago
                inscricao.Status = "pagamento_confirmado";
                inscricao.DataPagamento = charge.Paid_At ?? DateTime.Now;
                inscricao.NSU = charge.Payment_Response.Reference;

                CalculaValorLiquidoPagSeguro(ref inscricao, charge);

            }

            await _inscricaoRepository.Update(inscricao);

            try
            {
                await _financeiroService.RegistrarFinanceiro(inscricao, webhookPagSeguro);
            }
            catch (System.Exception ex)
            {
               Console.WriteLine("*-*-*-* ERRO FINANCEIRO *-*-*-*: ");

            }
            

            var inscricaoMQ = ConvertInscricaoMQ(inscricao);

            await _producer.PublishEmail(inscricaoMQ);

            await _whatsAppProducer.PublishWhatsAppMessage(inscricaoMQ);


            var limiteParticipantesEvento = inscricao.Evento.LimiteParticipantes;
            var participantesConfirmados = await _eventoRepository.GetLimiteParticipantes(inscricao.EventoId);

            Console.WriteLine($"EVENTO: {inscricao.Evento.Nome}");
            Console.WriteLine($"LIMITE PARTICIPANTES: {limiteParticipantesEvento}");
            Console.WriteLine($"PARTICIPANTES CONFIRMADOS: {participantesConfirmados}");


            return ValidationResult.Success;
        }

        public async Task<bool> VerificaLimiteParticipante(Guid eventoId)
        {
            var evento = await _eventoRepository.GetById(eventoId);
            var participantesConfirmados = await _eventoRepository.GetLimiteParticipantes(eventoId);

            if (evento.LimiteParticipantes == 0 )
                return true;

            if (participantesConfirmados >= evento.LimiteParticipantes)
            {
                evento.Status = "Encerradas";
                await _eventoRepository.Update(evento);

                return false;
            }
                

            return true;
        }

        public async Task GerarQrCodePNG(Guid eventoId)
        {
            var inscricoes = await _inscricaoRepository.GetAll(eventoId);

            foreach (var inscricao in inscricoes)
            {
                Utils.GerarQrCodePNG(inscricao.CodigoInscricao);
            }
        }

        public async Task<DataTable> ExportarInscricoes(Guid eventoId)
        {
            return await _eventoRepository.ExportarInscricoes(eventoId);
        }

        public byte[] GerarExcel(DataTable tabela)
        {
            using var workbook = new XLWorkbook();
            workbook.Worksheets.Add(tabela, "Inscricoes");

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return stream.ToArray();
        }

        public async Task MigrarInscricoes()
        {
            var inscricoes = await _inscricaoRepository.GetPagasSemFinanceiro();

            foreach(var inscricao in inscricoes)
            {
                await _financeiroService.RegistrarFinanceiroMigracao(inscricao);
            }
        }

        public async Task LiberarSaldo()
        {
            
            await _financeiroService.LiberarRecebimentosCartao();
            
        }

        private bool EmailValido(string email)
        {
            string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern);
        }

        private void CalculaValorLiquidoPagSeguro(ref Inscricao inscricao, ChargeWebhook charge)
        {
            decimal taxaServico = 0;

            var evento = _eventoRepository.GetById(inscricao.EventoId).GetAwaiter().GetResult();

            if (evento == null)
                decimal.TryParse(Environment.GetEnvironmentVariable("TaxaServico"), out taxaServico);

            decimal percentual = ObterPercentualTaxa(inscricao, charge);

            decimal valor = inscricao.ValorInscricao;

            decimal valorComTaxa = Math.Round(valor * (1 - percentual), 2);

            decimal taxaFinanceira = valor - valorComTaxa;

            inscricao.TaxaServico = taxaServico;

            if (inscricao.TipoPagamento == "pix")
                inscricao.DataLiberacao = DateTime.Now;
            else
                inscricao.DataLiberacao = DateTime.UtcNow.AddDays(14);
            inscricao.DataPagamento = DateTime.Now;
            inscricao.TaxaFinanceira = taxaFinanceira;
            inscricao.ValorLiquido = valor - taxaServico - taxaFinanceira;
        }

        private decimal ObterPercentualTaxa(Inscricao inscricao, ChargeWebhook charge)
        {
            if (inscricao.TipoPagamento == "pix")
            {
                decimal.TryParse(Environment.GetEnvironmentVariable("TaxaPix"), out decimal taxa);
                return taxa / 100m;
            }

            if (inscricao.TipoPagamento == "cartao")
            {
                if (charge.Payment_Method.Installments <= 1)
                {
                    decimal.TryParse(Environment.GetEnvironmentVariable("TaxaCartaoAvista"), out decimal taxa);
                    return taxa / 100m;
                }

                decimal.TryParse(Environment.GetEnvironmentVariable("TaxaCartaoParcelado"), out decimal taxaParcelado);
                return taxaParcelado / 100m;
            }

            return 0;
        }

        

        // ==========================================================
        // MÉTODO AUXILIAR PARA 1:1
        // ==========================================================
        private void AtualizarOuCriarFilha<TDto, TEntity>(TDto dto, TEntity entity, Action<TEntity> setEntity)
            where TDto : class
            where TEntity : class, new()
        {
            if (dto == null) return;

            if (entity == null)
            {
                entity = new TEntity();
                _mapper.Map(dto, entity);
                setEntity(entity);
            }
            else
            {
                _mapper.Map(dto, entity);
            }
        }

        // ==========================================================
        // MÉTODO AUXILIAR PARA 1:N (MERGE INTELIGENTE)
        // ==========================================================
        private void MergeColecao<TDto, TEntity>(
        IEnumerable<TDto>? dtos,
        ICollection<TEntity> entidades,
        Func<TDto, TEntity, bool> matchFunc)
        where TDto : class
        where TEntity : class, new()
        {
            if (dtos == null)
                return;

            // IDs válidos vindos do DTO
            var idsDto = dtos
                .Select(d => d.GetType().GetProperty("Id")?.GetValue(d))
                .OfType<Guid>()
                .Where(id => id != Guid.Empty)
                .ToList();

            // Atualizar e adicionar
            foreach (var dto in dtos)
            {
                var idDto = (Guid?)(dto.GetType().GetProperty("Id")?.GetValue(dto));

                // tenta achar item existente
                var existente = idDto.HasValue && idDto.Value != Guid.Empty
                    ? entidades.FirstOrDefault(e =>
                    {
                        var idEntity = (Guid)(e.GetType().GetProperty("Id")?.GetValue(e) ?? Guid.Empty);
                        return idEntity == idDto.Value;
                    })
                    : null;

                if (existente != null)
                {
                    _mapper.Map(dto, existente);
                }
                else
                {
                    // adiciona apenas se não duplicado
                    var novaEntidade = _mapper.Map<TEntity>(dto);
                    entidades.Add(novaEntidade);
                }
            }

            // Remover entidades que não vieram no DTO
            var remover = entidades
                .Where(e =>
                {
                    var id = (Guid)(e.GetType().GetProperty("Id")?.GetValue(e) ?? Guid.Empty);
                    return id != Guid.Empty && !idsDto.Contains(id);
                })
                .ToList();

            foreach (var item in remover)
                entidades.Remove(item);
        }

        private string GerarCodigoInscricao()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var bytes = new byte[6];
            var token = new StringBuilder(6);

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            foreach (var b in bytes)
            {
                token.Append(chars[b % chars.Length]);
            }

            return $"INS-{token}";
        }

        private  string FormatarCpf(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return string.Empty;

            // Remove tudo que não for número
            cpf = new string(cpf.Where(char.IsDigit).ToArray());

            // Garante que tenha 11 dígitos
            if (cpf.Length != 11)
                return cpf; // retorna como está se estiver incorreto

            return Convert.ToUInt64(cpf).ToString(@"000\.000\.000\-00");
        }

        private string FormatarTelefone(string telefone)
        {
            if (string.IsNullOrWhiteSpace(telefone))
                return string.Empty;

            // Remove tudo que não for número
            telefone = new string(telefone.Where(char.IsDigit).ToArray());

            // Garante que tenha DDD + número
            if (telefone.Length == 10)
            {
                // Exemplo: 43998887777 → (43) 9888-7777
                return Convert.ToUInt64(telefone).ToString(@"\(00\) 0000\-0000");
            }
            else if (telefone.Length == 11)
            {
                // Exemplo: 43998887777 → (43) 99888-7777
                return Convert.ToUInt64(telefone).ToString(@"\(00\) 0 0000\-0000");
            }

            // Caso formato inválido, retorna original
            return telefone;
        }

        private InscricaoMQResponse ConvertInscricaoMQ(Inscricao inscricao) 
        {
            var retorno = new InscricaoMQResponse
            {
                CodigoInscricao = inscricao.CodigoInscricao,
                Cpf = inscricao.Cpf,
                DataFim = inscricao.Evento.DataFim,
                DataInicio = inscricao.Evento.DataInicio,
                Email = inscricao.Email,
                Nome = inscricao.Nome,
                NomeEvento = inscricao.Evento.Nome,
                ValorInscricao = inscricao.ValorInscricao,
                OrganizadorNome = inscricao.Evento.OrganizadorNome,
                Local = formatarLocal(inscricao.Evento.Local),
                Status = inscricao.Status,
                Telefone = inscricao.Telefone
            };

            return retorno;
        }

        private string formatarLocal(Local local)
        {
            var partes = new List<string>();

            if (!string.IsNullOrWhiteSpace(local.Endereco))
                partes.Add(local.Endereco);

            if (!string.IsNullOrWhiteSpace(local.Complemento))
                partes.Add(local.Complemento);

            if (!string.IsNullOrWhiteSpace(local.Bairro))
                partes.Add(local.Bairro);

            if (!string.IsNullOrWhiteSpace(local.Cidade))
                partes.Add(local.Cidade);

            if (!string.IsNullOrWhiteSpace(local.Estado))
                partes.Add(local.Estado);

            return string.Join(", ", partes);
        }

        private string GerarNomeCampo(string label)
        {
            return label
                .ToLower()
                .Replace(" ", "")
                .Replace("ã", "a")
                .Replace("á", "a")
                .Replace("é", "e");
        }

        
    }
}
