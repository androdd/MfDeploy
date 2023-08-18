// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.DebugOutputEventArgs
// Assembly: MFDeployEngine, Version=2.0.0.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: 7FFE591E-9FC8-4A2A-9A07-642B2A02EB3C
// Assembly location: D:\Androvil\Visual Studio 2022\Projects\MfDeploy\Tools\MFDeployEngine.dll

using System;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
  public class DebugOutputEventArgs : EventArgs
  {
    private string m_txt;

    public DebugOutputEventArgs(string text) => this.m_txt = text;

    public string Text => this.m_txt;
  }
}
