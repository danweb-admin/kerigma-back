using System;
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using RccManager.Domain.Dtos.Servo;
using RccManager.Domain.Dtos.Users;
using RccManager.Domain.Dtos.UsuarioCheckin;
using RccManager.Domain.Exception.Servo;
using RccManager.Domain.Interfaces.Services;
using RccManager.Service.Services;

namespace RccManager.API.Controllers
{
    [Route("api/v1/usuarios-checkin")]
    [ApiController]
    public class UsuariosCheckinController : ControllerBase
    {
        private readonly IUsuarioCheckinService _service;
        private readonly ITokenService _tokenService;


        public UsuariosCheckinController(IUsuarioCheckinService service, ITokenService tokenService)
        {
            _service = service;
            _tokenService = tokenService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string email)
        {
            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

            var usuarios = await _service.GetAll(email, userId);
            return Ok(usuarios);
        }

        

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UsuarioCheckinDto viewModel)
        {
            try
            {

                var usuario = await _service.Create(viewModel);

                return Ok(HttpStatusCode.Created);
            }
            catch (ValidateByCpfOrEmailException ex)
            {
                return BadRequest(new Models.ValidationResult { Code = "400", Message = ex.Message, PropertyName = ex.Source });
            }catch(WebException ex)
            {
               
                return BadRequest(new Models.ValidationResult { Code = "400", Message = ex.Message, PropertyName = ex.Source });
            }

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] UsuarioCheckinDto viewModel)
        {
            try
            {
                var updatedUsuario = await _service.Update(viewModel, id);

                if (updatedUsuario == null)
                    return NotFound();

                return Ok(HttpStatusCode.NoContent);
            }
            catch (ValidateByCpfOrEmailException ex)
            {
                return BadRequest(new Models.ValidationResult { Code = "400", Message = ex.Message, PropertyName = ex.Source });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UsuarioCheckinDto viewModel)
        {
            try
            {

                var usuario = await _service.Login(viewModel.Email, viewModel.Senha);

                var token = _tokenService.GenerateTokenCheckin(usuario);

                return Ok(new
                {
                    usuario,
                    token
                });
            }
            catch (WebException ex)
            {
                return BadRequest(new Models.ValidationResult { Code = "400", Message = ex.Message, PropertyName = ex.Source });
            }

        }

    }
}

