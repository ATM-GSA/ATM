using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using TABS.Data;

namespace TABS.Pages.AppDetails
{
    public class Result
    {
        public DetailedModuleSearchResult SearchResult { get; set; }

        public IApplicationModule Module { get; set; }

        public List<Tuple<string, ResultFieldValue>> Fields { get; set; } // All matching (field name, field value) for this module.

        private bool _isLoaded = false;

        public List<Tuple<string, ResultFieldValue>> FieldResults { get; set; } // All (field name, field value) results but styled. Maintains the same order as Fields.
        private string ModuleNameResult { get; set; } // Module name with styling.

        public List<Tuple<string, ResultFieldValue>> GetFieldResults()
        {
            return FieldResults;
        }

        /// <summary>
        /// Returns the styled Module name.
        /// </summary>
        /// <returns></returns>
        public string GetModuleNameResult()
        {
            return ModuleNameResult;
        }

        /// <summary>
        /// Returns the LastUpdated value of the Module. Should only be called on Module types.
        /// </summary>
        /// <returns></returns>
        public DateTime GetModuleLastUpdated()
        {
            if (Module is Module)
            {
                return ((Module)Module).LastUpdate;
            }
            else
            {
                // throw an error here or something later
                return new DateTime();
            }
        }

        /// <summary>
        /// Returns the StatusFlags value of the Module. Should only be called on Module types.
        /// </summary>
        /// <returns></returns>
        public int GetModuleStatusFlags()
        {
            if (Module is Module)
            {
                return ((Module)Module).StatusFlags;
            }
            else
            {
                // throw an error here or something later
                return -1;
            }
        }

        /// <summary>
        /// Returns True when all the fields for this Result have been loaded.
        /// </summary>
        /// <returns></returns>
        public bool IsLoaded()
        {
            return _isLoaded;
        }

        /// <summary>
        /// Populate the search results.
        /// </summary>
        /// <param name="searchTerm"></param>
        public void PopulateResults(List<string> searchTerms, IStringLocalizer<App> localizer)
        {
            FieldResults = new List<Tuple<string, ResultFieldValue>>();
            ModuleNameResult = "";

            // Loop through all field-value matches
            for (int i = 0; i < Fields.Count; i++)
            {
                string fieldName = StyleWithoutTruncation(searchTerms, Fields[i].Item1, true);
                if (Fields[i].Item2.Nested) // nested table
                {
                    ResultFieldValue rfv = new ResultFieldValue()
                    {
                        Nested = true,
                        Value = "", // this should stay empty
                        ColumnMatches = new List<string>(),
                        ObjectMatches = new List<Tuple<string, string>>()
                    };
                    foreach (string colMatch in Fields[i].Item2.ColumnMatches)
                    {
                        rfv.ColumnMatches.Add(StyleWithoutTruncation(searchTerms, colMatch, true));
                    }
                    foreach (Tuple<string, string> objMatch in Fields[i].Item2.ObjectMatches)
                    {
                        // This bolds + makes matching text blue on the column name
                        //string objName = StyleWithoutTruncation(searchTerms, objMatch.Item1, true);
                        //string objVal = StyleWithTruncation(searchTerms, objMatch.Item2);
                        //rfv.ObjectMatches.Add(new Tuple<string, string>(objName, objVal));

                        // This makes column name gray regardless
                        string objName = $"<span class=\"temp-gray\">{objMatch.Item1}</span>";
                        string objVal = StyleWithTruncation(searchTerms, objMatch.Item2);
                        rfv.ObjectMatches.Add(new Tuple<string, string>(objName, objVal));
                    }
                    FieldResults.Add(new Tuple<string, ResultFieldValue>(fieldName, rfv));
                }
                else // non-nested table
                {
                    string fieldVal = StyleWithTruncation(searchTerms, Fields[i].Item2.Value);
                    FieldResults.Add(new Tuple<string, ResultFieldValue>(
                        fieldName,
                        new ResultFieldValue()
                        {
                            Nested = false,
                            Value = fieldVal,
                            ColumnMatches = new List<string>(), // this should stay empty
                            ObjectMatches = new List<Tuple<string, string>>() // this should stay empty
                        }));
                }

            }
            ModuleNameResult = StyleWithoutTruncation(searchTerms, localizer[Module.GetModuleType().ToString()]);
            _isLoaded = true;
        }

        /// <summary>
        /// Return the index of the first occurance of any of the given strings.
        /// </summary>
        /// <param name="searchTerms"> list of search terms</param>
        /// <param name="field">the field to search through</param>
        /// <param name="searchFrom">the index to search the field from</param>
        /// <returns>a tuple containing the matching search term and the index the search term starts at</returns>
        private Tuple<string, int> IndexOfAny(List<string> searchTerms, string field, int searchFrom)
        {
            string match = "";
            int startIdx = field.Length;
            string normalizedField = RemoveDiacritics(field).ToLower();

            foreach (string m in searchTerms)
            {
                int idx = normalizedField.IndexOf(RemoveDiacritics(m).ToLower(), searchFrom);
                if (idx < startIdx && idx >= searchFrom)
                {
                    match = m;
                    startIdx = idx;
                }
            }

            if (startIdx == field.Length)
            {
                startIdx = -1;
            }

            return new Tuple<string, int>(match, startIdx);
        }

        /// <summary>
        /// Builds all field name HTML elements so that matched search terms are highlighted.
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <param name="fieldName"></param>
        private string StyleWithoutTruncation(List<string> searchTerms, string field, bool bold = false)
        {
            // Bold the portions of the field that match (no truncating necessary here)
            int searchFrom = 0;
            string result = "";
            string searchTerm;
            int startIdx;
            while (searchFrom < field.Length)
            {
                (searchTerm, startIdx) = IndexOfAny(searchTerms, field, searchFrom);

                if (startIdx == -1)
                {
                    break;
                }

                // concat the portion of the string before the matched portion
                result += $"{field.Substring(searchFrom, startIdx - searchFrom)}";
                // concat the matched portion of the string
                result += bold ?
                    $"<span class=\"temp-blue bold-text\">{field.Substring(startIdx, searchTerm.Length)}</span>" :
                    $"<span class=\"temp-blue \">{field.Substring(startIdx, searchTerm.Length)}</span>";

                searchFrom = startIdx + searchTerm.Length;
            }
            // concat the remainder of the string
            result += $"{field.Substring(searchFrom, field.Length - searchFrom)}";

            return result;
        }

        /// <summary>
        /// Builds all field value HTML elements so that matched search terms are highlighted.
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <param name="fieldValue"></param>
        private string StyleWithTruncation(List<string> searchTerms, string field)
        {
            // Bold the portions of the field value that match. NOTE: need to consider truncation.
            int truncateAmt = 20; // number of characters to truncate on the front and end of a matched string
            int previewAmt = 100; // number of characters to preview when there is no value match

            int searchFrom = 0;
            string result = "";
            string searchTerm;
            int startIdx;
            while (searchFrom < field.Length)
            {
                (searchTerm, startIdx) = IndexOfAny(searchTerms, field, searchFrom);

                if (startIdx == -1)
                {
                    break;
                }

                // check if we are within 20*2 characters of the last match or the beginning of the string
                if (startIdx < truncateAmt || startIdx - searchFrom < truncateAmt * 2)
                {
                    // no truncation needed
                    result += $"{field.Substring(searchFrom, startIdx - searchFrom)}";
                }
                else
                {
                    if (searchFrom != 0)
                    {
                        // get the 20 characters after the last match
                        result += $"{field.Substring(searchFrom, truncateAmt)}";
                    }

                    result += $"[...]";
                    // get the 20 characters before the match
                    result += $"{field.Substring(startIdx - truncateAmt, truncateAmt)}";
                }

                // concat the matched part of the string
                result += $"<span class=\"temp-blue bold-text\">{field.Substring(startIdx, searchTerm.Length)}</span>";
                searchFrom = startIdx + searchTerm.Length;
            }

            if (result == "")
            {
                // There is no match. Just give a preview of the field value.
                result += $"{field.Substring(0, Math.Min(previewAmt, field.Length))}";
                // concat [...] if a truncation happened
                result += field.Length > previewAmt ? "[...]" : "";
            }
            else
            {
                // check if the last match was within 20 characters of the end of the string
                if (field.Length - 1 - searchFrom <= 20)
                {
                    // no truncation needed
                    result += $"{field.Substring(searchFrom, field.Length - searchFrom)}";
                }
                else
                {
                    // get the 20 characters after the last match
                    result += $"{field.Substring(searchFrom, 20)}";
                    result += $"[...]";
                }
            }

            return result;
        }

        /// <summary>
        /// Removes diacritics from a given string.
        /// 
        /// Method from SearchService.cs
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;
            text = text.Normalize(NormalizationForm.FormD);
            var chars = text.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
            return new string(chars).Normalize(NormalizationForm.FormC);
        }
    }
}
