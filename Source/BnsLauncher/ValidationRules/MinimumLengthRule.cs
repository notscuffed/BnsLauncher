using System.Globalization;
using System.Windows.Controls;

namespace BnsLauncher.ValidationRules
{
    public class MinimumLengthRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (!(value is string stringValue))
                return new ValidationResult(false, "Required field");
            
            if (string.IsNullOrWhiteSpace(stringValue))
                return new ValidationResult(false, "Required field");
            
            if (stringValue.Length < MinimumLength)
                return new ValidationResult(false, $"Minimum {MinimumLength} characters");
            
            return new ValidationResult(true, null);
        }
        
        public int MinimumLength { get; set; }
    }
}