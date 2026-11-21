using Lab2_DayOfFaculty.Models;

namespace Lab2_DayOfFaculty.Services;

public sealed class XmlSearchService
{
    public IReadOnlyList<EventRecord> Search(
        IEnumerable<EventRecord> records, string? attributeName, string? attributeValue, string? keyword)
    {
        IEnumerable<EventRecord> query = records;
        if (!string.IsNullOrWhiteSpace(attributeName) && !string.IsNullOrWhiteSpace(attributeValue))
            query = query.Where(record => record.Attributes.TryGetValue(attributeName, out var value) &&
                value.Contains(attributeValue, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(record => record.DisplayText.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        return query.ToList();
    }
}
