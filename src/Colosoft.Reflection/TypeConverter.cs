using System;
using System.Xml;

namespace Colosoft.Reflection
{
    public static class TypeConverter
    {
        public static object Get(Type targetType, XmlNode node)
        {
            if (targetType == typeof(XmlNode))
            {
                return node;
            }
            else
            {
#pragma warning disable CA1304 // Specify CultureInfo
                return Get(targetType, node?.InnerXml);
#pragma warning restore CA1304 // Specify CultureInfo
            }
        }

        public static object Get(Type targetType, string value)
        {
            return Get(targetType, value, System.Globalization.CultureInfo.CurrentCulture);
        }

        public static object Get(Type targetType, string value, System.Globalization.CultureInfo culture)
        {
            if (targetType is null)
            {
                throw new ArgumentNullException(nameof(targetType));
            }

            if (targetType.IsEnum)
            {
#pragma warning disable CA1031 // Do not catch general exception types
                try
                {
                    return Enum.Parse(targetType, value, true);
                }
                catch
                {
                    return Enum.ToObject(targetType, Convert.ToInt32(value, culture));
                }
#pragma warning restore CA1031 // Do not catch general exception types
            }

            if (value != null)
            {
                return Convert.ChangeType(value, targetType, culture);
            }

            return null;
        }

        public static object Get(Type targetType, object obj)
        {
            return Get(targetType, obj, System.Globalization.CultureInfo.InvariantCulture);
        }

        public static object Get(Type targetType, object obj, System.Globalization.CultureInfo culture)
        {
            if (targetType is null)
            {
                throw new ArgumentNullException(nameof(targetType));
            }

            if (targetType.IsEnum)
            {
#pragma warning disable CA1031 // Do not catch general exception types
                try
                {
                    return Enum.Parse(targetType, obj?.ToString(), true);
                }
                catch
                {
                    return Enum.ToObject(targetType, Convert.ToInt32(obj, culture));
                }
#pragma warning restore CA1031 // Do not catch general exception types
            }

            if (obj != null)
            {
                return Convert.ChangeType(obj, targetType, culture);
            }

            return null;
        }

        public static bool IsNullAssignable(Type type)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return !type.IsValueType;
        }
    }
}
