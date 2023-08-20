// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.MFNetworkConfiguration
// Assembly: MFDeployEngine, Version=4.3.1.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: C9A5873B-24F0-4236-8EF7-C28FFF230F5B
// Assembly location: C:\Program Files (x86)\Microsoft .NET Micro Framework\v4.3\Tools\MFDeployEngine.dll

using System;
using System.Net;
using System.Net.Sockets;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
  public class MFNetworkConfiguration
  {
    private const uint c_SOCK_NETWORKCONFIGURATION_FLAGS_DHCP = 1;
    private const uint c_SOCK_NETWORKCONFIGURATION_INTERFACETYPE_ETHERNET = 6;
    private const string c_CfgName = "NETWORK";
    private const int c_ConfigTypeFlagShift = 16;
    private HAL_NetworkConfiguration m_cfg = new HAL_NetworkConfiguration();
    private MFConfigHelper m_cfgHelper;

    public MFNetworkConfiguration(MFDevice dev)
    {
      this.m_cfg.networkInterfaceType = 6U;
      this.m_cfgHelper = new MFConfigHelper(dev);
    }

    public bool EnableDhcp
    {
      get => ((int) this.m_cfg.flags & 1) != 0;
      set
      {
        uint num = this.m_cfg.flags & 4294901760U;
        this.m_cfg.flags = value ? 1U : 0U;
        this.m_cfg.flags |= num;
      }
    }

    public IPAddress IpAddress
    {
      get => new IPAddress((long) this.m_cfg.ipaddr);
      set => this.m_cfg.ipaddr = value.AddressFamily == AddressFamily.InterNetwork ? BitConverter.ToUInt32(value.GetAddressBytes(), 0) : throw new MFInvalidNetworkAddressException();
    }

    public IPAddress SubNetMask
    {
      get => new IPAddress((long) this.m_cfg.subnetmask);
      set => this.m_cfg.subnetmask = value.AddressFamily == AddressFamily.InterNetwork ? BitConverter.ToUInt32(value.GetAddressBytes(), 0) : throw new MFInvalidNetworkAddressException();
    }

    public IPAddress Gateway
    {
      get => new IPAddress((long) this.m_cfg.gateway);
      set => this.m_cfg.gateway = value.AddressFamily == AddressFamily.InterNetwork ? BitConverter.ToUInt32(value.GetAddressBytes(), 0) : throw new MFInvalidNetworkAddressException();
    }

    public IPAddress PrimaryDns
    {
      get => new IPAddress((long) this.m_cfg.dnsServer1);
      set => this.m_cfg.dnsServer1 = value.AddressFamily == AddressFamily.InterNetwork ? BitConverter.ToUInt32(value.GetAddressBytes(), 0) : throw new MFInvalidNetworkAddressException();
    }

    public IPAddress SecondaryDns
    {
      get => new IPAddress((long) this.m_cfg.dnsServer2);
      set => this.m_cfg.dnsServer2 = value.AddressFamily == AddressFamily.InterNetwork ? BitConverter.ToUInt32(value.GetAddressBytes(), 0) : throw new MFInvalidNetworkAddressException();
    }

    public unsafe byte[] MacAddress
    {
      get
      {
        uint length = this.m_cfg.macAddressLen;
        if (length > 64U)
          length = 64U;
        byte[] macAddress = new byte[length];
        fixed (byte* numPtr = this.m_cfg.macAddressBuffer)
        {
          for (int index = 0; (long) index < (long) length; ++index)
            macAddress[index] = numPtr[index];
        }
        return macAddress;
      }
      set
      {
        if (value.Length > 64)
          throw new MFInvalidMacAddressException();
        fixed (byte* numPtr = this.m_cfg.macAddressBuffer)
        {
          for (int index = 0; index < value.Length; ++index)
            numPtr[index] = value[index];
        }
        this.m_cfg.macAddressLen = (uint) value.Length;
      }
    }

    public int MaxMacAddressLength => 64;

    public MFNetworkConfiguration.NetworkConfigType ConfigurationType
    {
      get => (MFNetworkConfiguration.NetworkConfigType) (this.m_cfg.flags >> 16 & 15U);
      set => this.m_cfg.flags |= (uint) (value & (MFNetworkConfiguration.NetworkConfigType) 15) << 16;
    }

    public void Load()
    {
      byte[] config = this.m_cfgHelper.FindConfig("NETWORK");
      if (config == null)
        return;
      this.m_cfg = (HAL_NetworkConfiguration) MFConfigHelper.UnmarshalData(config, typeof (HAL_NetworkConfiguration));
    }

    public void Save()
    {
      this.m_cfg.NetworkCount = 1;
      this.m_cfg.Enabled = 1;
      this.m_cfgHelper.WriteConfig("NETWORK", (IHAL_CONFIG_BASE) this.m_cfg);
    }

    public void SwapConfigBuffer(MFConfigHelper srcConfig) => this.m_cfgHelper.SwapAllConfigData(srcConfig);

    public enum NetworkConfigType
    {
      Generic,
      Wireless,
    }
  }
}
