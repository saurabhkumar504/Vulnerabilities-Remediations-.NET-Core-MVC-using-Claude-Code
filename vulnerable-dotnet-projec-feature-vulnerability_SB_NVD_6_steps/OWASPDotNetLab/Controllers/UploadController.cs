using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace OWASPDotNetLab.Controllers
{
    /// <summary>
    /// File upload endpoint - intentionally permissive so SAST scanners flag it
    /// as an unrestricted file upload vulnerability.
    /// </summary>
    public class UploadController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public UploadController(IWebHostEnvironment env)
        {
            _env = env;
        }

        public IActionResult Index()
        {
            return View();
        }

        // VULNERABILITY:
        // CWE-434: Unrestricted Upload of File with Dangerous Type
        // OWASP: A04:2021 - Insecure Design / A05 Security Misconfiguration
        // Description: Accepts any file type, any file name, any file size, and
        //              writes the uploaded content directly under wwwroot/uploads
        //              so it becomes publicly downloadable.
        // Example Attack: Upload a .aspx, .exe, or .php webshell. Then GET
        //              /uploads/webshell.aspx to execute it on the server.
        [HttpPost("/upload")]
        public IActionResult Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var uploads = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploads))
                Directory.CreateDirectory(uploads);

            var fileName = Path.GetFileName(file.FileName);
            var fullPath = Path.Combine(uploads, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            return Ok(new
            {
                fileName,
                size = file.Length,
                url  = "/uploads/" + fileName
            });
        }
    }
}