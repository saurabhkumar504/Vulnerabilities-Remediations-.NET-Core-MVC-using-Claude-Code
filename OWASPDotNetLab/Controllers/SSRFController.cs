using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;

namespace OWASPDotNetLab.Controllers
{
    /// <summary>
    /// Server-side request forgery demonstration endpoint.
    /// </summary>
    public class SSRFController : Controller
    {
        private readonly IHttpClientFactory _http;

        public SSRFController(IHttpClientFactory http)
        {
            _http = http;
        }

        public IActionResult Index()
        {
            return View();
        }

        // VULNERABILITY:
        // CWE-918: Server-Side Request Forgery (SSRF)
        // OWASP: A10:2021 - Server-Side Request Forgery
        // Description: The caller controls the entire URL passed to HttpClient.
        //              There is no allow-list, scheme check, or DNS rebinding
        //              mitigation. Internal hosts and cloud metadata are reachable.
        // Example Attack: GET /api/fetch?url=http://localhost:5000/api/config
        [HttpGet("/api/fetch")]
        public async Task<IActionResult> Fetch(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return BadRequest("url is required");

            var client = _http.CreateClient();
            var result = await client.GetStringAsync(url);
            return Content(result, "text/plain");
        }
    }
}