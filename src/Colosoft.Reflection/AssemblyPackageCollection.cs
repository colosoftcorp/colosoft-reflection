using System;
using System.Collections.Generic;

namespace Colosoft.Reflection
{
    [Serializable]
    [System.Xml.Serialization.XmlSchemaProvider("GetMySchema")]
    public sealed class AssemblyPackageCollection :
        ICollection<AssemblyPackage>,
        System.Runtime.Serialization.ISerializable,
        Serialization.ICompactSerializable,
        System.Xml.Serialization.IXmlSerializable
    {
        private List<AssemblyPackage> innerList;

        public static System.Xml.XmlQualifiedName GetMySchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            ReflectionNamespace.ResolveReflectionSchema(xs);
            return new System.Xml.XmlQualifiedName("ArrayOfAssemblyPackage", ReflectionNamespace.Data);
        }

        public int Count
        {
            get { return this.innerList.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public AssemblyPackage this[int index]
        {
            get { return this.innerList[index]; }
            set { this.innerList[index] = value; }
        }

        public AssemblyPackageCollection(IEnumerable<AssemblyPackage> packages)
        {
            if (packages is null)
            {
                throw new ArgumentNullException(nameof(packages));
            }

            this.innerList = new List<AssemblyPackage>(packages);
        }

        public AssemblyPackageCollection()
        {
            this.innerList = new List<AssemblyPackage>();
        }

        private AssemblyPackageCollection(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            var count = info.GetInt32("Count");

            for (var i = 0; i < count; i++)
            {
                this.innerList.Add((AssemblyPackage)info.GetValue("i" + i, typeof(AssemblyPackage)));
            }
        }

        public void Add(AssemblyPackage item)
        {
            this.innerList.Add(item);
        }

        public void Clear()
        {
            this.innerList.Clear();
        }

        public bool Contains(AssemblyPackage item)
        {
            return this.innerList.Contains(item);
        }

        public void CopyTo(AssemblyPackage[] array, int arrayIndex)
        {
            this.innerList.CopyTo(array, arrayIndex);
        }

        public bool Remove(AssemblyPackage item)
        {
            return this.innerList.Remove(item);
        }

        public IEnumerator<AssemblyPackage> GetEnumerator()
        {
            return this.innerList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.innerList.GetEnumerator();
        }

        public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue("Count", this.innerList.Count);

            for (var i = 0; i < this.innerList.Count; i++)
            {
                info.AddValue("i" + i, this.innerList[i]);
            }
        }

        System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema()
        {
            throw new NotImplementedException();
        }

        void System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader reader)
        {
            reader.ReadStartElement();

            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                if (reader.LocalName == "AssemblyPackage")
                {
                    var part = new AssemblyPackage();
                    ((System.Xml.Serialization.IXmlSerializable)part).ReadXml(reader);
                    this.innerList.Add(part);
                }
                else
                {
                    reader.Skip();
                }
            }

            reader.ReadEndElement();
        }

        void System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
        {
            foreach (System.Xml.Serialization.IXmlSerializable i in this.innerList)
            {
                writer.WriteStartElement("AssemblyPackage");
                i.WriteXml(writer);
                writer.WriteEndElement();
            }
        }

        void Serialization.ICompactSerializable.Deserialize(Serialization.IO.CompactReader reader)
        {
            var count = reader.ReadInt32();
            this.innerList = new List<AssemblyPackage>(count);

            while (count-- > 0)
            {
                var package = new AssemblyPackage();
                package.Deserialize(reader);
                this.innerList.Add(package);
            }
        }

        void Serialization.ICompactSerializable.Serialize(Serialization.IO.CompactWriter writer)
        {
            writer.Write(this.Count);

            foreach (Serialization.ICompactSerializable i in this.innerList)
            {
                i.Serialize(writer);
            }
        }
    }
}
