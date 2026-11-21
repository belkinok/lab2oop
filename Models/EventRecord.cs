namespace Lab2_DayOfFaculty.Models;

public sealed class EventRecord
{
    public string ElementName { get; set; } = "Event";
    public Dictionary<string, string> Attributes { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> Fields { get; } = new(StringComparer.OrdinalIgnoreCase);

    public string GetValue(string name)
    {
        if (Attributes.TryGetValue(name, out var attribute)) return attribute;
        return Fields.TryGetValue(name, out var field) ? field : string.Empty;
    }

    public string DisplayText =>
        string.Join(Environment.NewLine,
            Attributes.Select(x => $"@{x.Key}: {x.Value}")
                .Concat(Fields.Select(x => $"{x.Key}: {x.Value}")));
}
