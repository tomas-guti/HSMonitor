using System.Collections.Concurrent;
using HardwareMonitor.Core.Configuration;
using HardwareMonitor.Core.Monitoring;

namespace HardwareMonitor.Tests.Monitoring;

public sealed class MonitoringServiceTests
{
    [Fact]
    public async Task PublishesOneImmutableSnapshotAndStopsWithoutOverlappingReads()
    {
        var source = new FakeMonitoringSource((timestamp, _) => CreateBatch(timestamp));
        await using var service = new MonitoringService(source);
        var callerThreadId = Environment.CurrentManagedThreadId;

        await service.StartAsync();
        await source.WaitForReadAsync();
        await WaitForSnapshotAsync(service, snapshot => snapshot.Sequence == 1);
        await service.StopAsync();

        var snapshot = service.CurrentSnapshot;
        Assert.Equal(MonitoringStatus.Stopped, snapshot.Status);
        Assert.Equal(1, snapshot.Sequence);
        Assert.Equal(1, source.ReadCount);
        Assert.Equal(1, source.MaximumConcurrentReads);
        Assert.NotEqual(callerThreadId, source.ReadThreadIds.Single());
        Assert.Single(snapshot.Readings);
        Assert.Equal(SensorAvailability.Available, snapshot.Readings[0].Availability);
        Assert.Equal(snapshot.Timestamp, snapshot.Readings[0].Timestamp);
    }

    [Fact]
    public async Task ReportsAnEmptySuccessfulScanAsNoSensorsDetected()
    {
        var source = new FakeMonitoringSource((_, _) => new SensorReadBatch([]));
        await using var service = new MonitoringService(source);

        await service.StartAsync();
        await source.WaitForReadAsync();
        await WaitForSnapshotAsync(service, snapshot => snapshot.Sequence == 1);
        await service.StopAsync();

        Assert.Contains(
            service.CurrentSnapshot.Issues,
            issue => issue.Code == MonitoringIssueCode.NoSensorsDetected);
    }

    [Fact]
    public async Task PublishesSourceExceptionsAndPropagatesThemOnStop()
    {
        var source = new FakeMonitoringSource(
            (_, _) => throw new InvalidOperationException("source failed"));
        var service = new MonitoringService(source);

        await service.StartAsync();
        await WaitForSnapshotAsync(
            service,
            snapshot => snapshot.Status == MonitoringStatus.Faulted);
        Assert.Contains(
            service.CurrentSnapshot.Issues,
            issue => issue.Code == MonitoringIssueCode.SourceFailure);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.StopAsync());
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await service.DisposeAsync());
        Assert.True(source.IsDisposed);
    }

    [Fact]
    public async Task StopsAnInProgressReadBeforeDisposingTheSource()
    {
        var source = new BlockingMonitoringSource();
        var service = new MonitoringService(source);

        await service.StartAsync();
        await source.ReadStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var stopTask = service.StopAsync();
        Assert.False(stopTask.IsCompleted);

        source.AllowReadToFinish.TrySetResult();
        await stopTask;
        Assert.Equal(MonitoringStatus.Stopped, service.CurrentSnapshot.Status);
        await service.DisposeAsync();
        Assert.True(source.IsDisposed);
    }

    [Fact]
    public async Task RejectsAnInvalidSamplingInterval()
    {
        var source = new FakeMonitoringSource((timestamp, _) => CreateBatch(timestamp));

        Assert.Throws<ArgumentOutOfRangeException>(() => new MonitoringService(
            source,
            new MonitoringOptions { SamplingIntervalMilliseconds = 249 }));
        Assert.False(source.IsDisposed);
        source.Dispose();
    }

    [Fact]
    public async Task DoesNotStartTheNextCycleBeforeTheConfiguredInterval()
    {
        var source = new FakeMonitoringSource((timestamp, _) => CreateBatch(timestamp));
        var options = new MonitoringOptions { SamplingIntervalMilliseconds = 250 };
        await using var service = new MonitoringService(source, options);

        await service.StartAsync();
        await source.WaitForReadAsync();
        await Task.Delay(75);
        Assert.Equal(1, source.ReadCount);

        await source.WaitForReadCountAsync(2);
        await WaitForSnapshotAsync(service, snapshot => snapshot.Sequence == 2);
        await service.StopAsync();
        Assert.Equal(2, source.ReadCount);
    }

    private static SensorReadBatch CreateBatch(DateTimeOffset timestamp)
    {
        return new SensorReadBatch(
        [
            new SensorReading(
                new SensorId("/cpu/0/temperature/0"),
                new ComponentId("/cpu/0"),
                HardwareComponent.Cpu,
                "CPU Package",
                Metric.Temperature,
                42,
                MeasurementUnit.Celsius,
                SensorAvailability.Available,
                timestamp)
        ]);
    }

    private static async Task WaitForSnapshotAsync(
        MonitoringService service,
        Func<MonitoringSnapshot, bool> predicate)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        while (!predicate(service.CurrentSnapshot))
        {
            await Task.Delay(10, timeout.Token);
        }
    }

    private sealed class FakeMonitoringSource(
        Func<DateTimeOffset, CancellationToken, SensorReadBatch> read) : IMonitoringSource
    {
        private readonly ConcurrentQueue<int> _readThreadIds = new();
        private readonly TaskCompletionSource _readStarted =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _activeReads;
        private int _maximumConcurrentReads;
        private int _readCount;

        public int ReadCount => Volatile.Read(ref _readCount);
        public int MaximumConcurrentReads => Volatile.Read(ref _maximumConcurrentReads);
        public IEnumerable<int> ReadThreadIds => _readThreadIds;
        public bool IsDisposed { get; private set; }

        public SensorReadBatch Read(DateTimeOffset timestamp, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Interlocked.Increment(ref _readCount);
            _readThreadIds.Enqueue(Environment.CurrentManagedThreadId);
            var active = Interlocked.Increment(ref _activeReads);
            UpdateMaximum(active);
            _readStarted.TrySetResult();
            try
            {
                return read(timestamp, cancellationToken);
            }
            finally
            {
                Interlocked.Decrement(ref _activeReads);
            }
        }

        public Task WaitForReadAsync() => _readStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));

        public async Task WaitForReadCountAsync(int count)
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            while (ReadCount < count)
            {
                await Task.Delay(10, timeout.Token);
            }
        }

        public void Dispose() => IsDisposed = true;

        private void UpdateMaximum(int active)
        {
            int observed;
            do
            {
                observed = Volatile.Read(ref _maximumConcurrentReads);
                if (active <= observed)
                {
                    return;
                }
            }
            while (Interlocked.CompareExchange(ref _maximumConcurrentReads, active, observed) != observed);
        }
    }

    private sealed class BlockingMonitoringSource : IMonitoringSource
    {
        public TaskCompletionSource ReadStarted { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource AllowReadToFinish { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        public bool IsDisposed { get; private set; }

        public SensorReadBatch Read(DateTimeOffset timestamp, CancellationToken cancellationToken)
        {
            ReadStarted.TrySetResult();
            AllowReadToFinish.Task.GetAwaiter().GetResult();
            return CreateBatch(timestamp);
        }

        public void Dispose() => IsDisposed = true;
    }
}
