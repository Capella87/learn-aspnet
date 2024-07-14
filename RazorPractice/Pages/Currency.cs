using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPractice.Pages
{
    // This code is should be located in Controllers, Not here, "Page"
    // However, the author mentioned for comparison, I put this here in temporary.
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

    // Fit for here
    public class CurrencyModel : PageModel
    {
        private readonly LinkGenerator _link;
        // Access using Dependency Injection
        public CurrencyModel(LinkGenerator linkGenerator)
        {
            _link = linkGenerator;
        }

        public void OnGet()
        {
            var url1 = Url.Page("Currency/View", new { id = 5 });
            // Equivalent to Url.Page; Generates a relative URL
            var url3 = _link.GetPathByPage(HttpContext, "/Currency/View", values: new { id = 5 });
            var url2 = _link.GetPathByPage("/Currency/View", values: new { id = 5 });

            // Absolute URL
            var url4 = _link.GetUriByPage(
                page: "/Currency/View",
                handler: null,
                values: new { id = 5 },
                scheme: "https",
                host: new HostString("example.com"));
        }
    }
}
