using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OWASPDotNetLab.Controllers
{
    /// <summary>
    /// Dashboard controller - landing page after login.
    /// </summary>
    public class DashboardController : Controller
    {
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
    }
}