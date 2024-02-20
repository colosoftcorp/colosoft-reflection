using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Colosoft.Reflection
{
    public static class ExpressionExtensions
    {
        private static MemberExpression RemoveUnary(Expression toUnwrap)
        {
            return toUnwrap is UnaryExpression unaryExpression ? (MemberExpression)unaryExpression.Operand : toUnwrap as MemberExpression;
        }

        public static MemberInfo GetMember(this Expression<Func<string>> expression)
        {
            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var memberExp = RemoveUnary(expression.Body);

            if (memberExp == null)
            {
                return null;
            }

            return memberExp.Member;
        }

        public static MemberInfo GetMember<T>(this Expression<Func<T, object>> expression)
        {
            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var memberExp = RemoveUnary(expression.Body);

            if (memberExp == null)
            {
                return null;
            }

            return memberExp.Member;
        }

        public static MemberInfo GetMember<T, TResult>(this Expression<Func<T, TResult>> expression)
        {
            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var memberExp = RemoveUnary(expression.Body);

            if (memberExp == null)
            {
                return null;
            }

            return memberExp.Member;
        }
    }
}
