// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.EraseOptions
// Assembly: MFDeployEngine, Version=4.3.1.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: C9A5873B-24F0-4236-8EF7-C28FFF230F5B
// Assembly location: C:\Program Files (x86)\Microsoft .NET Micro Framework\v4.3\Tools\MFDeployEngine.dll

using System;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
  [Flags]
  public enum EraseOptions
  {
    Deployment = 1,
    UserStorage = 2,
    FileSystem = 4,
    Firmware = 8,
    UpdateStorage = 16, // 0x00000010
    SimpleStorage = 32, // 0x00000020
  }
}
