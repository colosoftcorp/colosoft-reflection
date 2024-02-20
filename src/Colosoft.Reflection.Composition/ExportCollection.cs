using System;
using System.Collections.Generic;

namespace Colosoft.Reflection.Composition
{
    [Serializable]
    public class ExportCollection : ICollection<IExport>, Serialization.ICompactSerializable, System.Runtime.Serialization.ISerializable
    {
        [NonSerialized]
        private readonly List<IExport> exports = new List<IExport>();

        public bool IsReadOnly
        {
            get { return false; }
        }

        public int Count
        {
            get { return this.exports.Count; }
        }

        public IExport this[int index]
        {
            get { return this.exports[index]; }
            set { this.exports[index] = value; }
        }

        public ExportCollection()
        {
        }

        public ExportCollection(IEnumerable<IExport> items)
        {
            this.exports.AddRange(items);
        }

        protected ExportCollection(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            var count = info.GetInt32("Count");

            for (var i = 0; i < count; i++)
            {
                this.exports.Add((IExport)info.GetValue(i.ToString(System.Globalization.CultureInfo.InvariantCulture), typeof(Export)));
            }
        }

        public void Add(IExport item)
        {
            this.exports.Add(item);
        }

        public void Clear()
        {
            this.exports.Clear();
        }

        public bool Contains(IExport item)
        {
            return this.exports.Contains(item);
        }

        public void CopyTo(IExport[] array, int arrayIndex)
        {
            this.exports.CopyTo(array, arrayIndex);
        }

        public bool Remove(IExport item)
        {
            return this.exports.Remove(item);
        }

        public void RemoveAt(int index)
        {
            this.exports.RemoveAt(index);
        }

        public void Deserialize(Serialization.IO.CompactReader reader)
        {
            if (reader is null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            this.exports.Clear();
            var count = reader.ReadInt32();

            for (var i = 0; i < count; i++)
            {
                var export = new Export();
                ((Serialization.ICompactSerializable)export).Deserialize(reader);
                this.exports.Add(export);
            }
        }

        public void Serialize(Serialization.IO.CompactWriter writer)
        {
            if (writer is null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.Write(this.Count);

            foreach (var item in this.exports)
            {
                if (!(item is Serialization.ICompactSerializable))
                {
                    new Export(item).Serialize(writer);
                }
                else
                {
                    ((Serialization.ICompactSerializable)item).Serialize(writer);
                }
            }
        }

        public virtual void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue("Count", this.exports.Count);

            for (var i = 0; i < this.exports.Count; i++)
            {
                info.AddValue(i.ToString(System.Globalization.CultureInfo.InvariantCulture), this.exports[i] is Export ? this.exports[i] : new Export(this.exports[i]), typeof(Export));
            }
        }

        public IEnumerator<IExport> GetEnumerator()
        {
            return this.exports.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.exports.GetEnumerator();
        }
    }
}
