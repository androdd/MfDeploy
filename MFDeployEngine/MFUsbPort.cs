// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.MFUsbPort
// Assembly: MFDeployEngine, Version=4.3.1.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: C9A5873B-24F0-4236-8EF7-C28FFF230F5B
// Assembly location: C:\Program Files (x86)\Microsoft .NET Micro Framework\v4.3\Tools\MFDeployEngine.dll

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
  public class MFUsbPort : MFPortDefinition
  {
    public MFUsbPort(string usbName)
    {
      this.m_name = usbName;
      this.m_transport = TransportType.USB;
      this.m_port = usbName;
      int num = this.m_port.IndexOf("_");
      if (num < 0)
        return;
      this.m_port = this.m_port.Substring(num + 1, this.m_port.Length - num - 1);
    }
  }
}
