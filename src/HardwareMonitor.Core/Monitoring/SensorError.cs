namespace HardwareMonitor.Core.Monitoring;

public sealed record SensorError
{
    public SensorError(SensorErrorCode code, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        Code = code;
        Message = message;
    }

    public SensorErrorCode Code { get; }
    public string Message { get; }
}
