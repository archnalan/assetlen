using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mowt.Shared.Models.Models.ViewModels
{

    public class ColumnMapping
    {
        public string SystemColumn { get; set; }
        public string SelectedFileColumn { get; set; }
        public List<string> FileColumnOptions { get; set; } = new List<string>();
    }
    public class ColumnMappingDto
    {
        private static readonly Dictionary<Type, string> _friendlyTypeNames = new()
        {
            { typeof(int), "number" },
            { typeof(int?), "number" },
            { typeof(decimal), "decimal" },
            { typeof(decimal?), "decimal" },
            { typeof(DateTime), "date" },
            { typeof(DateTime?), "date" },
            { typeof(bool), "yes/no" },
            { typeof(bool?), "yes/no" },
            { typeof(string), "text" }
        };

        public string SystemColumn { get; set; }
        public string SelectedFileColumn { get; set; }
        public List<string> FileColumnOptions { get; set; }
        public string Error { get; set; }
        public Type TargetType { get; private set; }
        public string FriendlyTypeName => GetFriendlyTypeName(TargetType);

        public void Initialize(Type modelType)
        {
            var prop = modelType.GetProperty(SystemColumn);
            TargetType = prop?.PropertyType ?? typeof(string);

            // Handle nullable types
            if (TargetType.IsGenericType && TargetType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                TargetType = Nullable.GetUnderlyingType(TargetType);
            }
        }

        private string GetFriendlyTypeName(Type type)
        {
            return _friendlyTypeNames.TryGetValue(type, out var name)
                ? name
                : type.Name;
        }

        public static string GetValueTypeName(string value)
        {
            if (string.IsNullOrEmpty(value)) return "empty";
            if (int.TryParse(value, out _)) return "integer";
            if (decimal.TryParse(value, out _)) return "decimal";
            if (DateTime.TryParse(value, out _)) return "date";
            if (bool.TryParse(value, out _)) return "yes/no";
            return "text";
        }
        // In ColumnMappingDto class
        public static async Task<bool> ValidateColumn(ColumnMappingDto mapping,
                                                    List<Dictionary<string, object>> uploadedExcelContent)
        {
            mapping.Error = null;

            if (string.IsNullOrEmpty(mapping.SelectedFileColumn) ||
                uploadedExcelContent == null)
                return true;

            try
            {
                foreach (var row in uploadedExcelContent)
                {
                    var value = GetValue(row, mapping.SelectedFileColumn);
                    if (!IsCompatibleType(value, mapping.TargetType))
                    {
                        mapping.Error = $"Invalid format for {mapping.SystemColumn}. " +
                                      $"Received {GetValueTypeName(value)}, Expected {mapping.FriendlyTypeName}";
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                mapping.Error = $"Validation error: {ex.Message}";
                return false;
            }
        }
        private static bool IsCompatibleType(string value, Type targetType)
        {
            try
            {
                if (string.IsNullOrEmpty(value)) return true;
                var converter = TypeDescriptor.GetConverter(targetType);
                converter.ConvertFromString(value);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static string GetValue(Dictionary<string, object> item, string key)
        {
            if (item == null || !item.TryGetValue(key, out object? value))
                return string.Empty;

            return value switch
            {
                null => string.Empty,
                DBNull => string.Empty,
                string s when s.Equals("null", StringComparison.OrdinalIgnoreCase) => string.Empty,
                string s => s,
                _ => value.ToString() ?? string.Empty
            };
        }

    }
}
