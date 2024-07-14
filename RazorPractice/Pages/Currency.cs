using Microsoft.AspNetCore.Mvc;

namespace RazorPractice.Pages
{
    public class Currency : Controller
    {
        [HttpGet("currency/index")]
        public IActionResult Index()
        {
            var url = Url.Action("View", "Currency",
                new { code = "USD" });
            return Content($"The URL is {url}");
        }

        [HttpGet("currency/view/{code}")]
        public IActionResult View(string code)
        {
            return View();
        }
    }
}
