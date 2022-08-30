using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace TABS.Shared.Helpers
{
    public class EnumType<T>
    {
        public string Name { get; set; }
        public T Value { get; set; }
        public EnumType(string name, T type)
        {
            this.Name = name;
            this.Value = type;
        }
    }

    public static class EnumTypeExtensions
    {
        static public string GetLocalizedDisplayName(Type EnumT, object EnumValue)
        {
            string enumName = Enum.GetName(EnumT, EnumValue);
            var fieldInfo = EnumT.GetField(enumName);
            string name = fieldInfo.GetCustomAttribute<DisplayAttribute>(true)?.GetName() ?? enumName;
            return name;
        }
    }
}
