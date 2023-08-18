// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.MFDeploy
// Assembly: MFDeployEngine, Version=2.0.0.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: 7FFE591E-9FC8-4A2A-9A07-642B2A02EB3C
// Assembly location: D:\Androvil\Visual Studio 2022\Projects\MfDeploy\Tools\MFDeployEngine.dll

using Microsoft.SPOT.Debugger;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

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

    public unsafe ReadOnlyCollection<MFPortDefinition> EnumPorts(params TransportType[] types)
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
        PortFilter[] portFilterArray1 = new PortFilter[1];
        PortFilter[] portFilterArray2 = portFilterArray1;
        int num;
        switch (type)
        {
          case TransportType.Serial:
            num = 0;
            break;
          case TransportType.USB:
            num = 1;
            break;
          default:
            num = 3;
            break;
        }
        portFilterArray2[0] = (PortFilter) num;
        foreach (PortDefinition portDefinition in PortDefinition.Enumerate(portFilterArray1))
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
              this.m_deviceList.Add((MFPortDefinition) new MFTcpIpPort(portDefinition.Port, ""));
              continue;
            default:
              continue;
          }
        }
        if (type == TransportType.TCPIP)
        {
          try
          {
            Hashtable hashtable = new Hashtable();
            foreach (IPAddress address in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
              if (address.AddressFamily == AddressFamily.InterNetwork)
              {
                int offset = 0;
                byte[] numArray = new byte[1024];
                Socket socket1 = (Socket) null;
                Socket socket2 = (Socket) null;
                IPEndPoint localEP1 = new IPEndPoint(address, 0);
                System.Net.EndPoint remoteEP1 = (System.Net.EndPoint) new IPEndPoint(IPAddress.Any, this.m_DiscoveryMulticastPort);
                IPEndPoint localEP2 = new IPEndPoint(address, this.m_DiscoveryMulticastPort);
                IPEndPoint remoteEP2 = new IPEndPoint(this.m_DiscoveryMulticastAddress, this.m_DiscoveryMulticastPort);
                try
                {
                  socket1 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                  socket2 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                  socket2.Bind((System.Net.EndPoint) localEP2);
                  socket2.ReceiveTimeout = this.m_DiscoveryMulticastTimeout;
                  socket2.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, (object) new MulticastOption(this.m_DiscoveryMulticastAddressRecv, address));
                  socket1.Bind((System.Net.EndPoint) localEP1);
                  socket1.MulticastLoopback = false;
                  socket1.Ttl = (short) this.m_DiscoveryTTL;
                  socket1.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 64);
                  socket1.SendTo(Encoding.ASCII.GetBytes(this.m_DiscoveryMulticastToken), SocketFlags.None, (System.Net.EndPoint) remoteEP2);
                  int from;
                  while (0 < (from = socket2.ReceiveFrom(numArray, offset, numArray.Length - offset, SocketFlags.None, ref remoteEP1)))
                  {
                    hashtable[(object) ((IPEndPoint) remoteEP1).Address.ToString()] = (object) "";
                    offset += from;
                    socket2.ReceiveTimeout = this.m_DiscoveryMulticastTimeout / 2;
                  }
                  socket2.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, (object) new MulticastOption(this.m_DiscoveryMulticastAddressRecv));
                }
                catch (SocketException ex)
                {
                }
                finally
                {
                  socket2?.Close();
                  socket1?.Close();
                }
                MFDeploy.SOCK_discoveryinfo structure1 = new MFDeploy.SOCK_discoveryinfo();
                int sourceIndex = 0;
                int length = Marshal.SizeOf((object) structure1);
                while (offset >= length)
                {
                  byte[] destinationArray = new byte[length];
                  Array.Copy((Array) numArray, sourceIndex, (Array) destinationArray, 0, length);
                  GCHandle gcHandle = GCHandle.Alloc((object) destinationArray, GCHandleType.Pinned);
                  MFDeploy.SOCK_discoveryinfo structure2 = (MFDeploy.SOCK_discoveryinfo) Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), typeof (MFDeploy.SOCK_discoveryinfo));
                  gcHandle.Free();
                  if (structure2.macAddressLen <= 64U)
                  {
                    IPAddress ipAddress = new IPAddress((long) structure2.ipaddr);
                    if (hashtable.ContainsKey((object) ipAddress.ToString()))
                    {
                      string str1 = "";
                      for (int index = 0; (long) index < (long) (structure2.macAddressLen - 1U); ++index)
                        str1 += string.Format("{0:x02}:", (object) structure2.macAddressBuffer[index]);
                      string str2 = str1 + string.Format("{0:x02}", (object) structure2.macAddressBuffer[(int) structure2.macAddressLen - 1]);
                      hashtable[(object) ipAddress.ToString()] = (object) str2;
                    }
                  }
                  offset -= length;
                  sourceIndex += length;
                }
              }
            }
            foreach (string key in (IEnumerable) hashtable.Keys)
              this.m_deviceList.Add((MFPortDefinition) new MFTcpIpPort(key, (string) hashtable[(object) key]));
          }
          catch (Exception ex)
          {
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
            foreach (PortDefinition portDefinition in PortDefinition.Enumerate(PortFilter.Usb, PortFilter.Serial, PortFilter.TcpIp))
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

    internal struct SOCK_discoveryinfo
    {
      internal uint ipaddr;
      internal uint macAddressLen;
      internal unsafe fixed byte macAddressBuffer[64];
    }
  }
}
