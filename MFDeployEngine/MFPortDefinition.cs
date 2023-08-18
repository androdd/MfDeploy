// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.MFPortDefinition
// Assembly: MFDeployEngine, Version=2.0.0.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: 7FFE591E-9FC8-4A2A-9A07-642B2A02EB3C
// Assembly location: D:\Androvil\Visual Studio 2022\Projects\MfDeploy\Tools\MFDeployEngine.dll

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
