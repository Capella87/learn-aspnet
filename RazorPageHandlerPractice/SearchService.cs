namespace RazorPageHandlerPractice.Services;

public class SearchService
{
    public SearchService()
    {
    }

    private static readonly List<Game> _items = new List<Game>
    {
        new Game { Name = "Age of Empires II" },
        new Game { Name = "Age of Empires III" },
        new Game { Name = "Age of Empires IV" },
        new Game { Name = "SimCity 4" },
        new Game { Name = "The Sims 3" },
        new Game { Name = "The Sims 2" },
        new Game { Name = "Euro Truck Simulator 2" },
        new Game { Name = "Stardew Valley" },
        new Game { Name = "Fallout 4" },
        new Game { Name = "Fallout 3" },
        new Game { Name = "Fallout: New Vegas" },
        new Game { Name = "Halo: The Master Chief Collection" },
    };

    public List<Game> Search(string term) => _items
        .Where(x => x.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
        .ToList();
}
