using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OWASPDotNetLab.Services;
using System.Linq;

namespace OWASPDotNetLab.Controllers
{
    /// <summary>
    /// User profile endpoints. Designed to demonstrate Broken Access Control and IDOR.
    /// </summary>
    public class UserController : Controller
    {
        private readonly UserService _users;

        public UserController(UserService users)
        {
            _users = users;
        }

        public IActionResult Index()
        {
            var all = _users.GetAll();
            return View(all);
        }

        // VULNERABILITY:
        // CWE-639: Authorization Bypass Through User-Controlled Key (IDOR)
        // OWASP: A01:2021 - Broken Access Control
        // Description: Any authenticated user can fetch any other user's profile
        //              simply by changing the {id} segment. The authenticated user
        //              is not compared against the requested id.
        // Example Attack: GET /api/profile/1, GET /api/profile/2, ...
        [HttpGet("/api/profile/{id}")]
        [Authorize]
        public IActionResult Profile(int id)
        {
            var user = _users.GetById(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        // VULNERABILITY:
        // CWE-639: Authorization Bypass Through User-Controlled Key (IDOR)
        // OWASP: A01:2021 - Broken Access Control
        // Description: No authorization at all. Anonymous users can enumerate the
        //              user table. Authenticated users can read any user's data.
        // Example Attack: GET /api/user/1 .. /api/user/9999
        [HttpGet("/api/user/{id}")]
        public IActionResult GetUser(int id)
        {
            var user = _users.GetById(id);
            if (user == null) return NotFound();
            return Ok(user);
        }
    }
}