// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.MFSslKeyConfig
// Assembly: MFDeployEngine, Version=4.3.1.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: C9A5873B-24F0-4236-8EF7-C28FFF230F5B
// Assembly location: C:\Program Files (x86)\Microsoft .NET Micro Framework\v4.3\Tools\MFDeployEngine.dll

using System;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
  public class MFSslKeyConfig
  {
    private const string c_CfgName = "SSL_SEED_KEY";
    private HAL_SslKeyConfiguration m_cfg = new HAL_SslKeyConfiguration();
    private MFDevice m_dev;

    public MFSslKeyConfig(MFDevice dev) => this.m_dev = dev;

    public unsafe void Save()
    {
      double num = new Random().NextDouble();
      KeyPair keyPair = new MFKeyConfig().CreateKeyPair();
      MFConfigHelper mfConfigHelper = new MFConfigHelper(this.m_dev);
      this.m_cfg.Enabled = 1U;
      this.m_cfg.Seed = (ulong) (1.8446744073709552E+19 * num);
      int index1;
      fixed (byte* numPtr = this.m_cfg.PrivateSslKey.SectorKey)
      {
        for (int index2 = 0; index2 < keyPair.PrivateKey.Length && index2 < 260; index2 = index1 + 1)
        {
            unsafe
            {
                // ISSUE: cast to a reference type
                byte* local = numPtr;
                int index3 = index2;
                index1 = index3 + 1;
                local[index3] = keyPair.PrivateKey[index1];
            }
        }
      }
      mfConfigHelper.WriteConfig("SSL_SEED_KEY", (IHAL_CONFIG_BASE) this.m_cfg, true);
      mfConfigHelper.Dispose();
    }
  }
}
