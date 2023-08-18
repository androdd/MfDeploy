// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.MFDeployDeployFailureException
// Assembly: MFDeploy, Version=2.0.0.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: 2BA2704E-D6D9-4FE0-ACF3-88D9CCA0E272
// Assembly location: D:\Androvil\Visual Studio 2022\Projects\MfDeploy\Tools\MFDeploy.exe

using Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties;
using System;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool
{
  [Serializable]
  public class MFDeployDeployFailureException : Exception
  {
    public override string Message => Resources.ErrorDeployment;
  }
}
