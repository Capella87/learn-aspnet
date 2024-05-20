using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ToDoList
{
    // PageModel is NOT a model
    public class CategoryModel : PageModel
    {
        private readonly ToDoService _service;
        public List<ToDoListModel> Items { get; set; }

        public CategoryModel(ToDoService service)
        {
            _service = service;
        }

        // Controller (handler)
        public ActionResult OnGet(string category)
        {
            // Request data to model and retrieve processed data
            Items = _service.GetItemsForCategory(category);

            // Create a new page
            return Page();
        }
    }
}
