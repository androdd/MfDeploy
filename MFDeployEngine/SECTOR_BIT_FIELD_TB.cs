﻿// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.SECTOR_BIT_FIELD_TB
// Assembly: MFDeployEngine, Version=4.3.1.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: C9A5873B-24F0-4236-8EF7-C28FFF230F5B
// Assembly location: C:\Program Files (x86)\Microsoft .NET Micro Framework\v4.3\Tools\MFDeployEngine.dll

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
  internal struct SECTOR_BIT_FIELD_TB
  {
    private const int c_MaxBitCount = 8640;
    private const int c_MaxFieldUnits = 270;
    internal unsafe fixed uint BitField[270];
  }
}
