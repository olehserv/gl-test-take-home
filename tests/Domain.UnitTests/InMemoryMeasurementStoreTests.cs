using Domain;

namespace Domain.UnitTests;

public class InMemoryMeasurementStoreTests
{
    private static Measurement At(DateTimeOffset timestamp, string type = "ECG") => new(
        MeasurementId: Guid.NewGuid(),
        Timestamp: timestamp,
        DeviceId: "device-1",
        PatientId: "patient-1",
        Type: type,
        Value: 42,
        Unit: "bpm");

    [Fact]
    public async Task QueryAsync_ReturnsAddedMeasurement()
    {
        var store = new InMemoryMeasurementStore();
        var m = At(DateTimeOffset.UtcNow);
        await store.AddAsync(m);

        var result = await store.QueryAsync(type: null, since: DateTimeOffset.UtcNow.AddMinutes(-1));

        result.Should().ContainSingle().Which.Should().Be(m);
    }

    [Fact]
    public async Task QueryAsync_ExcludesMeasurementsOlderThanSince()
    {
        var store = new InMemoryMeasurementStore();
        var now = DateTimeOffset.UtcNow;
        await store.AddAsync(At(now.AddHours(-2)));
        var recent = At(now);
        await store.AddAsync(recent);

        var result = await store.QueryAsync(type: null, since: now.AddHours(-1));

        result.Should().ContainSingle().Which.Should().Be(recent);
    }

    [Fact]
    public async Task QueryAsync_TypeFilterIsCaseInsensitive()
    {
        var store = new InMemoryMeasurementStore();
        var now = DateTimeOffset.UtcNow;
        await store.AddAsync(At(now, type: "ECG"));
        await store.AddAsync(At(now, type: "SpO2"));

        var result = await store.QueryAsync(type: "ecg", since: now.AddMinutes(-1));

        result.Should().ContainSingle().Which.Type.Should().Be("ECG");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task QueryAsync_NullOrWhitespaceType_DoesNotFilterByType(string? type)
    {
        var store = new InMemoryMeasurementStore();
        var now = DateTimeOffset.UtcNow;
        await store.AddAsync(At(now, type: "ECG"));
        await store.AddAsync(At(now, type: "SpO2"));

        var result = await store.QueryAsync(type, since: now.AddMinutes(-1));

        result.Should().HaveCount(2);
    }
}
