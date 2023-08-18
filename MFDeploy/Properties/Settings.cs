// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties.Settings
// Assembly: MFDeploy, Version=2.0.0.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: 2BA2704E-D6D9-4FE0-ACF3-88D9CCA0E272
// Assembly location: D:\Androvil\Visual Studio 2022\Projects\MfDeploy\Tools\MFDeploy.exe

using System.CodeDom.Compiler;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties
{
  [GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "8.0.0.0")]
  [CompilerGenerated]
  internal sealed class Settings : ApplicationSettingsBase
  {
    private static Settings defaultInstance = (Settings) SettingsBase.Synchronized((SettingsBase) new Settings());

    public static Settings Default => Settings.defaultInstance;

    [DebuggerNonUserCode]
    [UserScopedSetting]
    [DefaultSettingValue("COM1")]
    public string DefaultSerialPort
    {
      get => (string) this[nameof (DefaultSerialPort)];
      set => this[nameof (DefaultSerialPort)] = (object) value;
    }

    [DefaultSettingValue("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfString xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <string>10.52.32.196</string>\r\n</ArrayOfString>")]
    [UserScopedSetting]
    [DebuggerNonUserCode]
    public StringCollection TcpIpAddresses
    {
      get => (StringCollection) this[nameof (TcpIpAddresses)];
      set => this[nameof (TcpIpAddresses)] = (object) value;
    }

    [DefaultSettingValue("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfString xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" />")]
    [DebuggerNonUserCode]
    [UserScopedSetting]
    public StringCollection MRUFiles
    {
      get => (StringCollection) this[nameof (MRUFiles)];
      set => this[nameof (MRUFiles)] = (object) value;
    }

    [DefaultSettingValue("234.102.98.44")]
    [UserScopedSetting]
    [DebuggerNonUserCode]
    public string DiscoveryMulticastAddress
    {
      get => (string) this[nameof (DiscoveryMulticastAddress)];
      set => this[nameof (DiscoveryMulticastAddress)] = (object) value;
    }

    [UserScopedSetting]
    [DefaultSettingValue("234.102.98.45")]
    [DebuggerNonUserCode]
    public string DiscoveryMulticastAddressRecv
    {
      get => (string) this[nameof (DiscoveryMulticastAddressRecv)];
      set => this[nameof (DiscoveryMulticastAddressRecv)] = (object) value;
    }

    [DebuggerNonUserCode]
    [DefaultSettingValue("3000")]
    [UserScopedSetting]
    public int DiscoveryMulticastTimeout
    {
      get => (int) this[nameof (DiscoveryMulticastTimeout)];
      set => this[nameof (DiscoveryMulticastTimeout)] = (object) value;
    }

    [UserScopedSetting]
    [DebuggerNonUserCode]
    [DefaultSettingValue("26001")]
    public int DiscoveryMulticastPort
    {
      get => (int) this[nameof (DiscoveryMulticastPort)];
      set => this[nameof (DiscoveryMulticastPort)] = (object) value;
    }

    [DebuggerNonUserCode]
    [DefaultSettingValue("DOTNETMF")]
    [UserScopedSetting]
    public string DiscoveryMulticastToken
    {
      get => (string) this[nameof (DiscoveryMulticastToken)];
      set => this[nameof (DiscoveryMulticastToken)] = (object) value;
    }

    [DefaultSettingValue("1")]
    [UserScopedSetting]
    [DebuggerNonUserCode]
    public int DiscoveryTTL
    {
      get => (int) this[nameof (DiscoveryTTL)];
      set => this[nameof (DiscoveryTTL)] = (object) value;
    }

    [UserScopedSetting]
    [DefaultSettingValue("PlugIn")]
    [DebuggerNonUserCode]
    public string PlugIns
    {
      get => (string) this[nameof (PlugIns)];
      set => this[nameof (PlugIns)] = (object) value;
    }
  }
}
