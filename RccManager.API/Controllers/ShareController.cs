using System;
using Microsoft.AspNetCore.Mvc;
using RccManager.Domain.Interfaces.Services;

namespace RccManager.API.Controllers
{
    [ApiController]
    [Route("eventos")]
    public class ShareController : ControllerBase
    {
        private readonly IEventoService _eventoService;


        public ShareController(IEventoService eventoService)
        {
            _eventoService = eventoService;
        }

        [HttpGet("{slug}")]
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
    }
}

