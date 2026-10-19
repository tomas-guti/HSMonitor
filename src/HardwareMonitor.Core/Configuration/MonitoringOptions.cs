namespace HardwareMonitor.Core.Configuration;

public sealed record MonitoringOptions
{
    public const int DefaultSamplingIntervalMilliseconds = 1_000;
    public const int MinimumSamplingIntervalMilliseconds = 250;
    public const int MaximumSamplingIntervalMilliseconds = 60_000;

    public int SamplingIntervalMilliseconds { get; init; } =
        DefaultSamplingIntervalMilliseconds;

    public void Validate()
    {
        if (SamplingIntervalMilliseconds is < MinimumSamplingIntervalMilliseconds
            or > MaximumSamplingIntervalMilliseconds)
        {
            throw new ArgumentOutOfRangeException(
                nameof(SamplingIntervalMilliseconds),
                SamplingIntervalMilliseconds,
                $"El intervalo debe estar entre {MinimumSamplingIntervalMilliseconds} y " +
                $"{MaximumSamplingIntervalMilliseconds} milisegundos.");
        }
    }
}
