// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.PlugIns.MFPlugInMenuItem
// Assembly: MFDeployEngine, Version=2.0.0.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: 7FFE591E-9FC8-4A2A-9A07-642B2A02EB3C
// Assembly location: D:\Androvil\Visual Studio 2022\Projects\MfDeploy\Tools\MFDeployEngine.dll

using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using System.Collections.ObjectModel;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.PlugIns
{
  public abstract class MFPlugInMenuItem
  {
    private object m_tag;

    public abstract string Name { get; }

    public object Tag
    {
      get => this.m_tag;
      set => this.m_tag = value;
    }

    public virtual ReadOnlyCollection<MFPlugInMenuItem> Submenus => (ReadOnlyCollection<MFPlugInMenuItem>) null;

    public virtual bool RunInSeparateThread => true;

    public virtual bool RequiresConnection => true;

    public virtual void OnAction(IMFDeployForm form, MFDevice device)
    {
    }

    public override string ToString() => this.Name;
  }
}
