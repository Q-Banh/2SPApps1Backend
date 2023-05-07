using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TwoSPAppsOneBackend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class GetMyAppRegVersionController : ControllerBase
    {
        private readonly ILogger<GetMyAppRegVersionController> _logger;

        public GetMyAppRegVersionController(ILogger<GetMyAppRegVersionController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "Ping")]
        public void Get()
        {
            
        }
    }
}