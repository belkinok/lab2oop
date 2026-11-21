using System.Xml;
using System.Xml.Xsl;
using System.Xml.Linq;
using Lab2_DayOfFaculty.Models;

namespace Lab2_DayOfFaculty.Services;

public sealed class XmlDocumentService
{
    private readonly List<IXmlParsingStrategy> _strategies =
    [ new SaxParsingStrategy(), new DomParsingStrategy(), new LinqParsingStrategy() ];

    public IReadOnlyList<string> StrategyNames => _strategies.Select(x => x.Name).ToList();

    public IReadOnlyList<EventRecord> Parse(string path, string recordElement, string strategyName)
    {
        var strategy = _strategies.FirstOrDefault(x => x.Name == strategyName)
            ?? throw new InvalidOperationException("Не вибрано спосіб аналізу XML.");
        return strategy.Parse(path, recordElement);
    }

    public IReadOnlyList<string> GetAttributeNames(string path, string recordElement)
    {
        var document = XDocument.Load(path);
        return document.Descendants(recordElement).Attributes()
            .Select(x => x.Name.LocalName).Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x).ToList();
    }

    public IReadOnlyList<string> GetAttributeValues(string path, string recordElement, string attributeName)
    {
        var document = XDocument.Load(path);
        return document.Descendants(recordElement).Attributes(attributeName)
            .Select(x => x.Value).Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x).ToList();
    }

    public void SaveFilteredXml(IEnumerable<EventRecord> records, string path, string rootName, string recordName)
    {
        var items = records.ToList();
        var faculty = items.FirstOrDefault()?.GetValue("Faculty") ?? "Факультет комп’ютерних наук та кібернетики";
        var root = new XElement(rootName,
            new XAttribute("faculty", faculty),
            new XAttribute("year", DateTime.Now.Year),
            items.Select(record => new XElement(recordName,
                record.Attributes.Select(x => new XAttribute(x.Key, x.Value)),
                record.Fields.Select(x => new XElement(x.Key, x.Value)) )));
        new XDocument(new XDeclaration("1.0", "utf-8", "yes"), root)
            .Save(path, SaveOptions.None);
    }

    public void TransformToHtml(string xmlPath, string xslPath, string htmlPath)
    {
        var transform = new XslCompiledTransform();
        var settings = new XsltSettings(enableDocumentFunction: false, enableScript: false);
        using (var xslReader = XmlReader.Create(xslPath, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit }))
            transform.Load(xslReader, settings, new XmlUrlResolver());
        using var xmlReader = XmlReader.Create(xmlPath, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit });
        using var writer = XmlWriter.Create(htmlPath, transform.OutputSettings);
        transform.Transform(xmlReader, writer);
    }
}
