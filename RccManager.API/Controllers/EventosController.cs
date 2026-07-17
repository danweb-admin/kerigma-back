using System;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RccManager.Domain.Dtos.Evento;
using RccManager.Domain.Dtos.Formacao;
using RccManager.Domain.Exception.Decanato;
using RccManager.Domain.Interfaces.Services;
using System.Security.Claims;

namespace RccManager.API.Controllers
{
    [ApiController]
    [Route("api/v1/eventos")]
    [Authorize]
    public class EventosController : ControllerBase
    {
        private readonly IEventoService _eventoService;


        public EventosController(IEventoService eventoService)
        {
            _eventoService = eventoService;
        }

        [HttpGet("get-all-home")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllHome()
        {
            var eventos = await _eventoService.GetAllHome();
            return Ok(eventos);
        }

        [HttpGet("lote-inscricao")]
        [AllowAnonymous]
        public async Task<IActionResult> GetLoteInscricao(Guid eventoId)
        {
            try
            {
                var eventos = await _eventoService.LoteInscricao(eventoId);
                return Ok(eventos);
            }
            catch (Exception ex)
            {
                 return BadRequest(new Models.ValidationResult { Code = "500", Message = ex.Message, PropertyName = ex.Source });

            }
            
        }


        [HttpGet("get-all")]
        public async Task<IActionResult> Get()
        {
            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var eventos = await _eventoService.GetAll(userId);


            return Ok(eventos);
        }

        [HttpGet("get-events-by-email")]
        public async Task<IActionResult> GetByEmailCheckin(string email)
        {
            var eventos = await _eventoService.GetEventsByEmail(email);
            return Ok(eventos);
        }

        [HttpGet("get-slug")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSlug(string slug)
        {
            Console.WriteLine($"Evento: {slug}");
            
            var evento = await _eventoService.GetSlug(slug);
            return Ok(evento);
        }

        [HttpGet("share/{slug}")]
        [AllowAnonymous]
        public async Task<IActionResult> Share(string slug)
        {
            var evento = await _eventoService.GetSlug(slug);

            if (evento == null)
                return NotFound();

            var descricao = evento.Nome?.Replace("'", "&apos;") ?? "";
            var titulo = evento.Nome?.Replace("'", "&apos;") ?? "";
            var imagem = evento.CapaImagem;

            var html = $@"
                <!DOCTYPE html>
                <html>
                <head>
                <meta charset='utf-8'>

                <title>{titulo}</title>

                <meta property='og:title' content='{titulo}' />
                <meta property='og:description' content='{descricao}' />
                <meta property='og:image' content='{imagem}' />
                <meta property='og:url' content='https://www.kerigma-eventos.online/eventos/{slug}' />
                <meta property='og:type' content='website' />

                <meta name='twitter:card' content='summary_large_image' />

                <script>
                window.location.replace(
                'https://www.kerigma-eventos.online/eventos/{slug}'
                );
                </script>

                </head>
                <body>
                Redirecionando...
                </body>
                </html>";

            return Content(html, "text/html");
        }

        [HttpGet("verificar-cpf")]
        [AllowAnonymous]
        public async Task<IActionResult> VerificarCPF(string cpf, Guid eventoId)
        {
            try
            {
                Console.WriteLine($"CPF: {cpf}");
            
                var evento = await _eventoService.VerificarCPF(cpf,eventoId);

                return Ok(evento);
            }
            catch (WebException ex)
            {
               return NotFound("Servo não encontrado.");

            }catch(Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500);
            }
            
        }

        [HttpGet("{id}")]
        [AllowAnonymous]

        public async Task<IActionResult> GetById(Guid id)
        {
            var evento = await _eventoService.GetById(id);

            if (evento == null)
                return NotFound(new { message = "Evento não encontrado." });

            return Ok(evento);
        }

        [HttpGet("{eventoId}/campos")]
        [AllowAnonymous]

        public async Task<IActionResult> EventoCampos(Guid eventoId)
        {
            var camposFormularios = await _eventoService.GetCamposByEvento(eventoId);

            

            return Ok(camposFormularios);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] EventoDto eventoViewModel)
        {
            try
            {
                Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);


                var createdFormacao = await _eventoService.Create(eventoViewModel, userId);

                return Ok(HttpStatusCode.Created);
            }
            catch (ValidateByNameException ex)
            {
                return BadRequest(new Models.ValidationResult { Code = "400", Message = ex.Message, PropertyName = ex.Source });
            }

        }

        [HttpPost("inscricao")]
        [AllowAnonymous]
        public async Task<IActionResult> Inscricao([FromBody] InscricaoDto inscricao)
        {
            try
            {
                var inscricao_ = await _eventoService.Inscricao(inscricao);

                Console.WriteLine($"Inscricao: {inscricao.Nome}, {inscricao.CodigoInscricao}");

                return Ok(inscricao_);
            }
            catch (ValidateByNameException ex)
            {
                return BadRequest(new Models.ValidationResult { Code = "400", Message = ex.Message, PropertyName = ex.Source });
            }catch(WebException ex)
            {
               
                return BadRequest(new Models.ValidationResult { Code = "400", Message = ex.Message, PropertyName = ex.Source });
            }

        }

        [HttpGet("reenvio-comprovante")]
        public async Task<IActionResult> InscricaoReenvioComprovante([FromQuery] string codigoInscricao, string email)
        {
            try
            {
                var inscricao_ = await _eventoService.ReenvioComprovante(codigoInscricao,email);

                return Ok(inscricao_);
            }
            catch (ValidateByNameException ex)
            {
                return BadRequest(new Models.ValidationResult { Code = "400", Message = ex.Message, PropertyName = ex.Source });
            }
            catch (WebException ex)
            {

                return BadRequest(new Models.ValidationResult { Code = "400", Message = ex.Message, PropertyName = ex.Source });
            }

        }

        [HttpGet("verifica-status")]
        [AllowAnonymous]
        public async Task<IActionResult> VerificaStatus([FromQuery] string codigoInscricao)
        {
            try
            {
                var inscricao_ = await _eventoService.VerificaStatus(codigoInscricao);

                return Ok(new {
                    status = inscricao_
                });

            }
            catch (ValidateByNameException ex)
            {
                return BadRequest(new Models.ValidationResult { Code = "400", Message = ex.Message, PropertyName = ex.Source });
            }
            catch (WebException ex)
            {

                return BadRequest(new Models.ValidationResult { Code = "400", Message = ex.Message, PropertyName = ex.Source });
            }

        }

        [HttpGet("{eventoId}/inscricoes")]
        public async Task<IActionResult> InscricoesCheckin(Guid eventoId)
        {
            try
            {
                var inscricoes = await _eventoService.GetAllInscricoesByEvento(eventoId);

                return Ok(inscricoes);

            }
            catch (ValidateByNameException ex)
            {
                return BadRequest(new Models.ValidationResult { Code = "400", Message = ex.Message, PropertyName = ex.Source });
            }
            catch (WebException ex)
            {

                return BadRequest(new Models.ValidationResult { Code = "400", Message = ex.Message, PropertyName = ex.Source });
            }

        }

        [HttpGet("{codigoInscricao}/checkin")]
        public async Task<IActionResult> Checkin(string codigoInscricao)
        {
            try
            {

                var inscricoes = await _eventoService.FazerCheckin(codigoInscricao);

                return Ok(inscricoes);

            }
            catch (ValidateByNameException ex)
            {
                return BadRequest(new Models.ValidationResult { Code = "400", Message = ex.Message, PropertyName = ex.Source });
            }
            catch (WebException ex)
            {

                return BadRequest(new Models.ValidationResult { Code = "400", Message = ex.Message, PropertyName = ex.Source });
            }

        }

        [HttpGet("isentar-inscricao")]
        public async Task<IActionResult> IsentarInscricao([FromQuery] string codigoInscricao)
        {
            try
            {
                var inscricao_ = await _eventoService.IsentarInscricao(codigoInscricao);

                return Ok(inscricao_);
            }
            catch (ValidateByNameException ex)
            {
                return BadRequest(new Models.ValidationResult { Code = "400", Message = ex.Message, PropertyName = ex.Source });
            }
            catch (WebException ex)
            {

                return BadRequest(new Models.ValidationResult { Code = "400", Message = ex.Message, PropertyName = ex.Source });
            }

        }

        [HttpGet("verifica-limite-participantes")]
        [AllowAnonymous]
        public async Task<IActionResult> VerificaLimiteParticipantes([FromQuery] Guid eventoId)
        {
            try
            {
                var inscricao_ = await _eventoService.VerificaLimiteParticipante(eventoId);

                return Ok(inscricao_);
            }
            catch (ValidateByNameException ex)
            {
                return BadRequest(new Models.ValidationResult { Code = "400", Message = ex.Message, PropertyName = ex.Source });
            }
            catch (WebException ex)
            {

                return BadRequest(new Models.ValidationResult { Code = "400", Message = ex.Message, PropertyName = ex.Source });
            }

        }


        [HttpGet("remover-inscricao")]
        public async Task<IActionResult> RemoverInscricao([FromQuery] string codigoInscricao)
        {
            try
            {
                var inscricao_ = await _eventoService.RemoverInscricao(codigoInscricao);

                return Ok(inscricao_);
            }
            catch (ValidateByNameException ex)
            {
                return BadRequest(new Models.ValidationResult { Code = "400", Message = ex.Message, PropertyName = ex.Source });
            }
            catch (WebException ex)
            {

                return BadRequest(new Models.ValidationResult { Code = "400", Message = ex.Message, PropertyName = ex.Source });
            }
        }

        [HttpGet("gerar-qrcode-png")]
        [AllowAnonymous]
        public async Task<IActionResult> GerarQrCodePNG([FromQuery] Guid eventoId)
        {
            try
            {
                await _eventoService.GerarQrCodePNG(eventoId);

                return Ok();
            }
            catch (ValidateByNameException ex)
            {
                return BadRequest(new Models.ValidationResult { Code = "400", Message = ex.Message, PropertyName = ex.Source });
            }
            catch (WebException ex)
            {

                return BadRequest(new Models.ValidationResult { Code = "400", Message = ex.Message, PropertyName = ex.Source });
            }

        }

        [HttpGet("exportar-inscricoes/{eventoId}")]
        public async Task<IActionResult> Exportar(Guid eventoId)
        {
            var tabela = await _eventoService.ExportarInscricoes(eventoId);

            var excel = _eventoService.GerarExcel(tabela);

            return File(
                excel,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"inscricoes_{eventoId}.xlsx"
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] EventoDto eventoViewModel)
        {
            try
            {
                var updatedFormacao = await _eventoService.Update(eventoViewModel, id);

                if (updatedFormacao == null)
                    return NotFound();

                return Ok(HttpStatusCode.NoContent);
            }
            catch (ValidateByNameException ex)
            {
                return BadRequest(new Models.ValidationResult { Code = "400", Message = ex.Message, PropertyName = ex.Source });
            }
        }

        [HttpPut("migrar-inscricoes")]
        [AllowAnonymous]
        public async Task<IActionResult> MigrarInscricoes()
        {
            try
            {
                //await _eventoService.MigrarInscricoes();

                return Ok(HttpStatusCode.NoContent);
            }
            catch (ValidateByNameException ex)
            {
                return BadRequest(new Models.ValidationResult { Code = "400", Message = ex.Message, PropertyName = ex.Source });
            }
        }

        [HttpPut("liberar-saldo")]
        [AllowAnonymous]
        public async Task<IActionResult> LiberarSaldo()
        {
            try
            {
                //await _eventoService.LiberarSaldo();

                return Ok(HttpStatusCode.NoContent);
            }
            catch (ValidateByNameException ex)
            {
                return BadRequest(new Models.ValidationResult { Code = "400", Message = ex.Message, PropertyName = ex.Source });
            }
        }
        
    }
}

