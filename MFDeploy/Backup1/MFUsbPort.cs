﻿// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.MFUsbPort
// Assembly: MFDeployEngine, Version=2.0.0.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: 7FFE591E-9FC8-4A2A-9A07-642B2A02EB3C
// Assembly location: D:\Androvil\Visual Studio 2022\Projects\MfDeploy\Tools\MFDeployEngine.dll

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
