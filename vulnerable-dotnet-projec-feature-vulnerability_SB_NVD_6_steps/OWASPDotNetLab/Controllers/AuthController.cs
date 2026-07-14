using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OWASPDotNetLab.Models;
using OWASPDotNetLab.Services;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OWASPDotNetLab.Controllers
{
    /// <summary>
    /// Authentication-related endpoints. Login, logout, and registration live here.
    /// This controller is intentionally insecure.
    /// </summary>
    public class AuthController : Controller
    {
        private readonly UserService _users;

        public AuthController(UserService users)
        {
            _users = users;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // VULNERABILITY:
        // CWE-307: Improper Restriction of Excessive Authentication Attempts
        // OWASP: A07:2021 - Identification and Authentication Failures
        // Description: No lockout policy, no rate limiting, no MFA, no failed-login
        //              logging. Plaintext password comparison via UserService.
        // Example Attack: Brute-force the admin account online.
        // VULNERABILITY:
        // CWE-256: Plaintext Storage of a Password
        // OWASP: A02:2021 - Cryptographic Failures
        // Description: Password is sent over the wire and compared in plaintext.
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = _users.Authenticate(username, password);
            if (user == null)
            {
                // VULNERABILITY: CWE-778 / A09 - No log entry for failed login
                ModelState.AddModelError("", "Invalid credentials");
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // VULNERABILITY:
        // CWE-915: Improperly Controlled Modification of Dynamically-Determined Object Attributes
        // OWASP: A08:2021 - Software and Data Integrity Failures (Mass Assignment)
        // Description: The whole User model is bound from the request body and
        //              persisted verbatim. Role and Balance are accepted from the client.
        // Example Attack: POST /api/register with role=ADMIN and balance=999999.
        [HttpPost]
        public IActionResult Register([FromBody] User user)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // No allow-list, no validation, no sanitization. Stored as-is.
            var created = _users.Create(user);

            // VULNERABILITY: CWE-209 / A05 - Information Exposure Through an Error Message
            return Ok(new { id = created.Id, role = created.Role, balance = created.Balance });
        }
    }
}