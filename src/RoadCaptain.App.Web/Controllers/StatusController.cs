// Copyright (c) 2023 Sander van Vliet
// Licensed under Artistic License 2.0
// See LICENSE or https://choosealicense.com/licenses/artistic-2.0/

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

