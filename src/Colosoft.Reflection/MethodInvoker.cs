using System;
using System.Collections;
using System.Reflection;

namespace Colosoft.Reflection
{
    public class MethodInvoker
    {
        private readonly MethodInfo methodInfo;

        private readonly ParameterInfo[] parameterInfos;

        private readonly object[] parameterDefaultValues;

        private readonly int requiredParameters;

        public MethodInvoker(MethodInfo methodInfo, int requiredParameters)
        {
            this.methodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            this.requiredParameters = requiredParameters;
            this.parameterInfos = methodInfo.GetParameters();
            this.parameterDefaultValues = new object[this.parameterInfos.Length];
        }

        public MethodInfo MethodInfo
        {
            get { return this.methodInfo; }
        }

        public int RequiredParameters
        {
            get { return this.requiredParameters; }
        }

        public void SetDefaultValue(string parameterName, object value)
        {
            int index = this.FindParameter(parameterName);

            if (index < 0)
            {
                throw new InvalidOperationException("Method does not have a parameter named " + parameterName);
            }

            this.parameterDefaultValues[index] = value;
        }

        public MethodInvokable PrepareInvoke(Hashtable parameters)
        {
            if (parameters is null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            int normalCount = 0;
            int defaultCount = 0;
            int nullCount = 0;

            var invokeParameters = new object[this.parameterInfos.Length];
            for (int i = 0; i < this.parameterInfos.Length; i++)
            {
                ParameterInfo pi = this.parameterInfos[i];

                if (pi.GetCustomAttributes(typeof(ElementContentAttribute), false).Length > 0)
                {
                    invokeParameters[i] = parameters["$$content$$"];
                    normalCount++;
                    continue;
                }

#pragma warning disable CA1308 // Normalize strings to uppercase
                var parameterName = pi.Name.ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase

                if (parameterName.StartsWith("this.", StringComparison.InvariantCultureIgnoreCase))
                {
                    parameterName = parameterName.Substring(1, parameterName.Length - 1);
                }

                if (parameters.ContainsKey(parameterName))
                {
                    object val = parameters[parameterName];
                    if (val != null && val.GetType() != pi.ParameterType)
                    {
                        val = TypeConverter.Get(pi.ParameterType, val, System.Globalization.CultureInfo.InvariantCulture);
                    }

                    invokeParameters[i] = val;
                    normalCount++;
                }
                else
                {
                    if (i >= this.requiredParameters)
                    {
                        if (this.parameterDefaultValues[i] != null)
                        {
                            invokeParameters[i] = this.parameterDefaultValues[i];
                            defaultCount++;
                        }
                        else if (TypeConverter.IsNullAssignable(pi.ParameterType))
                        {
                            invokeParameters[i] = null;
                            nullCount++;
                        }
                    }
                }
            }

            bool isValid = this.parameterInfos.Length == normalCount + defaultCount + nullCount;

            if (!isValid)
            {
                return null;
            }

            int matchIndicator = normalCount << (16 - defaultCount) << (8 - nullCount);
            return new MethodInvokable(this, matchIndicator, invokeParameters);
        }

        public object Invoke(object target, object[] parameterValues)
        {
            return this.MethodInfo.Invoke(target, ReflectionFlags.InstanceCriteria, null, parameterValues, null);
        }

        public object Invoke(object target, Hashtable parameters)
        {
            var mi = this.PrepareInvoke(parameters);
            if (mi.MatchIndicator >= 0)
            {
                return this.Invoke(target, mi.ParameterValues);
            }
            else
            {
                throw new InvalidOperationException("Unable to invoke method using given parameters.");
            }
        }

        private int FindParameter(string parameterName)
        {
            if (this.parameterInfos == null || this.parameterInfos.Length == 0)
            {
                return -1;
            }

            for (int i = 0; i < this.parameterInfos.Length; i++)
            {
                if (this.parameterInfos[i].Name == parameterName)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
