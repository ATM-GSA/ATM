using System;
using System.Linq;
using System.Reflection;

namespace TABS.Shared.ModuleTables
{
    public static class ReflectionHelper
    {
        public static bool setPropertyValue(object target, Type targetType, string propertyName, object value)
        {
            try
            {
                PropertyInfo property = targetType.GetProperty(propertyName);
                property.SetValue(target, value);
                return true;
            }
            catch
            {
                return false;
            }

        }
    }

    public static class ObjectExtension
    {
        public static T GetAttributeFrom<T>(this object instance, string propertyName) where T : Attribute
        {
            var attributeType = typeof(T);
            var property = instance.GetType().GetProperty(propertyName);
            if (property == null) return default(T);
            return (T)property.GetCustomAttributes(attributeType, false).FirstOrDefault();
        }
    }
}
