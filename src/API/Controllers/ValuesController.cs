using Microsoft.AspNetCore.Mvc;

namespace LetsWork.API.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        /// <summary>
        /// The default GET API for Visneto
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(200)]
        public IActionResult Get()
        {
            return Ok(new { invocation_message = $"Hey from Visneto!" });
        }
    }
}
