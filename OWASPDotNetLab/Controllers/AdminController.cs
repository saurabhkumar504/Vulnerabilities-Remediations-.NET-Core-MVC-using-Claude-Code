using Microsoft.AspNetCore.Mvc;
using OWASPDotNetLab.Data;
using OWASPDotNetLab.Models;
using System.Linq;

namespace OWASPDotNetLab.Controllers
{
    /// <summary>
    /// Admin endpoints. Intentionally expose sensitive configuration and
    /// accept arbitrary state-changing actions without authorization.
    /// </summary>
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public AdminController(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        // VULNERABILITY:
        // CWE-862: Missing Authorization
        // OWASP: A01:2021 - Broken Access Control
        // Description: Admin index has no [Authorize(Roles = "ADMIN")] attribute.
        //              Any anonymous or authenticated user can reach it.
        // Example Attack: GET /Admin/Index without being authenticated.
        public IActionResult Index()
        {
            var users = _db.Users.ToList();
            return View(users);
        }

        // VULNERABILITY:
        // CWE-200: Exposure of Sensitive Information to an Unauthorized Actor
        // OWASP: A05:2021 - Security Misconfiguration / Sensitive Data Exposure
        // Description: Returns the full IConfiguration tree including API keys,
        //              JWT signing secrets, and database connection strings.
        // Example Attack: GET /api/config dumps every secret in appsettings.json.
        [HttpGet("/api/config")]
        public IActionResult Config()
        {
            return Ok(_config.AsEnumerable());
        }

        // VULNERABILITY:
        // CWE-285: Improper Authorization
        // OWASP: A04:2021 - Insecure Design
        // Description: Money transfer trusts client-supplied fromUserId, toUserId,
        //              and amount. No ownership verification, no transaction log,
        //              no authentication required.
        // Example Attack: POST /api/transfer fromUserId=1&toUserId=999&amount=999999
        [HttpPost("/api/transfer")]
        public IActionResult Transfer(int fromUserId, int toUserId, decimal amount)
        {
            var from = _db.Users.FirstOrDefault(u => u.Id == fromUserId);
            var to   = _db.Users.FirstOrDefault(u => u.Id == toUserId);

            if (from == null || to == null)
                return BadRequest("Invalid accounts");

            // VULNERABILITY: CWE-778 / A09 - No audit log of the transfer
            from.Balance -= amount;
            to.Balance   += amount;

            _db.SaveChanges();

            return Ok(new
            {
                fromUser = from.Username, fromBalance = from.Balance,
                toUser   = to.Username,   toBalance   = to.Balance,
                amount
            });
        }
    }
}