using System.Xml.Linq;
using Lab2_DayOfFaculty.Models;

namespace Lab2_DayOfFaculty.Services;

public sealed class LinqParsingStrategy : IXmlParsingStrategy
{
    public string Name => "LINQ to XML (XDocument)";

    public IReadOnlyList<EventRecord> Parse(string xmlPath, string recordElement)
    {
        var document = XDocument.Load(xmlPath, LoadOptions.None);
        return document.Descendants(recordElement).Select(element =>
        {
            var record = new EventRecord { ElementName = element.Name.LocalName };
            foreach (var attribute in element.Attributes())
                record.Attributes[attribute.Name.LocalName] = attribute.Value;
            foreach (var child in element.Elements())
                record.Fields[child.Name.LocalName] = child.Value.Trim();
            return record;
        }).ToList();
    }
}
