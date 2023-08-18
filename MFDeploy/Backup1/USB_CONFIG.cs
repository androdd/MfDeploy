// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.USB_CONFIG
// Assembly: MFDeployEngine, Version=2.0.0.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: 7FFE591E-9FC8-4A2A-9A07-642B2A02EB3C
// Assembly location: D:\Androvil\Visual Studio 2022\Projects\MfDeploy\Tools\MFDeployEngine.dll

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
  public struct USB_CONFIG : IHAL_CONFIG_BASE
  {
    private const int c_MaxNameLength = 30;
    public HAL_CONFIG_BLOCK Header;
    public unsafe fixed byte FriendlyName[31];
    public byte buffer;

    public int Size => sizeof (USB_CONFIG);

    public HAL_CONFIG_BLOCK ConfigHeader
    {
      get => this.Header;
      set => this.Header = value;
    }

    public unsafe string FriendlyNameString()
    {
      char[] chArray = new char[31];
      fixed (byte* numPtr = this.FriendlyName)
      {
        for (int index = 0; index < 30; ++index)
          chArray[index] = (char) numPtr[index];
        chArray[30] = char.MinValue;
      }
      return new string(chArray);
    }

    public unsafe void SetName(string name)
    {
      int num = name.Length;
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
