using Microsoft.AspNetCore.Mvc;
using OWASPDotNetLab.Data;
using OWASPDotNetLab.Models;
using System.Linq;

namespace OWASPDotNetLab.Controllers
{
    /// <summary>
    /// Comments board - intentionally vulnerable to stored XSS because the view
    /// renders the comment body with @Html.Raw.
    /// </summary>
    public class CommentController : Controller
    {
        private readonly AppDbContext _db;

        public CommentController(AppDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var comments = _db.Comments.ToList();
            return View(comments);
        }

        // VULNERABILITY:
        // CWE-79: Improper Neutralization of Input During Web Page Generation
        // OWASP: A03:2021 - Injection (Stored XSS)
        // Description: The Author and Body fields are stored verbatim and rendered
        //              with @Html.Raw in the view. No sanitization or encoding.
        // Example Attack: POST /Comments/Create with body=<script>alert(document.cookie)</script>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(string author, string body)
        {
            var c = new Comment
            {
                Author = author,
                Body   = body
            };
            _db.Comments.Add(c);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}