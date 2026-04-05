using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace devia_be.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class LlmController : ControllerBase
{
    [HttpPost("generate-app")]
    public ActionResult GenerateApp()
    {
        return StatusCode(StatusCodes.Status501NotImplemented, new
        {
            message = "LLM pipeline pendiente de implementacion.",
            status = "pending"
        });
    }
}
