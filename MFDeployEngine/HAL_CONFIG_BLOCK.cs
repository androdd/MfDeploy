// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.HAL_CONFIG_BLOCK
// Assembly: MFDeployEngine, Version=2.0.0.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: 7FFE591E-9FC8-4A2A-9A07-642B2A02EB3C
// Assembly location: D:\Androvil\Visual Studio 2022\Projects\MfDeploy\Tools\MFDeployEngine.dll

using System.Text;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
  public struct HAL_CONFIG_BLOCK
  {
    public uint Signature;
    public uint HeaderCRC;
    public uint DataCRC;
    public uint Size;
    public unsafe fixed byte DriverName[64];

    public unsafe string DriverNameString
    {
      get
      {
        StringBuilder stringBuilder = new StringBuilder(66);
        fixed (byte* numPtr = this.DriverName)
        {
          for (int index = 0; index < 64 && numPtr[index] != (byte) 0; ++index)
            stringBuilder.Append((char) numPtr[index]);
        }
        return stringBuilder.ToString();
      }
    }
  }
}
