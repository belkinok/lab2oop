using System.Xml;
using Lab2_DayOfFaculty.Models;

namespace Lab2_DayOfFaculty.Services;

// Потоковий послідовний розбір XML за допомогою XmlReader (SAX-підхід).
public sealed class SaxParsingStrategy : IXmlParsingStrategy
{
    public string Name => "SAX (XmlReader)";

    public IReadOnlyList<EventRecord> Parse(string xmlPath, string recordElement)
    {
        var records = new List<EventRecord>();
        using var reader = XmlReader.Create(xmlPath, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit });
        EventRecord? current = null;
        string? currentField = null;
        int recordDepth = -1;
        int fieldDepth = -1;

        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                if (current is null && reader.Name.Equals(recordElement, StringComparison.OrdinalIgnoreCase))
                {
                    current = new EventRecord { ElementName = reader.Name };
                    recordDepth = reader.Depth;
                    if (reader.HasAttributes)
                    {
                        while (reader.MoveToNextAttribute()) current.Attributes[reader.Name] = reader.Value;
                        reader.MoveToElement();
                    }
                    if (reader.IsEmptyElement)
                    {
                        records.Add(current);
                        current = null;
                        recordDepth = -1;
                    }
                }
                else if (current is not null && reader.Depth == recordDepth + 1)
                {
                    currentField = reader.Name;
                    fieldDepth = reader.Depth;
                    if (reader.IsEmptyElement)
                    {
                        current.Fields[currentField] = string.Empty;
                        currentField = null;
                        fieldDepth = -1;
                    }
                }
            }
            else if (current is not null && currentField is not null &&
                     reader.Depth == fieldDepth &&
                     (reader.NodeType == XmlNodeType.Text || reader.NodeType == XmlNodeType.CDATA))
            {
                current.Fields[currentField] = (current.Fields.TryGetValue(currentField, out var old) ? old : string.Empty) + reader.Value;
            }
            else if (reader.NodeType == XmlNodeType.EndElement)
            {
                if (current is not null && reader.Depth == fieldDepth)
                {
                    currentField = null;
                    fieldDepth = -1;
                }
                if (current is not null && reader.Depth == recordDepth)
                {
                    records.Add(current);
                    current = null;
                    recordDepth = -1;
                }
            }
        }
        return records;
    }
}
