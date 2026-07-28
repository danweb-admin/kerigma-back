using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RccManager.Domain.Interfaces.Services;

namespace RccManager.API.Controllers
{
    //[Authorize]
    [Route("api/v1/wallet")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService service;

        public WalletController(IWalletService _service)
        {
            service = _service;
        }

        /// <summary>
        /// Retorna o resumo financeiro do evento
        /// </summary>
        [HttpGet("evento/{eventoId}")]
        public async Task<IActionResult> GetWalletEvento(Guid eventoId)
        {
            var wallet = await service.GetByEvento(eventoId);

            if (wallet == null)
                return NotFound();

            return Ok(wallet);
        }

        /// <summary>
        /// Retorna o extrato financeiro do evento
        /// </summary>
        [HttpGet("evento/{eventoId}/extrato")]
        public async Task<IActionResult> GetExtratoEvento(Guid eventoId)
        {
            var movimentos = await service.GetExtrato(eventoId);

            return Ok(movimentos);
        }
    }
}

