// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.HAL_NetworkConfiguration
// Assembly: MFDeployEngine, Version=4.3.1.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: C9A5873B-24F0-4236-8EF7-C28FFF230F5B
// Assembly location: C:\Program Files (x86)\Microsoft .NET Micro Framework\v4.3\Tools\MFDeployEngine.dll

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
  internal struct HAL_NetworkConfiguration : IHAL_CONFIG_BASE
  {
    public const int c_maxMacBufferLength = 64;
    public HAL_CONFIG_BLOCK Header;
    public int NetworkCount;
    public int Enabled;
    public uint flags;
    public uint ipaddr;
    public uint subnetmask;
    public uint gateway;
    public uint dnsServer1;
    public uint dnsServer2;
    public uint networkInterfaceType;
    public uint macAddressLen;
    public unsafe fixed byte macAddressBuffer[64];

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
                return sizeof(HAL_NetworkConfiguration);
            }
        }
    }
  }
}
