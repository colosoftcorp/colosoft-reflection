using System;
using System.Collections.Generic;

namespace Colosoft.Reflection.Composition
{
    [Serializable]
    public class Export : IExport2, Serialization.ICompactSerializable, System.Runtime.Serialization.ISerializable
    {
        public TypeName Type { get; set; }

        public string ContractName { get; set; }

        public TypeName ContractType { get; set; }

        public bool ImportingConstructor { get; set; }

        public CreationPolicy CreationPolicy { get; set; }

        public bool UseDispatcher { get; set; }

#pragma warning disable CA2227 // Collection properties should be read only
        public IDictionary<string, object> Metadata { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only

        public string UIContext { get; set; }

        public Export()
        {
        }

        public Export(IExport export)
        {
            if (export is null)
            {
                throw new ArgumentNullException(nameof(export));
            }

            this.Type = new TypeName(export.Type.AssemblyQualifiedName);
            this.ContractName = export.ContractName;
            this.ContractType = new TypeName(export.ContractType.AssemblyQualifiedName);
            this.ImportingConstructor = export.ImportingConstructor;
            this.UseDispatcher = export.UseDispatcher;
            this.CreationPolicy = export.CreationPolicy;

            if (export is IExport2 export2)
            {
                this.UIContext = export2.UIContext;
            }

            if (export.Metadata != null)
            {
                this.Metadata = new Dictionary<string, object>();
                foreach (var i in export.Metadata)
                {
                    this.Metadata.Add(i);
                }
            }
        }

        protected Export(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            this.Type = (TypeName)info.GetValue("Type", typeof(TypeName));
            this.ContractName = info.GetString("ContractName");
            this.ContractType = (TypeName)info.GetValue("ContractType", typeof(TypeName));
            this.ImportingConstructor = info.GetBoolean("ImportingConstructor");
            this.CreationPolicy = (CreationPolicy)info.GetInt32("CreationPolicy");
            this.UseDispatcher = info.GetBoolean("UseDispatcher");
            this.UIContext = info.GetString("UIContext");
            this.Metadata = (Dictionary<string, object>)info.GetValue("Metadata", typeof(Dictionary<string, object>));
        }

        public virtual void Deserialize(Serialization.IO.CompactReader reader)
        {
            if (reader is null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (reader.ReadBoolean())
            {
                this.Type = new TypeName();
                this.Type.Deserialize(reader);
            }

            this.ContractName = reader.ReadString();

            if (reader.ReadBoolean())
            {
                this.ContractType = new TypeName();
                this.ContractType.Deserialize(reader);
            }

            this.ImportingConstructor = reader.ReadBoolean();
            this.CreationPolicy = (CreationPolicy)reader.ReadInt32();
            this.UseDispatcher = reader.ReadBoolean();
            this.UIContext = reader.ReadString();

            var count = reader.ReadInt32();
            this.Metadata = new Dictionary<string, object>(count);

            for (var i = 0; i < count; i++)
            {
                var key = reader.ReadString();
                var value = reader.ReadObject();
                this.Metadata.Add(key, value);
            }
        }

        public virtual void Serialize(Serialization.IO.CompactWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.Write(this.Type != null);
            if (this.Type != null)
            {
                this.Type.Serialize(writer);
            }

            writer.Write(this.ContractName);

            writer.Write(this.ContractType != null);
            if (this.Type != null)
            {
                this.ContractType.Serialize(writer);
            }

            writer.Write(this.ImportingConstructor);
            writer.Write((int)this.CreationPolicy);
            writer.Write(this.UseDispatcher);
            writer.Write(this.UIContext);

            writer.Write(this.Metadata != null ? this.Metadata.Count : 0);
            if (this.Metadata != null)
            {
                foreach (var item in this.Metadata)
                {
                    writer.Write(item.Key);
                    writer.WriteObject(item.Value);
                }
            }
        }

        public virtual void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue("Type", this.Type, typeof(TypeName));
            info.AddValue("ContractName", this.ContractName);
            info.AddValue("ContractType", this.ContractType, typeof(TypeName));
            info.AddValue("ImportingConstructor", this.ImportingConstructor);
            info.AddValue("CreationPolicy", (int)this.CreationPolicy);
            info.AddValue("UIContext", this.UIContext);
            info.AddValue("Metadata", this.Metadata is Dictionary<string, object> ? this.Metadata : new Dictionary<string, object>(this.Metadata), typeof(Dictionary<string, object>));
        }
    }
}
