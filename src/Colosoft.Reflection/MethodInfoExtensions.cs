using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Colosoft.Reflection
{
    public static class MethodInfoExtensions
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly Type[] KnownTypesWithReturn = { typeof(Func<>), typeof(Func<,>), typeof(Func<,,>), typeof(Func<,,,>), typeof(Func<,,,,>) };

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private static readonly Type[] KnownTypesWithoutReturn = { typeof(Action), typeof(Action<>), typeof(Action<,>), typeof(Action<,,>), typeof(Action<,,,>) };

        public static Delegate CreateDelegate(this MethodInfo method, object instance)
        {
            if (method is null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            if (method.GetType().IsGenericType)
            {
                throw new InvalidOperationException("Unable to create a delegate for a method with generic arguments.");
            }

            var parameters = method.GetParameters().ToList();

            if (parameters.Count > 4)
            {
                throw new InvalidOperationException("Unable to create a delegate for a method with more than 4 parameters.");
            }

            var hasReturnValue = method.ReturnType != typeof(void);

            var availableTypes = hasReturnValue ?
                KnownTypesWithReturn : KnownTypesWithoutReturn;

            var delegateType =
                availableTypes[parameters.Count];

            var geneticArgumenTypes = new List<Type>();
            parameters.ForEach(info =>
                geneticArgumenTypes.Add(info.ParameterType));

            if (hasReturnValue)
            {
                geneticArgumenTypes.Add(method.ReturnType);
            }

            var resultingType = delegateType.IsGenericType ?
                delegateType.MakeGenericType(geneticArgumenTypes.ToArray()) : delegateType;

            var methodWrapper =
                Delegate.CreateDelegate(resultingType, instance, method);

            return methodWrapper;
        }
    }
}
