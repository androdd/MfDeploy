﻿// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.SECTOR_BIT_FIELD_TB
// Assembly: MFDeployEngine, Version=2.0.0.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: 7FFE591E-9FC8-4A2A-9A07-642B2A02EB3C
// Assembly location: D:\Androvil\Visual Studio 2022\Projects\MfDeploy\Tools\MFDeployEngine.dll

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
  internal struct SECTOR_BIT_FIELD_TB
  {
    private const int c_MaxBitCount = 8640;
    private const int c_MaxFieldUnits = 270;
    internal unsafe fixed uint BitField[270];
  }
}
