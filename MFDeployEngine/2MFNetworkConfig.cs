// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.HAL_WirelessConfiguration
// Assembly: MFDeployEngine, Version=4.3.1.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: C9A5873B-24F0-4236-8EF7-C28FFF230F5B
// Assembly location: C:\Program Files (x86)\Microsoft .NET Micro Framework\v4.3\Tools\MFDeployEngine.dll

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
  internal struct HAL_WirelessConfiguration : IHAL_CONFIG_BASE
  {
    public const int c_PassPhraseLength = 64;
    public const int c_NetworkKeyLength = 256;
    public const int c_ReKeyInternalLength = 32;
    public const int c_SSIDLength = 32;
    public HAL_CONFIG_BLOCK Header;
    public int WirelessNetworkCount;
    public int Enabled;
    public uint WirelessFlags;
    public unsafe fixed byte PassPhrase[64];
    public int NetworkKeyLength;
    public unsafe fixed byte NetworkKey[256];
    public int ReKeyLength;
    public unsafe fixed byte ReKeyInternal[32];
    public unsafe fixed byte SSID[32];

    public HAL_CONFIG_BLOCK ConfigHeader
    {
      get => this.Header;
      set => this.Header = value;
    }

    public int Size
    {
        get
        {
            unsafe
            {
                return sizeof(HAL_WirelessConfiguration);
            }
        }
    }
  }
}
