// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Form1
// Assembly: MFDeploy, Version=2.0.0.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: 2BA2704E-D6D9-4FE0-ACF3-88D9CCA0E272
// Assembly location: D:\Androvil\Visual Studio 2022\Projects\MfDeploy\Tools\MFDeploy.exe

using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.PlugIns;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties;
using Microsoft.SPOT;
using Microsoft.SPOT.Debugger;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool
{
  public class Form1 : Form, IMFDeployForm
  {
    private MFDeploy m_deploy = new MFDeploy();
    private Thread m_pluginThread;
    private bool m_fShuttingDown;
    private MFPlugInMenuItem m_currentPlugInObject;
    private ToolStripMenuItem m_defaultSerialPort;
    private ToolStripMenuItem m_pluginMenu = new ToolStripMenuItem(Resources.ToolStripMenuPlugIn);
    private MFDevice m_device;
    private int m_deviceRefCount;
    private List<MFPlugInMenuItem> m_plugins = new List<MFPlugInMenuItem>();
    public bool m_fWaitForNewline;
    private MFPortDefinition m_transport;
    private MFPortDefinition m_transportTinyBooter;
    private IContainer components;
    private GroupBox groupBoxDevice;
    private Button buttonErase;
    private Button buttonPing;
    private ComboBox comboBoxDevice;
    private GroupBox groupBox1;
    private Button buttonDeploy;
    private Button buttonBrowse;
    private ComboBox comboBoxImageFile;
    private RichTextBox richTextBoxOutput;
    private Button button1;
    private MenuStrip menuStrip1;
    private ToolStripMenuItem pluginToolStripMenuItem;
    private ToolStripMenuItem cancelToolStripMenuItem;
    private ToolStripSeparator toolStripSeparator1;
    private ListView listViewFiles;
    private ColumnHeader columnHeaderFile;
    private ColumnHeader columnHeaderBaseAddress;
    private ColumnHeader columnHeaderSize;
    private ColumnHeader columnHeaderTimeStamp;
    private ComboBox comboBoxTransport;
    private ToolStripMenuItem optionsToolStripMenuItem;
    private ToolStripMenuItem defaultSerialPortToolStripMenuItem;
    private ToolStripMenuItem clearFileListToolStripMenuItem;
    private ToolStripMenuItem targetToolStripMenuItem;
    private ToolStripMenuItem publicKeyConfigurationToolStripMenuItem;
    private ToolStripMenuItem configurationToolStripMenuItem;
    private ToolStripMenuItem uSBToolStripMenuItem;
    private ToolStripMenuItem networkToolStripMenuItem;
    private ToolStripMenuItem applicationDeploymentToolStripMenuItem;
    private ToolStripMenuItem createApplicationDeploymentToolStripMenuItem1;
    private ToolStripMenuItem signDeploymentFileToolStripMenuItem1;
    private ToolStripMenuItem createKeyPairToolStripMenuItem;
    private ToolStripMenuItem updateDeviceKeysToolStripMenuItem;
    private ToolStripMenuItem helpToolStripMenuItem;
    private ToolStripMenuItem helpTopicsToolStripMenuItem;
    private ToolStripMenuItem aboutMFDeployToolStripMenuItem;
    private ToolStripMenuItem timeStampToolStripMenuItem;
    private ToolStripMenuItem connectToolStripMenuItem;
    private ToolStripMenuItem cancelToolStripMenuItem1;
    private ToolStripSeparator toolStripSeparator4;
    private ColumnHeader columnHeaderName;
    private ToolStripMenuItem deviceCapabilitiesToolStripMenuItem;

    public Form1() => this.InitializeComponent();

    public ReadOnlyCollection<string> Files
    {
      get
      {
        ReadOnlyCollection<string> files = (ReadOnlyCollection<string>) null;
        this.comboBoxDevice.Invoke((Action) (() => files = new ReadOnlyCollection<string>((IList<string>) this.comboBoxImageFile.Text.Trim().Split(new char[1]
        {
          ';'
        }, StringSplitOptions.RemoveEmptyEntries))));
        return files;
      }
    }

    public void DumpToOutput(string text) => this.DumpToOutput(text, true);

    public void DumpToOutput(string text, bool newLine)
    {
      if (this.m_fShuttingDown)
        return;
      this.richTextBoxOutput.Invoke((Action) (() =>
      {
        string text1 = "[" + DateTime.Now.ToLongTimeString() + " " + DateTime.Now.ToShortDateString() + "] ";
        if (this.timeStampToolStripMenuItem.Checked && !this.m_fWaitForNewline)
          this.richTextBoxOutput.AppendText(text1);
        newLine = newLine || text.EndsWith("\n");
        this.m_fWaitForNewline = !newLine;
        if (this.timeStampToolStripMenuItem.Checked)
          this.richTextBoxOutput.AppendText(text.TrimEnd('\r', '\n').Replace("\n", "\n" + text1));
        else
          this.richTextBoxOutput.AppendText(text.TrimEnd('\r', '\n'));
        if (newLine)
          this.richTextBoxOutput.AppendText("\r\n");
        this.richTextBoxOutput.ScrollToCaret();
      }));
    }

    public MFPortDefinition Transport
    {
      set => this.m_transport = value;
      get => this.m_transport;
    }

    public MFPortDefinition TransportTinyBooter
    {
      set => this.m_transportTinyBooter = value;
      get => this.m_transportTinyBooter;
    }

    private ReadOnlyCollection<MFPortDefinition> GeneratePortList()
    {
      ReadOnlyCollection<MFPortDefinition> portList = (ReadOnlyCollection<MFPortDefinition>) null;
      switch (this.comboBoxTransport.SelectedIndex)
      {
        case 0:
          portList = this.m_deploy.EnumPorts(new Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.TransportType[1]);
          break;
        case 1:
          portList = this.m_deploy.EnumPorts(Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.TransportType.USB);
          break;
        case 2:
          Cursor current = Cursor.Current;
          Cursor.Current = Cursors.WaitCursor;
          portList = this.m_deploy.EnumPorts(Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.TransportType.TCPIP);
          Cursor.Current = current;
          break;
      }
      return portList;
    }

    private void OnDeviceListUpdate(object sender, EventArgs e) => this.comboBoxDevice.Invoke((Action) (() =>
    {
      ReadOnlyCollection<MFPortDefinition> portList = this.GeneratePortList();
      this.comboBoxDevice.DataSource = (object) null;
      if (portList != null && portList.Count > 0)
      {
        this.comboBoxDevice.DataSource = (object) portList;
        if (portList[0] != null)
          this.comboBoxDevice.DisplayMember = "Name";
        this.comboBoxDevice.SelectedIndex = 0;
      }
      this.comboBoxDevice.Update();
    }));

    private void OnMenuClick(object sender, EventArgs ea)
    {
      if (!(sender is ToolStripItem toolStripItem))
        return;
      MFPlugInMenuItem tag = toolStripItem.Tag as MFPlugInMenuItem;
      if (this.m_pluginThread == null || !this.m_pluginThread.IsAlive)
      {
        this.m_currentPlugInObject = tag;
        if (!tag.RunInSeparateThread)
        {
          this.OnPlugInExecuteThread();
        }
        else
        {
          this.m_pluginThread = new Thread(new ThreadStart(this.OnPlugInExecuteThread));
          this.m_pluginThread.SetApartmentState(ApartmentState.STA);
          this.m_pluginThread.Start();
        }
      }
      else
        this.DumpToOutput(Resources.WarningPlugInPending);
    }

    private void OnDefaultSerialPortChange(object sender, EventArgs ea)
    {
      if (!(sender is ToolStripMenuItem toolStripMenuItem) || !(toolStripMenuItem.Tag is MFSerialPort tag))
        return;
      this.m_defaultSerialPort.Checked = false;
      Settings.Default.DefaultSerialPort = tag.Name;
      toolStripMenuItem.Checked = true;
      this.m_defaultSerialPort = toolStripMenuItem;
      this.m_transportTinyBooter = (MFPortDefinition) tag;
    }

    private void Form1_Load(object sender, EventArgs e)
    {
      this.comboBoxTransport.SelectedIndex = 0;
      this.m_deploy.OnDeviceListUpdate += new EventHandler<EventArgs>(this.OnDeviceListUpdate);
      if (this.m_transport != null)
        this.comboBoxDevice.Text = this.m_transport.Name;
      this.richTextBoxOutput.ScrollToCaret();
      foreach (MFSerialPort enumPort in this.m_deploy.EnumPorts(new Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.TransportType[1]))
      {
        ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem(enumPort.Name);
        toolStripMenuItem.Tag = (object) enumPort;
        toolStripMenuItem.Click += new EventHandler(this.OnDefaultSerialPortChange);
        this.defaultSerialPortToolStripMenuItem.DropDownItems.Add((ToolStripItem) toolStripMenuItem);
        if (Settings.Default.DefaultSerialPort == enumPort.Name)
        {
          this.m_defaultSerialPort = toolStripMenuItem;
          toolStripMenuItem.Checked = true;
          this.m_transportTinyBooter = (MFPortDefinition) enumPort;
        }
      }
      foreach (string mruFile in Settings.Default.MRUFiles)
      {
        bool flag = true;
        string str1 = mruFile;
        char[] separator = new char[1]{ ';' };
        foreach (string str2 in str1.Split(separator, StringSplitOptions.RemoveEmptyEntries))
        {
          string path = str2.Trim();
          if (path.Length > 0 && !System.IO.File.Exists(path))
            flag = false;
        }
        if (flag)
          this.comboBoxImageFile.Items.Add((object) mruFile);
      }
      try
      {
        this.m_deploy.DiscoveryMulticastAddress = IPAddress.Parse(Settings.Default.DiscoveryMulticastAddress);
      }
      catch
      {
        this.richTextBoxOutput.AppendText(string.Format(Resources.ErrorAppSettings, (object) "DiscoveryMulticastAddress"));
      }
      this.m_deploy.DiscoveryMulticastPort = Settings.Default.DiscoveryMulticastPort;
      this.m_deploy.DiscoveryMulticastToken = Settings.Default.DiscoveryMulticastToken;
      try
      {
        this.m_deploy.DiscoveryMulticastAddressRecv = IPAddress.Parse(Settings.Default.DiscoveryMulticastAddressRecv);
      }
      catch
      {
        this.richTextBoxOutput.AppendText(string.Format(Resources.ErrorAppSettings, (object) "DiscoveryMulticastAddressRecv"));
      }
      this.m_deploy.DiscoveryMulticastTimeout = Settings.Default.DiscoveryMulticastTimeout;
      this.m_deploy.DiscoveryTTL = Settings.Default.DiscoveryTTL;
      try
      {
        string plugIns = Settings.Default.PlugIns;
        if (plugIns == null || plugIns.Length <= 0)
          return;
        string path = AppDomain.CurrentDomain.BaseDirectory + plugIns;
        if (!Directory.Exists(path))
          return;
        foreach (string file in Directory.GetFiles(path, "*.dll"))
        {
          FileInfo fileInfo = new FileInfo(file);
          try
          {
            this.AddPlugIns(Assembly.LoadFrom(file).GetTypes(), fileInfo.Name.Substring(0, fileInfo.Name.Length - 4));
          }
          catch
          {
            this.DumpToOutput(string.Format(Resources.ErrorUnableToInstallPlugIn, (object) fileInfo.Name));
          }
        }
      }
      catch
      {
      }
    }

    private void OnCancelMenuClick(object sender, EventArgs e) => this.cancelToolStripMenuItem1_Click_1(sender, e);

    private int AddPlugIns(System.Type[] types, string menuName)
    {
      int num = 0;
      if (!this.menuStrip1.Items.Contains((ToolStripItem) this.m_pluginMenu))
      {
        this.menuStrip1.Items.Insert(3, (ToolStripItem) this.m_pluginMenu);
        this.m_pluginMenu.DropDownItems.Add((ToolStripItem) this.toolStripSeparator1);
        this.m_pluginMenu.DropDownItems.Add(Resources.MenuItemCancel).Click += new EventHandler(this.OnCancelMenuClick);
      }
      ToolStripMenuItem toolStripMenuItem1 = new ToolStripMenuItem(menuName);
      this.m_pluginMenu.DropDownItems.Insert(0, (ToolStripItem) toolStripMenuItem1);
      foreach (System.Type type in types)
      {
        if (type.IsSubclassOf(typeof (MFPlugInMenuItem)) && type.GetConstructor(new System.Type[0]).Invoke(new object[0]) is MFPlugInMenuItem mfPlugInMenuItem)
        {
          this.m_plugins.Add(mfPlugInMenuItem);
          ToolStripMenuItem toolStripMenuItem2 = new ToolStripMenuItem(mfPlugInMenuItem.Name);
          toolStripMenuItem2.Tag = (object) mfPlugInMenuItem;
          toolStripMenuItem2.Click += new EventHandler(this.OnMenuClick);
          toolStripMenuItem1.DropDownItems.Insert(0, (ToolStripItem) toolStripMenuItem2);
          ++num;
          if (mfPlugInMenuItem.Submenus != null)
          {
            foreach (MFPlugInMenuItem submenu in mfPlugInMenuItem.Submenus)
            {
              ToolStripMenuItem toolStripMenuItem3 = new ToolStripMenuItem(submenu.Name);
              toolStripMenuItem3.Tag = (object) submenu;
              toolStripMenuItem3.Click += new EventHandler(this.OnMenuClick);
              toolStripMenuItem2.DropDownItems.Add((ToolStripItem) toolStripMenuItem3);
            }
          }
        }
      }
      return num;
    }

    private MFPortDefinition GetSelectedItem()
    {
      if (this.comboBoxDevice.SelectedItem == null && this.comboBoxDevice.Text.Length == 0)
        return (MFPortDefinition) null;
      var selectedItem = this.comboBoxDevice.SelectedItem as MFPortDefinition;
      if (selectedItem == null)
      {
        ArgumentParser argumentParser = new ArgumentParser();
        if (argumentParser.ValidateArgs("/I:TcpIp:" + this.comboBoxDevice.Text, out string _))
          selectedItem = argumentParser.Interface;
      }
      if (selectedItem == null)
      {
        int num = (int) MessageBox.Show(Resources.ErrorInvalidDevice, Resources.Warning, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        this.comboBoxDevice.SelectionStart = 0;
        this.comboBoxDevice.SelectionLength = this.comboBoxDevice.Text.Length;
        this.comboBoxDevice.Focus();
      }
      return selectedItem;
    }

    private MFDevice ConnectToSelectedDevice()
    {
      if (this.m_device != null)
      {
        Interlocked.Increment(ref this.m_deviceRefCount);
      }
      else
      {
        MFPortDefinition port = (MFPortDefinition) null;
        this.Invoke((Action) (() => port = this.GetSelectedItem()));
        if (port != null)
        {
          try
          {
            this.m_device = this.m_deploy.Connect(port, port is MFTcpIpPort ? this.m_transportTinyBooter : (MFPortDefinition) null);
            if (this.m_device != null)
            {
              this.comboBoxTransport.Invoke((Action) (() =>
              {
                this.comboBoxDevice.Enabled = false;
                this.comboBoxTransport.Enabled = false;
                this.connectToolStripMenuItem.Enabled = false;
              }));
              this.m_device.OnDebugText += new EventHandler<DebugOutputEventArgs>(this.OnDbgTxt);
              Interlocked.Increment(ref this.m_deviceRefCount);
            }
          }
          catch (Exception ex)
          {
            this.DumpToOutput(Resources.ErrorPrefix + ex.Message);
          }
        }
      }
      return this.m_device;
    }

    private bool DisconnectFromSelectedDevice()
    {
      bool flag = this.m_device == null;
      if (this.m_device != null)
      {
        if (Interlocked.Decrement(ref this.m_deviceRefCount) <= 0)
        {
          this.Invoke((Action) (() =>
          {
            this.connectToolStripMenuItem.Checked = false;
            this.comboBoxDevice.Enabled = true;
            this.comboBoxTransport.Enabled = true;
            this.connectToolStripMenuItem.Enabled = true;
          }));
          flag = true;
          this.m_device.OnDebugText -= new EventHandler<DebugOutputEventArgs>(this.OnDbgTxt);
          this.m_device.Dispose();
          this.m_device = (MFDevice) null;
        }
      }
      else
        this.Invoke((Action) (() =>
        {
          this.connectToolStripMenuItem.Checked = false;
          this.comboBoxDevice.Enabled = true;
          this.comboBoxTransport.Enabled = true;
          this.connectToolStripMenuItem.Enabled = true;
        }));
      return flag;
    }

    private void buttonPing_Click(object sender, EventArgs e)
    {
      this.DumpToOutput(Resources.StatusPinging, false);
      Thread.Sleep(0);
      Cursor cursor = this.Cursor;
      this.Cursor = Cursors.WaitCursor;
      MFDevice selectedDevice = this.ConnectToSelectedDevice();
      if (selectedDevice != null)
      {
        this.DumpToOutput(selectedDevice.Ping().ToString());
        this.DisconnectFromSelectedDevice();
      }
      this.Cursor = cursor;
    }

    private void buttonErase_Click(object sender, EventArgs e)
    {
      MFDevice selectedDevice = this.ConnectToSelectedDevice();
      if (selectedDevice == null)
        return;
      if (DialogResult.Yes == MessageBox.Show(Resources.MessageEraseConfirmation, Resources.TitleEraseConfirmation, MessageBoxButtons.YesNo))
      {
        DeploymentStatusDialog deploymentStatusDialog = new DeploymentStatusDialog(selectedDevice);
        deploymentStatusDialog.StartPosition = FormStartPosition.CenterParent;
        int num = (int) deploymentStatusDialog.ShowDialog((IWin32Window) this);
      }
      this.DisconnectFromSelectedDevice();
    }

    private void RemoveMRUFiles(string files)
    {
      int index = this.comboBoxImageFile.Items.IndexOf((object) files);
      if (index == -1)
        return;
      Settings.Default.MRUFiles.Remove(files);
      this.comboBoxImageFile.Items.RemoveAt(index);
      Settings.Default.Save();
      if (this.comboBoxImageFile.Items.Count > 0)
        this.comboBoxImageFile.SelectedIndex = 0;
      else
        this.listViewFiles.Items.Clear();
    }

    private void AddMRUFiles(string files)
    {
      files = files.Trim();
      if (Settings.Default.MRUFiles.Contains(files))
      {
        Settings.Default.MRUFiles.Remove(files);
        this.comboBoxImageFile.Items.Remove((object) files);
      }
      Settings.Default.MRUFiles.Insert(0, files);
      Settings.Default.Save();
      this.comboBoxImageFile.Items.Insert(0, (object) files);
      this.comboBoxImageFile.SelectedIndex = 0;
    }

    private void buttonBrowse_Click(object sender, EventArgs e)
    {
      OpenFileDialog openFileDialog = new OpenFileDialog();
      openFileDialog.DefaultExt = "*.hex";
      openFileDialog.CheckFileExists = true;
      openFileDialog.Filter = Resources.OpenFileDialogFilterSREC;
      openFileDialog.FilterIndex = 0;
      openFileDialog.Multiselect = true;
      openFileDialog.InitialDirectory = this.comboBoxImageFile.Text;
      if (DialogResult.OK != openFileDialog.ShowDialog((IWin32Window) this))
        return;
      this.comboBoxImageFile.Text = string.Empty;
      string str = "";
      foreach (string fileName in openFileDialog.FileNames)
        str = str + fileName + "; ";
      this.comboBoxImageFile.Text = str.TrimEnd(' ', ';');
      this.AddMRUFiles(this.comboBoxImageFile.Text);
    }

    private void OnDbgTxt(object sender, DebugOutputEventArgs e)
    {
        if (e.Text.StartsWith("Type ") && e.Text.EndsWith(" bytes\r\n"))
        {
            return;
        }

        this.DumpToOutput(e.Text, false);
    }

    private void buttonDeploy_Click(object sender, EventArgs e)
    {
      ReadOnlyCollection<string> files = this.Files;
      string[] sig_files = new string[files.Count];
      int num1 = 0;
      if (files.Count <= 0)
      {
        int num2 = (int) MessageBox.Show(Resources.WarningNoFilesForDeploy, Resources.Warning, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      }
      else
      {
        foreach (string str in files)
        {
          if (str.Trim().Length != 0)
          {
            if (!System.IO.File.Exists(str))
            {
              int num3 = (int) MessageBox.Show((IWin32Window) this, string.Format(Resources.ErrorFileCantOpen, (object) str), Resources.ErrorTitleImageFile, MessageBoxButtons.OK, MessageBoxIcon.Hand);
              return;
            }
            string signatureFileName = this.GetSignatureFileName(str);
            sig_files[num1++] = signatureFileName;
          }
        }
        this.AddMRUFiles(this.comboBoxImageFile.Text.Trim());
        MFDevice selectedDevice = this.ConnectToSelectedDevice();
        if (selectedDevice == null)
          return;
        DeploymentStatusDialog deploymentStatusDialog = new DeploymentStatusDialog(selectedDevice, files, sig_files);
        deploymentStatusDialog.StartPosition = FormStartPosition.CenterParent;
        int num4 = (int) deploymentStatusDialog.ShowDialog((IWin32Window) this);
        this.DisconnectFromSelectedDevice();
      }
    }

    private void button1_Click(object sender, EventArgs e) => this.richTextBoxOutput.Text = string.Empty;

    private void comboBoxImageFile_DropDown(object sender, EventArgs e)
    {
      Graphics graphics = Graphics.FromHwnd(this.comboBoxImageFile.Handle);
      int num = this.comboBoxImageFile.Width;
      foreach (string text in this.comboBoxImageFile.Items)
      {
        int width = (int) graphics.MeasureString(text, this.comboBoxImageFile.Font).Width;
        if (width > num)
          num = width < 1000 ? width : 1000;
      }
      graphics.Dispose();
      this.comboBoxImageFile.DropDownWidth = num;
    }

    private void OnPlugInExecuteThread()
    {
      if (this.m_currentPlugInObject == null)
        return;
      try
      {
        this.m_deploy.OpenWithoutConnect = !this.m_currentPlugInObject.RequiresConnection;
        MFDevice selectedDevice = this.ConnectToSelectedDevice();
        if (selectedDevice == null)
          return;
        if (this.m_currentPlugInObject.RunInSeparateThread)
          this.DumpToOutput(string.Format(Resources.XCommand, (object) this.m_currentPlugInObject.Name));
        if (this.m_currentPlugInObject.RequiresConnection && !selectedDevice.DbgEngine.TryToConnect(5, 100))
          throw new MFDeviceNoResponseException();
        this.m_currentPlugInObject.OnAction((IMFDeployForm) this, selectedDevice);
      }
      catch (ThreadAbortException ex)
      {
      }
      catch (Exception ex)
      {
        this.DumpToOutput(Resources.ErrorPrefix + ex.Message);
      }
      finally
      {
        this.DisconnectFromSelectedDevice();
        this.m_deploy.OpenWithoutConnect = false;
        if (this.m_currentPlugInObject.RunInSeparateThread)
          this.DumpToOutput(string.Format(Resources.StatusXComplete, (object) this.m_currentPlugInObject.Name));
        this.m_currentPlugInObject = (MFPlugInMenuItem) null;
      }
    }

    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
      this.m_fShuttingDown = true;
      if (this.m_pluginThread != null && this.m_pluginThread.IsAlive)
        this.m_pluginThread.Abort();
      if (this.m_device != null)
      {
        this.m_device.Dispose();
        this.m_device = (MFDevice) null;
      }
      if (this.m_deploy == null)
        return;
      this.m_deploy.Dispose();
      this.m_deploy = (MFDeploy) null;
    }

    private string GetSignatureFileName(string hexFile)
    {
      string str = hexFile;
      FileInfo fileInfo = new FileInfo(hexFile);
      if (fileInfo.Extension != null || fileInfo.Extension.Length > 0)
      {
        int startIndex = hexFile.LastIndexOf(fileInfo.Extension);
        str = hexFile.Remove(startIndex, fileInfo.Extension.Length);
      }
      return str + ".sig";
    }

    private void comboBoxImageFile_SelectedIndexChanged(object sender, EventArgs e)
    {
    }

    private void comboBoxImageFile_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode != Keys.Delete || !e.Control || this.comboBoxImageFile.SelectedItem == null || this.comboBoxImageFile.DroppedDown)
        return;
      this.RemoveMRUFiles(this.comboBoxImageFile.SelectedItem as string);
      e.Handled = true;
    }

    private void comboBoxTransport_SelectedIndexChanged(object sender, EventArgs e) => this.OnDeviceListUpdate((object) null, (EventArgs) null);

    private void clearFileListToolStripMenuItem_Click(object sender, EventArgs e)
    {
      Settings.Default.MRUFiles.Clear();
      Settings.Default.Save();
      this.comboBoxImageFile.Items.Clear();
      this.comboBoxImageFile.Text = string.Empty;
      this.comboBoxImageFile_SelectedIndexChanged((object) null, (EventArgs) null);
    }

    private void OnMenuItem_Click(object sender, EventArgs e)
    {
      ToolStripMenuItem toolStripMenuItem = sender as ToolStripMenuItem;
      Form form = (Form) null;
      if (toolStripMenuItem == null)
        return;
      MFDevice device = (MFDevice) null;
      MFConfigHelper mfConfigHelper = (MFConfigHelper) null;
      Cursor current = Cursor.Current;
      Cursor.Current = Cursors.WaitCursor;
      try
      {
        if (toolStripMenuItem != this.signDeploymentFileToolStripMenuItem1)
        {
          device = this.ConnectToSelectedDevice();
          mfConfigHelper = new MFConfigHelper(device);
          if (!mfConfigHelper.IsValidConfig)
          {
            int num = (int) MessageBox.Show((IWin32Window) this, Resources.ErrorUnsupportedConfiguration, Resources.ApplicationTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            return;
          }
        }
        if (toolStripMenuItem == this.signDeploymentFileToolStripMenuItem1)
          form = (Form) new MFAppDeployConfigDialog(MFAppDeployConfigDialog.ConfigDialogCommand.SignDeployment);
        else if (toolStripMenuItem == this.updateDeviceKeysToolStripMenuItem)
          form = (Form) new MFAppDeployConfigDialog(device, MFAppDeployConfigDialog.ConfigDialogCommand.Configure);
        else if (toolStripMenuItem == this.createApplicationDeploymentToolStripMenuItem1)
          form = (Form) new MFAppDeployConfigDialog(device, MFAppDeployConfigDialog.ConfigDialogCommand.CreateDeployment);
        else if (toolStripMenuItem == this.uSBToolStripMenuItem)
          form = (Form) new MFUsbConfigDialog(device);
        else if (toolStripMenuItem == this.networkToolStripMenuItem)
          form = (Form) new MFNetworkConfigDialog(device);
        form.StartPosition = FormStartPosition.CenterParent;
        if (DialogResult.OK != form.ShowDialog() || !(form is MFUsbConfigDialog))
          return;
        do
          ;
        while (!this.DisconnectFromSelectedDevice());
        Thread.Sleep(2000);
        this.OnDeviceListUpdate((object) null, (EventArgs) null);
      }
      catch (Exception ex)
      {
        int num = (int) MessageBox.Show((IWin32Window) this, string.Format(Resources.ErrorX, (object) ex.Message), Resources.ApplicationTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
      }
      finally
      {
        Cursor.Current = current;
        mfConfigHelper?.Dispose();
        this.DisconnectFromSelectedDevice();
        form?.Dispose();
      }
    }

    private void createKeyPairToolStripMenuItem_Click(object sender, EventArgs e) => MFAppDeployConfigDialog.ShowCreateKeyPairFileDialog();

    private void aboutMFDeployToolStripMenuItem_Click(object sender, EventArgs e)
    {
      int num = (int) new MFAboutBox().ShowDialog();
    }

    private void helpTopicsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      try
      {
        Help.ShowHelp((Control) this, Resources.MFHelpFilename);
      }
      catch
      {
        int num = (int) MessageBox.Show(Resources.MFHelpError);
      }
    }

    private void cLRCapabilitiesToolStripMenuItem_Click(object sender, EventArgs e)
    {
      try
      {
        MFDevice selectedDevice = this.ConnectToSelectedDevice();
        if (selectedDevice == null)
          return;
        Microsoft.SPOT.Debugger.Engine dbgEngine = selectedDevice.DbgEngine;
        dbgEngine.TryToConnect(0, 100, true, ConnectionSource.Unknown);
        CLRCapabilities capabilities = dbgEngine.Capabilities;
        if (capabilities == null || capabilities.IsUnknown)
        {
          this.DumpToOutput(Resources.ErrorNotSupported);
          this.DisconnectFromSelectedDevice();
        }
        else
        {
          foreach (PropertyInfo property in typeof (CLRCapabilities).GetProperties())
          {
            object obj = property.GetValue((object) capabilities, (object[]) null);
            try
            {
              switch (obj)
              {
                case CLRCapabilities.LCDCapabilities _:
                case CLRCapabilities.SoftwareVersionProperties _:
                case CLRCapabilities.EvaluationVersionProperties _:
                  foreach (FieldInfo field in property.PropertyType.GetFields())
                    this.DumpToOutput(string.Format("{0,-34}{1}", (object) (property.Name + "." + field.Name + ":"), field.GetValue(obj)));
                  continue;
                default:
                  this.DumpToOutput(string.Format("{0,-34}{1}", (object) (property.Name + ":"), obj));
                  continue;
              }
            }
            catch
            {
              this.DumpToOutput(Resources.ErrorNotSupported);
            }
          }
          this.DisconnectFromSelectedDevice();
        }
      }
      catch
      {
        this.DumpToOutput(Resources.ErrorNotSupported);
      }
    }

    private void listenToolStripMenuItem_Click(object sender, EventArgs e)
    {
      this.DumpToOutput(string.Format(Resources.ConnectingToX, (object) this.comboBoxDevice.Text), false);
      try
      {
        Cursor cursor = this.Cursor;
        this.Cursor = Cursors.WaitCursor;
        if (this.comboBoxTransport.Text == Resources.Serial)
          this.m_deploy.OpenWithoutConnect = true;
        this.ConnectToSelectedDevice();
        this.Cursor = cursor;
      }
      catch
      {
        this.DisconnectFromSelectedDevice();
      }
      if (this.m_device != null)
        this.DumpToOutput(Resources.Connected);
      else
        this.DumpToOutput(string.Format(Resources.ErrorDeviceNotResponding, (object) this.comboBoxDevice.Text));
    }

    private void cancelToolStripMenuItem1_Click_1(object sender, EventArgs e)
    {
      this.DumpToOutput(Resources.UserCanceled);
      if (this.m_currentPlugInObject != null)
      {
        if (this.m_pluginThread != null && this.m_pluginThread.IsAlive)
        {
          this.m_pluginThread.Abort();
          this.m_pluginThread = (Thread) null;
        }
      }
      else
      {
        while (!this.DisconnectFromSelectedDevice())
          ;
      }
      this.m_deploy.OpenWithoutConnect = false;
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (Form1));
      this.groupBoxDevice = new GroupBox();
      this.comboBoxTransport = new ComboBox();
      this.buttonErase = new Button();
      this.buttonPing = new Button();
      this.comboBoxDevice = new ComboBox();
      this.groupBox1 = new GroupBox();
      this.buttonDeploy = new Button();
      this.buttonBrowse = new Button();
      this.comboBoxImageFile = new ComboBox();
      this.richTextBoxOutput = new RichTextBox();
      this.button1 = new Button();
      this.menuStrip1 = new MenuStrip();
      this.targetToolStripMenuItem = new ToolStripMenuItem();
      this.applicationDeploymentToolStripMenuItem = new ToolStripMenuItem();
      this.createApplicationDeploymentToolStripMenuItem1 = new ToolStripMenuItem();
      this.signDeploymentFileToolStripMenuItem1 = new ToolStripMenuItem();
      this.publicKeyConfigurationToolStripMenuItem = new ToolStripMenuItem();
      this.createKeyPairToolStripMenuItem = new ToolStripMenuItem();
      this.updateDeviceKeysToolStripMenuItem = new ToolStripMenuItem();
      this.configurationToolStripMenuItem = new ToolStripMenuItem();
      this.uSBToolStripMenuItem = new ToolStripMenuItem();
      this.networkToolStripMenuItem = new ToolStripMenuItem();
      this.toolStripSeparator4 = new ToolStripSeparator();
      this.connectToolStripMenuItem = new ToolStripMenuItem();
      this.cancelToolStripMenuItem1 = new ToolStripMenuItem();
      this.pluginToolStripMenuItem = new ToolStripMenuItem();
      this.toolStripSeparator1 = new ToolStripSeparator();
      this.cancelToolStripMenuItem = new ToolStripMenuItem();
      this.optionsToolStripMenuItem = new ToolStripMenuItem();
      this.defaultSerialPortToolStripMenuItem = new ToolStripMenuItem();
      this.clearFileListToolStripMenuItem = new ToolStripMenuItem();
      this.timeStampToolStripMenuItem = new ToolStripMenuItem();
      this.helpToolStripMenuItem = new ToolStripMenuItem();
      this.helpTopicsToolStripMenuItem = new ToolStripMenuItem();
      this.aboutMFDeployToolStripMenuItem = new ToolStripMenuItem();
      this.listViewFiles = new ListView();
      this.columnHeaderName = new ColumnHeader();
      this.columnHeaderFile = new ColumnHeader();
      this.columnHeaderBaseAddress = new ColumnHeader();
      this.columnHeaderSize = new ColumnHeader();
      this.columnHeaderTimeStamp = new ColumnHeader();
      this.deviceCapabilitiesToolStripMenuItem = new ToolStripMenuItem();
      this.groupBoxDevice.SuspendLayout();
      this.groupBox1.SuspendLayout();
      this.menuStrip1.SuspendLayout();
      this.SuspendLayout();
      this.groupBoxDevice.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      this.groupBoxDevice.Controls.Add((Control) this.comboBoxTransport);
      this.groupBoxDevice.Controls.Add((Control) this.buttonErase);
      this.groupBoxDevice.Controls.Add((Control) this.buttonPing);
      this.groupBoxDevice.Controls.Add((Control) this.comboBoxDevice);
      this.groupBoxDevice.Location = new Point(12, 34);
      this.groupBoxDevice.Name = "groupBoxDevice";
      this.groupBoxDevice.Size = new Size(673, 53);
      this.groupBoxDevice.TabIndex = 0;
      this.groupBoxDevice.TabStop = false;
      this.groupBoxDevice.Text = "De&vice";
      this.comboBoxTransport.DropDownStyle = ComboBoxStyle.DropDownList;
      this.comboBoxTransport.FormattingEnabled = true;
      this.comboBoxTransport.Items.AddRange(new object[3]
      {
        (object) Resources.TransportSerial,
        (object) Resources.TransportUsb,
        (object) Resources.TransportTcpIp
      });
      this.comboBoxTransport.Location = new Point(6, 19);
      this.comboBoxTransport.Name = "comboBoxTransport";
      this.comboBoxTransport.Size = new Size(87, 21);
      this.comboBoxTransport.TabIndex = 3;
      this.comboBoxTransport.SelectedIndexChanged += new EventHandler(this.comboBoxTransport_SelectedIndexChanged);
      this.buttonErase.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.buttonErase.Location = new Point(592, 17);
      this.buttonErase.Name = "buttonErase";
      this.buttonErase.Size = new Size(75, 23);
      this.buttonErase.TabIndex = 2;
      this.buttonErase.Text = Resources.ButtonErase;
      this.buttonErase.UseVisualStyleBackColor = true;
      this.buttonErase.Click += new EventHandler(this.buttonErase_Click);
      this.buttonPing.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.buttonPing.Location = new Point(511, 17);
      this.buttonPing.Name = "buttonPing";
      this.buttonPing.Size = new Size(75, 23);
      this.buttonPing.TabIndex = 1;
      this.buttonPing.Text = Resources.ButtonPing;
      this.buttonPing.UseVisualStyleBackColor = true;
      this.buttonPing.Click += new EventHandler(this.buttonPing_Click);
      this.comboBoxDevice.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      this.comboBoxDevice.FormattingEnabled = true;
      this.comboBoxDevice.Location = new Point(99, 19);
      this.comboBoxDevice.Name = "comboBoxDevice";
      this.comboBoxDevice.Size = new Size(406, 21);
      this.comboBoxDevice.TabIndex = 0;
      this.groupBox1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      this.groupBox1.Controls.Add((Control) this.buttonDeploy);
      this.groupBox1.Controls.Add((Control) this.buttonBrowse);
      this.groupBox1.Controls.Add((Control) this.comboBoxImageFile);
      this.groupBox1.Location = new Point(12, 93);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new Size(673, 53);
      this.groupBox1.TabIndex = 1;
      this.groupBox1.TabStop = false;
      this.groupBox1.Text = "&Image File";
      this.buttonDeploy.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.buttonDeploy.Location = new Point(592, 17);
      this.buttonDeploy.Name = "buttonDeploy";
      this.buttonDeploy.Size = new Size(75, 23);
      this.buttonDeploy.TabIndex = 2;
      this.buttonDeploy.Text = Resources.ButtonDeploy;
      this.buttonDeploy.UseVisualStyleBackColor = true;
      this.buttonDeploy.Click += new EventHandler(this.buttonDeploy_Click);
      this.buttonBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
      this.buttonBrowse.Location = new Point(511, 17);
      this.buttonBrowse.Name = "buttonBrowse";
      this.buttonBrowse.Size = new Size(75, 23);
      this.buttonBrowse.TabIndex = 1;
      this.buttonBrowse.Text = Resources.ButtonBrowse;
      this.buttonBrowse.UseVisualStyleBackColor = true;
      this.buttonBrowse.Click += new EventHandler(this.buttonBrowse_Click);
      this.comboBoxImageFile.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      this.comboBoxImageFile.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
      this.comboBoxImageFile.AutoCompleteSource = AutoCompleteSource.FileSystem;
      this.comboBoxImageFile.FormattingEnabled = true;
      this.comboBoxImageFile.Location = new Point(6, 19);
      this.comboBoxImageFile.Name = "comboBoxImageFile";
      this.comboBoxImageFile.Size = new Size(499, 21);
      this.comboBoxImageFile.TabIndex = 0;
      this.comboBoxImageFile.SelectedIndexChanged += new EventHandler(this.comboBoxImageFile_SelectedIndexChanged);
      this.comboBoxImageFile.KeyDown += new KeyEventHandler(this.comboBoxImageFile_KeyDown);
      this.comboBoxImageFile.DropDown += new EventHandler(this.comboBoxImageFile_DropDown);
      this.richTextBoxOutput.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
      this.richTextBoxOutput.Font = new Font("Courier New", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.richTextBoxOutput.Location = new Point(18, 159);
      this.richTextBoxOutput.Name = "richTextBoxOutput";
      this.richTextBoxOutput.ReadOnly = true;
      this.richTextBoxOutput.Size = new Size(661, 212);
      this.richTextBoxOutput.TabIndex = 2;
      this.richTextBoxOutput.Text = "";
      this.button1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
      this.button1.Location = new Point(18, 377);
      this.button1.Name = "button1";
      this.button1.Size = new Size(75, 23);
      this.button1.TabIndex = 3;
      this.button1.Text = Resources.ButtonClear;
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new EventHandler(this.button1_Click);
      this.menuStrip1.BackColor = SystemColors.Control;
      this.menuStrip1.Items.AddRange(new ToolStripItem[4]
      {
        (ToolStripItem) this.targetToolStripMenuItem,
        (ToolStripItem) this.pluginToolStripMenuItem,
        (ToolStripItem) this.optionsToolStripMenuItem,
        (ToolStripItem) this.helpToolStripMenuItem
      });
      this.menuStrip1.Location = new Point(0, 0);
      this.menuStrip1.Name = "menuStrip1";
      this.menuStrip1.Size = new Size(697, 24);
      this.menuStrip1.TabIndex = 8;
      this.menuStrip1.Text = "menuStrip1";
      this.targetToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[7]
      {
        (ToolStripItem) this.applicationDeploymentToolStripMenuItem,
        (ToolStripItem) this.publicKeyConfigurationToolStripMenuItem,
        (ToolStripItem) this.configurationToolStripMenuItem,
        (ToolStripItem) this.deviceCapabilitiesToolStripMenuItem,
        (ToolStripItem) this.toolStripSeparator4,
        (ToolStripItem) this.connectToolStripMenuItem,
        (ToolStripItem) this.cancelToolStripMenuItem1
      });
      this.targetToolStripMenuItem.Name = "targetToolStripMenuItem";
      this.targetToolStripMenuItem.Size = new Size(53, 20);
      this.targetToolStripMenuItem.Text = "Target";
      this.applicationDeploymentToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[2]
      {
        (ToolStripItem) this.createApplicationDeploymentToolStripMenuItem1,
        (ToolStripItem) this.signDeploymentFileToolStripMenuItem1
      });
      this.applicationDeploymentToolStripMenuItem.Name = "applicationDeploymentToolStripMenuItem";
      this.applicationDeploymentToolStripMenuItem.Size = new Size(247, 22);
      this.applicationDeploymentToolStripMenuItem.Text = "Application Deployment";
      this.createApplicationDeploymentToolStripMenuItem1.Name = "createApplicationDeploymentToolStripMenuItem1";
      this.createApplicationDeploymentToolStripMenuItem1.Size = new Size(240, 22);
      this.createApplicationDeploymentToolStripMenuItem1.Text = "Create Application Deployment";
      this.createApplicationDeploymentToolStripMenuItem1.Click += new EventHandler(this.OnMenuItem_Click);
      this.signDeploymentFileToolStripMenuItem1.Name = "signDeploymentFileToolStripMenuItem1";
      this.signDeploymentFileToolStripMenuItem1.Size = new Size(240, 22);
      this.signDeploymentFileToolStripMenuItem1.Text = "Sign Deployment File";
      this.signDeploymentFileToolStripMenuItem1.Click += new EventHandler(this.OnMenuItem_Click);
      this.publicKeyConfigurationToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[2]
      {
        (ToolStripItem) this.createKeyPairToolStripMenuItem,
        (ToolStripItem) this.updateDeviceKeysToolStripMenuItem
      });
      this.publicKeyConfigurationToolStripMenuItem.Name = "publicKeyConfigurationToolStripMenuItem";
      this.publicKeyConfigurationToolStripMenuItem.Size = new Size(247, 22);
      this.publicKeyConfigurationToolStripMenuItem.Text = "Manage Device Keys";
      this.createKeyPairToolStripMenuItem.Name = "createKeyPairToolStripMenuItem";
      this.createKeyPairToolStripMenuItem.Size = new Size(177, 22);
      this.createKeyPairToolStripMenuItem.Text = "Create Key Pair";
      this.createKeyPairToolStripMenuItem.Click += new EventHandler(this.createKeyPairToolStripMenuItem_Click);
      this.updateDeviceKeysToolStripMenuItem.Name = "updateDeviceKeysToolStripMenuItem";
      this.updateDeviceKeysToolStripMenuItem.Size = new Size(177, 22);
      this.updateDeviceKeysToolStripMenuItem.Text = "Update Device Keys";
      this.updateDeviceKeysToolStripMenuItem.Click += new EventHandler(this.OnMenuItem_Click);
      this.configurationToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[2]
      {
        (ToolStripItem) this.uSBToolStripMenuItem,
        (ToolStripItem) this.networkToolStripMenuItem
      });
      this.configurationToolStripMenuItem.Name = "configurationToolStripMenuItem";
      this.configurationToolStripMenuItem.Size = new Size(247, 22);
      this.configurationToolStripMenuItem.Text = "Configuration";
      this.uSBToolStripMenuItem.Name = "uSBToolStripMenuItem";
      this.uSBToolStripMenuItem.Size = new Size(152, 22);
      this.uSBToolStripMenuItem.Text = "USB Name";
      this.uSBToolStripMenuItem.Click += new EventHandler(this.OnMenuItem_Click);
      this.networkToolStripMenuItem.Name = "networkToolStripMenuItem";
      this.networkToolStripMenuItem.Size = new Size(152, 22);
      this.networkToolStripMenuItem.Text = "Network";
      this.networkToolStripMenuItem.Click += new EventHandler(this.OnMenuItem_Click);
      this.toolStripSeparator4.Name = "toolStripSeparator4";
      this.toolStripSeparator4.Size = new Size(244, 6);
      this.connectToolStripMenuItem.Name = "connectToolStripMenuItem";
      this.connectToolStripMenuItem.ShortcutKeys = Keys.F5;
      this.connectToolStripMenuItem.Size = new Size(247, 22);
      this.connectToolStripMenuItem.Text = "Connect";
      this.connectToolStripMenuItem.Click += new EventHandler(this.listenToolStripMenuItem_Click);
      this.cancelToolStripMenuItem1.Name = "cancelToolStripMenuItem1";
      this.cancelToolStripMenuItem1.ShortcutKeys = Keys.F5 | Keys.Control;
      this.cancelToolStripMenuItem1.Size = new Size(247, 22);
      this.cancelToolStripMenuItem1.Text = "Disconnect";
      this.cancelToolStripMenuItem1.Click += new EventHandler(this.cancelToolStripMenuItem1_Click_1);
      this.pluginToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[2]
      {
        (ToolStripItem) this.toolStripSeparator1,
        (ToolStripItem) this.cancelToolStripMenuItem
      });
      this.pluginToolStripMenuItem.Name = "pluginToolStripMenuItem";
      this.pluginToolStripMenuItem.Size = new Size(58, 20);
      this.pluginToolStripMenuItem.Text = Resources.ToolStripMenuPlugIn;
      this.pluginToolStripMenuItem.Visible = false;
      this.toolStripSeparator1.Name = "toolStripSeparator1";
      this.toolStripSeparator1.Size = new Size(149, 6);
      this.cancelToolStripMenuItem.Name = "cancelToolStripMenuItem";
      this.cancelToolStripMenuItem.Size = new Size(152, 22);
      this.optionsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[3]
      {
        (ToolStripItem) this.defaultSerialPortToolStripMenuItem,
        (ToolStripItem) this.clearFileListToolStripMenuItem,
        (ToolStripItem) this.timeStampToolStripMenuItem
      });
      this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
      this.optionsToolStripMenuItem.Size = new Size(61, 20);
      this.optionsToolStripMenuItem.Text = Resources.ToolStripMenuOptions;
      this.defaultSerialPortToolStripMenuItem.Name = "defaultSerialPortToolStripMenuItem";
      this.defaultSerialPortToolStripMenuItem.Size = new Size(188, 22);
      this.defaultSerialPortToolStripMenuItem.Text = "Bootloader Serial Port";
      this.clearFileListToolStripMenuItem.Name = "clearFileListToolStripMenuItem";
      this.clearFileListToolStripMenuItem.Size = new Size(188, 22);
      this.clearFileListToolStripMenuItem.Text = Resources.ToolStripMenuItemImageFileList;
      this.clearFileListToolStripMenuItem.Click += new EventHandler(this.clearFileListToolStripMenuItem_Click);
      this.timeStampToolStripMenuItem.CheckOnClick = true;
      this.timeStampToolStripMenuItem.Name = "timeStampToolStripMenuItem";
      this.timeStampToolStripMenuItem.ShortcutKeys = Keys.T | Keys.Control;
      this.timeStampToolStripMenuItem.Size = new Size(188, 22);
      this.timeStampToolStripMenuItem.Text = "Time Stamp";
      this.helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[2]
      {
        (ToolStripItem) this.helpTopicsToolStripMenuItem,
        (ToolStripItem) this.aboutMFDeployToolStripMenuItem
      });
      this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
      this.helpToolStripMenuItem.Size = new Size(44, 20);
      this.helpToolStripMenuItem.Text = "&Help";
      this.helpTopicsToolStripMenuItem.Name = "helpTopicsToolStripMenuItem";
      this.helpTopicsToolStripMenuItem.ShortcutKeys = Keys.F1;
      this.helpTopicsToolStripMenuItem.Size = new Size(164, 22);
      this.helpTopicsToolStripMenuItem.Text = "&Help Topics";
      this.helpTopicsToolStripMenuItem.Click += new EventHandler(this.helpTopicsToolStripMenuItem_Click);
      this.aboutMFDeployToolStripMenuItem.Name = "aboutMFDeployToolStripMenuItem";
      this.aboutMFDeployToolStripMenuItem.Size = new Size(164, 22);
      this.aboutMFDeployToolStripMenuItem.Text = "&About MFDeploy";
      this.aboutMFDeployToolStripMenuItem.Click += new EventHandler(this.aboutMFDeployToolStripMenuItem_Click);
      this.listViewFiles.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
      this.listViewFiles.BackColor = SystemColors.Window;
      this.listViewFiles.CheckBoxes = true;
      this.listViewFiles.Columns.AddRange(new ColumnHeader[5]
      {
        this.columnHeaderName,
        this.columnHeaderFile,
        this.columnHeaderBaseAddress,
        this.columnHeaderSize,
        this.columnHeaderTimeStamp
      });
      this.listViewFiles.Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
      this.listViewFiles.FullRowSelect = true;
      this.listViewFiles.GridLines = true;
      this.listViewFiles.HideSelection = false;
      this.listViewFiles.Location = new Point(18, 159);
      this.listViewFiles.Name = "listViewFiles";
      this.listViewFiles.Size = new Size(661, 100);
      this.listViewFiles.TabIndex = 9;
      this.listViewFiles.UseCompatibleStateImageBehavior = false;
      this.listViewFiles.View = View.Details;
      this.listViewFiles.Visible = false;
      this.columnHeaderName.Tag = (object) "";
      this.columnHeaderName.Text = "Name";
      this.columnHeaderName.Width = 123;
      this.columnHeaderFile.Tag = (object) "";
      this.columnHeaderFile.Text = Resources.ColumnHeaderFile;
      this.columnHeaderFile.Width = 249;
      this.columnHeaderBaseAddress.Text = Resources.ColumnHeaderBaseAddr;
      this.columnHeaderBaseAddress.Width = 80;
      this.columnHeaderSize.Text = Resources.ColumnHeaderSize;
      this.columnHeaderSize.Width = 68;
      this.columnHeaderTimeStamp.Text = Resources.ColumnHeaderTimeStamp;
      this.columnHeaderTimeStamp.Width = 135;
      this.deviceCapabilitiesToolStripMenuItem.Name = "deviceCapabilitiesToolStripMenuItem";
      this.deviceCapabilitiesToolStripMenuItem.ShortcutKeys = Keys.C | Keys.Shift | Keys.Control;
      this.deviceCapabilitiesToolStripMenuItem.Size = new Size(247, 22);
      this.deviceCapabilitiesToolStripMenuItem.Text = "Device Capabilities";
      this.deviceCapabilitiesToolStripMenuItem.Click += new EventHandler(this.cLRCapabilitiesToolStripMenuItem_Click);
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.BackColor = SystemColors.Control;
      this.ClientSize = new Size(697, 409);
      this.Controls.Add((Control) this.listViewFiles);
      this.Controls.Add((Control) this.button1);
      this.Controls.Add((Control) this.richTextBoxOutput);
      this.Controls.Add((Control) this.groupBox1);
      this.Controls.Add((Control) this.groupBoxDevice);
      this.Controls.Add((Control) this.menuStrip1);
      this.Icon = (Icon) componentResourceManager.GetObject("$this.Icon");
      this.MainMenuStrip = this.menuStrip1;
      this.MinimumSize = new Size(300, 250);
      this.Name = nameof (Form1);
      this.Text = ".NET Micro Framework Deployment Tool";
      this.FormClosing += new FormClosingEventHandler(this.Form1_FormClosing);
      this.Load += new EventHandler(this.Form1_Load);
      this.groupBoxDevice.ResumeLayout(false);
      this.groupBox1.ResumeLayout(false);
      this.menuStrip1.ResumeLayout(false);
      this.menuStrip1.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();
    }

    private enum TransportComboBoxType
    {
      Serial,
      Usb,
      TcpIp,
    }
  }
}
