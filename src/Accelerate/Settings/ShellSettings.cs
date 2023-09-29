namespace Accelerate.Settings;

public sealed class ShellSettings
{
    public string Shell { get; set; } = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "PowerShell" : "/bin/sh";
}