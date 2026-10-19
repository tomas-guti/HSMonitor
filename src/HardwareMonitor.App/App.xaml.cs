using System.Diagnostics;
using System.Windows;

namespace HardwareMonitor.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Trace.TraceInformation("Hardware Monitor started.");
    }
}
