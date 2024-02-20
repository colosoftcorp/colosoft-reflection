using System;
using System.Collections.Generic;

namespace Colosoft.Reflection
{
    public class AssemblyLoaderGetResult : IEnumerable<AssemblyLoaderGetResult.Entry>
    {
#pragma warning disable CA1034 // Nested types should not be visible
        public class Entry
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public string AssemblyName { get; }

            public System.Reflection.Assembly Assembly { get; }

            public bool Success { get; }

            public Exception Error { get; }

            public Entry(string assemblyName, System.Reflection.Assembly assembly, bool success, Exception error)
            {
                this.AssemblyName = assemblyName;
                this.Assembly = assembly;
                this.Success = success;
                this.Error = error;
            }
        }

        private readonly List<Entry> entries;

        public int Count
        {
            get { return this.entries.Count; }
        }

        public AssemblyLoaderGetResult(IEnumerable<Entry> entries)
        {
            this.entries = new List<Entry>(entries);
        }

        public IEnumerator<AssemblyLoaderGetResult.Entry> GetEnumerator()
        {
            return this.entries.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.entries.GetEnumerator();
        }
    }
}
