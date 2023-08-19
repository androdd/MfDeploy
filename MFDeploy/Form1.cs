namespace Microsoft.NetMicroFramework.Tools.MFDeployTool
{
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

    using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
    using Microsoft.NetMicroFramework.Tools.MFDeployTool.PlugIns;
    using Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties;
    using Microsoft.SPOT;
    using Microsoft.SPOT.Debugger;

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

        public Form1() => InitializeComponent();

        public ReadOnlyCollection<string> Files
        {
            get
            {
                ReadOnlyCollection<string> files = (ReadOnlyCollection<string>)null;
                comboBoxDevice.Invoke((Action)(() => files = new ReadOnlyCollection<string>((IList<string>)comboBoxImageFile.Text.Trim()
                    .Split(new char[1]
                        {
                            ';'
                        },
                        StringSplitOptions.RemoveEmptyEntries))));
                return files;
            }
        }

        public void DumpToOutput(string text) => DumpToOutput(text, true);

        public void DumpToOutput(string text, bool newLine)
        {
            if (m_fShuttingDown)
                return;
            richTextBoxOutput.Invoke((Action)(() =>
            {
                string text1 = "[" + DateTime.Now.ToLongTimeString() + " " + DateTime.Now.ToShortDateString() + "] ";
                if (timeStampToolStripMenuItem.Checked && !m_fWaitForNewline)
                    richTextBoxOutput.AppendText(text1);
                newLine = newLine || text.EndsWith("\n");
                m_fWaitForNewline = !newLine;
                if (timeStampToolStripMenuItem.Checked)
                    richTextBoxOutput.AppendText(text.TrimEnd('\r', '\n').Replace("\n", "\n" + text1));
                else
                    richTextBoxOutput.AppendText(text.TrimEnd('\r', '\n'));
                if (newLine)
                    richTextBoxOutput.AppendText("\r\n");
                richTextBoxOutput.ScrollToCaret();
            }));
        }

        public MFPortDefinition Transport
        {
            set => m_transport = value;
            get => m_transport;
        }

        public MFPortDefinition TransportTinyBooter
        {
            set => m_transportTinyBooter = value;
            get => m_transportTinyBooter;
        }

        private ReadOnlyCollection<MFPortDefinition> GeneratePortList()
        {
            ReadOnlyCollection<MFPortDefinition> portList = (ReadOnlyCollection<MFPortDefinition>)null;
            switch (comboBoxTransport.SelectedIndex)
            {
                case 0:
                    portList = m_deploy.EnumPorts(Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.TransportType.USB);
                    break;
                case 1:
                    portList = m_deploy.EnumPorts(new Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.TransportType[1]);
                    break;
                case 2:
                    Cursor current = Cursor.Current;
                    Cursor.Current = Cursors.WaitCursor;
                    portList = m_deploy.EnumPorts(Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.TransportType.TCPIP);
                    Cursor.Current = current;
                    break;
            }

            return portList;
        }

        private void OnDeviceListUpdate(object sender, EventArgs e) => comboBoxDevice.Invoke((Action)(() =>
        {
            ReadOnlyCollection<MFPortDefinition> portList = GeneratePortList();
            comboBoxDevice.DataSource = (object)null;
            if (portList != null && portList.Count > 0)
            {
                comboBoxDevice.DataSource = (object)portList;
                if (portList[0] != null)
                    comboBoxDevice.DisplayMember = "Name";
                comboBoxDevice.SelectedIndex = 0;
            }

            comboBoxDevice.Update();
        }));

        private void OnMenuClick(object sender, EventArgs ea)
        {
            if (!(sender is ToolStripItem toolStripItem))
                return;
            MFPlugInMenuItem tag = toolStripItem.Tag as MFPlugInMenuItem;
            if (m_pluginThread == null || !m_pluginThread.IsAlive)
            {
                m_currentPlugInObject = tag;
                if (!tag.RunInSeparateThread)
                {
                    OnPlugInExecuteThread();
                }
                else
                {
                    m_pluginThread = new Thread(new ThreadStart(OnPlugInExecuteThread));
                    m_pluginThread.SetApartmentState(ApartmentState.STA);
                    m_pluginThread.Start();
                }
            }
            else
                DumpToOutput(Resources.WarningPlugInPending);
        }

        private void OnDefaultSerialPortChange(object sender, EventArgs ea)
        {
            if (!(sender is ToolStripMenuItem toolStripMenuItem) || !(toolStripMenuItem.Tag is MFSerialPort tag))
                return;
            m_defaultSerialPort.Checked = false;
            Settings.Default.DefaultSerialPort = tag.Name;
            toolStripMenuItem.Checked = true;
            m_defaultSerialPort = toolStripMenuItem;
            m_transportTinyBooter = (MFPortDefinition)tag;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBoxTransport.SelectedIndex = 0;
            m_deploy.OnDeviceListUpdate += new EventHandler<EventArgs>(OnDeviceListUpdate);
            if (m_transport != null)
                comboBoxDevice.Text = m_transport.Name;
            richTextBoxOutput.ScrollToCaret();
            foreach (MFSerialPort enumPort in m_deploy.EnumPorts(new Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.TransportType[1]))
            {
                ToolStripMenuItem toolStripMenuItem = new ToolStripMenuItem(enumPort.Name);
                toolStripMenuItem.Tag = (object)enumPort;
                toolStripMenuItem.Click += new EventHandler(OnDefaultSerialPortChange);
                defaultSerialPortToolStripMenuItem.DropDownItems.Add((ToolStripItem)toolStripMenuItem);
                if (Settings.Default.DefaultSerialPort == enumPort.Name)
                {
                    m_defaultSerialPort = toolStripMenuItem;
                    toolStripMenuItem.Checked = true;
                    m_transportTinyBooter = (MFPortDefinition)enumPort;
                }
            }

            foreach (string mruFile in Settings.Default.MRUFiles)
            {
                bool flag = true;
                string str1 = mruFile;
                char[] separator = new char[1] { ';' };
                foreach (string str2 in str1.Split(separator, StringSplitOptions.RemoveEmptyEntries))
                {
                    string path = str2.Trim();
                    if (path.Length > 0 && !System.IO.File.Exists(path))
                        flag = false;
                }

                if (flag)
                    comboBoxImageFile.Items.Add((object)mruFile);
            }

            try
            {
                m_deploy.DiscoveryMulticastAddress = IPAddress.Parse(Settings.Default.DiscoveryMulticastAddress);
            }
            catch
            {
                richTextBoxOutput.AppendText(string.Format(Resources.ErrorAppSettings, (object)"DiscoveryMulticastAddress"));
            }

            m_deploy.DiscoveryMulticastPort = Settings.Default.DiscoveryMulticastPort;
            m_deploy.DiscoveryMulticastToken = Settings.Default.DiscoveryMulticastToken;
            try
            {
                m_deploy.DiscoveryMulticastAddressRecv = IPAddress.Parse(Settings.Default.DiscoveryMulticastAddressRecv);
            }
            catch
            {
                richTextBoxOutput.AppendText(string.Format(Resources.ErrorAppSettings, (object)"DiscoveryMulticastAddressRecv"));
            }

            m_deploy.DiscoveryMulticastTimeout = Settings.Default.DiscoveryMulticastTimeout;
            m_deploy.DiscoveryTTL = Settings.Default.DiscoveryTTL;
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
                        AddPlugIns(Assembly.LoadFrom(file).GetTypes(), fileInfo.Name.Substring(0, fileInfo.Name.Length - 4));
                    }
                    catch
                    {
                        DumpToOutput(string.Format(Resources.ErrorUnableToInstallPlugIn, (object)fileInfo.Name));
                    }
                }
            }
            catch
            {
            }
        }

        private void OnCancelMenuClick(object sender, EventArgs e) => cancelToolStripMenuItem1_Click_1(sender, e);

        private int AddPlugIns(System.Type[] types, string menuName)
        {
            int num = 0;
            if (!menuStrip1.Items.Contains((ToolStripItem)m_pluginMenu))
            {
                menuStrip1.Items.Insert(3, (ToolStripItem)m_pluginMenu);
                m_pluginMenu.DropDownItems.Add((ToolStripItem)toolStripSeparator1);
                m_pluginMenu.DropDownItems.Add(Resources.MenuItemCancel).Click += new EventHandler(OnCancelMenuClick);
            }

            ToolStripMenuItem toolStripMenuItem1 = new ToolStripMenuItem(menuName);
            m_pluginMenu.DropDownItems.Insert(0, (ToolStripItem)toolStripMenuItem1);
            foreach (System.Type type in types)
            {
                if (type.IsSubclassOf(typeof(MFPlugInMenuItem)) &&
                    type.GetConstructor(new System.Type[0]).Invoke(new object[0]) is MFPlugInMenuItem mfPlugInMenuItem)
                {
                    m_plugins.Add(mfPlugInMenuItem);
                    ToolStripMenuItem toolStripMenuItem2 = new ToolStripMenuItem(mfPlugInMenuItem.Name);
                    toolStripMenuItem2.Tag = (object)mfPlugInMenuItem;
                    toolStripMenuItem2.Click += new EventHandler(OnMenuClick);
                    toolStripMenuItem1.DropDownItems.Insert(0, (ToolStripItem)toolStripMenuItem2);
                    ++num;
                    if (mfPlugInMenuItem.Submenus != null)
                    {
                        foreach (MFPlugInMenuItem submenu in mfPlugInMenuItem.Submenus)
                        {
                            ToolStripMenuItem toolStripMenuItem3 = new ToolStripMenuItem(submenu.Name);
                            toolStripMenuItem3.Tag = (object)submenu;
                            toolStripMenuItem3.Click += new EventHandler(OnMenuClick);
                            toolStripMenuItem2.DropDownItems.Add((ToolStripItem)toolStripMenuItem3);
                        }
                    }
                }
            }

            return num;
        }

        private MFPortDefinition GetSelectedItem()
        {
            if (comboBoxDevice.SelectedItem == null && comboBoxDevice.Text.Length == 0)
                return (MFPortDefinition)null;
            var selectedItem = comboBoxDevice.SelectedItem as MFPortDefinition;
            if (selectedItem == null)
            {
                ArgumentParser argumentParser = new ArgumentParser();
                if (argumentParser.ValidateArgs("/I:TcpIp:" + comboBoxDevice.Text, out string _))
                    selectedItem = argumentParser.Interface;
            }

            if (selectedItem == null)
            {
                int num = (int)MessageBox.Show(Resources.ErrorInvalidDevice, Resources.Warning, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                comboBoxDevice.SelectionStart = 0;
                comboBoxDevice.SelectionLength = comboBoxDevice.Text.Length;
                comboBoxDevice.Focus();
            }

            return selectedItem;
        }

        private MFDevice ConnectToSelectedDevice()
        {
            if (m_device != null)
            {
                Interlocked.Increment(ref m_deviceRefCount);
            }
            else
            {
                MFPortDefinition port = (MFPortDefinition)null;
                Invoke((Action)(() => port = GetSelectedItem()));
                if (port != null)
                {
                    try
                    {
                        m_device = m_deploy.Connect(port,
                            port is MFTcpIpPort
                                ? m_transportTinyBooter
                                : (MFPortDefinition)null);
                        if (m_device != null)
                        {
                            comboBoxTransport.Invoke((Action)(() =>
                            {
                                comboBoxDevice.Enabled = false;
                                comboBoxTransport.Enabled = false;
                                connectToolStripMenuItem.Enabled = false;
                            }));
                            m_device.OnDebugText += new EventHandler<DebugOutputEventArgs>(OnDbgTxt);
                            Interlocked.Increment(ref m_deviceRefCount);
                        }
                    }
                    catch (Exception ex)
                    {
                        DumpToOutput(Resources.ErrorPrefix + ex.Message);
                    }
                }
            }

            return m_device;
        }

        private bool DisconnectFromSelectedDevice()
        {
            bool flag = m_device == null;
            if (m_device != null)
            {
                if (Interlocked.Decrement(ref m_deviceRefCount) <= 0)
                {
                    Invoke((Action)(() =>
                    {
                        connectToolStripMenuItem.Checked = false;
                        comboBoxDevice.Enabled = true;
                        comboBoxTransport.Enabled = true;
                        connectToolStripMenuItem.Enabled = true;
                    }));
                    flag = true;
                    m_device.OnDebugText -= new EventHandler<DebugOutputEventArgs>(OnDbgTxt);
                    m_device.Dispose();
                    m_device = (MFDevice)null;
                }
            }
            else
                Invoke((Action)(() =>
                {
                    connectToolStripMenuItem.Checked = false;
                    comboBoxDevice.Enabled = true;
                    comboBoxTransport.Enabled = true;
                    connectToolStripMenuItem.Enabled = true;
                }));

            return flag;
        }

        private void buttonPing_Click(object sender, EventArgs e)
        {
            DumpToOutput(Resources.StatusPinging, false);
            Thread.Sleep(0);
            Cursor cursor = Cursor;
            Cursor = Cursors.WaitCursor;
            MFDevice selectedDevice = ConnectToSelectedDevice();
            if (selectedDevice != null)
            {
                DumpToOutput(selectedDevice.Ping().ToString());
                DisconnectFromSelectedDevice();
            }

            Cursor = cursor;
        }

        private void buttonErase_Click(object sender, EventArgs e)
        {
            MFDevice selectedDevice = ConnectToSelectedDevice();
            if (selectedDevice == null)
                return;
            if (DialogResult.Yes == MessageBox.Show(Resources.MessageEraseConfirmation, Resources.TitleEraseConfirmation, MessageBoxButtons.YesNo))
            {
                DeploymentStatusDialog deploymentStatusDialog = new DeploymentStatusDialog(selectedDevice);
                deploymentStatusDialog.StartPosition = FormStartPosition.CenterParent;
                int num = (int)deploymentStatusDialog.ShowDialog((IWin32Window)this);
            }

            DisconnectFromSelectedDevice();
        }

        private void RemoveMRUFiles(string files)
        {
            int index = comboBoxImageFile.Items.IndexOf((object)files);
            if (index == -1)
                return;
            Settings.Default.MRUFiles.Remove(files);
            comboBoxImageFile.Items.RemoveAt(index);
            Settings.Default.Save();
            if (comboBoxImageFile.Items.Count > 0)
                comboBoxImageFile.SelectedIndex = 0;
            else
                listViewFiles.Items.Clear();
        }

        private void AddMRUFiles(string files)
        {
            files = files.Trim();
            if (Settings.Default.MRUFiles.Contains(files))
            {
                Settings.Default.MRUFiles.Remove(files);
                comboBoxImageFile.Items.Remove((object)files);
            }

            Settings.Default.MRUFiles.Insert(0, files);
            Settings.Default.Save();
            comboBoxImageFile.Items.Insert(0, (object)files);
            comboBoxImageFile.SelectedIndex = 0;
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = "*.hex";
            openFileDialog.CheckFileExists = true;
            openFileDialog.Filter = Resources.OpenFileDialogFilterSREC;
            openFileDialog.FilterIndex = 0;
            openFileDialog.Multiselect = true;
            openFileDialog.InitialDirectory = comboBoxImageFile.Text;
            if (DialogResult.OK != openFileDialog.ShowDialog((IWin32Window)this))
                return;
            comboBoxImageFile.Text = string.Empty;
            string str = "";
            foreach (string fileName in openFileDialog.FileNames)
                str = str + fileName + "; ";
            comboBoxImageFile.Text = str.TrimEnd(' ', ';');
            AddMRUFiles(comboBoxImageFile.Text);
        }

        private void OnDbgTxt(object sender, DebugOutputEventArgs e)
        {
            if (e.Text.StartsWith("Type ") && e.Text.EndsWith(" bytes\r\n"))
            {
                return;
            }

            DumpToOutput(e.Text, false);
        }

        private void buttonDeploy_Click(object sender, EventArgs e)
        {
            ReadOnlyCollection<string> files = Files;
            string[] sig_files = new string[files.Count];
            int num1 = 0;
            if (files.Count <= 0)
            {
                int num2 = (int)MessageBox.Show(Resources.WarningNoFilesForDeploy,
                    Resources.Warning,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
            }
            else
            {
                foreach (string str in files)
                {
                    if (str.Trim().Length != 0)
                    {
                        if (!System.IO.File.Exists(str))
                        {
                            int num3 = (int)MessageBox.Show((IWin32Window)this,
                                string.Format(Resources.ErrorFileCantOpen, (object)str),
                                Resources.ErrorTitleImageFile,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Hand);
                            return;
                        }

                        string signatureFileName = GetSignatureFileName(str);
                        sig_files[num1++] = signatureFileName;
                    }
                }

                AddMRUFiles(comboBoxImageFile.Text.Trim());
                MFDevice selectedDevice = ConnectToSelectedDevice();
                if (selectedDevice == null)
                    return;
                DeploymentStatusDialog deploymentStatusDialog = new DeploymentStatusDialog(selectedDevice, files, sig_files);
                deploymentStatusDialog.StartPosition = FormStartPosition.CenterParent;
                int num4 = (int)deploymentStatusDialog.ShowDialog((IWin32Window)this);
                DisconnectFromSelectedDevice();
            }
        }

        private void button1_Click(object sender, EventArgs e) => richTextBoxOutput.Text = string.Empty;

        private void comboBoxImageFile_DropDown(object sender, EventArgs e)
        {
            Graphics graphics = Graphics.FromHwnd(comboBoxImageFile.Handle);
            int num = comboBoxImageFile.Width;
            foreach (string text in comboBoxImageFile.Items)
            {
                int width = (int)graphics.MeasureString(text, comboBoxImageFile.Font).Width;
                if (width > num)
                    num = width < 1000
                        ? width
                        : 1000;
            }

            graphics.Dispose();
            comboBoxImageFile.DropDownWidth = num;
        }

        private void OnPlugInExecuteThread()
        {
            if (m_currentPlugInObject == null)
                return;
            try
            {
                m_deploy.OpenWithoutConnect = !m_currentPlugInObject.RequiresConnection;
                MFDevice selectedDevice = ConnectToSelectedDevice();
                if (selectedDevice == null)
                    return;
                if (m_currentPlugInObject.RunInSeparateThread)
                    DumpToOutput(string.Format(Resources.XCommand, (object)m_currentPlugInObject.Name));
                if (m_currentPlugInObject.RequiresConnection && !selectedDevice.DbgEngine.TryToConnect(5, 100))
                    throw new MFDeviceNoResponseException();
                m_currentPlugInObject.OnAction((IMFDeployForm)this, selectedDevice);
            }
            catch (ThreadAbortException ex)
            {
            }
            catch (Exception ex)
            {
                DumpToOutput(Resources.ErrorPrefix + ex.Message);
            }
            finally
            {
                DisconnectFromSelectedDevice();
                m_deploy.OpenWithoutConnect = false;
                if (m_currentPlugInObject.RunInSeparateThread)
                    DumpToOutput(string.Format(Resources.StatusXComplete, (object)m_currentPlugInObject.Name));
                m_currentPlugInObject = (MFPlugInMenuItem)null;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_fShuttingDown = true;
            if (m_pluginThread != null && m_pluginThread.IsAlive)
                m_pluginThread.Abort();
            if (m_device != null)
            {
                m_device.Dispose();
                m_device = (MFDevice)null;
            }

            if (m_deploy == null)
                return;
            m_deploy.Dispose();
            m_deploy = (MFDeploy)null;
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
            if (e.KeyCode != Keys.Delete || !e.Control || comboBoxImageFile.SelectedItem == null || comboBoxImageFile.DroppedDown)
                return;
            RemoveMRUFiles(comboBoxImageFile.SelectedItem as string);
            e.Handled = true;
        }

        private void comboBoxTransport_SelectedIndexChanged(object sender, EventArgs e) => OnDeviceListUpdate((object)null, (EventArgs)null);

        private void clearFileListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings.Default.MRUFiles.Clear();
            Settings.Default.Save();
            comboBoxImageFile.Items.Clear();
            comboBoxImageFile.Text = string.Empty;
            comboBoxImageFile_SelectedIndexChanged((object)null, (EventArgs)null);
        }

        private void OnMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem toolStripMenuItem = sender as ToolStripMenuItem;
            Form form = (Form)null;
            if (toolStripMenuItem == null)
                return;
            MFDevice device = (MFDevice)null;
            MFConfigHelper mfConfigHelper = (MFConfigHelper)null;
            Cursor current = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                if (toolStripMenuItem != signDeploymentFileToolStripMenuItem1)
                {
                    device = ConnectToSelectedDevice();
                    mfConfigHelper = new MFConfigHelper(device);
                    if (!mfConfigHelper.IsValidConfig)
                    {
                        int num = (int)MessageBox.Show((IWin32Window)this,
                            Resources.ErrorUnsupportedConfiguration,
                            Resources.ApplicationTitle,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Exclamation);
                        return;
                    }
                }

                if (toolStripMenuItem == signDeploymentFileToolStripMenuItem1)
                    form = (Form)new MFAppDeployConfigDialog(MFAppDeployConfigDialog.ConfigDialogCommand.SignDeployment);
                else if (toolStripMenuItem == updateDeviceKeysToolStripMenuItem)
                    form = (Form)new MFAppDeployConfigDialog(device, MFAppDeployConfigDialog.ConfigDialogCommand.Configure);
                else if (toolStripMenuItem == createApplicationDeploymentToolStripMenuItem1)
                    form = (Form)new MFAppDeployConfigDialog(device, MFAppDeployConfigDialog.ConfigDialogCommand.CreateDeployment);
                else if (toolStripMenuItem == uSBToolStripMenuItem)
                    form = (Form)new MFUsbConfigDialog(device);
                else if (toolStripMenuItem == networkToolStripMenuItem)
                    form = (Form)new MFNetworkConfigDialog(device);
                form.StartPosition = FormStartPosition.CenterParent;
                if (DialogResult.OK != form.ShowDialog() || !(form is MFUsbConfigDialog))
                    return;
                do
                    ;
                while (!DisconnectFromSelectedDevice());
                Thread.Sleep(2000);
                OnDeviceListUpdate((object)null, (EventArgs)null);
            }
            catch (Exception ex)
            {
                int num = (int)MessageBox.Show((IWin32Window)this,
                    string.Format(Resources.ErrorX, (object)ex.Message),
                    Resources.ApplicationTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
            }
            finally
            {
                Cursor.Current = current;
                mfConfigHelper?.Dispose();
                DisconnectFromSelectedDevice();
                form?.Dispose();
            }
        }

        private void createKeyPairToolStripMenuItem_Click(object sender, EventArgs e) => MFAppDeployConfigDialog.ShowCreateKeyPairFileDialog();

        private void aboutMFDeployToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int num = (int)new MFAboutBox().ShowDialog();
        }

        private void helpTopicsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Help.ShowHelp((Control)this, Resources.MFHelpFilename);
            }
            catch
            {
                int num = (int)MessageBox.Show(Resources.MFHelpError);
            }
        }

        private void cLRCapabilitiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                MFDevice selectedDevice = ConnectToSelectedDevice();
                if (selectedDevice == null)
                    return;
                Microsoft.SPOT.Debugger.Engine dbgEngine = selectedDevice.DbgEngine;
                dbgEngine.TryToConnect(0, 100, true, ConnectionSource.Unknown);
                CLRCapabilities capabilities = dbgEngine.Capabilities;
                if (capabilities == null || capabilities.IsUnknown)
                {
                    DumpToOutput(Resources.ErrorNotSupported);
                    DisconnectFromSelectedDevice();
                }
                else
                {
                    foreach (PropertyInfo property in typeof(CLRCapabilities).GetProperties())
                    {
                        object obj = property.GetValue((object)capabilities, (object[])null);
                        try
                        {
                            switch (obj)
                            {
                                case CLRCapabilities.LCDCapabilities _:
                                case CLRCapabilities.SoftwareVersionProperties _:
                                case CLRCapabilities.EvaluationVersionProperties _:
                                    foreach (FieldInfo field in property.PropertyType.GetFields())
                                        DumpToOutput(string.Format("{0,-34}{1}",
                                            (object)(property.Name + "." + field.Name + ":"),
                                            field.GetValue(obj)));
                                    continue;
                                default:
                                    DumpToOutput(string.Format("{0,-34}{1}", (object)(property.Name + ":"), obj));
                                    continue;
                            }
                        }
                        catch
                        {
                            DumpToOutput(Resources.ErrorNotSupported);
                        }
                    }

                    DisconnectFromSelectedDevice();
                }
            }
            catch
            {
                DumpToOutput(Resources.ErrorNotSupported);
            }
        }

        private void listenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DumpToOutput(string.Format(Resources.ConnectingToX, (object)comboBoxDevice.Text), false);
            try
            {
                Cursor cursor = Cursor;
                Cursor = Cursors.WaitCursor;
                if (comboBoxTransport.Text == Resources.Serial)
                    m_deploy.OpenWithoutConnect = true;
                ConnectToSelectedDevice();
                Cursor = cursor;
            }
            catch
            {
                DisconnectFromSelectedDevice();
            }

            if (m_device != null)
                DumpToOutput(Resources.Connected);
            else
                DumpToOutput(string.Format(Resources.ErrorDeviceNotResponding, (object)comboBoxDevice.Text));
        }

        private void cancelToolStripMenuItem1_Click_1(object sender, EventArgs e)
        {
            DumpToOutput(Resources.UserCanceled);
            if (m_currentPlugInObject != null)
            {
                if (m_pluginThread != null && m_pluginThread.IsAlive)
                {
                    m_pluginThread.Abort();
                    m_pluginThread = (Thread)null;
                }
            }
            else
            {
                while (!DisconnectFromSelectedDevice())
                    ;
            }

            m_deploy.OpenWithoutConnect = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.groupBoxDevice = new System.Windows.Forms.GroupBox();
            this.comboBoxTransport = new System.Windows.Forms.ComboBox();
            this.buttonErase = new System.Windows.Forms.Button();
            this.buttonPing = new System.Windows.Forms.Button();
            this.comboBoxDevice = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonDeploy = new System.Windows.Forms.Button();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.comboBoxImageFile = new System.Windows.Forms.ComboBox();
            this.richTextBoxOutput = new System.Windows.Forms.RichTextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.targetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.applicationDeploymentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createApplicationDeploymentToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.signDeploymentFileToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.publicKeyConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createKeyPairToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateDeviceKeysToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.configurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.uSBToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.networkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deviceCapabilitiesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.connectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cancelToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.pluginToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.cancelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.defaultSerialPortToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearFileListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timeStampToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpTopicsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutMFDeployToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.listViewFiles = new System.Windows.Forms.ListView();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFile = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderBaseAddress = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderTimeStamp = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.groupBoxDevice.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxDevice
            // 
            this.groupBoxDevice.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxDevice.Controls.Add(this.comboBoxTransport);
            this.groupBoxDevice.Controls.Add(this.buttonErase);
            this.groupBoxDevice.Controls.Add(this.buttonPing);
            this.groupBoxDevice.Controls.Add(this.comboBoxDevice);
            this.groupBoxDevice.Location = new System.Drawing.Point(16, 42);
            this.groupBoxDevice.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBoxDevice.Name = "groupBoxDevice";
            this.groupBoxDevice.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBoxDevice.Size = new System.Drawing.Size(897, 65);
            this.groupBoxDevice.TabIndex = 0;
            this.groupBoxDevice.TabStop = false;
            this.groupBoxDevice.Text = "De&vice";
            // 
            // comboBoxTransport
            // 
            this.comboBoxTransport.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTransport.FormattingEnabled = true;
            this.comboBoxTransport.Items.AddRange(new object[] {
            "USB",
            "Serial",
            "TCP/IP"});
            this.comboBoxTransport.Location = new System.Drawing.Point(8, 23);
            this.comboBoxTransport.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboBoxTransport.Name = "comboBoxTransport";
            this.comboBoxTransport.Size = new System.Drawing.Size(115, 24);
            this.comboBoxTransport.TabIndex = 3;
            this.comboBoxTransport.SelectedIndexChanged += new System.EventHandler(this.comboBoxTransport_SelectedIndexChanged);
            // 
            // buttonErase
            // 
            this.buttonErase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonErase.Location = new System.Drawing.Point(789, 21);
            this.buttonErase.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonErase.Name = "buttonErase";
            this.buttonErase.Size = new System.Drawing.Size(100, 28);
            this.buttonErase.TabIndex = 2;
            this.buttonErase.Text = global::Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties.Resources.ButtonErase;
            this.buttonErase.UseVisualStyleBackColor = true;
            this.buttonErase.Click += new System.EventHandler(this.buttonErase_Click);
            // 
            // buttonPing
            // 
            this.buttonPing.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPing.Location = new System.Drawing.Point(681, 21);
            this.buttonPing.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonPing.Name = "buttonPing";
            this.buttonPing.Size = new System.Drawing.Size(100, 28);
            this.buttonPing.TabIndex = 1;
            this.buttonPing.Text = global::Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties.Resources.ButtonPing;
            this.buttonPing.UseVisualStyleBackColor = true;
            this.buttonPing.Click += new System.EventHandler(this.buttonPing_Click);
            // 
            // comboBoxDevice
            // 
            this.comboBoxDevice.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxDevice.FormattingEnabled = true;
            this.comboBoxDevice.Location = new System.Drawing.Point(132, 23);
            this.comboBoxDevice.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboBoxDevice.Name = "comboBoxDevice";
            this.comboBoxDevice.Size = new System.Drawing.Size(540, 24);
            this.comboBoxDevice.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.buttonDeploy);
            this.groupBox1.Controls.Add(this.buttonBrowse);
            this.groupBox1.Controls.Add(this.comboBoxImageFile);
            this.groupBox1.Location = new System.Drawing.Point(16, 114);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Size = new System.Drawing.Size(897, 65);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "&Image File";
            // 
            // buttonDeploy
            // 
            this.buttonDeploy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDeploy.Location = new System.Drawing.Point(789, 21);
            this.buttonDeploy.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonDeploy.Name = "buttonDeploy";
            this.buttonDeploy.Size = new System.Drawing.Size(100, 28);
            this.buttonDeploy.TabIndex = 2;
            this.buttonDeploy.Text = global::Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties.Resources.ButtonDeploy;
            this.buttonDeploy.UseVisualStyleBackColor = true;
            this.buttonDeploy.Click += new System.EventHandler(this.buttonDeploy_Click);
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBrowse.Location = new System.Drawing.Point(681, 21);
            this.buttonBrowse.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(100, 28);
            this.buttonBrowse.TabIndex = 1;
            this.buttonBrowse.Text = global::Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties.Resources.ButtonBrowse;
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // comboBoxImageFile
            // 
            this.comboBoxImageFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxImageFile.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.comboBoxImageFile.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.FileSystem;
            this.comboBoxImageFile.FormattingEnabled = true;
            this.comboBoxImageFile.Location = new System.Drawing.Point(8, 23);
            this.comboBoxImageFile.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboBoxImageFile.Name = "comboBoxImageFile";
            this.comboBoxImageFile.Size = new System.Drawing.Size(664, 24);
            this.comboBoxImageFile.TabIndex = 0;
            this.comboBoxImageFile.DropDown += new System.EventHandler(this.comboBoxImageFile_DropDown);
            this.comboBoxImageFile.SelectedIndexChanged += new System.EventHandler(this.comboBoxImageFile_SelectedIndexChanged);
            this.comboBoxImageFile.KeyDown += new System.Windows.Forms.KeyEventHandler(this.comboBoxImageFile_KeyDown);
            // 
            // richTextBoxOutput
            // 
            this.richTextBoxOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxOutput.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.richTextBoxOutput.Location = new System.Drawing.Point(24, 196);
            this.richTextBoxOutput.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.richTextBoxOutput.Name = "richTextBoxOutput";
            this.richTextBoxOutput.ReadOnly = true;
            this.richTextBoxOutput.Size = new System.Drawing.Size(880, 260);
            this.richTextBoxOutput.TabIndex = 2;
            this.richTextBoxOutput.Text = "";
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button1.Location = new System.Drawing.Point(24, 464);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 28);
            this.button1.TabIndex = 3;
            this.button1.Text = global::Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties.Resources.ButtonClear;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.SystemColors.Control;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.targetToolStripMenuItem,
            this.pluginToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(8, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(929, 24);
            this.menuStrip1.TabIndex = 8;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // targetToolStripMenuItem
            // 
            this.targetToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.applicationDeploymentToolStripMenuItem,
            this.publicKeyConfigurationToolStripMenuItem,
            this.configurationToolStripMenuItem,
            this.deviceCapabilitiesToolStripMenuItem,
            this.toolStripSeparator4,
            this.connectToolStripMenuItem,
            this.cancelToolStripMenuItem1});
            this.targetToolStripMenuItem.Name = "targetToolStripMenuItem";
            this.targetToolStripMenuItem.Size = new System.Drawing.Size(51, 20);
            this.targetToolStripMenuItem.Text = "Target";
            // 
            // applicationDeploymentToolStripMenuItem
            // 
            this.applicationDeploymentToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.createApplicationDeploymentToolStripMenuItem1,
            this.signDeploymentFileToolStripMenuItem1});
            this.applicationDeploymentToolStripMenuItem.Name = "applicationDeploymentToolStripMenuItem";
            this.applicationDeploymentToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
            this.applicationDeploymentToolStripMenuItem.Text = "Application Deployment";
            // 
            // createApplicationDeploymentToolStripMenuItem1
            // 
            this.createApplicationDeploymentToolStripMenuItem1.Name = "createApplicationDeploymentToolStripMenuItem1";
            this.createApplicationDeploymentToolStripMenuItem1.Size = new System.Drawing.Size(240, 22);
            this.createApplicationDeploymentToolStripMenuItem1.Text = "Create Application Deployment";
            this.createApplicationDeploymentToolStripMenuItem1.Click += new System.EventHandler(this.OnMenuItem_Click);
            // 
            // signDeploymentFileToolStripMenuItem1
            // 
            this.signDeploymentFileToolStripMenuItem1.Name = "signDeploymentFileToolStripMenuItem1";
            this.signDeploymentFileToolStripMenuItem1.Size = new System.Drawing.Size(240, 22);
            this.signDeploymentFileToolStripMenuItem1.Text = "Sign Deployment File";
            this.signDeploymentFileToolStripMenuItem1.Click += new System.EventHandler(this.OnMenuItem_Click);
            // 
            // publicKeyConfigurationToolStripMenuItem
            // 
            this.publicKeyConfigurationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.createKeyPairToolStripMenuItem,
            this.updateDeviceKeysToolStripMenuItem});
            this.publicKeyConfigurationToolStripMenuItem.Name = "publicKeyConfigurationToolStripMenuItem";
            this.publicKeyConfigurationToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
            this.publicKeyConfigurationToolStripMenuItem.Text = "Manage Device Keys";
            // 
            // createKeyPairToolStripMenuItem
            // 
            this.createKeyPairToolStripMenuItem.Name = "createKeyPairToolStripMenuItem";
            this.createKeyPairToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.createKeyPairToolStripMenuItem.Text = "Create Key Pair";
            this.createKeyPairToolStripMenuItem.Click += new System.EventHandler(this.createKeyPairToolStripMenuItem_Click);
            // 
            // updateDeviceKeysToolStripMenuItem
            // 
            this.updateDeviceKeysToolStripMenuItem.Name = "updateDeviceKeysToolStripMenuItem";
            this.updateDeviceKeysToolStripMenuItem.Size = new System.Drawing.Size(177, 22);
            this.updateDeviceKeysToolStripMenuItem.Text = "Update Device Keys";
            this.updateDeviceKeysToolStripMenuItem.Click += new System.EventHandler(this.OnMenuItem_Click);
            // 
            // configurationToolStripMenuItem
            // 
            this.configurationToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.uSBToolStripMenuItem,
            this.networkToolStripMenuItem});
            this.configurationToolStripMenuItem.Name = "configurationToolStripMenuItem";
            this.configurationToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
            this.configurationToolStripMenuItem.Text = "Configuration";
            // 
            // uSBToolStripMenuItem
            // 
            this.uSBToolStripMenuItem.Name = "uSBToolStripMenuItem";
            this.uSBToolStripMenuItem.Size = new System.Drawing.Size(130, 22);
            this.uSBToolStripMenuItem.Text = "USB Name";
            this.uSBToolStripMenuItem.Click += new System.EventHandler(this.OnMenuItem_Click);
            // 
            // networkToolStripMenuItem
            // 
            this.networkToolStripMenuItem.Name = "networkToolStripMenuItem";
            this.networkToolStripMenuItem.Size = new System.Drawing.Size(130, 22);
            this.networkToolStripMenuItem.Text = "Network";
            this.networkToolStripMenuItem.Click += new System.EventHandler(this.OnMenuItem_Click);
            // 
            // deviceCapabilitiesToolStripMenuItem
            // 
            this.deviceCapabilitiesToolStripMenuItem.Name = "deviceCapabilitiesToolStripMenuItem";
            this.deviceCapabilitiesToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.C)));
            this.deviceCapabilitiesToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
            this.deviceCapabilitiesToolStripMenuItem.Text = "Device Capabilities";
            this.deviceCapabilitiesToolStripMenuItem.Click += new System.EventHandler(this.cLRCapabilitiesToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(244, 6);
            // 
            // connectToolStripMenuItem
            // 
            this.connectToolStripMenuItem.Name = "connectToolStripMenuItem";
            this.connectToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.connectToolStripMenuItem.Size = new System.Drawing.Size(247, 22);
            this.connectToolStripMenuItem.Text = "Connect";
            this.connectToolStripMenuItem.Click += new System.EventHandler(this.listenToolStripMenuItem_Click);
            // 
            // cancelToolStripMenuItem1
            // 
            this.cancelToolStripMenuItem1.Name = "cancelToolStripMenuItem1";
            this.cancelToolStripMenuItem1.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F5)));
            this.cancelToolStripMenuItem1.Size = new System.Drawing.Size(247, 22);
            this.cancelToolStripMenuItem1.Text = "Disconnect";
            this.cancelToolStripMenuItem1.Click += new System.EventHandler(this.cancelToolStripMenuItem1_Click_1);
            // 
            // pluginToolStripMenuItem
            // 
            this.pluginToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator1,
            this.cancelToolStripMenuItem});
            this.pluginToolStripMenuItem.Name = "pluginToolStripMenuItem";
            this.pluginToolStripMenuItem.Size = new System.Drawing.Size(58, 20);
            this.pluginToolStripMenuItem.Text = global::Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties.Resources.ToolStripMenuPlugIn;
            this.pluginToolStripMenuItem.Visible = false;
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(64, 6);
            // 
            // cancelToolStripMenuItem
            // 
            this.cancelToolStripMenuItem.Name = "cancelToolStripMenuItem";
            this.cancelToolStripMenuItem.Size = new System.Drawing.Size(67, 22);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.defaultSerialPortToolStripMenuItem,
            this.clearFileListToolStripMenuItem,
            this.timeStampToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = global::Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties.Resources.ToolStripMenuOptions;
            // 
            // defaultSerialPortToolStripMenuItem
            // 
            this.defaultSerialPortToolStripMenuItem.Name = "defaultSerialPortToolStripMenuItem";
            this.defaultSerialPortToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.defaultSerialPortToolStripMenuItem.Text = "Bootloader Serial Port";
            // 
            // clearFileListToolStripMenuItem
            // 
            this.clearFileListToolStripMenuItem.Name = "clearFileListToolStripMenuItem";
            this.clearFileListToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.clearFileListToolStripMenuItem.Text = global::Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties.Resources.ToolStripMenuItemImageFileList;
            this.clearFileListToolStripMenuItem.Click += new System.EventHandler(this.clearFileListToolStripMenuItem_Click);
            // 
            // timeStampToolStripMenuItem
            // 
            this.timeStampToolStripMenuItem.Checked = true;
            this.timeStampToolStripMenuItem.CheckOnClick = true;
            this.timeStampToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.timeStampToolStripMenuItem.Name = "timeStampToolStripMenuItem";
            this.timeStampToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
            this.timeStampToolStripMenuItem.Size = new System.Drawing.Size(188, 22);
            this.timeStampToolStripMenuItem.Text = "Time Stamp";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.helpTopicsToolStripMenuItem,
            this.aboutMFDeployToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // helpTopicsToolStripMenuItem
            // 
            this.helpTopicsToolStripMenuItem.Name = "helpTopicsToolStripMenuItem";
            this.helpTopicsToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.helpTopicsToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.helpTopicsToolStripMenuItem.Text = "&Help Topics";
            this.helpTopicsToolStripMenuItem.Click += new System.EventHandler(this.helpTopicsToolStripMenuItem_Click);
            // 
            // aboutMFDeployToolStripMenuItem
            // 
            this.aboutMFDeployToolStripMenuItem.Name = "aboutMFDeployToolStripMenuItem";
            this.aboutMFDeployToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.aboutMFDeployToolStripMenuItem.Text = "&About MFDeploy";
            this.aboutMFDeployToolStripMenuItem.Click += new System.EventHandler(this.aboutMFDeployToolStripMenuItem_Click);
            // 
            // listViewFiles
            // 
            this.listViewFiles.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewFiles.BackColor = System.Drawing.SystemColors.Window;
            this.listViewFiles.CheckBoxes = true;
            this.listViewFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderFile,
            this.columnHeaderBaseAddress,
            this.columnHeaderSize,
            this.columnHeaderTimeStamp});
            this.listViewFiles.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listViewFiles.FullRowSelect = true;
            this.listViewFiles.GridLines = true;
            this.listViewFiles.HideSelection = false;
            this.listViewFiles.Location = new System.Drawing.Point(24, 196);
            this.listViewFiles.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.listViewFiles.Name = "listViewFiles";
            this.listViewFiles.Size = new System.Drawing.Size(880, 122);
            this.listViewFiles.TabIndex = 9;
            this.listViewFiles.UseCompatibleStateImageBehavior = false;
            this.listViewFiles.View = System.Windows.Forms.View.Details;
            this.listViewFiles.Visible = false;
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Tag = "";
            this.columnHeaderName.Text = "Name";
            this.columnHeaderName.Width = 123;
            // 
            // columnHeaderFile
            // 
            this.columnHeaderFile.Tag = "";
            this.columnHeaderFile.Text = global::Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties.Resources.ColumnHeaderFile;
            this.columnHeaderFile.Width = 249;
            // 
            // columnHeaderBaseAddress
            // 
            this.columnHeaderBaseAddress.Text = global::Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties.Resources.ColumnHeaderBaseAddr;
            this.columnHeaderBaseAddress.Width = 80;
            // 
            // columnHeaderSize
            // 
            this.columnHeaderSize.Text = global::Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties.Resources.ColumnHeaderSize;
            this.columnHeaderSize.Width = 68;
            // 
            // columnHeaderTimeStamp
            // 
            this.columnHeaderTimeStamp.Text = global::Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties.Resources.ColumnHeaderTimeStamp;
            this.columnHeaderTimeStamp.Width = 135;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(929, 503);
            this.Controls.Add(this.listViewFiles);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.richTextBoxOutput);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBoxDevice);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MinimumSize = new System.Drawing.Size(395, 299);
            this.Name = "Form1";
            this.Text = "A&D Soft .NET Micro Framework Deployment Tool";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
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
