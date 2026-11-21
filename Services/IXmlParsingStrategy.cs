using Lab2_DayOfFaculty.Models;

namespace Lab2_DayOfFaculty.Services;

public interface IXmlParsingStrategy
{
    string Name { get; }
    IReadOnlyList<EventRecord> Parse(string xmlPath, string recordElement);
}
