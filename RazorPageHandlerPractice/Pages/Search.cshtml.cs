using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using RazorPageHandlerPractice.Services;

namespace RazorPageHandlerPractice.Pages
{

    public class SearchModel : PageModel
    {
        private readonly SearchService _searchService;

        public SearchModel(SearchService searchService)
        {
            _searchService = searchService;
        }

        // Bind property
        public BindingModel Input { get; set; }
        public List<Game> Results { get; set; }

        // Returns nothing
        public void OnGet()
        {
        }

        public IActionResult OnPost(int max)
        {
            if (ModelState.IsValid)
            {
                Results = _searchService.Search(Input.SearchTerm);

                // Returns PageResult (Derived from ActionResult)
                return Page();
            }

            // Returns RedirectToPageResult (Derived from ActionResult and implements IActionResult)
            return RedirectToPage("./Index");
        }

        public class BindingModel
        {
            [Required]
            public string SearchTerm { get; set; }
        }
    }
}
