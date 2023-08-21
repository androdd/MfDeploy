// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.PlugIns.IMFDeployForm
// Assembly: MFDeployEngine, Version=4.3.1.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: C9A5873B-24F0-4236-8EF7-C28FFF230F5B
// Assembly location: C:\Program Files (x86)\Microsoft .NET Micro Framework\v4.3\Tools\MFDeployEngine.dll

using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using System.Collections.ObjectModel;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.PlugIns
{
  public interface IMFDeployForm
  {
    void DumpToOutput(string text);

    void DumpToOutput(string text, bool newLine);

    MFPortDefinition TransportTinyBooter { get; set; }
  }
}
