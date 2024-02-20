using Colosoft.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Colosoft
{
    public class PropertiesComparer<T> : IComparer<T>
    {
        private readonly PropertyComparer[] comparers;

        public PropertiesComparer(System.Linq.Expressions.Expression<Func<T, object>>[] properties)
        {
            if (properties is null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            this.comparers = properties
                .Select(f => new PropertyComparer((System.Reflection.PropertyInfo)f.GetMember(), f.Compile()))
                .ToArray();
        }

        public PropertiesComparer(string[] propertyNames)
        {
            if (propertyNames is null)
            {
                throw new ArgumentNullException(nameof(propertyNames));
            }

            var type = typeof(T);

            this.comparers = propertyNames
                .Select(f => type.GetProperty(f))
                .Where(f => f != null)
                .Select(f => new PropertyComparer(f, this.CreateGetter(f)))
                .ToArray();
        }

        private Func<T, object> CreateGetter(System.Reflection.PropertyInfo propertyInfo)
        {
            var parameter = System.Linq.Expressions.Expression.Parameter(typeof(T), "f");
            var expr = System.Linq.Expressions.Expression.Property(parameter, propertyInfo.Name);
            return System.Linq.Expressions.Expression.Lambda<Func<T, object>>(expr, parameter).Compile();
        }

        public int Compare(T x, T y)
        {
            foreach (var comparer in this.comparers)
            {
                var result = comparer.Compare(x, y);
                if (result != 0)
                {
                    return result;
                }
            }

            return 0;
        }

        private sealed class PropertyComparer : IComparer<T>
        {
            private readonly System.Collections.IComparer comparer;
            private readonly Func<T, object> getter;

            public PropertyComparer(System.Reflection.PropertyInfo propertyInfo, Func<T, object> getter)
            {
                if (propertyInfo is null)
                {
                    throw new ArgumentNullException(nameof(propertyInfo));
                }

                this.comparer = typeof(Comparer<>).MakeGenericType(propertyInfo.PropertyType)
                    .GetProperty("Default", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                    .GetValue(null, null) as System.Collections.IComparer;

                this.getter = getter;
            }

            public int Compare(T x, T y)
            {
                if (x != null && y != null)
                {
                    var x1 = this.getter(x);
                    var y1 = this.getter(y);

                    return this.comparer.Compare(x1, y1);
                }

                if (x != null)
                {
                    return 1;
                }
                else if (y != null)
                {
                    return -1;
                }

                return 0;
            }
        }
    }
}
