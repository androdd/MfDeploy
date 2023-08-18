// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.MFNetworkConfiguration
// Assembly: MFDeployEngine, Version=2.0.0.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: 7FFE591E-9FC8-4A2A-9A07-642B2A02EB3C
// Assembly location: D:\Androvil\Visual Studio 2022\Projects\MfDeploy\Tools\MFDeployEngine.dll

using System;
using System.Net;
using System.Net.Sockets;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
  public class MFNetworkConfiguration
  {
    private const uint c_SOCK_NETWORKCONFIGURATION_FLAGS_DHCP = 1;
    private const uint c_SOCK_NETWORKCONFIGUATION_INTERFACETYPE_ETHERNET = 6;
    private const string c_CfgName = "NETWORK";
    private HAL_NetworkConfiguration m_cfg = new HAL_NetworkConfiguration();
    private MFConfigHelper m_cfgHelper;

    public MFNetworkConfiguration(MFDevice dev) => this.m_cfgHelper = new MFConfigHelper(dev);

    public bool EnableDhcp
    {
      get => this.m_cfg.flags == 1U;
      set => this.m_cfg.flags = value ? 1U : 0U;
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
        byte[] macAddress = new byte[this.m_cfg.macAddressLen];
        fixed (byte* numPtr = this.m_cfg.macAddressBuffer)
        {
          for (int index = 0; (long) index < (long) this.m_cfg.macAddressLen; ++index)
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
      this.m_cfg.networkInterfaceType = 6U;
      this.m_cfgHelper.WriteConfig("NETWORK", (IHAL_CONFIG_BASE) this.m_cfg);
    }
  }
}
