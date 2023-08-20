// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties.Settings
// Assembly: MFDeploy, Version=4.3.1.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: 9518305E-2E58-43E8-9F45-41B78F83BC9E
// Assembly location: C:\Program Files (x86)\Microsoft .NET Micro Framework\v4.3\Tools\MFDeploy.exe

using System.CodeDom.Compiler;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties
{
  [CompilerGenerated]
  [GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "11.0.0.0")]
  internal sealed class Settings : ApplicationSettingsBase
  {
    private static Settings defaultInstance = (Settings) SettingsBase.Synchronized((SettingsBase) new Settings());

    public static Settings Default => Settings.defaultInstance;

    [DebuggerNonUserCode]
    [DefaultSettingValue("COM1")]
    [UserScopedSetting]
    public string DefaultSerialPort
    {
      get => (string) this[nameof (DefaultSerialPort)];
      set => this[nameof (DefaultSerialPort)] = (object) value;
    }

    [UserScopedSetting]
    [DefaultSettingValue("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfString xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <string>10.52.32.196</string>\r\n</ArrayOfString>")]
    [DebuggerNonUserCode]
    public StringCollection TcpIpAddresses
    {
      get => (StringCollection) this[nameof (TcpIpAddresses)];
      set => this[nameof (TcpIpAddresses)] = (object) value;
    }

    [DebuggerNonUserCode]
    [DefaultSettingValue("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfString xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" />")]
    [UserScopedSetting]
    public StringCollection MRUFiles
    {
      get => (StringCollection) this[nameof (MRUFiles)];
      set => this[nameof (MRUFiles)] = (object) value;
    }

    [UserScopedSetting]
    [DefaultSettingValue("234.102.98.44")]
    [DebuggerNonUserCode]
    public string DiscoveryMulticastAddress
    {
      get => (string) this[nameof (DiscoveryMulticastAddress)];
      set => this[nameof (DiscoveryMulticastAddress)] = (object) value;
    }

    [DefaultSettingValue("234.102.98.45")]
    [UserScopedSetting]
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

    [DefaultSettingValue("26001")]
    [UserScopedSetting]
    [DebuggerNonUserCode]
    public int DiscoveryMulticastPort
    {
      get => (int) this[nameof (DiscoveryMulticastPort)];
      set => this[nameof (DiscoveryMulticastPort)] = (object) value;
    }

    [DebuggerNonUserCode]
    [UserScopedSetting]
    [DefaultSettingValue("DOTNETMF")]
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

    [DebuggerNonUserCode]
    [UserScopedSetting]
    [DefaultSettingValue("Deployment; StorageA; StorageB")]
    public string EraseSectors
    {
      get => (string) this[nameof (EraseSectors)];
      set => this[nameof (EraseSectors)] = (object) value;
    }

    [DebuggerNonUserCode]
    [DefaultSettingValue("NetMFDeviceCert.pfx")]
    [UserScopedSetting]
    public string SslCert
    {
      get => (string) this[nameof (SslCert)];
      set => this[nameof (SslCert)] = (object) value;
    }

    [DefaultSettingValue("True")]
    [DebuggerNonUserCode]
    [UserScopedSetting]
    public bool SslRequireClientCert
    {
      get => (bool) this[nameof (SslRequireClientCert)];
      set => this[nameof (SslRequireClientCert)] = (object) value;
    }
  }
}
