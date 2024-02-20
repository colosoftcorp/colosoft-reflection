using System;
using System.Collections.Generic;
using System.Linq;

namespace Colosoft.Reflection
{
    public class AssemblyRepositoryMaintenanceExecuteResult : IEnumerable<AssemblyRepositoryMaintenanceExecuteResult.Entry>
    {
        private readonly List<Entry> entries;

        public int Count => this.entries.Count;

        public Entry this[int index]
        {
            get { return this.entries[index]; }
        }

        public bool HasError
        {
            get { return this.entries.Any(f => f.Type == EntryType.Error); }
        }

        public AssemblyRepositoryMaintenanceExecuteResult(IEnumerable<Entry> entries)
        {
            if (entries is null)
            {
                throw new ArgumentNullException(nameof(entries));
            }

            this.entries = new List<Entry>(entries);
        }

        public enum EntryType
        {
            Error,
            Info,
            Warn,
        }

#pragma warning disable CA1034 // Nested types should not be visible
        public sealed class Entry
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public IMessageFormattable Message { get; set; }

            public EntryType Type { get; set; }

            public Exception Error { get; set; }

            public Entry(IMessageFormattable message, EntryType type, Exception error = null)
            {
                this.Message = message;
                this.Type = type;
                this.Error = error;
            }
        }

        public IEnumerator<AssemblyRepositoryMaintenanceExecuteResult.Entry> GetEnumerator()
        {
            return this.entries.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.entries.GetEnumerator();
        }
    }
}
