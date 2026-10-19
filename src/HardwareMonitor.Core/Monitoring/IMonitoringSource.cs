namespace HardwareMonitor.Core.Monitoring;

public interface IMonitoringSource : IDisposable
{
    SensorReadBatch Read(DateTimeOffset timestamp, CancellationToken cancellationToken);
}
