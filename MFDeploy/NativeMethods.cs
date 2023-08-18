// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.NativeMethods
// Assembly: MFDeploy, Version=2.0.0.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: 2BA2704E-D6D9-4FE0-ACF3-88D9CCA0E272
// Assembly location: D:\Androvil\Visual Studio 2022\Projects\MfDeploy\Tools\MFDeploy.exe

using System.Runtime.InteropServices;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool
{
  internal class NativeMethods
  {
    private const string KERNEL32 = "kernel32.dll";
    internal const uint ATTACH_PARENT_PROCESS = 4294967295;

    private NativeMethods()
    {
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern bool AttachConsole(uint dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern bool FreeConsole();

    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern bool AllocConsole();
  }
}
