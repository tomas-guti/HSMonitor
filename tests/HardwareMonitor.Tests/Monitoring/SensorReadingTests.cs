using HardwareMonitor.Core.Monitoring;

namespace HardwareMonitor.Tests.Monitoring;

public sealed class SensorReadingTests
{
    private static readonly DateTimeOffset Timestamp =
        new(2026, 10, 19, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void AcceptsZeroAsAvailableMeasurement()
    {
        var reading = new SensorReading(
            new SensorId("/cpu/0/temperature/0"),
            new ComponentId("/cpu/0"),
            HardwareComponent.Cpu,
            "CPU Package",
            Metric.Temperature,
            0,
            MeasurementUnit.Celsius,
            SensorAvailability.Available,
            Timestamp);

        Assert.Equal(0, reading.Value);
        Assert.Equal(Timestamp, reading.Timestamp);
    }

    [Fact]
    public void RequiresAReasonForUnavailableValues()
    {
        var reading = new SensorReading(
            new SensorId("/cpu/0/load/0"),
            new ComponentId("/cpu/0"),
            HardwareComponent.Cpu,
            "CPU Load",
            Metric.Load,
            null,
            MeasurementUnit.Percent,
            SensorAvailability.Unavailable,
            Timestamp,
            SensorUnavailableReason.ValueNotReported);

        Assert.Null(reading.Value);
        Assert.Equal(SensorUnavailableReason.ValueNotReported, reading.UnavailableReason);
    }

    [Fact]
    public void RequiresStructuredErrorForErrorAvailability()
    {
        var error = new SensorError(SensorErrorCode.AccessDenied, "Permission denied.");
        var reading = new SensorReading(
            new SensorId("/gpu/0/temperature/0"),
            new ComponentId("/gpu/0"),
            HardwareComponent.Gpu,
            "GPU Temperature",
            Metric.Temperature,
            null,
            MeasurementUnit.Celsius,
            SensorAvailability.Error,
            Timestamp,
            error: error);

        Assert.Same(error, reading.Error);
        Assert.Null(reading.Value);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void RejectsNonFiniteAvailableValues(double value)
    {
        Assert.Throws<ArgumentException>(() => new SensorReading(
            new SensorId("/cpu/0/load/0"),
            new ComponentId("/cpu/0"),
            HardwareComponent.Cpu,
            "CPU Load",
            Metric.Load,
            value,
            MeasurementUnit.Percent,
            SensorAvailability.Available,
            Timestamp));
    }

    [Fact]
    public void KeepsSameNamedSensorsDistinctByComponentAndIdentifier()
    {
        var cpu = CreateReading("/cpu/0/temperature/0", "/cpu/0");
        var gpu = CreateReading("/gpu/0/temperature/0", "/gpu/0");

        Assert.Equal(cpu.Name, gpu.Name);
        Assert.NotEqual(cpu.Id, gpu.Id);
        Assert.NotEqual(cpu.ComponentId, gpu.ComponentId);
    }

    private static SensorReading CreateReading(string sensorId, string componentId) =>
        new(
            new SensorId(sensorId),
            new ComponentId(componentId),
            HardwareComponent.Other,
            "Temperature",
            Metric.Temperature,
            35,
            MeasurementUnit.Celsius,
            SensorAvailability.Available,
            Timestamp);
}
