using Microsoft.AspNetCore.Mvc;

namespace RazorPractice.Pages
{
    public class Currency : Controller
    {
        [HttpGet("currency/index")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
