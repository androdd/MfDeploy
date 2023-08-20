// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.CONFIG_SECTOR_VERSION
// Assembly: MFDeployEngine, Version=4.3.1.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: C9A5873B-24F0-4236-8EF7-C28FFF230F5B
// Assembly location: C:\Program Files (x86)\Microsoft .NET Micro Framework\v4.3\Tools\MFDeployEngine.dll

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
