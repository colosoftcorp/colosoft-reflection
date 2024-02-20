using System;

namespace Colosoft.Reflection
{
    public class AssemblyPackageDownloadCompletedEventArgs : Colosoft.Net.DownloadCompletedEventArgs
    {
        public AssemblyPackageDownloaderResult PackagesResult { get; }

        public AssemblyPackageDownloadCompletedEventArgs(Exception error, bool cancelled, object userState, AssemblyPackageDownloaderResult result)
            : base(error, cancelled, userState, null)
        {
            this.PackagesResult = result;
        }
    }
}
