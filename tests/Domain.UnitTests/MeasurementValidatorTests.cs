using Domain;

namespace Domain.UnitTests;

public class MeasurementValidatorTests
{
    private static Measurement Valid() => new(
        MeasurementId: Guid.NewGuid(),
        Timestamp: DateTimeOffset.UtcNow,
        DeviceId: "device-1",
        PatientId: "patient-1",
        Type: "ECG",
        Value: 42,
        Unit: "bpm");

    [Fact]
    public void Validate_ValidMeasurement_ReturnsNoErrors()
    {
        var m = Valid();

        MeasurementValidator.Validate(m).Should().BeEmpty();
        MeasurementValidator.IsValid(m).Should().BeTrue();
    }

    // Each case breaks one required field; we assert exactly one error pointing at
    // that field. The data is built here (not in [InlineData]) because Guid/record
    // values are not compile-time constants.
    public static TheoryData<Measurement, string> InvalidFieldCases() => new()
    {
        { Valid() with { MeasurementId = Guid.Empty }, nameof(Measurement.MeasurementId) },
        { Valid() with { Timestamp = default }, nameof(Measurement.Timestamp) },
        { Valid() with { DeviceId = null! }, nameof(Measurement.DeviceId) },
        { Valid() with { DeviceId = "" }, nameof(Measurement.DeviceId) },
        { Valid() with { DeviceId = "   " }, nameof(Measurement.DeviceId) },
        { Valid() with { Type = null! }, nameof(Measurement.Type) },
        { Valid() with { Type = "" }, nameof(Measurement.Type) },
        { Valid() with { Type = "   " }, nameof(Measurement.Type) },
    };

    [Theory]
    [MemberData(nameof(InvalidFieldCases))]
    public void Validate_SingleBadField_ReturnsOneErrorForThatField(Measurement m, string expectedField)
    {
        var errors = MeasurementValidator.Validate(m);

        errors.Should().ContainSingle()
            .Which.Field.Should().Be(expectedField);
        MeasurementValidator.IsValid(m).Should().BeFalse();
    }

    [Fact]
    public void Validate_AllRequiredFieldsBad_ReturnsErrorPerField()
    {
        var m = Valid() with
        {
            MeasurementId = Guid.Empty,
            Timestamp = default,
            DeviceId = " ",
            Type = "",
        };

        MeasurementValidator.Validate(m).Select(e => e.Field).Should().BeEquivalentTo(
        [
            nameof(Measurement.MeasurementId),
            nameof(Measurement.Timestamp),
            nameof(Measurement.DeviceId),
            nameof(Measurement.Type),
        ]);
    }
}
