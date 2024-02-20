using System;
using System.Reflection;

namespace Colosoft
{
    public static class ObjectExtensions
    {
        private static Tuple<object, Type, MemberInfo> GetMemberInfo(object instance, string propertyPath)
        {
            if ((instance == null) || string.IsNullOrEmpty(propertyPath))
            {
                return null;
            }

            var parts = propertyPath.Split('.');
            var index = 0;
            var size = parts.Length;
            var result = Tuple.Create<object, Type, MemberInfo>(instance, instance.GetType(), null);
            var name = string.Empty;
            MemberInfo propertyInfo;
            while (index < size)
            {
                name = parts[index++];
                propertyInfo = result.Item2.GetProperty(name);
                if (propertyInfo != null)
                {
                    var asP = (PropertyInfo)propertyInfo;
                    var source = (result.Item3 == null) ? instance : GetMemberValue(result.Item1, result.Item3);
                    result = Tuple.Create<object, Type, MemberInfo>(source, asP.PropertyType, asP);
                    continue;
                }

                propertyInfo = result.Item2.GetField(name);
                if (propertyInfo != null)
                {
                    var asF = (FieldInfo)propertyInfo;
                    var source = (result.Item3 == null) ? instance : GetMemberValue(result.Item1, result.Item3);
                    result = Tuple.Create<object, Type, MemberInfo>(source, asF.FieldType, asF);
                }
                else
                {
                    return null;
                }
            }

            return result;
        }

        private static Type GetMemberType(MemberInfo info)
        {
            if (info == null)
            {
                return null;
            }

            PropertyInfo asP;
            FieldInfo asF;
            if (info.TryCastAs<PropertyInfo>(out asP))
            {
                return asP.PropertyType;
            }
            else if (info.TryCastAs<FieldInfo>(out asF))
            {
                return asF.FieldType;
            }

            return null;
        }

        private static object GetMemberValue(object instance, MemberInfo info)
        {
            if ((instance == null) || (info == null))
            {
                return null;
            }

            PropertyInfo asP;
            FieldInfo asF;
            if (info.TryCastAs<PropertyInfo>(out asP))
            {
                return asP.GetValue(instance, null);
            }
            else if (info.TryCastAs<FieldInfo>(out asF))
            {
                return asF.GetValue(instance);
            }

            return null;
        }

        private static bool SetMemberValue(object instance, MemberInfo info, object value)
        {
            if ((instance == null) || (info == null))
            {
                return false;
            }

            PropertyInfo asP;
            FieldInfo asF;
            if (info.TryCastAs<PropertyInfo>(out asP))
            {
                asP.SetValue(instance, value, null);
                return true;
            }
            else if (info.TryCastAs<FieldInfo>(out asF))
            {
                asF.SetValue(instance, value);
                return true;
            }

            return false;
        }

        public static Type GetMemberType(this object instance, string propertyPath)
        {
            if ((instance == null) || string.IsNullOrEmpty(propertyPath))
            {
                return null;
            }

            var info = GetMemberInfo(instance, propertyPath);
            return (info != null) ? GetMemberType(info.Item3) : null;
        }

        public static T GetMemberValue<T>(this object instance, string propertyPath)
        {
            var info = GetMemberInfo(instance, propertyPath);
            var obj = (info != null) && (info.Item1 != null) ? GetMemberValue(info.Item1, info.Item3) : null;
            return (obj is T) ? (T)obj : default(T);
        }

        public static object GetMemberValue(this object instance, string propertyPath)
        {
            var info = GetMemberInfo(instance, propertyPath);
            return (info != null) && (info.Item1 != null) ? GetMemberValue(info.Item1, info.Item3) : null;
        }

        public static bool SetMemberValue(this object instance, string propertyPath, object value)
        {
            var info = GetMemberInfo(instance, propertyPath);
            if ((info == null) || (info.Item1 == null))
            {
                return false;
            }

            return SetMemberValue(info.Item1, info.Item3, value);
        }

        public static bool TryCastAs<T>(this object instance, out T result, T stdVal = null)
            where T : class
        {
            var valid = instance != null;
            result = valid ? (instance as T) : stdVal;
            return valid && (result != null);
        }

        public static bool TryConvertAs<T>(this object instance, out T result, T stdVal = default(T))
            where T : struct
        {
            var isValid = instance is T;
            result = isValid ? (T)instance : stdVal;
            return isValid;
        }
    }
}