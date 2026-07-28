using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RccManager.Domain.Interfaces.Services;

namespace RccManager.Api.Controllers
{
    //[Authorize]
    [Route("api/repasse")]
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
        [HttpPost("solicitar")]
        public async Task<IActionResult> SolicitarRepasse(Guid eventoId, decimal valor)
        {
            try
            {
                var result = await repasseService.SolicitarRepasse(eventoId, valor);

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
        public async Task<IActionResult> Aprovar(Guid id, Guid usuarioId)
        {
            try
            {
                await repasseService.AprovarRepasse(id, usuarioId);

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
            Guid usuarioId,
            [FromBody] string comprovante = null)
        {
            try
            {
                await repasseService.PagarRepasse(id, usuarioId, comprovante);

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