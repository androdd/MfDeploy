// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Debug.DebugPlugins
// Assembly: MFDeploy, Version=4.3.1.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: 9518305E-2E58-43E8-9F45-41B78F83BC9E
// Assembly location: C:\Program Files (x86)\Microsoft .NET Micro Framework\v4.3\Tools\MFDeploy.exe

using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.PlugIns;
using Microsoft.SPOT.Debugger;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Debug
{
  public class DebugPlugins
  {
    public class MFDeployDebug_RebootAndStop : MFPlugInMenuItem
    {
      public override string Name => "Reboot and Stop";

      public override void OnAction(IMFDeployForm form, MFDevice device)
      {
        if (form == null || device == null)
          return;
        device.DbgEngine.RebootDevice(Microsoft.SPOT.Debugger.Engine.RebootOption.EnterBootloader);
        device.ConnectToTinyBooter();
      }
    }

    public class MFDeployDebug_DeployWithPortBooter : MFPlugInMenuItem
    {
      private IMFDeployForm m_form;
      private int m_lastPercent = -1;

      public override bool RequiresConnection => false;

      public override string Name => "Deploy with PortBooter";

      public override void OnAction(IMFDeployForm form, MFDevice device)
      {
        if (form == null || device == null)
          return;
        Microsoft.SPOT.Debugger.Engine dbgEngine = device.DbgEngine;
        ReadOnlyCollection<string> files = form.Files;
        if (files.Count == 0)
          return;
        uint address = uint.MaxValue;
        PortBooter portBooter = new PortBooter(dbgEngine);
        portBooter.OnProgress += new PortBooter.ProgressEventHandler(this.OnProgress);
        portBooter.Start();
        try
        {
          foreach (string str in files)
          {
            if (!File.Exists(str))
            {
              form.DumpToOutput(string.Format("Error: File doesn't exist {0}", (object) str));
            }
            else
            {
              ArrayList blocks = new ArrayList();
              uint num = SRecordFile.Parse(str, blocks, (string) null);
              this.m_form = form;
              portBooter.Program(blocks);
              if (address == uint.MaxValue && num != 0U)
                address = num;
            }
          }
          if (address == uint.MaxValue)
            address = 0U;
          this.m_form.DumpToOutput(string.Format("Executing address 0x{0:x08}", (object) address));
          portBooter.Execute(address);
          Thread.Sleep(200);
        }
        finally
        {
          if (portBooter != null)
          {
            portBooter.OnProgress -= new PortBooter.ProgressEventHandler(this.OnProgress);
            portBooter.Stop();
            portBooter.Dispose();
          }
        }
      }

      private void OnProgress(SRecordFile.Block block, int offset, bool fLast)
      {
        int num = (int) ((long) (offset * 100) / block.data.Length);
        if (num % 10 != 0 || this.m_lastPercent == num)
          return;
        this.m_lastPercent = num;
        this.m_form.DumpToOutput(string.Format("Percent Complete {0}%", (object) num));
      }
    }

    public class MFDeployDebug_DeploymentMap : MFPlugInMenuItem
    {
      public override string Name => "Deployment Map";

      public override void OnAction(IMFDeployForm form, MFDevice device)
      {
        if (form == null || device == null)
          return;
        Microsoft.SPOT.Debugger.WireProtocol.Commands.Monitor_DeploymentMap.Reply reply = device.DbgEngine.DeploymentMap();
        if (reply != null)
        {
          for (int index = 0; index < reply.m_count; ++index)
          {
            Microsoft.SPOT.Debugger.WireProtocol.Commands.Monitor_DeploymentMap.DeploymentData deploymentData = reply.m_map[index];
            form.DumpToOutput("Assembly " + index.ToString());
            form.DumpToOutput("  Address: " + deploymentData.m_address.ToString());
            form.DumpToOutput("  Size   : " + deploymentData.m_size.ToString());
            form.DumpToOutput("  CRC    : " + deploymentData.m_CRC.ToString());
          }
          if (reply.m_count != 0)
            return;
          form.DumpToOutput("No deployed assemblies");
        }
        else
          form.DumpToOutput("Command Not Supported by Device");
      }
    }

    public class MFDeployDebug_FlashSectorMap : MFPlugInMenuItem
    {
      public override string Name => "Flash Sector Map";

      public override void OnAction(IMFDeployForm form, MFDevice device)
      {
        if (form == null || device == null)
          return;
        Microsoft.SPOT.Debugger.WireProtocol.Commands.Monitor_FlashSectorMap.Reply flashSectorMap = device.DbgEngine.GetFlashSectorMap();
        if (flashSectorMap == null)
          return;
        form.DumpToOutput(" Sector    Start       Size        Usage");
        form.DumpToOutput("-----------------------------------------------");
        for (int index = 0; index < flashSectorMap.m_map.Length; ++index)
        {
          Microsoft.SPOT.Debugger.WireProtocol.Commands.Monitor_FlashSectorMap.FlashSectorData flashSectorData = flashSectorMap.m_map[index];
          string str = "";
          switch (flashSectorData.m_flags & 240U)
          {
            case 16:
              str = "Bootstrap";
              break;
            case 32:
              str = "Code";
              break;
            case 48:
              str = "Configuration";
              break;
            case 64:
              str = "File System";
              break;
            case 80:
              str = "Deployment";
              break;
            case 96:
              str = "Update Storage";
              break;
            case 144:
              str = "Simple Storage (A)";
              break;
            case 160:
              str = "Simple Storage (B)";
              break;
            case 224:
              str = "EWR Storage (A)";
              break;
            case 240:
              str = "EWR Storage (B)";
              break;
          }
          form.DumpToOutput(string.Format("{0,5}  {1,12}{2,12}   {3}", (object) index, (object) string.Format("0x{0:x08}", (object) flashSectorData.m_address), (object) string.Format("0x{0:x08}", (object) flashSectorData.m_size), (object) str));
        }
      }
    }

    public class MFDeployDebug_MemoryMap : MFPlugInMenuItem
    {
      public override string Name => "Memory Map";

      public override void OnAction(IMFDeployForm form, MFDevice device)
      {
        if (device == null || form == null)
          return;
        Microsoft.SPOT.Debugger.WireProtocol.Commands.Monitor_MemoryMap.Range[] rangeArray = device.DbgEngine.MemoryMap();
        if (rangeArray == null || rangeArray.Length <= 0)
          return;
        form.DumpToOutput("Type     Start       Size");
        form.DumpToOutput("--------------------------------");
        for (int index = 0; index < rangeArray.Length; ++index)
        {
          string str = "";
          switch (rangeArray[index].m_flags)
          {
            case 1:
              str = "RAM";
              break;
            case 2:
              str = "FLASH";
              break;
          }
          form.DumpToOutput(string.Format("{0,-6} 0x{1:x08}  0x{2:x08}", (object) str, (object) rangeArray[index].m_address, (object) rangeArray[index].m_length));
        }
      }
    }

    public class MFDeployDebug_EnumAndExecute : MFPlugInMenuItem
    {
      public override string Name => "Clear BootLoader Flag";

      public override void OnAction(IMFDeployForm form, MFDevice device)
      {
        Microsoft.SPOT.Debugger.Engine dbgEngine = device.DbgEngine;
        if (device.ConnectToTinyBooter())
          dbgEngine.ExecuteMemory(0U);
        else
          form.DumpToOutput("Unable to connect to TinyBooter!");
      }
    }

    public class MFDeployDebug_CreateEmptyKey : MFPlugInMenuItem
    {
      public override string Name => "Create Empty Key";

      public override bool RequiresConnection => false;

      public override bool RunInSeparateThread => false;

      public override void OnAction(IMFDeployForm form, MFDevice device)
      {
        SaveFileDialog saveFileDialog = new SaveFileDialog();
        saveFileDialog.DefaultExt = "*.key";
        saveFileDialog.CheckPathExists = true;
        saveFileDialog.Filter = "Key File (*.key)|*.key|All Files (*.*)|*.*";
        saveFileDialog.FilterIndex = 0;
        saveFileDialog.AddExtension = true;
        saveFileDialog.OverwritePrompt = true;
        saveFileDialog.Title = "Create Empty Key";
        if (DialogResult.OK != saveFileDialog.ShowDialog())
          return;
        MFKeyConfig mfKeyConfig = new MFKeyConfig();
        KeyPair emptyKeyPair = mfKeyConfig.CreateEmptyKeyPair();
        mfKeyConfig.SaveKeyPair(emptyKeyPair, saveFileDialog.FileName);
      }
    }

    public class MFDeployDebug_EraseFirmware : MFPlugInMenuItem
    {
      public override string Name => "Erase Firmware";

      public override bool RequiresConnection => true;

      public override bool RunInSeparateThread => false;

      public override void OnAction(IMFDeployForm form, MFDevice device)
      {
        if (!device.ConnectToTinyBooter())
          return;
        Microsoft.SPOT.Debugger.WireProtocol.Commands.Monitor_FlashSectorMap.Reply flashSectorMap = device.DbgEngine.GetFlashSectorMap();
        if (flashSectorMap == null)
          return;
        foreach (Microsoft.SPOT.Debugger.WireProtocol.Commands.Monitor_FlashSectorMap.FlashSectorData flashSectorData in flashSectorMap.m_map)
        {
          switch (flashSectorData.m_flags & 240U)
          {
            case 32:
            case 48:
              device.DbgEngine.EraseMemory(flashSectorData.m_address, flashSectorData.m_size);
              break;
          }
        }
      }
    }

    public class MFDeployDebug_RebootClr : MFPlugInMenuItem
    {
      public override string Name => "Reboot CLR";

      public override bool RequiresConnection => true;

      public override bool RunInSeparateThread => false;

      public override void OnAction(IMFDeployForm form, MFDevice device) => device.Reboot(false);
    }

    public class MFDeployDebug_ShowDeviceInfo : MFPlugInMenuItem
    {
      public override string Name => "Show Device Info";

      public override bool RunInSeparateThread => false;

      public override void OnAction(IMFDeployForm form, MFDevice device)
      {
        MFDevice.IMFDeviceInfo mfDeviceInfo = device.MFDeviceInfo;
        if (!mfDeviceInfo.Valid)
        {
          form.DumpToOutput("DeviceInfo is not valid!");
        }
        else
        {
          form.DumpToOutput("DeviceInfo:");
          form.DumpToOutput(string.Format("  HAL build info: {0}, {1}", (object) mfDeviceInfo.HalBuildVersion.ToString(), (object) mfDeviceInfo.HalBuildInfo));
          form.DumpToOutput(string.Format("  OEM Product codes (vendor, model, SKU): {0}, {1}, {2}", (object) mfDeviceInfo.OEM.ToString(), (object) mfDeviceInfo.Model.ToString(), (object) mfDeviceInfo.SKU.ToString()));
          form.DumpToOutput("  Serial Numbers (module, system):");
          form.DumpToOutput("    " + mfDeviceInfo.ModuleSerialNumber);
          form.DumpToOutput("    " + mfDeviceInfo.SystemSerialNumber);
          form.DumpToOutput(string.Format("  Solution Build Info: {0}, {1}", (object) mfDeviceInfo.SolutionBuildVersion.ToString(), (object) mfDeviceInfo.SolutionBuildInfo));
          form.DumpToOutput("  AppDomains:");
          foreach (MFDevice.IAppDomainInfo appDomain in mfDeviceInfo.AppDomains)
            form.DumpToOutput(string.Format("    {0}, id={1}", (object) appDomain.Name, (object) appDomain.ID));
          form.DumpToOutput("  Assemblies:");
          foreach (MFDevice.IAssemblyInfo assembly in mfDeviceInfo.Assemblies)
            form.DumpToOutput(string.Format("    {0},{1}", (object) assembly.Name, (object) assembly.Version));
        }
      }
    }
  }
}
