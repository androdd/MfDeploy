// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.MFUsbConfiguration
// Assembly: MFDeployEngine, Version=2.0.0.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: 7FFE591E-9FC8-4A2A-9A07-642B2A02EB3C
// Assembly location: D:\Androvil\Visual Studio 2022\Projects\MfDeploy\Tools\MFDeployEngine.dll

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
  public class MFUsbConfiguration
  {
    private const string c_name = "USB_NAME_CONFIG";
    private USB_CONFIG m_cfg = new USB_CONFIG();
    private MFConfigHelper m_cfgHelper;

    public MFUsbConfiguration(MFDevice dev) => this.m_cfgHelper = new MFConfigHelper(dev);

    public string Name
    {
      get => this.m_cfg.FriendlyNameString();
      set => this.m_cfg.SetName(value);
    }

    public void Load()
    {
      byte[] config = this.m_cfgHelper.FindConfig("USB_NAME_CONFIG");
      if (config == null)
        return;
      this.m_cfg = (USB_CONFIG) MFConfigHelper.UnmarshalData(config, typeof (USB_CONFIG));
    }

    public void Save() => this.m_cfgHelper.WriteConfig("USB_NAME_CONFIG", (IHAL_CONFIG_BASE) this.m_cfg);
  }
}
