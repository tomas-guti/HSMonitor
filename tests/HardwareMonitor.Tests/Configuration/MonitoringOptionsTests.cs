using HardwareMonitor.Core.Configuration;

namespace HardwareMonitor.Tests.Configuration;

public sealed class MonitoringOptionsTests
{
    [Fact]
    public void DefaultsToOneSecondSamplingInterval()
    {
        var options = new MonitoringOptions();

        Assert.Equal(1_000, options.SamplingIntervalMilliseconds);
        options.Validate();
    }

    [Theory]
    [InlineData(250)]
    [InlineData(1_000)]
    [InlineData(60_000)]
    public void AcceptsIntervalsWithinSupportedRange(int intervalMilliseconds)
    {
        var options = new MonitoringOptions
        {
            SamplingIntervalMilliseconds = intervalMilliseconds
        };

        options.Validate();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(249)]
    [InlineData(60_001)]
    public void RejectsIntervalsOutsideSupportedRange(int intervalMilliseconds)
    {
        var options = new MonitoringOptions
        {
            SamplingIntervalMilliseconds = intervalMilliseconds
        };

        Assert.Throws<ArgumentOutOfRangeException>(options.Validate);
    }
}
