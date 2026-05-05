using System.Globalization;
using System.Windows.Controls;

namespace FiveMVehicleMetaEditorWPF.Core.Converters
{
    /// <summary>
    /// ValidationRule that ensures the input can be parsed as a number.
    /// Attach to TextBox bindings for real-time red-border feedback.
    /// </summary>
    public class NumericValidationRule : ValidationRule
    {
        public double Minimum { get; set; } = double.MinValue;
        public double Maximum { get; set; } = double.MaxValue;
        public bool AllowEmpty { get; set; } = false;

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var input = value?.ToString()?.Trim();

            if (string.IsNullOrEmpty(input))
                return AllowEmpty
                    ? ValidationResult.ValidResult
                    : new ValidationResult(false, "Value cannot be empty");

            if (!double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
                return new ValidationResult(false, $"'{input}' is not a valid number");

            if (number < Minimum)
                return new ValidationResult(false, $"Value must be ≥ {Minimum}");

            if (number > Maximum)
                return new ValidationResult(false, $"Value must be ≤ {Maximum}");

            return ValidationResult.ValidResult;
        }
    }
}
