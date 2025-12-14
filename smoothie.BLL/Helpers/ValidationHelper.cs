using System.ComponentModel.DataAnnotations;

namespace smoothie.BLL.Helpers;

/// <summary>
/// Provides helper methods for object validation using data annotations.
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// Validates an object using data annotations and throws an exception if validation fails.
    /// </summary>
    /// <param name="obj">The object to validate.</param>
    /// <param name="paramName">The name of the parameter (used in exception message). If null, uses "obj".</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="obj"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if the object fails validation. The exception message contains all validation errors.</exception>
    /// <example>
    /// <code>
    /// var foo = new Foo { Name = "Test" };
    /// ValidationHelper.ValidateOrThrow(foo, nameof(foo));
    /// </code>
    /// </example>
    public static void ValidateOrThrow(object obj, string? paramName = null) {
        if (obj is null)
            throw new ArgumentNullException(paramName ?? nameof(obj));

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(obj);
        bool isValid = Validator.TryValidateObject(
            obj,
            validationContext,
            validationResults,
            validateAllProperties: true
        );

        if (!isValid) {
            var errors = string.Join("; ", validationResults.Select(r => r.ErrorMessage));
            throw new ArgumentException($"Validation failed: {errors}", paramName ?? nameof(obj));
        }
    }
}