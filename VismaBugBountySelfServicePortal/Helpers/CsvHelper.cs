using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace VismaBugBountySelfServicePortal.Helpers
{
    public static class CsvHelper
    {
        private const string CsvRegex = @"(?:{0}|\n|^)(""(?:(?:"""")*[^""]*)*""|[^""{0}\n]*|(?:\n|$))";

        public static IEnumerable<string> GetCsvElements(this string line, string separator = ",")
        {
            if (separator != "," && separator != ";")
                throw new ArgumentOutOfRangeException(nameof(separator), "separator should be: , or ;");
            var regex = new Regex(string.Format(CsvRegex, separator), RegexOptions.None, TimeSpan.FromSeconds(1));
            foreach (Match match in regex.Matches(line))
            {
                var value = match.Value;

                if (value.StartsWith(separator))
                    value = value[1..];
                if (value.StartsWith("\"") && value.EndsWith("\""))
                    value = value[1..^1];
                value = value.Replace("\"\"", "\"");
                yield return value;
            }
        }
    }
}
