namespace HardwareMonitor.Core.Monitoring;

public readonly record struct SensorId
{
    public SensorId(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;
}
