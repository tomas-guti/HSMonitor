# Avisos de terceros

Inventario verificado con el nuspec oficial de NuGet y el cierre resuelto de
`HardwareMonitor.Hardware` para `net10.0` el 19 de octubre de 2026. Las licencias
de las dependencias no se aplican al código de `HardwareMonitor.Core`.

| Paquete | Versión resuelta | Licencia/metadato | Fuente |
| --- | ---: | --- | --- |
| LibreHardwareMonitorLib | 0.9.6 | MPL-2.0 | [NuGet nuspec](https://api.nuget.org/v3-flatcontainer/librehardwaremonitorlib/0.9.6/librehardwaremonitorlib.nuspec), [LICENSE upstream](https://github.com/LibreHardwareMonitor/LibreHardwareMonitor/blob/master/LICENSE) |
| DiskInfoToolkit | 1.1.2 | MPL-2.0 | [NuGet nuspec](https://api.nuget.org/v3-flatcontainer/diskinfotoolkit/1.1.2/diskinfotoolkit.nuspec) |
| BlackSharp.Core | 1.0.7 | MPL-2.0 | [NuGet nuspec](https://api.nuget.org/v3-flatcontainer/blacksharp.core/1.0.7/blacksharp.core.nuspec) |
| RAMSPDToolkit-NDD | 1.4.2 | MPL-2.0 | [NuGet nuspec](https://api.nuget.org/v3-flatcontainer/ramspdtoolkit-ndd/1.4.2/ramspdtoolkit-ndd.nuspec) |
| HidSharp | 2.6.4 | Apache-2.0 | El nupkg contiene `LICENSE.txt`; [paquete](https://api.nuget.org/v3-flatcontainer/hidsharp/2.6.4/hidsharp.2.6.4.nupkg) |
| Mono.Posix.NETStandard | 1.0.0 | Nuspec heredado con licencia Microsoft/Mono, sin expresión SPDX | [nuspec](https://api.nuget.org/v3-flatcontainer/mono.posix.netstandard/1.0.0/mono.posix.netstandard.nuspec), [licencia Mono](https://github.com/mono/mono/blob/main/LICENSE) |
| System.CodeDom | 10.0.2 | MIT | [nuspec](https://api.nuget.org/v3-flatcontainer/system.codedom/10.0.2/system.codedom.nuspec) |
| System.IO.FileSystem.AccessControl | 5.0.0 | MIT | [nuspec](https://api.nuget.org/v3-flatcontainer/system.io.filesystem.accesscontrol/5.0.0/system.io.filesystem.accesscontrol.nuspec) |
| System.IO.Ports y runtime packs `runtime.*.System.IO.Ports` | 10.0.3 | MIT | [nuspec del paquete base](https://api.nuget.org/v3-flatcontainer/system.io.ports/10.0.3/system.io.ports.nuspec), [nuspec runtime pack](https://api.nuget.org/v3-flatcontainer/runtime.native.system.io.ports/10.0.3/runtime.native.system.io.ports.nuspec) |
| System.Management | 10.0.2 | MIT | [nuspec](https://api.nuget.org/v3-flatcontainer/system.management/10.0.2/system.management.nuspec) |

NuGet también resuelve los paquetes `runtime.*.System.IO.Ports` para distintas
plataformas; se listan como grupo porque comparten versión y licencia MIT.
Para producción/distribución, conservar las copias o avisos de MPL-2.0,
Apache-2.0, MIT y los términos asociados al paquete heredado Mono.Posix junto
con los binarios de terceros correspondientes. No se ha creado todavía un
paquete distribuible.
