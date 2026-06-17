namespace Domain;

public static class MeasurementValidator
{
    private const string NonEmptyFieldErrorMessage = "Field should not be empty";

    /// <summary>
    /// Returns one <see cref="ValidationError"/> per failed rule (field + reason).
    /// An empty list means the measurement is valid. This is the single place that
    /// knows the rules, so callers never hardcode field names or reasons.
    /// </summary>
    public static IReadOnlyList<ValidationError> Validate(Measurement m)
    {
        var errors = new List<ValidationError>();

        if (m.MeasurementId == Guid.Empty)
        {
            errors.Add(new ValidationError(nameof(Measurement.MeasurementId), NonEmptyFieldErrorMessage));
        }
        if (m.Timestamp == default)
        {
            errors.Add(new ValidationError(nameof(Measurement.Timestamp), NonEmptyFieldErrorMessage));
        }
        if (string.IsNullOrWhiteSpace(m.DeviceId))
        {
            errors.Add(new ValidationError(nameof(Measurement.DeviceId), NonEmptyFieldErrorMessage));
        }
        if (string.IsNullOrWhiteSpace(m.Type))
        {
            errors.Add(new ValidationError(nameof(Measurement.Type), NonEmptyFieldErrorMessage));
        }

        return errors;
    }

    public static bool IsValid(Measurement m) => Validate(m).Count == 0;
}
