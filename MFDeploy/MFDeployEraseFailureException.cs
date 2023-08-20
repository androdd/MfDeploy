// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.MFDeployEraseFailureException
// Assembly: MFDeploy, Version=4.3.1.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: 9518305E-2E58-43E8-9F45-41B78F83BC9E
// Assembly location: C:\Program Files (x86)\Microsoft .NET Micro Framework\v4.3\Tools\MFDeploy.exe

using Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties;
using System;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool
{
  [Serializable]
  public class MFDeployEraseFailureException : Exception
  {
    public override string Message => Resources.ErrorErase;
  }
}
