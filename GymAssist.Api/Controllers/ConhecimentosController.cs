using GymAssist.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GymAssist.Api.Controllers
{
    [Route("api/v1/conhecimentos")]
    [ApiController]
    public class ConhecimentosController : ControllerBase
    {
        private readonly ConhecimentoService _service;

        public ConhecimentosController(ConhecimentoService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Post(ConhecimentoRequest request)
        {
            await _service.Cadastrar(request);

            return Ok(
                new
                {
                    mensagem = "Conhecimento armazenado com sucesso. "
                }
            );
        }
    }
}
