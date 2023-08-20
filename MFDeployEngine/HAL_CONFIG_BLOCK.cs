// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.HAL_CONFIG_BLOCK
// Assembly: MFDeployEngine, Version=4.3.1.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: C9A5873B-24F0-4236-8EF7-C28FFF230F5B
// Assembly location: C:\Program Files (x86)\Microsoft .NET Micro Framework\v4.3\Tools\MFDeployEngine.dll

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
      set
      {
        fixed (byte* numPtr = this.DriverName)
        {
          int num = value.Length;
          if (num > 64)
            num = 64;
          for (int index = 0; index < num; ++index)
            numPtr[index] = (byte) value[index];
        }
      }
    }
  }
}
