// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.MFInvalidFileFormatException
// Assembly: MFDeployEngine, Version=4.3.1.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: C9A5873B-24F0-4236-8EF7-C28FFF230F5B
// Assembly location: C:\Program Files (x86)\Microsoft .NET Micro Framework\v4.3\Tools\MFDeployEngine.dll

using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.Properties;
using System;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
  [Serializable]
  public class MFInvalidFileFormatException : Exception
  {
    private string m_file;

    public MFInvalidFileFormatException(string file) => this.m_file = file;

    public override string Message => string.Format(Resources.ExceptionInvalidFileFormat, (object) this.m_file);
  }
}
