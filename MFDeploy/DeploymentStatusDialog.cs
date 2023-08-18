// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.DeploymentStatusDialog
// Assembly: MFDeploy, Version=2.0.0.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: 2BA2704E-D6D9-4FE0-ACF3-88D9CCA0E272
// Assembly location: D:\Androvil\Visual Studio 2022\Projects\MfDeploy\Tools\MFDeploy.exe

using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties;
using Microsoft.SPOT.Debugger;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool
{
  public class DeploymentStatusDialog : Form
  {
    private MFDevice m_dev;
    private bool m_fEraseCmd;
    private ReadOnlyCollection<string> m_files;
    private string[] m_sigFiles;
    private AutoResetEvent m_evtDevice;
    private ManualResetEvent m_evtShutdown = new ManualResetEvent(false);
    private AutoResetEvent m_evtDeviceFinished = new AutoResetEvent(false);
    private Thread m_thread;
    private IContainer components;
    private ProgressBar progressBar1;
    private Button button1;
    private TextBox textBox1;

    private void OnStatus(long value, long total, string status)
    {
      if (this.m_evtShutdown.WaitOne(0, false))
        return;
      int val;
      int tot;
      if (total > 100L)
      {
        val = (int) (value / 100L);
        tot = (int) (total / 100L);
      }
      else
      {
        val = (int) value;
        tot = (int) total;
      }
      this.textBox1.Invoke((Action) (() =>
      {
        this.textBox1.Text = status;
        this.textBox1.Invalidate();
        this.progressBar1.Maximum = tot;
        this.progressBar1.Value = val;
        this.progressBar1.Invalidate();
        this.Update();
      }));
    }

    public DeploymentStatusDialog(
      MFDevice dev,
      ReadOnlyCollection<string> files,
      string[] sig_files)
    {
      this.m_dev = dev;
      this.m_files = files;
      this.m_sigFiles = sig_files;
      this.InitializeComponent();
    }

    public DeploymentStatusDialog(MFDevice dev)
      : this(dev, (ReadOnlyCollection<string>) null, (string[]) null)
    {
      this.m_fEraseCmd = true;
      this.Text = Resources.DeploymentStatusTitleErase;
    }

    private void ThreadProc()
    {
      try
      {
        this.m_dev.OnProgress += new MFDevice.OnProgressHandler(this.OnStatus);
        if (this.m_evtShutdown.WaitOne(0, false))
          return;
        this.m_evtDevice = this.m_dev.EventCancel;
        this.m_dev.DbgEngine.PauseExecution();
        if (this.m_fEraseCmd)
        {
          if (!this.m_dev.Erase())
            throw new MFDeployEraseFailureException();
        }
        else
        {
          int num = 0;
          List<uint> uintList = new List<uint>();
          foreach (string file in this.m_files)
          {
            uint entryPoint = 0;
            if (!this.m_dev.Deploy(file, this.m_sigFiles[num++], ref entryPoint))
              throw new MFDeployDeployFailureException();
            if (entryPoint != 0U)
              uintList.Add(entryPoint);
          }
          uintList.Add(0U);
          this.OnStatus(100L, 100L, "Executing Application");
          using (List<uint>.Enumerator enumerator = uintList.GetEnumerator())
          {
            do
              ;
            while (enumerator.MoveNext() && !this.m_dev.Execute(enumerator.Current));
          }
        }
      }
      catch (ThreadAbortException ex)
      {
      }
      catch (MFUserExitException ex)
      {
      }
      catch (Exception ex)
      {
        DeploymentStatusDialog owner = this;
        if (this.m_evtShutdown.WaitOne(0, false))
          return;
        int num;
        this.Invoke((Action) (() => num = (int) MessageBox.Show((IWin32Window) owner, Resources.ErrorPrefix + ex.Message, Resources.ApplicationTitle, MessageBoxButtons.OK, MessageBoxIcon.Hand)));
      }
      finally
      {
        this.m_evtDeviceFinished.Set();
        this.m_evtDevice = (AutoResetEvent) null;
        if (this.m_dev.DbgEngine != null)
          this.m_dev.DbgEngine.ResumeExecution();
        this.m_dev.OnProgress -= new MFDevice.OnProgressHandler(this.OnStatus);
        if (!this.m_evtShutdown.WaitOne(0, false))
          this.Invoke((Action) (() => this.Close()));
      }
    }

    private void DeploymentStatusDialog_Load(object sender, EventArgs e)
    {
      this.m_thread = new Thread(new ThreadStart(this.ThreadProc));
      this.m_thread.Start();
    }

    private void Shutdown()
    {
      this.m_evtShutdown.Set();
      if (this.m_evtDevice != null)
        this.m_evtDevice.Set();
      if (this.m_evtDeviceFinished.WaitOne(500, false) || this.m_thread == null || !this.m_thread.IsAlive)
        return;
      this.m_thread.Abort();
      this.m_thread = (Thread) null;
    }

    private void button1_Click(object sender, EventArgs e)
    {
      try
      {
        if (this.m_dev != null)
        {
          if (this.m_dev.DbgEngine.ConnectionSource == ConnectionSource.TinyBooter)
            this.m_dev.Execute(0U);
        }
      }
      catch
      {
      }
      this.Shutdown();
    }

    private void DeploymentStatusDialog_FormClosing(object sender, FormClosingEventArgs e)
    {
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.progressBar1 = new ProgressBar();
      this.button1 = new Button();
      this.textBox1 = new TextBox();
      this.SuspendLayout();
      this.progressBar1.Location = new Point(12, 52);
      this.progressBar1.Name = "progressBar1";
      this.progressBar1.Size = new Size(270, 23);
      this.progressBar1.TabIndex = 0;
      this.button1.DialogResult = DialogResult.Cancel;
      this.button1.Location = new Point(110, 91);
      this.button1.Name = "button1";
      this.button1.Size = new Size(75, 23);
      this.button1.TabIndex = 1;
      this.button1.Text = Resources.ButtonCancel;
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new EventHandler(this.button1_Click);
      this.textBox1.BackColor = SystemColors.Control;
      this.textBox1.BorderStyle = BorderStyle.None;
      this.textBox1.Location = new Point(12, 19);
      this.textBox1.Name = "textBox1";
      this.textBox1.Size = new Size(270, 13);
      this.textBox1.TabIndex = 2;
      this.textBox1.TabStop = false;
      this.textBox1.Text = Resources.StatusConnectingToDevice;
      this.textBox1.TextAlign = HorizontalAlignment.Center;
      this.AutoScaleDimensions = new SizeF(6f, 13f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(294, 126);
      this.ControlBox = false;
      this.Controls.Add((Control) this.textBox1);
      this.Controls.Add((Control) this.button1);
      this.Controls.Add((Control) this.progressBar1);
      this.FormBorderStyle = FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = nameof (DeploymentStatusDialog);
      this.ShowInTaskbar = false;
      this.StartPosition = FormStartPosition.CenterParent;
      this.Text = Resources.StatusDeployment;
      this.FormClosing += new FormClosingEventHandler(this.DeploymentStatusDialog_FormClosing);
      this.Load += new EventHandler(this.DeploymentStatusDialog_Load);
      this.ResumeLayout(false);
      this.PerformLayout();
    }
  }
}
