// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.MFDeploy
// Assembly: MFDeployEngine, Version=4.3.1.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: C9A5873B-24F0-4236-8EF7-C28FFF230F5B
// Assembly location: C:\Program Files (x86)\Microsoft .NET Micro Framework\v4.3\Tools\MFDeployEngine.dll

using Microsoft.SPOT.Debugger;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
  public class MFDeploy : IDisposable
  {
    private bool m_tryConnect = true;
    private bool m_disposed;
    private UsbDeviceDiscovery m_usbDiscovery;
    private List<MFPortDefinition> m_deviceList = new List<MFPortDefinition>();
    private IPAddress m_DiscoveryMulticastAddress = IPAddress.Parse("234.102.98.44");
    private IPAddress m_DiscoveryMulticastAddressRecv = IPAddress.Parse("234.102.98.45");
    private int m_DiscoveryMulticastPort = 26001;
    private string m_DiscoveryMulticastToken = "DOTNETMF";
    private int m_DiscoveryMulticastTimeout = 3000;
    private int m_DiscoveryTTL = 1;

    public event EventHandler<EventArgs> OnDeviceListUpdate;

    public IPAddress DiscoveryMulticastAddress
    {
      get => this.m_DiscoveryMulticastAddress;
      set => this.m_DiscoveryMulticastAddress = value;
    }

    public IPAddress DiscoveryMulticastAddressRecv
    {
      get => this.m_DiscoveryMulticastAddressRecv;
      set => this.m_DiscoveryMulticastAddressRecv = value;
    }

    public int DiscoveryMulticastPort
    {
      get => this.m_DiscoveryMulticastPort;
      set => this.m_DiscoveryMulticastPort = value;
    }

    public string DiscoveryMulticastToken
    {
      get => this.m_DiscoveryMulticastToken;
      set => this.m_DiscoveryMulticastToken = value;
    }

    public int DiscoveryMulticastTimeout
    {
      get => this.m_DiscoveryMulticastTimeout;
      set => this.m_DiscoveryMulticastTimeout = value;
    }

    public int DiscoveryTTL
    {
      get => this.m_DiscoveryTTL;
      set => this.m_DiscoveryTTL = value;
    }

    public ReadOnlyCollection<MFPortDefinition> EnumPorts(params TransportType[] types)
    {
      if (types == null || types.Length == 0)
        types = new TransportType[3]
        {
          TransportType.Serial,
          TransportType.TCPIP,
          TransportType.USB
        };
      this.m_deviceList.Clear();
      foreach (TransportType type in types)
      {
        ArrayList arrayList1 = new ArrayList();
        ArrayList arrayList2;
        switch (type)
        {
          case TransportType.Serial:
            arrayList2 = PortDefinition.Enumerate(new PortFilter[1]);
            break;
          case TransportType.USB:
            arrayList2 = PortDefinition.Enumerate(PortFilter.Usb);
            break;
          case TransportType.TCPIP:
            arrayList2 = new ArrayList((ICollection) PortDefinition_Tcp.EnumeratePorts(this.m_DiscoveryMulticastAddress, this.m_DiscoveryMulticastAddressRecv, this.m_DiscoveryMulticastPort, this.m_DiscoveryMulticastToken, this.m_DiscoveryMulticastTimeout, this.m_DiscoveryTTL));
            break;
          default:
            throw new ArgumentException();
        }
        foreach (PortDefinition portDefinition in arrayList2)
        {
          switch (type)
          {
            case TransportType.Serial:
              this.m_deviceList.Add((MFPortDefinition) new MFSerialPort(portDefinition.DisplayName, portDefinition.Port));
              continue;
            case TransportType.USB:
              this.m_deviceList.Add((MFPortDefinition) new MFUsbPort(portDefinition.DisplayName));
              continue;
            case TransportType.TCPIP:
              PortDefinition_Tcp portDefinitionTcp = portDefinition as PortDefinition_Tcp;
              string[] strArray = portDefinitionTcp.Port.Split(new char[1]
              {
                ':'
              }, StringSplitOptions.RemoveEmptyEntries);
              if (strArray.Length > 0)
              {
                this.m_deviceList.Add((MFPortDefinition) new MFTcpIpPort(strArray[0], portDefinitionTcp.MacAddress));
                continue;
              }
              continue;
            default:
              continue;
          }
        }
      }
      return new ReadOnlyCollection<MFPortDefinition>((IList<MFPortDefinition>) this.m_deviceList);
    }

    public MFDevice Connect(MFPortDefinition portDefinition, MFPortDefinition tinyBooterPortDef) => this.InitializePorts(portDefinition, tinyBooterPortDef);

    public MFDevice Connect(MFPortDefinition portDefinition) => this.InitializePorts(portDefinition, (MFPortDefinition) null);

    public IList DeviceList
    {
      get
      {
        this.EnumPorts();
        ArrayList deviceList = new ArrayList();
        deviceList.AddRange((ICollection) this.m_deviceList);
        return (IList) deviceList;
      }
    }

    public MFDeploy()
    {
      try
      {
        this.m_usbDiscovery = new UsbDeviceDiscovery();
        this.m_usbDiscovery.OnDeviceChanged += new UsbDeviceDiscovery.DeviceChangedEventHandler(this.OnDeviceListChanged);
      }
      catch
      {
      }
    }

    private void Dispose(bool disposing)
    {
      if (this.m_disposed)
        return;
      if (disposing && this.m_usbDiscovery != null)
      {
        this.m_usbDiscovery.OnDeviceChanged -= new UsbDeviceDiscovery.DeviceChangedEventHandler(this.OnDeviceListChanged);
        this.m_usbDiscovery.Dispose();
        this.m_usbDiscovery = (UsbDeviceDiscovery) null;
      }
      this.m_disposed = true;
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    ~MFDeploy() => this.Dispose(false);

    public bool OpenWithoutConnect
    {
      set => this.m_tryConnect = !value;
    }

    private MFDevice InitializePorts(
      MFPortDefinition portDefinitionMain,
      MFPortDefinition portDefinitionTinyBooter)
    {
      MFPortDefinition[] mfPortDefinitionArray = new MFPortDefinition[2]
      {
        portDefinitionMain,
        portDefinitionTinyBooter
      };
      PortDefinition[] portDefinitionArray = new PortDefinition[2];
      for (int index = 0; index < mfPortDefinitionArray.Length; ++index)
      {
        MFPortDefinition mfPortDefinition = mfPortDefinitionArray[index];
        if (mfPortDefinition != null)
        {
          if (mfPortDefinition.Transport == TransportType.TCPIP)
          {
            IPAddress[] hostAddresses = Dns.GetHostAddresses(mfPortDefinition.Port);
            portDefinitionArray[index] = (PortDefinition) new PortDefinition_Tcp(hostAddresses[0]);
          }
          else
          {
            foreach (PortDefinition portDefinition in PortDefinition.Enumerate(PortFilter.Usb, PortFilter.Serial))
            {
              if (mfPortDefinition.Port.Length > 0 && string.Equals(mfPortDefinition.Port, portDefinition.UniqueId.ToString()))
              {
                portDefinitionArray[index] = portDefinition;
                break;
              }
              if (string.Equals(mfPortDefinition.Name, portDefinition.DisplayName))
              {
                portDefinitionArray[index] = portDefinition;
                break;
              }
            }
          }
        }
      }
      if (portDefinitionArray[0] == null && portDefinitionArray[1] != null)
      {
        portDefinitionArray[0] = portDefinitionArray[1];
        portDefinitionArray[1] = (PortDefinition) null;
      }
      MFDevice mfDevice = portDefinitionArray[0] != null || portDefinitionArray[1] != null ? new MFDevice(portDefinitionArray[0], portDefinitionArray[1]) : throw new MFDeviceUnknownDeviceException();
      return mfDevice.Connect(2000, this.m_tryConnect) ? mfDevice : throw new MFDeviceNoResponseException();
    }

    private void OnDeviceListChanged(UsbDeviceDiscovery.DeviceChanged devChange)
    {
      if (this.OnDeviceListUpdate == null)
        return;
      this.OnDeviceListUpdate((object) null, (EventArgs) null);
    }
  }
}
