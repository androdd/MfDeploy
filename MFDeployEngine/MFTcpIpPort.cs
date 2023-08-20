// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.MFTcpIpPort
// Assembly: MFDeployEngine, Version=4.3.1.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: C9A5873B-24F0-4236-8EF7-C28FFF230F5B
// Assembly location: C:\Program Files (x86)\Microsoft .NET Micro Framework\v4.3\Tools\MFDeployEngine.dll

using System.Net;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
  public class MFTcpIpPort : MFPortDefinition
  {
    internal IPAddress m_ipaddress;

    public MFTcpIpPort(string ipAddress, string mac)
    {
      this.m_name = ipAddress;
      if (mac.Trim().Length > 0)
      {
        MFTcpIpPort mfTcpIpPort = this;
        mfTcpIpPort.m_name = mfTcpIpPort.m_name + " - (" + mac + ")";
      }
      this.m_transport = TransportType.TCPIP;
      this.m_port = ipAddress;
      IPAddress.TryParse(this.m_name, out this.m_ipaddress);
    }

    public IPAddress Address => this.m_ipaddress;
  }
}
