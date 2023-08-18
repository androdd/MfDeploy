// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.CONFIG_SECTOR_VERSION
// Assembly: MFDeployEngine, Version=2.0.0.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: 7FFE591E-9FC8-4A2A-9A07-642B2A02EB3C
// Assembly location: D:\Androvil\Visual Studio 2022\Projects\MfDeploy\Tools\MFDeployEngine.dll

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
  internal struct CONFIG_SECTOR_VERSION
  {
    internal const int c_CurrentTinyBooterVersion = 4;
    internal byte Major;
    internal byte Minor;
    internal byte TinyBooter;
    internal byte Extra;
  }
}
