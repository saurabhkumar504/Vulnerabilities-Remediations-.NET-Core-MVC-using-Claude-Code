using Microsoft.AspNetCore.Mvc;
using OWASPDotNetLab.Services;
using System.Collections.Generic;

namespace OWASPDotNetLab.Controllers
{
    /// <summary>
    /// Product catalog endpoints, including the SQL injection demonstration.
    /// </summary>
    public class ProductController : Controller
    {
        private readonly ProductService _products;

        public ProductController(ProductService products)
        {
            _products = products;
        }

        public IActionResult Index()
        {
            var items = _products.GetAll();
            return View(items);
        }

        // VULNERABILITY:
        // CWE-89: SQL Injection
        // OWASP: A03:2021 - Injection
        // Description: The query string 'q' is concatenated into a raw SQL query
        //              executed through EF Core's FromSqlRaw. No parameterization.
        // Example Attack: GET /api/search?q=' OR 1=1 --
        [HttpGet("/api/search")]
        public IActionResult Search(string q)
        {
            var results = _products.Search(q ?? string.Empty);
            return View("Index", results);
        }

        // VULNERABILITY:
        // CWE-79: Improper Neutralization of Input During Web Page Generation
        // OWASP: A03:2021 - Injection (Reflected XSS)
        // Description: The 'name' query parameter is reflected verbatim into an
        //              HTML response with Content-Type: text/html. No encoding.
        // Example Attack: GET /api/greet?name=<script>alert(1)</script>
        [HttpGet("/api/greet")]
        public IActionResult Greet(string name)
        {
            return Content("<h1>Hello " + name + "</h1>", "text/html");
        }
    }
}