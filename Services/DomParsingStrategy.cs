using System.Xml;
using Lab2_DayOfFaculty.Models;

namespace Lab2_DayOfFaculty.Services;

public sealed class DomParsingStrategy : IXmlParsingStrategy
{
    public string Name => "DOM (XmlDocument)";

    public IReadOnlyList<EventRecord> Parse(string xmlPath, string recordElement)
    {
        var document = new XmlDocument { XmlResolver = null };
        document.Load(xmlPath);
        var result = new List<EventRecord>();
        var nodes = document.GetElementsByTagName(recordElement);
        foreach (XmlNode node in nodes)
        {
            if (node is not XmlElement element) continue;
            var record = new EventRecord { ElementName = element.Name };
            foreach (XmlAttribute attribute in element.Attributes)
                record.Attributes[attribute.Name] = attribute.Value;
            foreach (XmlNode child in element.ChildNodes)
            {
                if (child is XmlElement childElement)
                    record.Fields[childElement.Name] = childElement.InnerText.Trim();
            }
            result.Add(record);
        }
        return result;
    }
}
