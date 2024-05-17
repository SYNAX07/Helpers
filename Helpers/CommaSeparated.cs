namespace Helpers;

// Instead of,
// public string NamesCsv { get; set; }
//
// Do,
// public CommaSeparated<string> Names { get; set; }
public class CommaSeparated<T>
{
    private readonly List<T> _items;

    public CommaSeparated(IEnumerable<T> collection)
    {
        _items = collection.ToList();
    }

    public void Add(T element)
    {
        _items.Add(element);
    }

    public string Value => string.Join(",", _items);
}