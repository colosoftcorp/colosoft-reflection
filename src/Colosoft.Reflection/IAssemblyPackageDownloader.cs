namespace Colosoft.Reflection
{
    public interface IAssemblyPackageDownloader : Colosoft.Net.IDownloader
    {
        void Add(AssemblyPackage package);
    }
}
