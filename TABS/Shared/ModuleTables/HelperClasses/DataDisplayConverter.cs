using System;


namespace TABS.Shared.ModuleTables
{
    public static class DataDisplayConverter
    {
        public static bool PopulateObjWithDataDisplayValue(object target, Type targetType, DataDisplay dataDisplay)
        {
            if (dataDisplay.Field.Type == FieldDisplay.InputType.NestedTable)
            {
                return true;
            }
            else if (dataDisplay.Field.Type == FieldDisplay.InputType.Boolean)
            {
                return ReflectionHelper.setPropertyValue(target, targetType, dataDisplay.Field.Name, bool.Parse(dataDisplay.Value.Value));
            }
            else if (dataDisplay.Field.Type == FieldDisplay.InputType.Date)
            {
                return ReflectionHelper.setPropertyValue(target, targetType, dataDisplay.Field.Name, DateTime.Parse(dataDisplay.Value.Value));
            }
            else if (dataDisplay.Field.Type == FieldDisplay.InputType.Number)
            {
                return ReflectionHelper.setPropertyValue(target, targetType, dataDisplay.Field.Name, Int32.Parse(dataDisplay.Value.Value));
            }
            else
            {
                return ReflectionHelper.setPropertyValue(target, targetType, dataDisplay.Field.Name, dataDisplay.Value.Value);
            }
        }
    }

}
