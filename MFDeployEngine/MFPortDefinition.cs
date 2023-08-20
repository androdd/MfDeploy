// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.MFPortDefinition
// Assembly: MFDeployEngine, Version=4.3.1.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: C9A5873B-24F0-4236-8EF7-C28FFF230F5B
// Assembly location: C:\Program Files (x86)\Microsoft .NET Micro Framework\v4.3\Tools\MFDeployEngine.dll

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
  public abstract class MFPortDefinition
  {
    internal TransportType m_transport;
    internal string m_name;
    internal string m_port;

    public string Name => this.m_name;

    public string Port => this.m_port;

    public TransportType Transport => this.m_transport;

    public override string ToString() => this.m_name;
  }
}
