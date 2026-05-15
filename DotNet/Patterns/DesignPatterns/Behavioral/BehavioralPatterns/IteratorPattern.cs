/*
 * Iterator
 * Description: Exposes traversal logic without leaking the collection's internal representation.
 * Usage frequency: Very common; built into .NET through IEnumerable and used broadly
 * for collections, query results, streams, and paged data.
 */

internal sealed class IteratorPattern
{
    public string Description => "Traverse backlog cards by priority without exposing storage details.";

    public string UsageFrequency => "Very common";

    public void Run()
    {
        ConsoleSection.Print("Iterator");

        var backlog = new SprintBacklog();
        backlog.Add(new BacklogCard("Protect admin endpoint", 1));
        backlog.Add(new BacklogCard("Improve cache invalidation", 2));
        backlog.Add(new BacklogCard("Refresh README diagrams", 3));

        foreach (var card in backlog.GetByPriority(maxPriority: 2))
        {
            Console.WriteLine($"{card.Title} (priority {card.Priority})");
        }
    }
}

internal sealed record BacklogCard(string Title, int Priority);

internal sealed class SprintBacklog
{
    private readonly List<BacklogCard> cards = new();

    public void Add(BacklogCard card) => cards.Add(card);

    public IEnumerable<BacklogCard> GetByPriority(int maxPriority)
    {
        foreach (var card in cards.Where(card => card.Priority <= maxPriority))
        {
            yield return card;
        }
    }
}
