using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RoadCaptain.App.Shared;

namespace RoadCaptain.App.Web.Controllers
{
    [ApiController]
    [Route("/2023-01/status")]
    public class StatusController : ControllerBase
    {
        [HttpGet(Name = "GetStatus")]
        public IActionResult GetStatus()
        {
            var applicationDiagnosticInformation = ApplicationDiagnosticInformation.GetFrom(GetType().Assembly);
            return Ok(JsonConvert.SerializeObject(applicationDiagnosticInformation));
        }
    }
}
