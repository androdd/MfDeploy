// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.MFSignatureFailureException
// Assembly: MFDeployEngine, Version=2.0.0.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: 7FFE591E-9FC8-4A2A-9A07-642B2A02EB3C
// Assembly location: D:\Androvil\Visual Studio 2022\Projects\MfDeploy\Tools\MFDeployEngine.dll

using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.Properties;
using System;
using System.IO;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
  [Serializable]
  public class MFSignatureFailureException : Exception
  {
    private string m_file;

    public MFSignatureFailureException(string file) => this.m_file = new FileInfo(file).Name;

    public override string Message => string.Format(Resources.ExceptionSignatureFailure, (object) this.m_file);
  }
}
