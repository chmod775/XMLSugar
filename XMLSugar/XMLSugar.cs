using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace XMLSugar
{
    public class StringWriterUTF8 : StringWriter
    {
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }

    public class XMLSugar
    {
        public XMLSugar_Element rootElement = null;

        public XMLSugar() { }
        public XMLSugar(string xml)
        {
            this.FromXML(xml);
        }

        public static XMLSugar FromFile(string path)
        {
            var ret = new XMLSugar();
            if (!File.Exists(path)) return null;
            ret.LoadFromFile(path);
            return ret;
        }

        public static XMLSugar FromExample(XMLSugar_Element example)
        {
            var ret = new XMLSugar();
            ret.rootElement = new XMLSugar_Element(example);
            return ret;
        }

        public void LoadFromFile(string path)
        {
            using (StreamReader r = new StreamReader(path))
            {
                string xmlraw = r.ReadToEnd();
                this.FromXML(xmlraw);
            }
        }

        public void SaveToFile(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (StreamWriter w = new StreamWriter(path, false))
            {
                w.Write(this.ToXML());
            }
        }

        private bool ParseNext(XmlReader reader, XmlNodeType expectedType)
        {
            var ret = true;

            if (reader.NodeType != XmlNodeType.None)
            {
                if (reader.NodeType != expectedType)
                    ret = false;
                reader.Read();

                if (!ret)
                    throw new Exception("Expected node not matching");
            }

            return ret;
        }

        private XMLSugar_Element ParseReader(XmlReader reader, XMLSugar_Element parentElement = null)
        {
            XMLSugar_Element ret = new XMLSugar_Element(reader.Name);

            ret.Value = null;

            ret.Parent = parentElement;

            var isEmpty = reader.IsEmptyElement;

            // Read attributes
            if (reader.AttributeCount > 0)
            {
                while (reader.MoveToNextAttribute())
                {
                    var newAttr = new XMLSugar_ElementAttribute()
                    {
                        Name = reader.Name,
                        Value = reader.Value
                    };
                    ret.Attributes.Add(newAttr);
                }
                reader.MoveToElement();
            }

            this.ParseNext(reader, XmlNodeType.Element);

            if (isEmpty)
                return ret;

            while (reader.NodeType == XmlNodeType.Element)
            {
                var childrenElement = this.ParseReader(reader, ret);
                if (childrenElement == null) break;
                ret.Childrens.Add(childrenElement);
            }

            if (reader.NodeType == XmlNodeType.Text)
            {
                ret.Value = reader.Value;
                this.ParseNext(reader, XmlNodeType.Text);
            }

            this.ParseNext(reader, XmlNodeType.EndElement);

            return ret;
        }

        private void GenerateWriter(XmlWriter writer, XMLSugar_Element element)
        {
            var namespc = element.Attributes.Where(t => t.Name == "xmlns");
            if (namespc.Count() > 0)
                writer.WriteStartElement(element.Name, namespc.First().Value);
            else
                writer.WriteStartElement(element.Name);

            foreach (var attr in element.Attributes)
            {
                if (attr.Name != "xmlns")
                    writer.WriteAttributeString(attr.Name, attr.Value);
            }

            foreach (var item in element.Childrens)
            {
                this.GenerateWriter(writer, item);
            }

            if (element.Value != null)
                writer.WriteValue(element.Value);

            writer.WriteEndElement();
        }

        public void FromXML(string xml)
        {
            using (var sr = new StringReader(xml))
            {
                var _xmlReaderSettings = new XmlReaderSettings();
                _xmlReaderSettings.IgnoreComments = true;
                _xmlReaderSettings.IgnoreWhitespace = true;
                _xmlReaderSettings.IgnoreProcessingInstructions = true;

                var _xmlReader = XmlReader.Create(sr, _xmlReaderSettings);

                _xmlReader.Read();
                if (_xmlReader.NodeType == XmlNodeType.XmlDeclaration)
                    this.ParseNext(_xmlReader, XmlNodeType.XmlDeclaration);

                var root = this.ParseReader(_xmlReader);
                this.rootElement = root;
            }
        }

        public string ToXML()
        {
            using (var sw = new StringWriterUTF8())
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.Encoding = Encoding.UTF8;

                using (var writer = XmlWriter.Create(sw, settings))
                {
                    this.GenerateWriter(writer, this.rootElement);
                }
                return sw.ToString();
            }
        }

        public XMLSugar_Element Access(string path) => this.rootElement.Access(path);
        public List<XMLSugar_Element> AccessAll(string path) => this.rootElement.AccessAll(path);

        public List<XMLSugar_Element> Find(string selector, bool deep = true) => this.rootElement.Find(selector, deep);
        public XMLSugar_Element FindFirstOrNull(string selector, bool deep = false) => this.rootElement.FindFirstOrNull(selector, deep);
    }
}
