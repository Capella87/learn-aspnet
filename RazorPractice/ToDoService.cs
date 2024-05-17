namespace ToDoList;

// Model
public record class ToDoListModel(string Category, string Title);

// "Brain"
public class ToDoService
{
    private static readonly List<ToDoListModel> _items = new List<ToDoListModel>
    {
        new ToDoListModel("Work", "Project 1"),
        new ToDoListModel("Work", "Project 2"),
        new ToDoListModel("Game", "SimCity 4"),
        new ToDoListModel("Game", "Chess"),
    };

    public List<ToDoListModel> GetItemsForCategory(string category)
    {
        return _items.Where(x => x.Category == category).ToList();
    }
}
