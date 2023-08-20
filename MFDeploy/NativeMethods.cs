// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.NativeMethods
// Assembly: MFDeploy, Version=4.3.1.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: 9518305E-2E58-43E8-9F45-41B78F83BC9E
// Assembly location: C:\Program Files (x86)\Microsoft .NET Micro Framework\v4.3\Tools\MFDeploy.exe

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
