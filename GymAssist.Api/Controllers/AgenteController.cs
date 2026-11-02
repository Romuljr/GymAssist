using GymAssist.Api.Dtos;
using GymAssist.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GymAssist.Api.Controllers
{
    [Route("api/v1/agente")]
    [ApiController]
    public class AgenteController : ControllerBase
    {
        private readonly AgenteService _agenteService;

        public AgenteController(AgenteService agenteService)
        {
            _agenteService = agenteService;
        }

        [HttpPost]
        public async Task<IActionResult> Post(AgenteRequest request) 
        {
            var resposta = await _agenteService
                .ProcessarPergunta(request);

            return Ok(new
            {
                resposta
            });
        }
    }
}
