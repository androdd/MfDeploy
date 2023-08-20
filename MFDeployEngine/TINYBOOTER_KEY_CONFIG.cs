// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.TINYBOOTER_KEY_CONFIG
// Assembly: MFDeployEngine, Version=4.3.1.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: C9A5873B-24F0-4236-8EF7-C28FFF230F5B
// Assembly location: C:\Program Files (x86)\Microsoft .NET Micro Framework\v4.3\Tools\MFDeployEngine.dll

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
  internal struct TINYBOOTER_KEY_CONFIG
  {
    internal const int c_KeySignatureLength = 128;
    internal const int c_RSAKeyLength = 260;
    internal unsafe fixed byte SectorKey[260];
  }
}
