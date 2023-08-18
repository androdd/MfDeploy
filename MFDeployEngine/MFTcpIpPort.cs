// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.MFTcpIpPort
// Assembly: MFDeployEngine, Version=2.0.0.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: 7FFE591E-9FC8-4A2A-9A07-642B2A02EB3C
// Assembly location: D:\Androvil\Visual Studio 2022\Projects\MfDeploy\Tools\MFDeployEngine.dll

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
