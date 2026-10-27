using System;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RccManager.Domain.Dtos.InscricoesEvento;
using RccManager.Domain.Dtos.Servo;
using RccManager.Domain.Exception.Servo;
using RccManager.Domain.Interfaces.Services;

namespace RccManager.API.Controllers
{
    [ApiController]
    [Route("api/v1/inscricoes-evento")]
    [Authorize]
    public class InscricoesEventoController : ControllerBase
    {
        private readonly IInscricoesEventoService _inscricaoService;

        public InscricoesEventoController(IInscricoesEventoService inscricaoService)
		{
            _inscricaoService = inscricaoService;
		}

        [HttpGet]
        public async Task<IActionResult> Get(Guid grupoOracaoId)
        {
            var servos = await _inscricaoService.GetAll(grupoOracaoId);
            return Ok(servos);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] InscricoesEventoDto inscricaoViewModel)
        {
            try
            {

                var createdServo = await _inscricaoService.Create(inscricaoViewModel);

                return Ok(HttpStatusCode.Created);
            }
            catch (ValidateByCpfOrEmailException ex)
            {
                return BadRequest(new Models.ValidationResult { Code = "400", Message = ex.Message, PropertyName = ex.Source });
            }

        }

        [HttpGet("inscricao-cpf")]
        public async Task<IActionResult> InscricaoByCpf(string cpf, Guid eventId)
        {
            try
            {

                var inscricao = await _inscricaoService.CreateInscricaoByCpf(cpf,eventId);

                if (inscricao.StatusCode > 300)
                    return BadRequest(new Models.ValidationResult { Code = "400", Message = inscricao.Message });

                return Ok(HttpStatusCode.Created);
            }
            catch (ValidateByCpfOrEmailException ex)
            {
                return BadRequest(new Models.ValidationResult { Code = "400", Message = ex.Message, PropertyName = ex.Source });
            }

        }
    }
}

