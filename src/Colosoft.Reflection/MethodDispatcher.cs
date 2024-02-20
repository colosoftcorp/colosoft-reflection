using System;
using System.Collections;

namespace Colosoft.Reflection
{
    public class MethodDispatcher
    {
        private readonly object objLock = new object();
        private readonly IList invokers;
        private readonly Hashtable dispatchCache;

        public MethodDispatcher(MethodInvoker invoker)
        {
            this.invokers = new ArrayList();
            this.invokers.Add(invoker);
            this.dispatchCache = new Hashtable();
        }

        public void AddInvoker(MethodInvoker invoker)
        {
            lock (this.objLock)
            {
                this.invokers.Add(invoker);
            }
        }

        public object Invoke(object target, Hashtable parameters)
        {
            var invokable = this.DetermineBestMatch(parameters);

            if (invokable == null)
            {
                throw new InvalidOperationException("No compatible method found to invoke for the given parameters.");
            }

            return invokable.Invoke(target);
        }

        private MethodInvokable DetermineBestMatch(Hashtable parameters)
        {
            MethodInvokable best = null;

            foreach (MethodInvoker invoker in this.invokers)
            {
                MethodInvokable invokable = invoker.PrepareInvoke(parameters);
                bool isBetter = best == null && invokable != null && invokable.MatchIndicator > 0;
                isBetter |= best != null && invokable != null && invokable.MatchIndicator > best.MatchIndicator;
                if (isBetter)
                {
                    best = invokable;
                }
            }

            return best;
        }
    }
}
