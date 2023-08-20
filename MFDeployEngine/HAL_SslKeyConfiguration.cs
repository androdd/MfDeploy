// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.HAL_SslKeyConfiguration
// Assembly: MFDeployEngine, Version=4.3.1.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: C9A5873B-24F0-4236-8EF7-C28FFF230F5B
// Assembly location: C:\Program Files (x86)\Microsoft .NET Micro Framework\v4.3\Tools\MFDeployEngine.dll

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
  internal struct HAL_SslKeyConfiguration : IHAL_CONFIG_BASE
  {
    public HAL_CONFIG_BLOCK Header;
    public uint Enabled;
    public ulong Seed;
    internal TINYBOOTER_KEY_CONFIG PrivateSslKey;

    public HAL_CONFIG_BLOCK ConfigHeader
    {
      get => this.Header;
      set => this.Header = value;
    }

    public int Size
    {
        get
        {
            unsafe
            {
                return sizeof(HAL_SslKeyConfiguration);
            }
        }
    }
  }
}
