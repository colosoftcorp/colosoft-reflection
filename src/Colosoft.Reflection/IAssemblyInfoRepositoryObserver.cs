namespace Colosoft.Reflection
{
    public interface IAssemblyInfoRepositoryObserver
    {
        void OnAnalysisAssemblyProgressChanged(IMessageFormattable message, int percentage);

        void OnLoadingAssemblyFiles();

        void OnLoaded();
    }
}
