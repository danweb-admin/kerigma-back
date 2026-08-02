using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RccManager.Domain.Dtos.Repasse;
using RccManager.Domain.Interfaces.Services;

namespace RccManager.Api.Controllers
{
    //[Authorize]
    [Route("api/v1/repasse")]
    [ApiController]
    public class RepasseController : ControllerBase
    {
        private readonly IRepasseService repasseService;

        public RepasseController(IRepasseService repasseService)
        {
            this.repasseService = repasseService;
        }

        /// <summary>
        /// Organizador solicita um repasse
        /// </summary>
        [HttpPost()]
        public async Task<IActionResult> SolicitarRepasse([FromBody] RepasseDto repasse)
        {
            try
            {
                var result = await repasseService.SolicitarRepasse(repasse);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Administrador aprova o repasse
        /// </summary>
        [HttpPut("{id}/aprovar")]
        public async Task<IActionResult> Aprovar(Guid id)
        {
            try
            {
                Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                await repasseService.AprovarRepasse(id, userId);

                return Ok("Repasse aprovado com sucesso.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Administrador confirma o pagamento do repasse
        /// </summary>
        [HttpPut("{id}/pagar")]
        public async Task<IActionResult> Pagar(
            Guid id,
            [FromBody] RepasseDto repasse)
        {
            try
            {
                Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

                await repasseService.PagarRepasse(id, userId, repasse.Comprovante);

                return Ok("Repasse pago com sucesso.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Cancela um repasse
        /// </summary>
        [HttpPut("{id}/cancelar")]
        public async Task<IActionResult> Cancelar(Guid id, [FromBody] string motivo)
        {
            try
            {
                await repasseService.CancelarRepasse(id, motivo);

                return Ok("Repasse cancelado.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Lista os repasses de um evento
        /// </summary>
        [HttpGet("evento/{eventoId}")]
        public async Task<IActionResult> GetByEvento(Guid eventoId)
        {
            var result = await repasseService.GetByEvento(eventoId);

            return Ok(result);
        }

        /// <summary>
        /// Lista todos os repasses pendentes
        /// </summary>
        [HttpGet("pendentes")]
        public async Task<IActionResult> GetPendentes()
        {
            var result = await repasseService.GetPendentes();

            return Ok(result);
        }

        /// <summary>
        /// Busca um repasse pelo Id
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await repasseService.GetById(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }
    }
}