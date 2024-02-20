using Colosoft.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colosoft.Reflection
{
    [Serializable]
    [System.Xml.Serialization.XmlSchemaProvider("GetMySchema")]
    public sealed class TypeName
        : System.Runtime.Serialization.ISerializable, Serialization.ICompactSerializable, System.Xml.Serialization.IXmlSerializable
    {
        public TypeName()
        {
        }

        public TypeName(string assemblyQualifiedName)
        {
            new Parser().Parse(assemblyQualifiedName, this);
        }

        private TypeName(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            var name = info.GetString("AssemblyQualifiedName");
            new Parser().Parse(name, this);
        }

        public string Name { get; private set; }

        public IList<string> Namespace { get; private set; }

        public IList<string> Nesting { get; private set; }

        public IList<TypeName> TypeArguments { get; private set; }

        public System.Reflection.AssemblyName AssemblyName { get; private set; }

        public bool IsPointer { get; private set; }

        public bool IsByRef { get; private set; }

        public string AssemblyQualifiedName
        {
            get
            {
                if (this.AssemblyName == null)
                {
                    return this.FullName;
                }
                else
                {
                    return $"{this.FullName}, {this.AssemblyName.FullName}";
                }
            }
        }

        public string FullName
        {
            get
            {
                var args = this.TypeArguments
                    .Select(t => t.AssemblyQualifiedName)
                    .DelimitWith(",", format: "[{0}]", prefix: $"`{this.TypeArguments.Count}[", suffix: "]");

                return string.Concat(
                    this.Namespace.DelimitWith(string.Empty, "{0}."),
                    this.Nesting.DelimitWith(string.Empty, "{0}+"),
                    this.Name,
                    args,
                    this.Suffix);
            }
        }

        private string Suffix
        {
            get
            {
                var result = new StringBuilder();
                if (this.IsPointer)
                {
                    result.Append('*');
                }

                if (this.IsByRef)
                {
                    result.Append('&');
                }

                return result.ToString();
            }
        }

        public static TypeName Get<T>()
        {
            return new TypeName(typeof(T).AssemblyQualifiedName);
        }

        public static TypeName Get(Type type)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return new TypeName(type.AssemblyQualifiedName);
        }

        public override string ToString()
        {
            var args = this.TypeArguments
                .Select(r => r.ToString())
                .DelimitWith(", ", null, "<", ">");

            return $"{this.Name}{args}{this.Suffix}";
        }

        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.LinkDemand)]
        public void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue("AssemblyQualifiedName", this.AssemblyQualifiedName);
        }

        public void Deserialize(Serialization.IO.CompactReader reader)
        {
            if (reader is null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var name = reader.ReadString();
            new Parser().Parse(name, this);
        }

        public void Serialize(Serialization.IO.CompactWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.Write(this.AssemblyQualifiedName);
        }

#pragma warning disable CA1001 // Types that own disposable fields should be disposable
        private sealed class Parser
#pragma warning restore CA1001 // Types that own disposable fields should be disposable
        {
            private System.IO.TextReader reader;
            private char? nextChar;

            private char Read()
            {
                var result = this.nextChar ?? '\0';

                this.ReadNext();
                if (this.nextChar == '\\')
                {
                    this.ReadNext();
                }

                return result;
            }

            private void Read(char expected)
            {
                if (this.Read() != expected)
                {
                    throw new FormatException($"Expected '{expected}'.");
                }
            }

            private void ReadNext()
            {
                var next = this.reader.Read();
                this.nextChar = next >= 0 ? (char)next : (char?)null;
            }

            private void IgnoreSpaces()
            {
                do
                {
                    this.Read();
                }
                while (this.nextChar == ' ');
            }

            private void ReadUntil(StringBuilder buffer, string delimiters)
            {
                while (this.nextChar != null && delimiters.IndexOf(this.nextChar.Value) < 0)
                {
                    buffer.Append(this.Read());
                }
            }

            private string ReadUntil(string delimiters)
            {
                var buffer = new StringBuilder();
                this.ReadUntil(buffer, delimiters);
                return buffer.ToString();
            }

            public void Parse(string assemblyQualifiedName, TypeName typeName)
            {
                this.reader = new System.IO.StringReader(assemblyQualifiedName);
                this.Read();
                this.TypeSpec(typeName);

                if (this.nextChar != null)
                {
                    throw new FormatException("There are remaining unparsed characters.");
                }
            }

            private void TypeSpec(TypeName typeName)
            {
                var @namespace = new List<string>();
                while (true)
                {
                    @namespace.Add(this.ReadUntil(".,+`[&*"));
                    if (this.nextChar != '.')
                    {
                        break;
                    }

                    this.Read('.');
                }

                var typeNameList = @namespace;
                var nesting = new List<string>();
                if (this.nextChar == '+')
                {
                    while (true)
                    {
                        nesting.Add(this.ReadUntil(",+`&*"));
                        if (this.nextChar != '+')
                        {
                            break;
                        }

                        this.Read('+');
                    }

                    typeNameList = nesting;
                }

                typeName.Name = typeNameList[typeNameList.Count - 1];
                typeNameList.RemoveAt(typeNameList.Count - 1);

                typeName.Namespace = new System.Collections.ObjectModel.ReadOnlyCollection<string>(@namespace);
                typeName.Nesting = new System.Collections.ObjectModel.ReadOnlyCollection<string>(nesting);

                while (this.nextChar == '[')
                {
#pragma warning disable S1643 // Strings should not be concatenated using '+' in a loop
                    typeName.Name += $"{this.ReadUntil("]")}]";
#pragma warning restore S1643 // Strings should not be concatenated using '+' in a loop
                    this.Read(']');
                }

                if (this.nextChar == '`')
                {
                    this.Read('`');
                    var argCount = int.Parse(this.ReadUntil("["), System.Globalization.CultureInfo.InvariantCulture);

                    var typeArgs = new TypeName[argCount];
                    this.Read('[');

                    for (var i = 0; i < argCount; ++i)
                    {
                        this.Read('[');

                        typeArgs[i] = new TypeName();
                        this.TypeSpec(typeArgs[i]);

                        if (i < argCount - 1)
                        {
                            this.ReadUntil("[");
                        }
                        else
                        {
                            this.Read(']');
                        }
                    }

                    typeName.TypeArguments = new System.Collections.ObjectModel.ReadOnlyCollection<TypeName>(typeArgs);

                    this.Read(']');
                }
                else
                {
                    typeName.TypeArguments = EmptyTypes;
                }

                if (this.nextChar == '*')
                {
                    this.Read('*');
                    typeName.IsPointer = true;
                }

                if (this.nextChar == '&')
                {
                    this.Read('&');
                    typeName.IsByRef = true;
                }

                if (this.nextChar == ',')
                {
                    this.IgnoreSpaces();
                    var assemblyName = this.ReadUntil("]");
                    typeName.AssemblyName = new System.Reflection.AssemblyName(assemblyName);
                }
            }

            private static readonly IList<TypeName> EmptyTypes = Array.Empty<TypeName>();
        }

        public static System.Xml.XmlQualifiedName GetMySchema(System.Xml.Schema.XmlSchemaSet xs)
        {
            ReflectionNamespace.ResolveReflectionSchema(xs);
            return new System.Xml.XmlQualifiedName("TypeName", ReflectionNamespace.Data);
        }

        System.Xml.Schema.XmlSchema System.Xml.Serialization.IXmlSerializable.GetSchema()
        {
            throw new NotImplementedException();
        }

        void System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader reader)
        {
            if (reader.MoveToAttribute("AssemblyQualifiedName"))
            {
                new Parser().Parse(reader.ReadContentAsString(), this);
            }

            reader.MoveToElement();
            reader.Skip();
        }

        void System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteAttributeString("AssemblyQualifiedName", this.AssemblyQualifiedName);
        }
    }
}
