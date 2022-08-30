namespace TABS.Shared.ModuleTables
{
    public class DataDisplay
    {
        public FieldDisplay Field { get; set; }
        public ValueDisplay Value { get; set; }

        public DataDisplay Clone()
        {
            return new DataDisplay()
            {
                Field = this.Field,
                Value = new ValueDisplay(this.Value.Value)
            };
        }
    }

    public class FieldDisplay
    {
        public enum InputType
        {
            ShortText,
            LongText,
            Number,
            SingleSelect,
            Boolean,
            Date,
            Link,
            NestedTable
        }
        public string Name { get; set; }
        public string LocalizationKey { get; set; }
        public InputType Type { get; set; }
        public (string Name, AntDesign.PresetColor tagColor)[] Options { get; set; }
        public int MaxLength { get; set; }
        public int? IntRangeMin { get; set; }
        public int? IntRangeMax { get; set; }
        public string? Description { get; set; }

    }

    public class ValueDisplay
    {
        public string Value { get; set; }
        public string? Error { get; set; }
        public ValueDisplay(string val)
        {
            Value = val;
            Error = null;
        }
    }
}