using Syncfusion.Blazor.Chart3D;
using Syncfusion.Blazor.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace assetlen.Shared.statics
{
    public static class ClassExtensions
    {
        public static string ToSentenceCase(this string input)
        {
            if (string.IsNullOrWhiteSpace(input.Trim())) return string.Empty;

            input = input.Trim();
            var wordsArray = input.Split(' ');
            wordsArray = wordsArray.Where(x => x.Trim().Length > 0)
                .Select(x => x.Length > 1 ? x[0].ToString().ToUpperInvariant() + x.Substring(1) : x[0].ToString().ToUpperInvariant())
                .ToArray();

            return string.Join(" ", wordsArray);

        }
        public static T RemoveEmptyStrings<T>(this T classObject) where T : class
        {
            var properties = GetEmptyProperties(classObject);

            foreach (var property in properties)
            {
                property.SetValue(classObject, null);
            }
            return classObject;
        }
        private static PropertyInfo[] GetEmptyProperties<T>(T obj)
        {
            // Get all properties of the object
            var properties = typeof(T).GetProperties();
            var emptyProps = new List<PropertyInfo>();

            // Loop through properties and check if they are null
            foreach (var prop in properties)
            {
                var value = prop.GetValue(obj);
                if (value == "")
                {
                    emptyProps.Add(prop);
                }
            }

            return emptyProps.ToArray();
        }
        public static string ToFormattedPrice(this decimal value, int? decimalPlaces = null)
        {
            int defDecimalPlaces = int.TryParse(assetlen.Shared.Models.statics.statics.allSettings[
                (int)assetlen.Shared.Models.statics.statics.Configurations.NoOfDecimalPlaces], out var decPlaces)
                ? decPlaces
                : 2;//default decimal places

            return value.ToString($"F{decimalPlaces ?? defDecimalPlaces}");
        }
        public static string ToFormattedCurrency(this decimal value, int? decimalPlaces = null)
        {
            int defDecimalPlaces = int.TryParse(assetlen.Shared.Models.statics.statics.allSettings[
                (int)assetlen.Shared.Models.statics.statics.Configurations.NoOfDecimalPlaces], out var decPlaces)
                ? decPlaces
                : 2;//default decimal places

            var culture = CultureInfo.CurrentCulture.Clone() as CultureInfo;
            if (culture != null)
            {
                culture.NumberFormat.CurrencyDecimalDigits = decimalPlaces ?? defDecimalPlaces;
            }

            return value.ToString("C", culture);
        }
    }
}
