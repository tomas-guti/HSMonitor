namespace HardwareMonitor.Core.Monitoring;

public sealed record MonitoringIssue(
    MonitoringIssueCode Code,
    string Message,
    HardwareComponent? Component = null,
    ComponentId? ComponentId = null);
