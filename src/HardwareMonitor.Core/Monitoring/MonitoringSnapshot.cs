using System.Collections.Immutable;

namespace HardwareMonitor.Core.Monitoring;

public sealed record MonitoringSnapshot(
    long Sequence,
    DateTimeOffset Timestamp,
    MonitoringStatus Status,
    ImmutableArray<SensorReading> Readings,
    ImmutableArray<MonitoringIssue> Issues);
