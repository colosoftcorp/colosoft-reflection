using System;
using System.Collections.Generic;

namespace Colosoft.Reflection
{
    [Serializable]
    [System.Xml.Serialization.XmlSchemaProvider("GetMySchema")]
    public sealed class AssemblyInfo : System.Xml.Serialization.IXmlSerializable
    {
        private string[] references;

        public static System.Xml.XmlQualifiedName GetMySchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            ReflectionNamespace.ResolveReflectionSchema(xs);
            return new System.Xml.XmlQualifiedName("AssemblyInfo", ReflectionNamespace.Data);
        }

        public string Name { get; set; }

        public DateTime LastWriteTime { get; set; }

#pragma warning disable CA1819 // Properties should not return arrays
        public string[] References
#pragma warning restore CA1819 // Properties should not return arrays
        {
            get { return this.references ?? Array.Empty<string>(); }
            set { this.references = value; }
        }

        public override string ToString()
        {
            return this.Name ?? "Empty";
        }

        System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema()
        {
            throw new NotImplementedException();
        }

        void System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader reader)
        {
            if (reader.MoveToAttribute("Name"))
            {
                this.Name = reader.ReadContentAsString();
            }

            if (reader.MoveToAttribute("LastWriteTime"))
            {
                this.LastWriteTime = reader.ReadContentAsDateTime();
            }

            reader.MoveToElement();

            if (!reader.IsEmptyElement)
            {
                reader.ReadStartElement();

                var refs = new List<string>();

                while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
                {
                    if (reader.LocalName == "Reference")
                    {
                        refs.Add(reader.ReadElementString());
                    }
                    else
                    {
                        reader.Skip();
                    }
                }

                this.References = refs.ToArray();

                reader.ReadEndElement();
            }
            else
            {
                reader.Skip();
            }
        }

        void System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteAttributeString("Name", this.Name);
            writer.WriteStartAttribute("LastWriteTime");
            writer.WriteValue(this.LastWriteTime);
            writer.WriteEndAttribute();

            foreach (var reference in this.references)
            {
                writer.WriteStartElement("Reference");
                writer.WriteValue(reference);
                writer.WriteEndElement();
            }
        }
    }
}
