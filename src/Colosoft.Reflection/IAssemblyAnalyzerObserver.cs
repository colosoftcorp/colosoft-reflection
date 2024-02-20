namespace Colosoft.Reflection
{
    public interface IAssemblyAnalyzerObserver
    {
        int Progress { get; set; }

        IMessageFormattable Text { get; set; }

        bool CancellationPending { get; }

        void ReportProgress(int progressPercentage, IMessageFormattable actionText);

        void ReportErrorMessage(IMessageFormattable message);
    }
}
