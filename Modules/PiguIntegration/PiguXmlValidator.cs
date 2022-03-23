using System;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Ikrito_Fulfillment_Platform.Modules.PiguIntegration
{
    internal static class PiguXmlValidator
    {
        static void Main(string[] args)
        {
            var path = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase)).LocalPath;
            XmlSchemaSet schema = new XmlSchemaSet();
            schema.Add("", path + "\\input.xsd");
            XmlReader rd = XmlReader.Create(path + "\\input.xml");
            XDocument doc = XDocument.Load(rd);
            doc.Validate(schema, ValidationEventHandler);
        }
        static void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            XmlSeverityType type = XmlSeverityType.Warning;
            if (Enum.TryParse<XmlSeverityType>("Error", out type))
            {
                if (type == XmlSeverityType.Error) throw new Exception(e.Message);
            }
        }
    }
}
