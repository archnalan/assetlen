using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace mowt.Shared.Models.Validators
{
    /// <summary>
    /// Validates that at least one of the specified properties has a non-null value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class AtLeastOneRequiredAttribute : ValidationAttribute
    {
        private readonly string[] _propertyNames;

        public AtLeastOneRequiredAttribute(params string[] propertyNames)
        {
            _propertyNames = propertyNames;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("The object cannot be null.");
            }

            var type = value.GetType();
            var hasAtLeastOne = false;

            foreach (var propertyName in _propertyNames)
            {
                var property = type.GetProperty(propertyName);
                if (property == null)
                {
                    return new ValidationResult($"Property '{propertyName}' not found on type '{type.Name}'.");
                }

                var propertyValue = property.GetValue(value);

                // Check if property has a value (not null and not empty string)
                if (propertyValue != null)
                {
                    if (propertyValue is string stringValue)
                    {
                        if (!string.IsNullOrWhiteSpace(stringValue))
                        {
                            hasAtLeastOne = true;
                            break;
                        }
                    }
                    else
                    {
                        hasAtLeastOne = true;
                        break;
                    }
                }
            }

            if (!hasAtLeastOne)
            {
                return new ValidationResult($"At least one of the following properties must be provided: {string.Join(", ", _propertyNames)}.");
            }

            return ValidationResult.Success;
        }
    }
}
