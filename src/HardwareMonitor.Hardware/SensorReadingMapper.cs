using HardwareMonitor.Core.Monitoring;
using LibreHardwareMonitor.Hardware;

namespace HardwareMonitor.Hardware;

internal static class SensorReadingMapper
{
    public static SensorReading Map(
        SensorId id,
        ComponentId componentId,
        HardwareComponent component,
        string name,
        SensorType sensorType,
        float? value,
        DateTimeOffset timestamp)
    {
        var (metric, unit) = MapType(sensorType);
        name = string.IsNullOrWhiteSpace(name) ? metric.ToString() : name;
        if (value is null)
        {
            return new SensorReading(
                id,
                componentId,
                component,
                name,
                metric,
                null,
                unit,
                SensorAvailability.Unavailable,
                timestamp,
                SensorUnavailableReason.ValueNotReported);
        }

        if (!float.IsFinite(value.Value))
        {
            return CreateError(
                id,
                componentId,
                component,
                name,
                sensorType,
                timestamp,
                new SensorError(SensorErrorCode.InvalidValue, "The sensor returned a non-finite value."));
        }

        return new SensorReading(
            id,
            componentId,
            component,
            name,
            metric,
            value.Value,
            unit,
            SensorAvailability.Available,
            timestamp);
    }

    public static SensorReading CreateError(
        SensorId id,
        ComponentId componentId,
        HardwareComponent component,
        string name,
        SensorType sensorType,
        DateTimeOffset timestamp,
        SensorError error)
    {
        var (metric, unit) = MapType(sensorType);
        name = string.IsNullOrWhiteSpace(name) ? metric.ToString() : name;
        return new SensorReading(
            id,
            componentId,
            component,
            name,
            metric,
            null,
            unit,
            SensorAvailability.Error,
            timestamp,
            error: error);
    }

    private static (Metric Metric, MeasurementUnit Unit) MapType(SensorType sensorType) =>
        sensorType switch
        {
            SensorType.Voltage => (Metric.Voltage, MeasurementUnit.Volt),
            SensorType.Current => (Metric.Current, MeasurementUnit.Ampere),
            SensorType.Power => (Metric.Power, MeasurementUnit.Watt),
            SensorType.Clock => (Metric.Clock, MeasurementUnit.Megahertz),
            SensorType.Temperature => (Metric.Temperature, MeasurementUnit.Celsius),
            SensorType.Load => (Metric.Load, MeasurementUnit.Percent),
            SensorType.Frequency => (Metric.Frequency, MeasurementUnit.Hertz),
            SensorType.Fan => (Metric.FanSpeed, MeasurementUnit.RevolutionsPerMinute),
            SensorType.Flow => (Metric.Flow, MeasurementUnit.LiterPerHour),
            SensorType.Control => (Metric.Control, MeasurementUnit.Percent),
            SensorType.Level => (Metric.Level, MeasurementUnit.Percent),
            SensorType.Factor => (Metric.Factor, MeasurementUnit.Unitless),
            SensorType.Data => (Metric.Data, MeasurementUnit.Gibibyte),
            SensorType.SmallData => (Metric.SmallData, MeasurementUnit.Mebibyte),
            SensorType.Throughput => (Metric.Throughput, MeasurementUnit.BytesPerSecond),
            SensorType.TimeSpan => (Metric.TimeSpan, MeasurementUnit.Second),
            SensorType.Timing => (Metric.Timing, MeasurementUnit.Nanosecond),
            SensorType.Energy => (Metric.Energy, MeasurementUnit.MilliwattHour),
            SensorType.Noise => (Metric.Noise, MeasurementUnit.DecibelA),
            SensorType.Conductivity => (Metric.Conductivity, MeasurementUnit.MicrosiemensPerCentimeter),
            SensorType.Humidity => (Metric.Humidity, MeasurementUnit.RelativeHumidityPercent),
            _ => (Metric.Other, MeasurementUnit.Unknown)
        };
}
