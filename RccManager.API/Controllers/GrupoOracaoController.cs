using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RccManager.Domain.Dtos.GrupoOracao;
using RccManager.Domain.Interfaces.Services;
using RccManager.Service.Services;

namespace RccManager.API.Controllers;

[ApiController]
[Route("api/v1/grupo-oracao")]
[Authorize]
public class GrupoOracaoController : ControllerBase
{
    private readonly IGrupoOracaoService _grupoOracaoService;
    private readonly IUserService _userService;


    public GrupoOracaoController(IGrupoOracaoService grupoOracaoService, IUserService userService)
    {
        _grupoOracaoService = grupoOracaoService;
        _userService = userService;

    }

    [HttpGet("get-all")]
    public async Task<IActionResult> GetAll()
    {
        var grupoOracoes = await _grupoOracaoService.GetAll();
        return Ok(grupoOracoes);
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string search)
    {
        string _search = string.Empty;
        if (search != null)
            _search = search;

        var user = await _userService.GetUserContext(User);

        var grupoOracoes = await _grupoOracaoService.GetAll(_search, user);
        return Ok(grupoOracoes);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] GrupoOracaoDto grupoOracaoDto)
    {

        await _grupoOracaoService.Create(grupoOracaoDto);
        return Ok(HttpStatusCode.Created);

    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(Guid id, [FromBody] GrupoOracaoDto grupoOracaoDto)
    {

        var updatedGrupoOracao = await _grupoOracaoService.Update(grupoOracaoDto, id);
        if (updatedGrupoOracao == null)
            return NotFound();

        return Ok(HttpStatusCode.NoContent);

    }

    [HttpPut("import-csv/{id}")]
    public async Task<IActionResult> ImportCSV(Guid id)
    {
        var user = await _userService.GetUserContext(User);

        var updatedGrupoOracao = await _grupoOracaoService.ImportCSV(id,user);
        if (updatedGrupoOracao == null)
            return NotFound();

        return Ok(HttpStatusCode.NoContent);

    }


    [HttpPut("import-csv/all")]
    public async Task<IActionResult> ImportCSV()
    {
        var user = await _userService.GetUserContext(User);

        var updatedGrupoOracao = await _grupoOracaoService.ImportCSV(user);
        if (updatedGrupoOracao == null)
            return NotFound();

        return Ok(HttpStatusCode.NoContent);

    }
}

