using System.Collections.Immutable;
using HardwareMonitor.Core.Configuration;

namespace HardwareMonitor.Core.Monitoring;

public sealed class MonitoringService : IAsyncDisposable
{
    private readonly object _lifecycleGate = new();
    private readonly IMonitoringSource _source;
    private readonly TimeSpan _samplingInterval;
    private readonly TimeProvider _timeProvider;
    private MonitoringSnapshot _currentSnapshot;
    private CancellationTokenSource? _shutdown;
    private Task? _worker;
    private bool _disposed;

    public MonitoringService(
        IMonitoringSource source,
        MonitoringOptions? options = null,
        TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(source);

        options ??= new MonitoringOptions();
        options.Validate();
        _source = source;
        _samplingInterval = TimeSpan.FromMilliseconds(options.SamplingIntervalMilliseconds);
        _timeProvider = timeProvider ?? TimeProvider.System;
        _currentSnapshot = new MonitoringSnapshot(
            0,
            DateTimeOffset.MinValue,
            MonitoringStatus.NotStarted,
            ImmutableArray<SensorReading>.Empty,
            ImmutableArray<MonitoringIssue>.Empty);
    }

    public MonitoringSnapshot CurrentSnapshot => Volatile.Read(ref _currentSnapshot);

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_lifecycleGate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_worker is not null)
            {
                throw new InvalidOperationException("The monitoring service can only be started once.");
            }

            _shutdown = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            PublishStatus(MonitoringStatus.Running);
            _worker = Task.Run(() => RunAsync(_shutdown.Token), CancellationToken.None);
        }

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        Task? worker;
        lock (_lifecycleGate)
        {
            _shutdown?.Cancel();
            worker = _worker;
        }

        if (worker is not null)
        {
            await worker.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public async ValueTask DisposeAsync()
    {
        lock (_lifecycleGate)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
        }

        try
        {
            await StopAsync().ConfigureAwait(false);
        }
        finally
        {
            try
            {
                _source.Dispose();
            }
            finally
            {
                _shutdown?.Dispose();
            }
        }
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var timestamp = _timeProvider.GetUtcNow();
                var batch = _source.Read(timestamp, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();

                var issues = batch.Issues;
                if (batch.Readings.IsEmpty && issues.IsEmpty)
                {
                    issues = [new MonitoringIssue(
                        MonitoringIssueCode.NoSensorsDetected,
                        "No hardware sensors are currently available.")];
                }

                var next = new MonitoringSnapshot(
                    CurrentSnapshot.Sequence + 1,
                    timestamp,
                    MonitoringStatus.Running,
                    batch.Readings,
                    issues);
                Volatile.Write(ref _currentSnapshot, next);

                await Task.Delay(_samplingInterval, _timeProvider, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            PublishStatus(MonitoringStatus.Stopped);
        }
        catch (Exception exception)
        {
            PublishFailure(exception);
            throw;
        }
    }

    private void PublishStatus(MonitoringStatus status)
    {
        var current = CurrentSnapshot;
        Volatile.Write(
            ref _currentSnapshot,
            current with { Status = status });
    }

    private void PublishFailure(Exception exception)
    {
        var current = CurrentSnapshot;
        var issues = current.Issues.Add(new MonitoringIssue(
            MonitoringIssueCode.SourceFailure,
            $"The monitoring source failed: {exception.Message}"));
        Volatile.Write(
            ref _currentSnapshot,
            current with
            {
                Status = MonitoringStatus.Faulted,
                Issues = issues
            });
    }
}
