using System.Collections.Immutable;

namespace HardwareMonitor.Core.Monitoring;

public sealed class SensorReadBatch
{
    public SensorReadBatch(
        IEnumerable<SensorReading> readings,
        IEnumerable<MonitoringIssue>? issues = null)
    {
        ArgumentNullException.ThrowIfNull(readings);

        Readings = ImmutableArray.CreateRange(readings);
        Issues = issues is null ? [] : ImmutableArray.CreateRange(issues);
    }

    public ImmutableArray<SensorReading> Readings { get; }
    public ImmutableArray<MonitoringIssue> Issues { get; }
}
