using System;
using System.Collections.Generic;
using System.Linq;

namespace Colosoft.Reflection
{
    [Serializable]
    [System.Xml.Serialization.XmlSchemaProvider("GetMySchema")]
    public class AssemblyPackage :
        System.Runtime.Serialization.ISerializable,
        Serialization.ICompactSerializable,
        System.Xml.Serialization.IXmlSerializable,
        IDisposable,
        IAssemblyPackage
    {
        private List<AssemblyPart> items = new List<AssemblyPart>();

        [NonSerialized]
        private IAssemblyPackageResult result;

        public static System.Xml.XmlQualifiedName GetMySchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            ReflectionNamespace.ResolveReflectionSchema(xs);
            return new System.Xml.XmlQualifiedName("AssemblyPackage", ReflectionNamespace.Data);
        }

        public Guid Uid { get; set; } = Guid.NewGuid();

        public DateTime CreateTime { get; set; } = DateTime.Now;

        public int Count
        {
            get { return this.items.Count; }
        }

        public AssemblyPart this[int index]
        {
            get { return this.items[index]; }
        }

        public IAssemblyPackageResult Result
        {
            get { return this.result; }
            set { this.result = value; }
        }

        public AssemblyPackage()
        {
        }

        public AssemblyPackage(IEnumerable<AssemblyPart> assemblies)
        {
            if (assemblies is null)
            {
                throw new ArgumentNullException(nameof(assemblies));
            }

            foreach (var i in assemblies)
            {
                if (!this.items.Contains(i, AssemblyPartEqualityComparer.Instance))
                {
                    this.items.Add(i);
                }
            }
        }

        protected AssemblyPackage(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            this.Uid = (Guid)info.GetValue("Uid", typeof(Guid));
            this.CreateTime = info.GetDateTime("CreateTime");

            var count = info.GetInt32("Count");

            for (var i = 0; i < count; i++)
            {
                this.items.Add((AssemblyPart)info.GetValue("i" + i, typeof(AssemblyPart)));
            }
        }

        public void Add(AssemblyPart name)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (!this.items.Contains(name, AssemblyPartEqualityComparer.Instance))
            {
                this.items.Add(name);
            }
        }

        public bool Remove(AssemblyPart name)
        {
            return this.items.Remove(name);
        }

        public System.Reflection.Assembly GetAssembly(AssemblyPart name)
        {
            if (name != null && name.Assembly != null)
            {
                return name.Assembly;
            }

            if (this.Result == null)
            {
                throw new InvalidOperationException("Result not loaded");
            }

            var assembly = this.Result.GetAssembly(name);

            return assembly;
        }

        public System.IO.Stream GetAssemblyStream(AssemblyPart name)
        {
            if (name != null && name.Assembly != null)
            {
                var fileName = name.Assembly.Location;

                if (System.IO.File.Exists(fileName))
                {
                    return System.IO.File.OpenRead(fileName);
                }
            }

            if (this.Result == null)
            {
                throw new InvalidOperationException("Result not loaded");
            }

            return this.Result.GetAssemblyStream(name);
        }

        public System.Reflection.Assembly LoadAssemblyGuarded(AssemblyPart name, out Exception exception)
        {
            if (name != null && name.Assembly != null)
            {
                exception = null;
                return name.Assembly;
            }

            if (this.Result == null)
            {
                throw new InvalidOperationException("Result not loaded");
            }

            var assembly = this.Result.LoadAssemblyGuarded(name, out var ex2);

            exception = ex2;
            return assembly;
        }

        public bool ExtractPackageFiles(string outputDirectory, bool canOverride)
        {
            if (this.Result == null)
            {
                return false;
            }

            this.Result.ExtractPackageFiles(outputDirectory, canOverride);

            return true;
        }

        public bool Contains(AssemblyPart assemblyPart)
        {
            return this.items.Contains(assemblyPart, AssemblyPartEqualityComparer.Instance);
        }

        public IEnumerator<AssemblyPart> GetEnumerator()
        {
            return this.items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.items.GetEnumerator();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.result != null)
            {
                this.result.Dispose();
            }
        }

        public virtual void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue("Uid", this.Uid);
            info.AddValue("CreateTime", this.CreateTime);
            info.AddValue("Count", this.items.Count);

            for (var i = 0; i < this.items.Count; i++)
            {
                info.AddValue("i" + i, this.items[i]);
            }
        }

        System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema()
        {
            throw new NotImplementedException();
        }

#pragma warning disable CA1033 // Interface methods should be callable by child types
        void System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader reader)
#pragma warning restore CA1033 // Interface methods should be callable by child types
        {
            if (reader is null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (reader.MoveToAttribute("Uid"))
            {
                this.Uid = Guid.Parse(reader.ReadContentAsString());
            }

            if (reader.MoveToAttribute("CreateTime"))
            {
                this.CreateTime = reader.ReadContentAsDateTime();
            }

            reader.MoveToElement();
            reader.ReadStartElement();

            reader.ReadStartElement("AssemblyParts");
            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                if (reader.LocalName == "AssemblyPart")
                {
                    AssemblyPart part = new AssemblyPart();
                    ((System.Xml.Serialization.IXmlSerializable)part).ReadXml(reader);
                    this.items.Add(part);
                }
                else
                {
                    reader.Skip();
                }
            }

            reader.ReadEndElement();
        }

#pragma warning disable CA1033 // Interface methods should be callable by child types
        void System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
#pragma warning restore CA1033 // Interface methods should be callable by child types
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteAttributeString("Uid", this.Uid.ToString());
            writer.WriteStartAttribute("CreateTime");
            writer.WriteValue(this.CreateTime);
            writer.WriteEndAttribute();
            writer.WriteStartElement("AssemblyParts");

            foreach (System.Xml.Serialization.IXmlSerializable i in this.items)
            {
                writer.WriteStartElement("AssemblyPart");
                i.WriteXml(writer);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        public void Deserialize(Serialization.IO.CompactReader reader)
        {
            if (reader is null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            this.Uid = reader.ReadGuid();
            this.CreateTime = reader.ReadDateTime();

            var count = reader.ReadInt32();

            while (count-- > 0)
            {
                var part = new AssemblyPart();
                part.Deserialize(reader);
                this.items.Add(part);
            }
        }

        public void Serialize(Serialization.IO.CompactWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.Write(this.Uid);
            writer.Write(this.CreateTime);
            writer.Write(this.items.Count);

            foreach (var i in this.items)
            {
                i.Serialize(writer);
            }
        }
    }
}
