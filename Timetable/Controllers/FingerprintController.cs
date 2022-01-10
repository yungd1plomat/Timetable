using Microsoft.AspNetCore.Mvc;

namespace Timetable.Controllers
{
    public class FingerprintController : ControllerBase
    {
        [HttpGet]
        [Route("/")]
        [Route("api/{callback}")]
        public IActionResult Signature()
        {
            return Ok("Created by d1plomat");
        }
    }
}
