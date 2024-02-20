using System;

namespace Colosoft.Reflection
{
    [Serializable]
    [System.Xml.Serialization.XmlSchemaProvider("GetMySchema")]
    public sealed class AssemblyPart : System.Runtime.Serialization.ISerializable, Serialization.ICompactSerializable, System.Xml.Serialization.IXmlSerializable
    {
        [NonSerialized]
        private System.Reflection.Assembly assembly;

        public static System.Xml.XmlQualifiedName GetMySchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            ReflectionNamespace.ResolveReflectionSchema(xs);
            return new System.Xml.XmlQualifiedName("AssemblyPart", ReflectionNamespace.Data);
        }

        public string Source { get; set; }

        [System.Xml.Serialization.XmlIgnore]
        internal System.Reflection.Assembly Assembly
        {
            get { return this.assembly; }
            set { this.assembly = value; }
        }

        public AssemblyPart()
        {
        }

        public AssemblyPart(string source)
        {
            this.Source = source;
        }

        internal AssemblyPart(string source, System.Reflection.Assembly assembly)
            : this(source)
        {
            this.assembly = assembly;
        }

        private AssemblyPart(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            this.Source = info.GetString("Source");
        }

        [System.Security.SecuritySafeCritical]
        public System.Reflection.Assembly Load(AppDomain appDomain, System.IO.Stream assemblyStream, int length)
        {
            if (assemblyStream is null)
            {
                throw new ArgumentNullException(nameof(assemblyStream));
            }

            byte[] buffer = new byte[length];
            int offset = 0;
            while (length > 0)
            {
                int num3 = assemblyStream.Read(buffer, offset, length);
                if (num3 == 0)
                {
                    break;
                }

                offset += num3;
                length -= num3;
            }

            if (appDomain != null)
            {
                return appDomain.Load(buffer);
            }
            else
            {
                return System.Reflection.Assembly.Load(buffer);
            }
        }

        [System.Security.SecuritySafeCritical]
        public System.Reflection.Assembly Load(AppDomain appDomain, byte[] raw)
        {
            if (raw is null)
            {
                throw new ArgumentNullException(nameof(raw));
            }

            if (appDomain != null)
            {
                return appDomain.Load(raw);
            }
            else
            {
                return System.Reflection.Assembly.Load(raw);
            }
        }

        [System.Security.SecuritySafeCritical]
        public System.Reflection.Assembly Load(AppDomain appDomain, string assemblyCodeBase)
        {
            if (assemblyCodeBase is null)
            {
                throw new ArgumentNullException(nameof(assemblyCodeBase));
            }

            var name = System.Reflection.AssemblyName.GetAssemblyName(assemblyCodeBase);

            if (appDomain != null)
            {
                return appDomain.Load(name);
            }
            else
            {
                return System.Reflection.Assembly.Load(name);
            }
        }

        public override string ToString()
        {
            return this.Source;
        }

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.LinkDemand)]
        public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue("Source", this.Source);
        }

        public void Deserialize(Serialization.IO.CompactReader reader)
        {
            if (reader is null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            this.Source = reader.ReadString();
        }

        public void Serialize(Serialization.IO.CompactWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.Write(this.Source);
        }

        System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema()
        {
            throw new NotImplementedException();
        }

        void System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader reader)
        {
            if (reader.MoveToAttribute("Source"))
            {
                this.Source = reader.ReadContentAsString();
            }

            reader.MoveToElement();
            reader.Skip();
        }

        void System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteAttributeString("Source", this.Source);
        }
    }
}
