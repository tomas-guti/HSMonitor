using System.Collections.Immutable;
using System.ComponentModel;
using System.IO;
using HardwareMonitor.Core.Monitoring;
using LibreHardwareMonitor.Hardware;

namespace HardwareMonitor.Hardware;

public sealed class LibreHardwareMonitorSource : IMonitoringSource
{
    private readonly Computer _computer = new()
    {
        IsBatteryEnabled = true,
        IsControllerEnabled = true,
        IsCpuEnabled = true,
        IsGpuEnabled = true,
        IsMemoryEnabled = true,
        IsMotherboardEnabled = true,
        IsNetworkEnabled = true,
        IsPowerMonitorEnabled = true,
        IsPsuEnabled = true,
        IsStorageEnabled = true
    };

    private bool _isOpen;
    private bool _disposed;

    public SensorReadBatch Read(DateTimeOffset timestamp, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();

        var readings = ImmutableArray.CreateBuilder<SensorReading>();
        var issues = ImmutableArray.CreateBuilder<MonitoringIssue>();
        try
        {
            if (!_isOpen)
            {
                _computer.Open();
                _isOpen = true;
            }
        }
        catch (Exception exception) when (IsHardwareAccessException(exception))
        {
            issues.Add(CreateIssue(
                exception,
                null,
                null,
                MonitoringIssueCode.HardwareUnavailable));
            return new SensorReadBatch(readings, issues);
        }

        foreach (var hardware in _computer.Hardware)
        {
            ReadHardware(hardware, timestamp, cancellationToken, readings, issues);
        }

        if (readings.Count == 0 && issues.Count == 0)
        {
            issues.Add(new MonitoringIssue(
                MonitoringIssueCode.NoSensorsDetected,
                "LibreHardwareMonitor did not report any sensors for the enabled hardware."));
        }

        return new SensorReadBatch(readings, issues);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        if (_isOpen)
        {
            _computer.Close();
        }
    }

    private static void ReadHardware(
        IHardware hardware,
        DateTimeOffset timestamp,
        CancellationToken cancellationToken,
        ImmutableArray<SensorReading>.Builder readings,
        ImmutableArray<MonitoringIssue>.Builder issues)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var component = MapComponent(hardware.HardwareType);
        var componentId = new ComponentId(hardware.Identifier.ToString());
        SensorError? updateError = null;
        try
        {
            hardware.Update();
        }
        catch (Exception exception) when (IsHardwareAccessException(exception))
        {
            updateError = CreateSensorError(exception, SensorErrorCode.UpdateFailed);
            issues.Add(CreateIssue(
                exception,
                component,
                componentId,
                MonitoringIssueCode.HardwareUpdateFailed));
        }

        foreach (var sensor in hardware.Sensors)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var sensorId = new SensorId(sensor.Identifier.ToString());
            if (updateError is not null)
            {
                readings.Add(SensorReadingMapper.CreateError(
                    sensorId,
                    componentId,
                    component,
                    sensor.Name,
                    sensor.SensorType,
                    timestamp,
                    updateError));
                continue;
            }

            try
            {
                readings.Add(SensorReadingMapper.Map(
                    sensorId,
                    componentId,
                    component,
                    sensor.Name,
                    sensor.SensorType,
                    sensor.Value,
                    timestamp));
            }
            catch (Exception exception) when (IsHardwareAccessException(exception))
            {
                readings.Add(SensorReadingMapper.CreateError(
                    sensorId,
                    componentId,
                    component,
                    sensor.Name,
                    sensor.SensorType,
                    timestamp,
                    CreateSensorError(exception, SensorErrorCode.ReadFailed)));
            }
        }

        foreach (var child in hardware.SubHardware)
        {
            ReadHardware(child, timestamp, cancellationToken, readings, issues);
        }
    }

    private static MonitoringIssue CreateIssue(
        Exception exception,
        HardwareComponent? component,
        ComponentId? componentId,
        MonitoringIssueCode failureCode)
    {
        return new MonitoringIssue(
            IsAccessDenied(exception) ? MonitoringIssueCode.AccessDenied : failureCode,
            exception.Message,
            component,
            componentId);
    }

    private static SensorError CreateSensorError(Exception exception, SensorErrorCode fallbackCode)
    {
        var code = IsAccessDenied(exception)
                ? SensorErrorCode.AccessDenied
                : exception is IOException or PlatformNotSupportedException
                    ? SensorErrorCode.DeviceUnavailable
                    : fallbackCode;

        return new SensorError(code, exception.Message);
    }

    private static bool IsAccessDenied(Exception exception) =>
        exception is UnauthorizedAccessException
            or Win32Exception { NativeErrorCode: 5 };

    private static bool IsHardwareAccessException(Exception exception) =>
        exception is UnauthorizedAccessException
            or IOException
            or InvalidOperationException
            or PlatformNotSupportedException
            or Win32Exception;

    private static HardwareComponent MapComponent(HardwareType type) =>
        type switch
        {
            HardwareType.Cpu => HardwareComponent.Cpu,
            HardwareType.GpuAmd or HardwareType.GpuIntel or HardwareType.GpuNvidia => HardwareComponent.Gpu,
            HardwareType.Memory => HardwareComponent.Memory,
            HardwareType.Motherboard => HardwareComponent.Motherboard,
            HardwareType.Storage => HardwareComponent.Storage,
            HardwareType.Network => HardwareComponent.Network,
            HardwareType.Battery => HardwareComponent.Battery,
            HardwareType.Cooler => HardwareComponent.Cooling,
            HardwareType.Psu => HardwareComponent.PowerSupply,
            HardwareType.PowerMonitor => HardwareComponent.PowerMonitor,
            HardwareType.SuperIO or HardwareType.EmbeddedController => HardwareComponent.Controller,
            _ => HardwareComponent.Other
        };
}
