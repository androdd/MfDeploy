// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.OemMonitorInfo
// Assembly: MFDeployEngine, Version=4.3.1.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: C9A5873B-24F0-4236-8EF7-C28FFF230F5B
// Assembly location: C:\Program Files (x86)\Microsoft .NET Micro Framework\v4.3\Tools\MFDeployEngine.dll

using Microsoft.SPOT.Debugger.WireProtocol;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
  public class OemMonitorInfo
  {
    private ReleaseInfo m_releaseInfo;

    public OemMonitorInfo(Commands.Monitor_OemInfo.Reply reply) => this.m_releaseInfo = reply.m_releaseInfo;

    public System.Version Version => this.m_releaseInfo.Version;

    public string OemString => this.m_releaseInfo.Info;

    public override string ToString() => string.Format("Bootloader build info: {0}\nVersion {1}\n", (object) this.OemString, (object) this.Version);

    public bool Valid => true;
  }
}
