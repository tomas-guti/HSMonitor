using HardwareMonitor.Core.Monitoring;
using HardwareMonitor.Hardware;
using LibreHardwareMonitor.Hardware;

namespace HardwareMonitor.Tests.Monitoring;

public sealed class SensorReadingMapperTests
{
    private static readonly DateTimeOffset Timestamp =
        new(2026, 10, 19, 12, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(SensorType.Voltage, Metric.Voltage, MeasurementUnit.Volt)]
    [InlineData(SensorType.Current, Metric.Current, MeasurementUnit.Ampere)]
    [InlineData(SensorType.Power, Metric.Power, MeasurementUnit.Watt)]
    [InlineData(SensorType.Clock, Metric.Clock, MeasurementUnit.Megahertz)]
    [InlineData(SensorType.Temperature, Metric.Temperature, MeasurementUnit.Celsius)]
    [InlineData(SensorType.Load, Metric.Load, MeasurementUnit.Percent)]
    [InlineData(SensorType.Frequency, Metric.Frequency, MeasurementUnit.Hertz)]
    [InlineData(SensorType.Fan, Metric.FanSpeed, MeasurementUnit.RevolutionsPerMinute)]
    [InlineData(SensorType.Flow, Metric.Flow, MeasurementUnit.LiterPerHour)]
    [InlineData(SensorType.Control, Metric.Control, MeasurementUnit.Percent)]
    [InlineData(SensorType.Level, Metric.Level, MeasurementUnit.Percent)]
    [InlineData(SensorType.Factor, Metric.Factor, MeasurementUnit.Unitless)]
    [InlineData(SensorType.Data, Metric.Data, MeasurementUnit.Gibibyte)]
    [InlineData(SensorType.SmallData, Metric.SmallData, MeasurementUnit.Mebibyte)]
    [InlineData(SensorType.Throughput, Metric.Throughput, MeasurementUnit.BytesPerSecond)]
    [InlineData(SensorType.TimeSpan, Metric.TimeSpan, MeasurementUnit.Second)]
    [InlineData(SensorType.Timing, Metric.Timing, MeasurementUnit.Nanosecond)]
    [InlineData(SensorType.Energy, Metric.Energy, MeasurementUnit.MilliwattHour)]
    [InlineData(SensorType.Noise, Metric.Noise, MeasurementUnit.DecibelA)]
    [InlineData(SensorType.Conductivity, Metric.Conductivity, MeasurementUnit.MicrosiemensPerCentimeter)]
    [InlineData(SensorType.Humidity, Metric.Humidity, MeasurementUnit.RelativeHumidityPercent)]
    public void MapsProviderMetricsAndUnits(SensorType sensorType, Metric metric, MeasurementUnit unit)
    {
        var reading = SensorReadingMapper.Map(
            new SensorId("/cpu/0/sensor/0"),
            new ComponentId("/cpu/0"),
            HardwareComponent.Cpu,
            "Sensor",
            sensorType,
            0,
            Timestamp);

        Assert.Equal(metric, reading.Metric);
        Assert.Equal(unit, reading.Unit);
        Assert.Equal(SensorAvailability.Available, reading.Availability);
        Assert.Equal(0, reading.Value);
    }

    [Fact]
    public void UsesTypedFallbacksForUnrecognizedProviderMetric()
    {
        var reading = SensorReadingMapper.Map(
            new SensorId("/cpu/0/sensor/0"),
            new ComponentId("/cpu/0"),
            HardwareComponent.Cpu,
            " ",
            (SensorType)int.MaxValue,
            12,
            Timestamp);

        Assert.Equal(Metric.Other, reading.Metric);
        Assert.Equal(MeasurementUnit.Unknown, reading.Unit);
        Assert.Equal(nameof(Metric.Other), reading.Name);
    }

    [Fact]
    public void MapsMissingProviderValueToUnavailableInsteadOfZero()
    {
        var reading = SensorReadingMapper.Map(
            new SensorId("/cpu/0/sensor/0"),
            new ComponentId("/cpu/0"),
            HardwareComponent.Cpu,
            "Sensor",
            SensorType.Temperature,
            null,
            Timestamp);

        Assert.Equal(SensorAvailability.Unavailable, reading.Availability);
        Assert.Equal(SensorUnavailableReason.ValueNotReported, reading.UnavailableReason);
        Assert.Null(reading.Value);
    }

    [Fact]
    public void MapsNonFiniteProviderValueToStructuredError()
    {
        var reading = SensorReadingMapper.Map(
            new SensorId("/cpu/0/sensor/0"),
            new ComponentId("/cpu/0"),
            HardwareComponent.Cpu,
            "Sensor",
            SensorType.Temperature,
            float.NaN,
            Timestamp);

        Assert.Equal(SensorAvailability.Error, reading.Availability);
        Assert.Equal(SensorErrorCode.InvalidValue, reading.Error?.Code);
    }
}
