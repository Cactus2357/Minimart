using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MinimartApi.Services;

namespace MinimartApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IFileService fileService;

        public TestController(IFileService fileService)
        {
            this.fileService = fileService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");
            var folder = "test/child1";
            var fileUrl = await fileService.UploadAsync(file, folder);
            return Ok(new { FileUrl = fileUrl });
        }
    }
}
