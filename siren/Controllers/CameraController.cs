using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CatSiren
{
    [ApiController]
    [Route("[controller]")]
    public class CameraController : ControllerBase
    {
        private readonly ILogger<CameraController> _logger;

        public CameraController(ILogger<CameraController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetPhoto()
        {
            // TODO: Unosquare camera picture taking replacement
            throw new System.NotImplementedException();
            //byte[] pictureBytes = Pi.Camera.CaptureImageJpeg(1024, 768);
            //return File(pictureBytes, "image/jpeg");
        }
    }
}
