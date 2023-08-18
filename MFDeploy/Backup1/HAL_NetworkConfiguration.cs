// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.HAL_NetworkConfiguration
// Assembly: MFDeployEngine, Version=2.0.0.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: 7FFE591E-9FC8-4A2A-9A07-642B2A02EB3C
// Assembly location: D:\Androvil\Visual Studio 2022\Projects\MfDeploy\Tools\MFDeployEngine.dll

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

    public int Size => sizeof (HAL_NetworkConfiguration);
  }
}
