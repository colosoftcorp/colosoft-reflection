using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Colosoft.Reflection
{
    public class TypeConverterScheme<T1, T2>
    {
        private readonly List<ISchemeItem> items = new List<ISchemeItem>();

        private TypeConverterScheme()
        {
        }

#pragma warning disable CA1000 // Do not declare static members on generic types
        public static FluentTypeConverterSchema Create()
#pragma warning restore CA1000 // Do not declare static members on generic types
        {
            return new FluentTypeConverterSchema(new TypeConverterScheme<T1, T2>());
        }

        public void Apply(T1 source, T2 destination)
        {
            foreach (var item in this.items)
            {
                item.Apply(source, destination);
            }
        }

        public void Apply(T2 source, T1 destination)
        {
            foreach (var item in this.items)
            {
                item.Apply(source, destination);
            }
        }

        private interface ISchemeItem
        {
            void Apply(T1 source, T2 destination);

            void Apply(T2 source, T1 destination);
        }

        private sealed class SchemeItem<TValue> : ISchemeItem
        {
            private readonly Expression<Func<T1, TValue>> t1Expression;
            private readonly Expression<Func<T2, TValue>> t2Expression;
            private readonly Lazy<Func<T1, TValue>> t1Getter;
            private readonly Lazy<Func<T2, TValue>> t2Getter;

            private readonly Lazy<System.Reflection.MemberInfo> t1Member;
            private readonly Lazy<System.Reflection.MemberInfo> t2Member;

            public SchemeItem(Expression<Func<T1, TValue>> t1Expression, Expression<Func<T2, TValue>> t2Expression)
            {
                this.t1Expression = t1Expression;
                this.t2Expression = t2Expression;

                this.t1Getter = new Lazy<Func<T1, TValue>>(() => this.t1Expression.Compile());
                this.t2Getter = new Lazy<Func<T2, TValue>>(() => this.t2Expression.Compile());
                this.t1Member = new Lazy<System.Reflection.MemberInfo>(() => this.t1Expression.GetMember());
                this.t2Member = new Lazy<System.Reflection.MemberInfo>(() => this.t2Expression.GetMember());
            }

            private void SetValue(T1 instance, TValue value)
            {
                this.SetValue(instance, value, this.t1Member.Value);
            }

            private void SetValue(T2 instance, TValue value)
            {
                this.SetValue(instance, value, this.t2Member.Value);
            }

            private void SetValue<T>(T instance, TValue value, System.Reflection.MemberInfo member)
            {
                try
                {
#pragma warning disable S2219 // Runtime type checking should be simplified
                    if (instance == null || !member.ReflectedType.IsAssignableFrom(instance.GetType()))
                    {
                        return;
                    }
#pragma warning restore S2219 // Runtime type checking should be simplified

                    if (member is System.Reflection.PropertyInfo propertyInfo && propertyInfo.CanWrite)
                    {
                        propertyInfo.SetValue(instance, value, null);
                    }
                    else if (member is System.Reflection.FieldInfo fieldInfo)
                    {
                        fieldInfo.SetValue(instance, value);
                    }
                }
                catch (System.Reflection.TargetInvocationException ex)
                {
                    throw ex.InnerException;
                }
            }

            public void Apply(T1 source, T2 destination)
            {
                this.SetValue(destination, this.t1Getter.Value(source));
            }

            public void Apply(T2 source, T1 destination)
            {
                this.SetValue(destination, this.t2Getter.Value(source));
            }
        }

        private sealed class CastSchemeItem<TLeft, TRight> : ISchemeItem
        {
            private readonly Expression<Func<T1, TLeft>> leftExpression;
            private readonly Expression<Func<T2, TRight>> rightExpression;
            private readonly Lazy<Func<T1, TLeft>> leftGetter;
            private readonly Lazy<Func<T2, TRight>> rightGetter;

            private readonly Func<TLeft, TRight> leftRightCast;
            private readonly Func<TRight, TLeft> rightLeftCast;

            private readonly Lazy<System.Reflection.MemberInfo> t1Member;
            private readonly Lazy<System.Reflection.MemberInfo> t2Member;

            public CastSchemeItem(
                Expression<Func<T1, TLeft>> leftExpression,
                Expression<Func<T2, TRight>> rightExpression,
                Func<TLeft, TRight> leftRightCast,
                Func<TRight, TLeft> rightLeftCast)
            {
                this.leftExpression = leftExpression;
                this.rightExpression = rightExpression;

                this.leftGetter = new Lazy<Func<T1, TLeft>>(() => this.leftExpression.Compile());
                this.rightGetter = new Lazy<Func<T2, TRight>>(() => this.rightExpression.Compile());
                this.t1Member = new Lazy<System.Reflection.MemberInfo>(() => this.leftExpression.GetMember());
                this.t2Member = new Lazy<System.Reflection.MemberInfo>(() => this.rightExpression.GetMember());

                this.leftRightCast = leftRightCast;
                this.rightLeftCast = rightLeftCast;
            }

            private void SetValue(T1 instance, TRight value)
            {
                var value2 = this.rightLeftCast(value);
                this.SetValue(instance, value2, this.t1Member.Value);
            }

            private void SetValue(T2 instance, TLeft value)
            {
                var value2 = this.leftRightCast(value);
                this.SetValue(instance, value2, this.t2Member.Value);
            }

            private void SetValue<T, TValue>(T instance, TValue value, System.Reflection.MemberInfo member)
            {
                try
                {
#pragma warning disable S2219 // Runtime type checking should be simplified
                    if (instance == null || !member.ReflectedType.IsAssignableFrom(instance.GetType()))
                    {
                        return;
                    }
#pragma warning restore S2219 // Runtime type checking should be simplified

                    if (member is System.Reflection.PropertyInfo && ((System.Reflection.PropertyInfo)member).CanWrite)
                    {
                        ((System.Reflection.PropertyInfo)member).SetValue(instance, value, null);
                    }
                    else if (member is System.Reflection.FieldInfo fieldInfo)
                    {
                        fieldInfo.SetValue(instance, value);
                    }
                }
                catch (System.Reflection.TargetInvocationException ex)
                {
                    throw ex.InnerException;
                }
            }

            public void Apply(T1 source, T2 destination)
            {
                this.SetValue(destination, this.leftGetter.Value(source));
            }

            public void Apply(T2 source, T1 destination)
            {
                this.SetValue(destination, this.rightGetter.Value(source));
            }
        }

        private sealed class SchemaItem : ISchemeItem
        {
            private readonly Action<T1, T2> t1ToT2;
            private readonly Action<T2, T1> t2ToT1;

            public SchemaItem(Action<T1, T2> t1ToT2, Action<T2, T1> t2ToT1)
            {
                this.t1ToT2 = t1ToT2;
                this.t2ToT1 = t2ToT1;
            }

            public void Apply(T1 source, T2 destination)
            {
                this.t1ToT2(source, destination);
            }

            public void Apply(T2 source, T1 destination)
            {
                this.t2ToT1(source, destination);
            }
        }

#pragma warning disable CA1034 // Nested types should not be visible
        public sealed class FluentTypeConverterSchema
#pragma warning restore CA1034 // Nested types should not be visible
        {
            private readonly TypeConverterScheme<T1, T2> schema;

            internal FluentTypeConverterSchema(TypeConverterScheme<T1, T2> scheme)
            {
                this.schema = scheme;
            }

            public FluentTypeConverterSchema Property<TValue>(Expression<Func<T1, TValue>> sourceProperty, Expression<Func<T2, TValue>> destinationProperty)
            {
                if (sourceProperty is null)
                {
                    throw new ArgumentNullException(nameof(sourceProperty));
                }

                if (destinationProperty is null)
                {
                    throw new ArgumentNullException(nameof(destinationProperty));
                }

                this.schema.items.Add(new SchemeItem<TValue>(sourceProperty, destinationProperty));

                return this;
            }

            public FluentTypeConverterSchema Property<TLeft, TRight>(
                Expression<Func<T1, TLeft>> sourceProperty,
                Expression<Func<T2, TRight>> destinationProperty,
                Func<TLeft, TRight> leftRightCast,
                Func<TRight, TLeft> rightLeftCast)
            {
                if (sourceProperty is null)
                {
                    throw new ArgumentNullException(nameof(sourceProperty));
                }

                if (destinationProperty is null)
                {
                    throw new ArgumentNullException(nameof(destinationProperty));
                }

                this.schema.items.Add(new CastSchemeItem<TLeft, TRight>(sourceProperty, destinationProperty, leftRightCast, rightLeftCast));

                return this;
            }

            public FluentTypeConverterSchema Apply(Action<T1, T2> t1ToT2, Action<T2, T1> t2ToT1)
            {
                if (t1ToT2 is null)
                {
                    throw new ArgumentNullException(nameof(t1ToT2));
                }

                if (t2ToT1 is null)
                {
                    throw new ArgumentNullException(nameof(t2ToT1));
                }

                this.schema.items.Add(new SchemaItem(t1ToT2, t2ToT1));

                return this;
            }

            public TypeConverterScheme<T1, T2> Finalize()
            {
                return this.schema;
            }
        }
    }
}
