namespace HardwareMonitor.Core.Monitoring;

public sealed record SensorReading
{
    public SensorReading(
        SensorId id,
        ComponentId componentId,
        HardwareComponent component,
        string name,
        Metric metric,
        double? value,
        MeasurementUnit unit,
        SensorAvailability availability,
        DateTimeOffset timestamp,
        SensorUnavailableReason? unavailableReason = null,
        SensorError? error = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id.Value);
        ArgumentException.ThrowIfNullOrWhiteSpace(componentId.Value);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        switch (availability)
        {
            case SensorAvailability.Available when value is null || !double.IsFinite(value.Value):
                throw new ArgumentException("Available readings require a finite value.", nameof(value));
            case SensorAvailability.Available when unavailableReason is not null || error is not null:
                throw new ArgumentException("Available readings cannot include an unavailable reason or error.");
            case SensorAvailability.Unavailable when value is not null || unavailableReason is null || error is not null:
                throw new ArgumentException("Unavailable readings require a reason and cannot include a value or error.");
            case SensorAvailability.Error when value is not null || error is null || unavailableReason is not null:
                throw new ArgumentException("Error readings require an error and cannot include a value or unavailable reason.");
            case SensorAvailability.Available or SensorAvailability.Unavailable or SensorAvailability.Error:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(availability));
        }

        Id = id;
        ComponentId = componentId;
        Component = component;
        Name = name;
        Metric = metric;
        Value = value;
        Unit = unit;
        Availability = availability;
        Timestamp = timestamp;
        UnavailableReason = unavailableReason;
        Error = error;
    }

    public SensorId Id { get; }
    public ComponentId ComponentId { get; }
    public HardwareComponent Component { get; }
    public string Name { get; }
    public Metric Metric { get; }
    public double? Value { get; }
    public MeasurementUnit Unit { get; }
    public SensorAvailability Availability { get; }
    public DateTimeOffset Timestamp { get; }
    public SensorUnavailableReason? UnavailableReason { get; }
    public SensorError? Error { get; }
}
