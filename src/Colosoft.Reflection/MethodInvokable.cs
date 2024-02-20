namespace Colosoft.Reflection
{
    public class MethodInvokable
    {
        public MethodInvoker MethodInvoker { get; }
        public int MatchIndicator { get; }
#pragma warning disable CA1819 // Properties should not return arrays
        public object[] ParameterValues { get; }
#pragma warning restore CA1819 // Properties should not return arrays

        public MethodInvokable(MethodInvoker invoker, int matchIndicator, object[] parameterValues)
        {
            this.MethodInvoker = invoker;
            this.MatchIndicator = matchIndicator;
            this.ParameterValues = parameterValues;
        }

        public object Invoke(object target)
        {
            return this.MethodInvoker.Invoke(target, this.ParameterValues);
        }
    }
}
