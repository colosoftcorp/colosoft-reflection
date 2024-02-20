using System;

namespace Colosoft.Reflection
{
    [Serializable]
    public class LibraryImport
    {
        private string fullPath;

        public bool Exists { get; set; }

        public string FileName { get; set; }

        public string FullPath
        {
            get { return this.fullPath; }
#if !NETSTANDARD2_0 && !NETCOREAPP2_0
            [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.LinkDemand)]
#endif
            set
            {
                this.fullPath = value;
                if (System.IO.File.Exists(this.fullPath))
                {
                    var info = System.Diagnostics.FileVersionInfo.GetVersionInfo(this.fullPath);
                    this.Version = info.FileVersion;
                }
            }
        }

        public string Version { get; set; }

#if !NETSTANDARD2_0 && !NETCOREAPP2_0
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2141:TransparentMethodsMustNotSatisfyLinkDemandsFxCopRule")]
        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.LinkDemand)]
#endif
        public LibraryImport(string fullPath, bool exists)
            : this(System.IO.Path.GetFileName(fullPath), fullPath, exists)
        {
        }

#if !NETSTANDARD2_0 && !NETCOREAPP2_0
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2141:TransparentMethodsMustNotSatisfyLinkDemandsFxCopRule"), 
         System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.LinkDemand)]
#endif
        public LibraryImport(string fileName, string fullPath, bool exists)
        {
            this.FileName = fileName;
            this.fullPath = fullPath;
            this.Exists = exists;
            if (System.IO.File.Exists(fullPath))
            {
                var info = System.Diagnostics.FileVersionInfo.GetVersionInfo(fullPath);
                this.Version = info.FileVersion;
            }
        }

        public string GetLongDescription()
        {
            if (!this.Exists)
            {
                return $"{this.FileName} [missing]";
            }

            return $"{this.fullPath} (version {this.Version})";
        }

        public override string ToString()
        {
            return this.FileName;
        }
    }
}
