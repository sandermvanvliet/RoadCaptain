using Microsoft.AspNetCore.Mvc;

namespace RoadCaptain.App.Web.Controllers
{
    [ApiController]
    [Route("/2023-01/status")]
    public class StatusController : ControllerBase
    {
        [HttpGet(Name = "GetStatus")]
        public IActionResult GetStatus()
        {
            return Ok();
        }
    }
}
