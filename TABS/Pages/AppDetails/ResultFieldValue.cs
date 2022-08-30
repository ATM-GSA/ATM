using System;
using System.Collections.Generic;

namespace TABS.Pages.AppDetails
{
    public class ResultFieldValue
    {
        public bool Nested { get; set; }                                // Whether or not the field value is a nested table.

        public string Value { get; set; }                               // For fields with a single value (non-nested table). The field value.

        public List<string> ColumnMatches { get; set; }                 // All column matches in a nested table.

        public List<Tuple<string, string>> ObjectMatches { get; set; }   // All (field name, field value) matches in the nested table.
    }
}
