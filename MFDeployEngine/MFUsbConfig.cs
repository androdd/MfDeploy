// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.USB_CONFIG
// Assembly: MFDeployEngine, Version=4.3.1.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: C9A5873B-24F0-4236-8EF7-C28FFF230F5B
// Assembly location: C:\Program Files (x86)\Microsoft .NET Micro Framework\v4.3\Tools\MFDeployEngine.dll

using System;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
  public struct USB_CONFIG : IHAL_CONFIG_BASE
  {
    private const int c_MaxNameLength = 30;
    public HAL_CONFIG_BLOCK Header;
    public unsafe fixed byte FriendlyName[31];
    public byte buffer;

    public int Size
    {
        get
        {
            unsafe
            {
                    return sizeof(USB_CONFIG);
            }
        }
    }

    public HAL_CONFIG_BLOCK ConfigHeader
    {
      get => this.Header;
      set => this.Header = value;
    }

    public unsafe string FriendlyNameString()
    {
      char[] chArray = new char[31];
      int length;
      fixed (byte* numPtr = this.FriendlyName)
      {
        for (length = 0; length < 30 && numPtr[length] != (byte) 0; ++length)
          chArray[length] = (char) numPtr[length];
      }
      return new string(chArray, 0, length);
    }

    public unsafe void SetName(string name)
    {
      int num = name != null && !name.Equals("") ? name.Length : throw new ArgumentException("The device USB name is invalid");
      if (30 < num)
        num = 30;
      fixed (byte* numPtr = this.FriendlyName)
      {
        int index;
        for (index = 0; index < num; ++index)
          numPtr[index] = (byte) name[index];
        numPtr[index] = (byte) 0;
      }
    }
  }
}
