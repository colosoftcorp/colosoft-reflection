using System;
using System.Collections.Generic;
using System.IO.Compression;

namespace Colosoft.Reflection
{
    /// <summary>
    /// Resultado do download de um pacote de assembly.
    /// </summary>
    public class AssemblyPackageDownloaderResult : IEnumerable<AssemblyPackageDownloaderResult.Item>, IDisposable
    {
        private readonly List<Item> items = new List<Item>();
        private System.IO.Stream stream;
        private ZipArchive zipArchive;

        public AssemblyPackageDownloaderResult(System.IO.Stream inputStream)
        {
            this.stream = inputStream ?? throw new ArgumentNullException(nameof(inputStream));
        }

        public static void Build(IEnumerable<Item> items, System.IO.Stream outStream)
        {
            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            var buffer = new byte[1024];
            var read = 0;
            using (var archive = new ZipArchive(outStream, ZipArchiveMode.Create, true))
            {
                foreach (var i in items)
                {
                    if (i.Stream != null)
                    {
                        var entry = archive.CreateEntry($"{i.Uid.ToString()}.xap");

                        using (var entryStream = entry.Open())
                        {
                            while ((read = i.Stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                entryStream.Write(buffer, 0, read);
                            }

                            entryStream.Flush();
                        }

                        entry.LastWriteTime = i.LastWriteTime;
                    }
                }
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.stream != null)
            {
                this.stream.Dispose();
                this.stream = null;
            }

            if (this.zipArchive != null)
            {
                this.zipArchive.Dispose();
                this.zipArchive = null;
            }

            foreach (var i in this.items)
            {
                i.Dispose();
            }

            this.items.Clear();
        }

#pragma warning disable CA1034 // Nested types should not be visible
        public sealed class Item : IDisposable
#pragma warning restore CA1034 // Nested types should not be visible
        {
            private readonly DateTime lastWriteTime;
            private Guid uid;
            private System.IO.Stream stream;

            public Guid Uid => this.uid;

            public System.IO.Stream Stream => this.stream;

            public DateTime LastWriteTime
            {
                get { return this.lastWriteTime; }
            }

            public Item(Guid uid, DateTime lastWriteTime, System.IO.Stream stream)
            {
                this.uid = uid;
                this.lastWriteTime = lastWriteTime;
                this.stream = stream;
            }

            public void Dispose()
            {
                if (this.stream != null)
                {
                    this.stream.Dispose();
                    this.stream = null;
                }

                GC.SuppressFinalize(this);
            }
        }

        private IEnumerator<AssemblyPackageDownloaderResult.Item> GetEnumeratorInternal()
        {
            if (this.zipArchive == null)
            {
                this.zipArchive = new ZipArchive(this.stream, ZipArchiveMode.Read, true);
            }

            foreach (var file in this.zipArchive.Entries)
            {
                Guid uid = Guid.Empty;

                try
                {
                    var name = System.IO.Path.GetFileNameWithoutExtension(file.Name);

                    uid = Guid.Parse(name);
                }
                catch
                {
                    continue;
                }

                var item = this.items.Find(f => f.Uid == uid);

                if (item == null)
                {
                    item = new Item(uid, file.LastWriteTime.DateTime, file.Open());
                    this.items.Add(item);
                }

                yield return item;
            }
        }

        public IEnumerator<AssemblyPackageDownloaderResult.Item> GetEnumerator()
        {
            return this.GetEnumeratorInternal();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumeratorInternal();
        }
    }
}
