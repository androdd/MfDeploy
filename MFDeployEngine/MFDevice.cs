// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.MFDevice
// Assembly: MFDeployEngine, Version=2.0.0.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: 7FFE591E-9FC8-4A2A-9A07-642B2A02EB3C
// Assembly location: D:\Androvil\Visual Studio 2022\Projects\MfDeploy\Tools\MFDeployEngine.dll

using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.Properties;
using Microsoft.SPOT.Debugger;
using Microsoft.SPOT.Debugger.WireProtocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
  public class MFDevice : IDisposable
  {
    private Microsoft.SPOT.Debugger.Engine m_eng;
    private PortDefinition m_port;
    private PortDefinition m_portTinyBooter;
    private bool disposed;
    public AutoResetEvent EventCancel = new AutoResetEvent(false);
    private readonly byte[] m_data = new byte[256]
    {
      (byte) 67,
      (byte) 111,
      (byte) 112,
      (byte) 121,
      (byte) 114,
      (byte) 105,
      (byte) 103,
      (byte) 104,
      (byte) 116,
      (byte) 32,
      (byte) 50,
      (byte) 48,
      (byte) 48,
      (byte) 51,
      (byte) 13,
      (byte) 10,
      (byte) 77,
      (byte) 105,
      (byte) 99,
      (byte) 114,
      (byte) 111,
      (byte) 115,
      (byte) 111,
      (byte) 102,
      (byte) 116,
      (byte) 32,
      (byte) 67,
      (byte) 111,
      (byte) 114,
      (byte) 112,
      (byte) 13,
      (byte) 10,
      (byte) 49,
      (byte) 32,
      (byte) 77,
      (byte) 105,
      (byte) 99,
      (byte) 114,
      (byte) 111,
      (byte) 115,
      (byte) 111,
      (byte) 102,
      (byte) 116,
      (byte) 32,
      (byte) 87,
      (byte) 97,
      (byte) 121,
      (byte) 13,
      (byte) 10,
      (byte) 82,
      (byte) 101,
      (byte) 100,
      (byte) 109,
      (byte) 111,
      (byte) 110,
      (byte) 100,
      (byte) 44,
      (byte) 32,
      (byte) 87,
      (byte) 65,
      (byte) 13,
      (byte) 10,
      (byte) 57,
      (byte) 56,
      (byte) 48,
      (byte) 53,
      (byte) 50,
      (byte) 45,
      (byte) 54,
      (byte) 51,
      (byte) 57,
      (byte) 57,
      (byte) 13,
      (byte) 10,
      (byte) 85,
      (byte) 46,
      (byte) 83,
      (byte) 46,
      (byte) 65,
      (byte) 46,
      (byte) 13,
      (byte) 10,
      (byte) 65,
      (byte) 108,
      (byte) 108,
      (byte) 32,
      (byte) 114,
      (byte) 105,
      (byte) 103,
      (byte) 104,
      (byte) 116,
      (byte) 115,
      (byte) 32,
      (byte) 114,
      (byte) 101,
      (byte) 115,
      (byte) 101,
      (byte) 114,
      (byte) 118,
      (byte) 101,
      (byte) 100,
      (byte) 46,
      (byte) 13,
      (byte) 10,
      (byte) 77,
      (byte) 73,
      (byte) 67,
      (byte) 82,
      (byte) 79,
      (byte) 83,
      (byte) 79,
      (byte) 70,
      (byte) 84,
      (byte) 32,
      (byte) 67,
      (byte) 79,
      (byte) 78,
      (byte) 70,
      (byte) 73,
      (byte) 68,
      (byte) 69,
      (byte) 78,
      (byte) 84,
      (byte) 73,
      (byte) 65,
      (byte) 76,
      (byte) 13,
      (byte) 10,
      (byte) 55,
      (byte) 231,
      (byte) 64,
      (byte) 0,
      (byte) 118,
      (byte) 157,
      (byte) 50,
      (byte) 129,
      (byte) 173,
      (byte) 196,
      (byte) 117,
      (byte) 75,
      (byte) 87,
      byte.MaxValue,
      (byte) 238,
      (byte) 223,
      (byte) 181,
      (byte) 114,
      (byte) 130,
      (byte) 29,
      (byte) 130,
      (byte) 170,
      (byte) 89,
      (byte) 70,
      (byte) 194,
      (byte) 108,
      (byte) 71,
      (byte) 230,
      (byte) 192,
      (byte) 61,
      (byte) 9,
      (byte) 29,
      (byte) 216,
      (byte) 23,
      (byte) 196,
      (byte) 204,
      (byte) 21,
      (byte) 89,
      (byte) 242,
      (byte) 196,
      (byte) 143,
      byte.MaxValue,
      (byte) 49,
      (byte) 65,
      (byte) 179,
      (byte) 224,
      (byte) 237,
      (byte) 213,
      (byte) 15,
      (byte) 250,
      (byte) 92,
      (byte) 181,
      (byte) 77,
      (byte) 10,
      (byte) 200,
      (byte) 21,
      (byte) 219,
      (byte) 202,
      (byte) 181,
      (byte) 127,
      (byte) 64,
      (byte) 172,
      (byte) 101,
      (byte) 87,
      (byte) 166,
      (byte) 35,
      (byte) 162,
      (byte) 28,
      (byte) 70,
      (byte) 172,
      (byte) 138,
      (byte) 40,
      (byte) 35,
      (byte) 215,
      (byte) 207,
      (byte) 160,
      (byte) 195,
      (byte) 119,
      (byte) 187,
      (byte) 95,
      (byte) 239,
      (byte) 213,
      (byte) 127,
      (byte) 201,
      (byte) 46,
      (byte) 15,
      (byte) 60,
      (byte) 225,
      (byte) 19,
      (byte) 252,
      (byte) 227,
      (byte) 17,
      (byte) 211,
      (byte) 80,
      (byte) 209,
      (byte) 52,
      (byte) 74,
      (byte) 122,
      (byte) 115,
      (byte) 2,
      (byte) 144,
      (byte) 20,
      (byte) 153,
      (byte) 241,
      (byte) 244,
      (byte) 57,
      (byte) 139,
      (byte) 10,
      (byte) 57,
      (byte) 65,
      (byte) 248,
      (byte) 204,
      (byte) 149,
      (byte) 252,
      (byte) 17,
      (byte) 159,
      (byte) 244,
      (byte) 11,
      (byte) 186,
      (byte) 176,
      (byte) 59,
      (byte) 187,
      (byte) 167,
      (byte) 107,
      (byte) 83,
      (byte) 163,
      (byte) 62,
      (byte) 122
    };

    public event MFDevice.OnProgressHandler OnProgress;

    private MFDevice()
    {
    }

    ~MFDevice() => this.Dispose(false);

    public Microsoft.SPOT.Debugger.Engine DbgEngine => this.m_eng;

    private void OnNoiseHandler(byte[] data, int index, int count)
    {
      if (this.OnDebugText == null)
        return;
      this.OnDebugText((object) this, new DebugOutputEventArgs(Encoding.ASCII.GetString(data, index, count)));
    }

    private void OnMessage(IncomingMessage msg, string text)
    {
      if (this.OnDebugText == null)
        return;
      this.OnDebugText((object) this, new DebugOutputEventArgs(text));
    }

    private void PrepareForDeploy(ArrayList blocks)
    {
      bool flag = false;
      if (!this.IsClrDebuggerEnabled())
      {
        if (this.OnProgress != null)
          this.OnProgress(0L, 1L, Resources.StatusConnectingToTinyBooter);
        if (!this.ConnectToTinyBooter())
          throw new MFDeviceNoResponseException();
      }
      Commands.Monitor_FlashSectorMap.Reply flashSectorMap = this.m_eng.GetFlashSectorMap();
      if (flashSectorMap == null)
        throw new MFDeviceNoResponseException();
      foreach (SRecordFile.Block block in blocks)
      {
        foreach (Commands.Monitor_FlashSectorMap.FlashSectorData flashSectorData in flashSectorMap.m_map)
        {
          if ((int) flashSectorData.m_address == (int) block.address)
          {
            if (80 == (240 & (int) flashSectorData.m_flags))
            {
              flag = true;
              break;
            }
            if (this.m_eng.ConnectionSource != ConnectionSource.TinyBooter)
            {
              if (this.OnProgress != null)
                this.OnProgress(0L, 1L, Resources.StatusConnectingToTinyBooter);
              if (!this.ConnectToTinyBooter())
                throw new MFDeviceNoResponseException();
              break;
            }
            break;
          }
        }
      }
      if (flag)
        this.Erase(EraseOptions.Deployment);
      else if (this.m_eng.ConnectionSource != ConnectionSource.TinyBooter)
        this.ConnectToTinyBooter();
      if (this.m_eng.ConnectionSource != ConnectionSource.TinyCLR)
        return;
      this.m_eng.PauseExecution();
    }

    internal MFDevice(PortDefinition port, PortDefinition tinyBooterPort)
    {
      this.m_port = port;
      this.m_portTinyBooter = tinyBooterPort;
    }

    internal bool Connect(int timeout_ms, bool tryConnect)
    {
      int num1 = timeout_ms / 100;
      int num2 = 1;
      if (num1 == 0)
        num1 = 1;
      if (this.m_portTinyBooter != null && this.m_port.UniqueId != this.m_portTinyBooter.UniqueId)
      {
        num1 /= 2;
        num2 = 2;
      }
      for (int index1 = 0; index1 < num2; ++index1)
      {
        PortDefinition pd = index1 == 0 ? this.m_port : this.m_portTinyBooter;
        if (this.EventCancel.WaitOne(0, false))
          throw new MFUserExitException();
        try
        {
          if (this.m_eng == null)
          {
            this.m_eng = new Microsoft.SPOT.Debugger.Engine(pd);
            this.m_eng.OnNoise += new NoiseEventHandler(this.OnNoiseHandler);
            this.m_eng.OnMessage += new MessageEventHandler(this.OnMessage);
          }
          if (!this.m_eng.IsConnected)
          {
            this.m_eng.Start();
            if (tryConnect)
            {
              for (int index2 = num1; index2 > 0; index2 -= 5)
              {
                if (this.m_eng.TryToConnect(5, 100, true, ConnectionSource.Unknown))
                {
                  this.m_eng.UnlockDevice(this.m_data);
                  break;
                }
                if (this.EventCancel.WaitOne(0, false))
                  throw new MFUserExitException();
              }
              if (!this.m_eng.IsConnected)
                this.Disconnect();
              else
                break;
            }
            else
              break;
          }
        }
        catch (MFUserExitException ex)
        {
          this.Disconnect();
          throw;
        }
        catch
        {
          this.Disconnect();
        }
      }
      if (this.m_eng == null)
        return false;
      return !tryConnect || this.m_eng.IsConnected;
    }

    internal bool Disconnect()
    {
      if (this.m_eng != null)
      {
        this.m_eng.OnNoise -= new NoiseEventHandler(this.OnNoiseHandler);
        this.m_eng.OnMessage -= new MessageEventHandler(this.OnMessage);
        this.m_eng.Stop();
        this.m_eng = (Microsoft.SPOT.Debugger.Engine) null;
      }
      return true;
    }

    private void Dispose(bool disposing)
    {
      if (this.disposed)
        return;
      if (disposing)
        this.Disconnect();
      this.disposed = true;
    }

    private bool IsClrDebuggerEnabled()
    {
      try
      {
        if (this.m_eng.IsConnectedToTinyCLR)
          return this.m_eng.Capabilities.SourceLevelDebugging;
      }
      catch
      {
      }
      return false;
    }

    public event EventHandler<DebugOutputEventArgs> OnDebugText;

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    public bool ConnectToTinyBooter()
    {
      bool tinyBooter = false;
      if (this.m_eng == null)
      {
        PortDefinition portTinyBooter = this.m_portTinyBooter;
        try
        {
          if (this.m_eng == null)
          {
            this.m_eng = new Microsoft.SPOT.Debugger.Engine(portTinyBooter);
            this.m_eng.OnNoise += new NoiseEventHandler(this.OnNoiseHandler);
            this.m_eng.OnMessage += new MessageEventHandler(this.OnMessage);
            this.m_eng.Start();
            this.m_eng.TryToConnect(5, 100, true, ConnectionSource.Unknown);
          }
        }
        catch
        {
        }
      }
      if (this.m_eng != null)
      {
        if (this.m_eng.ConnectionSource == ConnectionSource.TinyBooter)
          return true;
        this.m_eng.RebootDevice(Microsoft.SPOT.Debugger.Engine.RebootOption.EnterBootloader);
        if (this.m_port is PortDefinition_Tcp)
        {
          this.Disconnect();
          this.m_port = this.m_portTinyBooter;
          if (!this.Connect(60000, true))
          {
            Console.WriteLine(Resources.ErrorUnableToConnectToTinyBooterSerial);
            return false;
          }
        }
        bool flag = false;
        for (int index = 0; index < 40; ++index)
        {
          if (this.EventCancel.WaitOne(0, false))
            throw new MFUserExitException();
          if (flag = this.m_eng.TryToConnect(0, 500, true, ConnectionSource.Unknown))
          {
            tinyBooter = this.m_eng.GetConnectionSource().m_source == 1U;
            break;
          }
        }
        if (!flag)
          Console.WriteLine(Resources.ErrorUnableToConnectToTinyBooter);
      }
      return tinyBooter;
    }

    public bool Erase(params EraseOptions[] options)
    {
      bool flag1 = false;
      if (this.m_eng == null)
        throw new MFDeviceNoResponseException();
      EraseOptions eraseOptions = (EraseOptions) 0;
      if (options == null || options.Length == 0)
      {
        eraseOptions = EraseOptions.Deployment | EraseOptions.UserStorage | EraseOptions.Logging;
      }
      else
      {
        foreach (EraseOptions option in options)
          eraseOptions |= option;
      }
      if (!this.m_eng.TryToConnect(5, 100, true, ConnectionSource.Unknown))
        throw new MFDeviceNoResponseException();
      if (!this.IsClrDebuggerEnabled())
      {
        flag1 = this.Ping() == PingConnectionType.TinyCLR;
        this.ConnectToTinyBooter();
      }
      Commands.Monitor_FlashSectorMap.Reply flashSectorMap = this.m_eng.GetFlashSectorMap();
      if (flashSectorMap == null)
        throw new MFDeviceNoResponseException();
      Commands.Monitor_Ping.Reply connectionSource = this.m_eng.GetConnectionSource();
      bool flag2 = true;
      long total = 0;
      long num = 0;
      bool flag3 = connectionSource != null && connectionSource.m_source == 0U;
      if (flag3)
        this.m_eng.PauseExecution();
      List<Commands.Monitor_FlashSectorMap.FlashSectorData> flashSectorDataList = new List<Commands.Monitor_FlashSectorMap.FlashSectorData>();
      foreach (Commands.Monitor_FlashSectorMap.FlashSectorData flashSectorData in flashSectorMap.m_map)
      {
        if (this.EventCancel.WaitOne(0, false))
          throw new MFUserExitException();
        switch (flashSectorData.m_flags & 240U)
        {
          case 64:
            if (EraseOptions.Logging == (eraseOptions & EraseOptions.Logging))
            {
              flashSectorDataList.Add(flashSectorData);
              ++total;
              break;
            }
            break;
          case 80:
            if (EraseOptions.Deployment == (eraseOptions & EraseOptions.Deployment))
            {
              flashSectorDataList.Add(flashSectorData);
              ++total;
              break;
            }
            break;
          case 224:
          case 240:
            if (EraseOptions.UserStorage == (eraseOptions & EraseOptions.UserStorage))
            {
              flashSectorDataList.Add(flashSectorData);
              ++total;
              break;
            }
            break;
        }
      }
      foreach (Commands.Monitor_FlashSectorMap.FlashSectorData flashSectorData in flashSectorDataList)
      {
        flag2 &= this.m_eng.EraseMemory(flashSectorData.m_address, flashSectorData.m_size);
        ++num;
        if (this.OnProgress != null)
          this.OnProgress(num, total, string.Format(Resources.StatusEraseSector, (object) flashSectorData.m_address));
      }
      if (flag1)
        this.m_eng.ExecuteMemory(0U);
      if (flag3)
      {
        if (this.OnProgress != null)
          this.OnProgress(0L, 0L, Resources.StatusRebooting);
        this.m_eng.RebootDevice(Microsoft.SPOT.Debugger.Engine.RebootOption.RebootClrOnly);
        this.m_eng.ResumeExecution();
      }
      return flag2;
    }

    public PingConnectionType Ping()
    {
      PingConnectionType pingConnectionType = PingConnectionType.NoConnection;
      if (this.m_eng != null)
      {
        Commands.Monitor_Ping.Reply connectionSource = this.m_eng.GetConnectionSource();
        if (connectionSource != null)
        {
          switch (connectionSource.m_source)
          {
            case 0:
              pingConnectionType = PingConnectionType.TinyCLR;
              break;
            case 1:
              pingConnectionType = PingConnectionType.TinyBooter;
              break;
          }
        }
      }
      return pingConnectionType;
    }

    public bool Deploy(string filePath, string signatureFile, ref uint entryPoint)
    {
      entryPoint = 0U;
      if (!File.Exists(filePath))
        throw new FileNotFoundException(filePath);
      if (this.m_eng == null)
        throw new MFDeviceNoResponseException();
      this.m_eng.TryToConnect(1, 100, true, ConnectionSource.Unknown);
      bool flag1 = File.Exists(signatureFile);
      FileInfo fileInfo = new FileInfo(filePath);
      ArrayList blocks = new ArrayList();
      entryPoint = SRecordFile.Parse(filePath, blocks, flag1 ? signatureFile : (string) null);
      if (blocks.Count > 0)
      {
        long total = 0;
        long num = 0;
        for (int index = 0; index < blocks.Count; ++index)
          total += (blocks[index] as SRecordFile.Block).data.Length;
        this.PrepareForDeploy(blocks);
        foreach (SRecordFile.Block block in blocks)
        {
          long length = block.data.Length;
          uint address = block.address;
          if (this.EventCancel.WaitOne(0, false))
            throw new MFUserExitException();
          block.data.Seek(0L, SeekOrigin.Begin);
          if (this.OnProgress != null)
            this.OnProgress(0L, total, string.Format(Resources.StatusEraseSector, (object) block.address));
          if (!this.m_eng.EraseMemory(block.address, (uint) length))
            return false;
          while (length > 0L)
          {
            if (this.EventCancel.WaitOne(0, false))
              throw new MFUserExitException();
            int count = length > 1024L ? 1024 : (int) length;
            byte[] numArray = new byte[count];
            if (block.data.Read(numArray, 0, count) <= 0)
              return false;
            bool flag2 = false;
            for (int index = 5; !flag2 && index > 0; --index)
              flag2 = this.m_eng.WriteMemory(address, numArray);
            if (!flag2)
              return false;
            num += (long) count;
            address += (uint) count;
            length -= (long) count;
            if (this.OnProgress != null)
              this.OnProgress(num, total, string.Format(Resources.StatusFlashing, (object) fileInfo.Name));
          }
          if (ConnectionSource.TinyCLR != this.m_eng.ConnectionSource)
          {
            byte[] numArray = new byte[128];
            if (this.OnProgress != null)
              this.OnProgress(num, total, Resources.StatusCheckingSignature);
            if (!this.m_eng.CheckSignature(block.signature == null ? numArray : block.signature, 0U))
              throw new MFSignatureFailureException(signatureFile);
          }
        }
      }
      return true;
    }

    public bool Execute(uint entryPoint)
    {
      Commands.Monitor_Ping.Reply reply = this.m_eng != null ? this.m_eng.GetConnectionSource() : throw new MFDeviceNoResponseException();
      if (reply == null)
        throw new MFDeviceNoResponseException();
      if (reply.m_source == 1U)
        return this.m_eng.ExecuteMemory(entryPoint);
      this.m_eng.RebootDevice(Microsoft.SPOT.Debugger.Engine.RebootOption.RebootClrOnly);
      return true;
    }

    public void Reboot(bool coldBoot)
    {
      if (this.m_eng == null)
        throw new MFDeviceNoResponseException();
      this.m_eng.RebootDevice(coldBoot ? Microsoft.SPOT.Debugger.Engine.RebootOption.NoReconnect : Microsoft.SPOT.Debugger.Engine.RebootOption.RebootClrOnly);
    }

    public bool IsConnected => this.m_eng != null && this.m_eng.IsConnected;

    public delegate void OnProgressHandler(long value, long total, string status);
  }
}
