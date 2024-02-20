using System;

namespace Colosoft.Reflection
{
    public delegate void AssemblyResolverLoadHandler(object sender, AssemblyResolverLoadEventArgs e);

    public class AssemblyResolverLoadEventArgs : EventArgs
    {
        public AssemblyLoaderGetResult Result { get; set; }
    }
}
