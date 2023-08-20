// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.MFDevice
// Assembly: MFDeployEngine, Version=4.3.1.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: C9A5873B-24F0-4236-8EF7-C28FFF230F5B
// Assembly location: C:\Program Files (x86)\Microsoft .NET Micro Framework\v4.3\Tools\MFDeployEngine.dll

using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.Properties;
using Microsoft.SPOT.Debugger;
using Microsoft.SPOT.Debugger.WireProtocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
  public class MFDevice : IDisposable
  {
    private Microsoft.SPOT.Debugger.Engine m_eng;
    private PortDefinition m_port;
    private PortDefinition m_portTinyBooter;
    private bool disposed;
    private string m_CurrentNoise = "";
    public AutoResetEvent EventCancel = new AutoResetEvent(false);
    private AutoResetEvent m_evtMicroBooter = new AutoResetEvent(false);
    private AutoResetEvent m_evtMicroBooterError = new AutoResetEvent(false);
    private ManualResetEvent m_evtMicroBooterStart = new ManualResetEvent(false);
    private Dictionary<uint, string> m_execSrecHash = new Dictionary<uint, string>();
    private Dictionary<uint, int> m_srecHash = new Dictionary<uint, int>();
    private X509Certificate2 m_serverCert;
    private bool m_requireClientCert;
    private Regex m_srecExpr = new Regex("<MB>([\\d\\w]+)</MB>");
    private int m_totalSrecs;
    private uint m_minSrecAddr;
    private uint m_maxSrecAddr;
    private MFDevice.IMFDeviceInfo m_deviceInfoCache;
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

    public Microsoft.SPOT.Debugger.Engine DbgEngine
    {
      get
      {
        if (this.m_eng == null)
          this.Connect(500, true);
        return this.m_eng;
      }
    }

    private void OnNoiseHandler(byte[] data, int index, int count)
    {
      string text = Encoding.ASCII.GetString(data, index, count);
      if (this.m_evtMicroBooterStart.WaitOne(0))
      {
        this.m_CurrentNoise += text;
        int startIndex = 0;
        for (Match match = this.m_srecExpr.Match(this.m_CurrentNoise); match.Success; match = match.NextMatch())
        {
          string str = match.Groups[1].Value;
          if (string.Compare(str, "ERROR", true) == 0)
          {
            this.m_evtMicroBooterError.Set();
          }
          else
          {
            try
            {
              uint result;
              if (uint.TryParse(str, NumberStyles.HexNumber, (IFormatProvider) null, out result))
              {
                if (!this.m_srecHash.ContainsKey(result) && this.m_minSrecAddr <= result && this.m_maxSrecAddr >= result)
                {
                  this.m_srecHash[result] = 1;
                  if (this.OnProgress != null)
                    this.OnProgress((long) this.m_srecHash.Count, (long) this.m_totalSrecs, string.Format(Resources.StatusFlashing, (object) str));
                }
              }
              else
                Console.WriteLine(str);
              this.m_evtMicroBooter.Set();
            }
            catch
            {
            }
          }
          startIndex = match.Index + match.Length;
        }
        this.m_CurrentNoise = this.m_CurrentNoise.Substring(startIndex);
      }
      else
      {
        this.m_CurrentNoise = "";
        if (this.OnDebugText == null)
          return;
        this.OnDebugText((object) this, new DebugOutputEventArgs(text));
      }
    }

    public void OnMessage(IncomingMessage msg, string text)
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

    internal bool CheckForMicroBooter()
    {
      if (this.m_eng == null)
        return false;
      try
      {
        this.m_evtMicroBooterStart.Set();
        this.m_evtMicroBooterError.Reset();
        for (int index = 0; index < 5; ++index)
        {
          this.m_eng.SendRawBuffer(Encoding.ASCII.GetBytes("xx\n"));
          if (this.m_evtMicroBooterError.WaitOne(100))
            return true;
        }
      }
      finally
      {
        this.m_evtMicroBooterStart.Reset();
      }
      return false;
    }

    internal bool ConnectTo(int timeout_ms, bool tryToConnect, ConnectionSource target)
    {
      int num = this.m_port is PortDefinition_Tcp ? 2 : timeout_ms / 300;
      if (num == 0)
        num = 1;
      try
      {
        if (this.m_eng == null)
        {
          this.m_eng = new Microsoft.SPOT.Debugger.Engine(this.m_port);
          this.m_eng.OnNoise += new NoiseEventHandler(this.OnNoiseHandler);
          this.m_eng.OnMessage += new MessageEventHandler(this.OnMessage);
          this.m_eng.Start();
        }
        if (tryToConnect)
        {
          for (int index = num; index > 0; --index)
          {
            if (target == ConnectionSource.MicroBooter)
            {
              if (this.CheckForMicroBooter())
                return true;
              Thread.Sleep(timeout_ms / num);
            }
            else if (this.m_eng.TryToConnect(0, timeout_ms / num, true, ConnectionSource.Unknown))
            {
              if (this.m_eng.ConnectionSource == ConnectionSource.TinyCLR)
                this.m_eng.UnlockDevice(this.m_data);
              else if (this.m_eng.ConnectionSource == ConnectionSource.TinyBooter && target == ConnectionSource.TinyCLR)
              {
                this.m_eng.ExecuteMemory(0U);
                Thread.Sleep(100);
              }
            }
            else if (this.EventCancel.WaitOne(0, false))
              throw new MFUserExitException();
            if (this.m_eng.IsConnected && target == this.m_eng.ConnectionSource)
              break;
          }
          if (target != this.m_eng.ConnectionSource)
            this.Disconnect();
        }
      }
      catch (ThreadAbortException ex)
      {
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
      if (this.m_eng == null)
        return false;
      return !tryToConnect || this.m_eng.IsConnected;
    }

    public bool Connect(int timeout_ms, bool tryConnect)
    {
      int num1 = this.m_port is PortDefinition_Tcp ? 2 : timeout_ms / 100;
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
            this.m_eng.Start();
          }
          if (tryConnect)
          {
            if (this.CheckForMicroBooter())
              return true;
            for (int index2 = num1; index2 > 0; --index2)
            {
              if (this.m_eng.TryToConnect(0, timeout_ms / num1, true, ConnectionSource.Unknown))
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
        catch (ThreadAbortException ex)
        {
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

    public bool Disconnect()
    {
      if (this.m_eng != null)
      {
        this.m_eng.OnNoise -= new NoiseEventHandler(this.OnNoiseHandler);
        this.m_eng.OnMessage -= new MessageEventHandler(this.OnMessage);
        this.m_eng.Stop();
        this.m_eng.Dispose();
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

    public bool UseSsl(X509Certificate2 cert, bool fRequireClientCert)
    {
      this.m_serverCert = cert;
      this.m_requireClientCert = fRequireClientCert;
      return false;
    }

    public bool ConnectToTinyBooter()
    {
      bool tinyBooter = false;
      if (!this.Connect(500, true))
        return false;
      if (this.m_eng != null)
      {
        if (this.m_eng.ConnectionSource == ConnectionSource.TinyBooter)
          return true;
        this.m_eng.RebootDevice(Microsoft.SPOT.Debugger.Engine.RebootOption.EnterBootloader);
        if (this.m_port is PortDefinition_Tcp)
        {
          PortDefinition port = this.m_port;
          this.Disconnect();
          try
          {
            this.m_port = this.m_portTinyBooter;
            if (!this.Connect(60000, true))
            {
              Console.WriteLine(Resources.ErrorUnableToConnectToTinyBooterSerial);
              return false;
            }
          }
          finally
          {
            this.m_port = port;
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
        eraseOptions = EraseOptions.Deployment | EraseOptions.UserStorage | EraseOptions.FileSystem | EraseOptions.UpdateStorage | EraseOptions.SimpleStorage;
      }
      else
      {
        foreach (EraseOptions option in options)
          eraseOptions |= option;
      }
      if (!this.Connect(500, true))
        throw new MFDeviceNoResponseException();
      if (!this.IsClrDebuggerEnabled() || (eraseOptions & EraseOptions.Firmware) != (EraseOptions) 0)
      {
        flag1 = this.Ping() == PingConnectionType.TinyCLR;
        if (!this.ConnectToTinyBooter())
          throw new MFTinyBooterConnectionFailureException();
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
          case 32:
          case 48:
            if (EraseOptions.Firmware == (eraseOptions & EraseOptions.Firmware))
            {
              flashSectorDataList.Add(flashSectorData);
              ++total;
              break;
            }
            break;
          case 64:
            if (EraseOptions.FileSystem == (eraseOptions & EraseOptions.FileSystem))
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
          case 96:
            if (EraseOptions.UpdateStorage == (eraseOptions & EraseOptions.UpdateStorage))
            {
              flashSectorDataList.Add(flashSectorData);
              ++total;
              break;
            }
            break;
          case 144:
          case 160:
            if (EraseOptions.SimpleStorage == (eraseOptions & EraseOptions.SimpleStorage))
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
        if (this.OnProgress != null)
          this.OnProgress(num, total, string.Format(Resources.StatusEraseSector, (object) flashSectorData.m_address));
        flag2 &= this.m_eng.EraseMemory(flashSectorData.m_address, flashSectorData.m_size);
        ++num;
      }
      if (flag1)
        this.m_eng.ExecuteMemory(0U);
      if (flag3)
      {
        if (this.OnProgress != null)
          this.OnProgress(0L, 0L, Resources.StatusRebooting);
        this.m_eng.RebootDevice(Microsoft.SPOT.Debugger.Engine.RebootOption.RebootClrOnly);
      }
      return flag2;
    }

    public PingConnectionType Ping()
    {
      PingConnectionType pingConnectionType = PingConnectionType.NoConnection;
      if (this.m_eng == null)
      {
        this.Connect(1000, true);
      }
      else
      {
        this.m_eng.OnNoise -= new NoiseEventHandler(this.OnNoiseHandler);
        this.m_eng.OnMessage -= new MessageEventHandler(this.OnMessage);
        this.m_eng.OnNoise += new NoiseEventHandler(this.OnNoiseHandler);
        this.m_eng.OnMessage += new MessageEventHandler(this.OnMessage);
      }
      if (this.m_eng != null)
      {
        try
        {
          if (this.CheckForMicroBooter())
            return PingConnectionType.MicroBooter;
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
        catch (ThreadAbortException ex)
        {
        }
        catch
        {
          this.Disconnect();
        }
      }
      return pingConnectionType;
    }

    public OemMonitorInfo GetOemMonitorInfo()
    {
      if (this.m_eng == null || !this.m_eng.IsConnected || this.m_eng.IsConnectedToTinyCLR)
        return (OemMonitorInfo) null;
      Commands.Monitor_OemInfo.Reply monitorOemInfo = this.m_eng.GetMonitorOemInfo();
      return monitorOemInfo != null ? new OemMonitorInfo(monitorOemInfo) : (OemMonitorInfo) null;
    }

    private bool PreProcesSREC(string srecFile)
    {
      if (!File.Exists(srecFile))
        return false;
      try
      {
        using (TextReader textReader = (TextReader) File.OpenText(srecFile))
        {
          using (TextWriter text = (TextWriter) File.CreateText(srecFile + ".ext"))
          {
            int num1 = 0;
            int num2 = 0;
            int num3 = 0;
            string str1 = "";
            StringBuilder stringBuilder1 = new StringBuilder();
            while (textReader.Peek() != -1)
            {
              string str2 = textReader.ReadLine();
              if (str2.ToLower().StartsWith("s7"))
                str1 = str2;
              else if (str2.ToLower().StartsWith("s3"))
              {
                string str3 = num1 != 0 ? str2.Substring(12, str2.Length - 14) : str2.Substring(4, str2.Length - 6);
                num3 += str3.Length / 2;
                if (num1 == 0)
                  stringBuilder1.Append(str2.Substring(0, 2));
                stringBuilder1.Append(str3);
                ++num1;
                for (int startIndex = 0; startIndex < str3.Length - 1; startIndex += 2)
                  num2 += (int) byte.Parse(str3.Substring(startIndex, 2), NumberStyles.HexNumber);
                if (num1 == 8)
                {
                  int num4 = num3 + 1;
                  stringBuilder1 = stringBuilder1.Insert(2, string.Format("{0:X02}", (object) num4));
                  int num5 = num2 + ((num4 & (int) byte.MaxValue) + (num4 >> 8 & (int) byte.MaxValue));
                  stringBuilder1.Append(string.Format("{0:X02}", (object) ((int) byte.MaxValue - ((int) byte.MaxValue & num5))));
                  text.WriteLine(stringBuilder1.ToString());
                  num2 = 0;
                  num1 = 0;
                  num3 = 0;
                  stringBuilder1.Length = 0;
                }
              }
            }
            if (num1 != 0)
            {
              int num6 = num3 + 1;
              StringBuilder stringBuilder2 = stringBuilder1.Insert(2, string.Format("{0:X02}", (object) num6));
              int num7 = num2 + ((num6 & (int) byte.MaxValue) + (num6 >> 8 & (int) byte.MaxValue));
              stringBuilder2.Append(string.Format("{0:X02}", (object) ((int) byte.MaxValue - ((int) byte.MaxValue & num7))));
              text.WriteLine(stringBuilder2.ToString());
            }
            if (str1 != "")
              text.WriteLine(str1);
          }
        }
      }
      catch
      {
        if (File.Exists(srecFile + ".ext"))
          File.Delete(srecFile + ".ext");
        return false;
      }
      return true;
    }

    private Dictionary<uint, string> ParseSrecFile(
      string srecFile,
      out uint entryPoint,
      out uint imageSize)
    {
      entryPoint = 0U;
      imageSize = 0U;
      Dictionary<uint, string> srecFile1 = new Dictionary<uint, string>();
      if (!File.Exists(srecFile))
        return (Dictionary<uint, string>) null;
      FileInfo fileInfo = new FileInfo(srecFile);
      try
      {
        int num = 0;
        using (TextReader textReader = (TextReader) File.OpenText(srecFile))
        {
          while (textReader.Peek() != -1)
          {
            string str = textReader.ReadLine();
            string s = str.Substring(4, 8);
            if (str.ToLower().StartsWith("s7"))
              entryPoint = uint.Parse(s, NumberStyles.HexNumber);
            else if (str.ToLower().StartsWith("s3"))
            {
              num += str.Length - 14;
              srecFile1[uint.Parse(s, NumberStyles.HexNumber)] = str;
            }
          }
        }
        imageSize = (uint) num;
      }
      catch
      {
        return (Dictionary<uint, string>) null;
      }
      return srecFile1;
    }

    private bool DeploySREC(string srecFile, ref uint entryPoint)
    {
      entryPoint = 0U;
      uint imageSize = 0;
      this.m_srecHash.Clear();
      this.m_execSrecHash.Clear();
      this.m_totalSrecs = 0;
      this.m_minSrecAddr = uint.MaxValue;
      this.m_maxSrecAddr = 0U;
      if (!File.Exists(srecFile))
        return false;
      if (File.Exists(srecFile + ".ext"))
        File.Delete(srecFile + ".ext");
      if (this.PreProcesSREC(srecFile) && File.Exists(srecFile + ".ext"))
        srecFile += ".ext";
      Dictionary<uint, string> srecFile1 = this.ParseSrecFile(srecFile, out entryPoint, out imageSize);
      try
      {
        int millisecondsTimeout = 5000;
        uint maxValue = uint.MaxValue;
        this.m_totalSrecs = srecFile1.Count;
        this.m_evtMicroBooterStart.Set();
        this.m_evtMicroBooter.Reset();
        this.m_evtMicroBooterError.Reset();
        while (srecFile1.Count > 0)
        {
          List<uint> uintList = new List<uint>();
          int num = 4;
          uint[] array = new uint[srecFile1.Count];
          srecFile1.Keys.CopyTo(array, 0);
          Array.Sort<uint>(array);
          if (array[0] < maxValue)
            maxValue = array[0];
          foreach (uint key in array)
          {
            if (key < this.m_minSrecAddr)
              this.m_minSrecAddr = key;
            if (key > this.m_maxSrecAddr)
              this.m_maxSrecAddr = key;
            if (this.m_srecHash.ContainsKey(key))
            {
              uintList.Add(key);
            }
            else
            {
              this.m_eng.SendRawBuffer(Encoding.ASCII.GetBytes("\n"));
              this.m_eng.SendRawBuffer(Encoding.ASCII.GetBytes(srecFile1[key]));
              this.m_eng.SendRawBuffer(Encoding.ASCII.GetBytes("\n"));
              if (num-- <= 0)
              {
                this.m_evtMicroBooter.WaitOne(millisecondsTimeout);
                num = 4;
              }
            }
          }
          int count = uintList.Count;
          if (count > 0)
          {
            for (int index = 0; index < count; ++index)
              srecFile1.Remove(uintList[index]);
          }
        }
        if (maxValue != 0U)
        {
          string withoutExtension = Path.GetFileNameWithoutExtension(srecFile);
          if (!string.IsNullOrEmpty(Path.GetExtension(withoutExtension)))
            withoutExtension = Path.GetFileNameWithoutExtension(withoutExtension);
          string directoryName = Path.GetDirectoryName(srecFile);
          string str1;
          string path;
          if (directoryName.ToLower().EndsWith("\\tinyclr.hex"))
          {
            str1 = Path.GetDirectoryName(directoryName) + "\\tinyclr.bin\\" + withoutExtension;
            path = Path.GetDirectoryName(directoryName) + "\\tinyclr.symdefs";
          }
          else
          {
            str1 = Path.GetDirectoryName(srecFile) + "\\" + withoutExtension + ".bin";
            path = Path.GetDirectoryName(srecFile) + "\\" + withoutExtension + ".symdefs";
          }
          if (File.Exists(str1) && File.Exists(path))
          {
            FileInfo fileInfo = new FileInfo(str1);
            uint num1 = 0;
            using (TextReader textReader = (TextReader) File.OpenText(path))
            {
              while (textReader.Peek() != -1)
              {
                string str2 = textReader.ReadLine();
                if (str2.Contains("LOAD_IMAGE_CRC"))
                {
                  int num2 = str2.IndexOf(' ', 2);
                  num1 = uint.Parse(str2.Substring(2, num2 - 2), NumberStyles.HexNumber);
                }
              }
            }
            this.m_execSrecHash[entryPoint] = string.Format("<CRC>{0:X08},{1:X08},{2:X08},{3:X08}</CRC>\n", (object) maxValue, (object) fileInfo.Length, (object) num1, (object) entryPoint);
          }
        }
        return true;
      }
      finally
      {
        this.m_evtMicroBooterStart.Reset();
      }
    }

    private bool CanUpgradeToSsl()
    {
      if (this.m_port == null || this.m_eng == null || !(this.m_port is PortDefinition_Tcp) || this.m_serverCert == null)
        return false;
      return this.m_eng.IsUsingSsl || this.m_eng.CanUpgradeToSsl();
    }

    public bool UpgradeToSsl()
    {
      if (this.CanUpgradeToSsl())
      {
        if (this.m_eng.IsUsingSsl)
          return true;
        IAsyncResult sslBegin = this.m_eng.UpgradeConnectionToSsl_Begin(this.m_serverCert, this.m_requireClientCert);
        if (WaitHandle.WaitAny(new WaitHandle[2]
        {
          sslBegin.AsyncWaitHandle,
          (WaitHandle) this.EventCancel
        }, 10000) == 0)
        {
          try
          {
            if (this.m_eng.UpgradeConnectionToSSL_End(sslBegin))
              return true;
          }
          catch
          {
          }
          this.m_eng.Dispose();
          this.m_eng = (Microsoft.SPOT.Debugger.Engine) null;
        }
      }
      return false;
    }

    private bool DeployMFUpdate(string zipFile)
    {
      if (File.Exists(zipFile))
      {
        byte[] numArray1 = new byte[1024];
        try
        {
          int updateHandle = -1;
          int num1 = 0;
          FileInfo fileInfo = new FileInfo(zipFile);
          int total = ((int) fileInfo.Length + 1024 - 1) / 1024;
          byte[] bytes = Encoding.UTF8.GetBytes(fileInfo.Name + fileInfo.LastWriteTimeUtc.ToString());
          uint crc1 = CRC.ComputeCRC(bytes, 0, bytes.Length, 0U);
          uint crc2 = 0;
          Console.WriteLine(crc1);
          if (this.m_eng.StartUpdate("NetMF", (ushort) 4, (ushort) 2, crc1, 0U, 0U, (uint) fileInfo.Length, 1024U, 0U, ref updateHandle))
          {
            byte[] response = new byte[4];
            if (!this.m_eng.UpdateAuthCommand(updateHandle, 1U, (byte[]) null, ref response) || response.Length < 4)
              return false;
            uint num2;
            using (MemoryStream input = new MemoryStream(response))
            {
              using (BinaryReader binaryReader = new BinaryReader((Stream) input))
                num2 = binaryReader.ReadUInt32();
            }
            byte[] authenticationData = (byte[]) null;
            if (this.m_serverCert != null && this.m_serverCert.PrivateKey is RSACryptoServiceProvider privateKey)
              authenticationData = privateKey.ExportCspBlob(false);
            if (!this.m_eng.UpdateAuthenticate(updateHandle, authenticationData))
              return false;
            if (num2 == 1U && this.m_serverCert != null)
            {
              IAsyncResult sslBegin = this.m_eng.UpgradeConnectionToSsl_Begin(this.m_serverCert, this.m_requireClientCert);
              if (WaitHandle.WaitAny(new WaitHandle[2]
              {
                sslBegin.AsyncWaitHandle,
                (WaitHandle) this.EventCancel
              }, 10000) != 0)
                return false;
              try
              {
                if (!this.m_eng.UpgradeConnectionToSSL_End(sslBegin))
                {
                  this.m_eng.Dispose();
                  this.m_eng = (Microsoft.SPOT.Debugger.Engine) null;
                  return false;
                }
              }
              catch
              {
                this.m_eng.Dispose();
                this.m_eng = (Microsoft.SPOT.Debugger.Engine) null;
                return false;
              }
            }
            RSAPKCS1SignatureFormatter signatureFormatter = (RSAPKCS1SignatureFormatter) null;
            HashAlgorithm hash = (HashAlgorithm) null;
            try
            {
              if (this.m_serverCert != null)
              {
                signatureFormatter = new RSAPKCS1SignatureFormatter(this.m_serverCert.PrivateKey);
                signatureFormatter.SetHashAlgorithm("SHA1");
                hash = (HashAlgorithm) new SHA1CryptoServiceProvider();
                int num3 = hash.HashSize / 8;
              }
            }
            catch
            {
            }
            using (FileStream fileStream = File.OpenRead(zipFile))
            {
              int length;
              while ((length = fileStream.Read(numArray1, 0, 1024)) != 0)
              {
                byte[] numArray2 = numArray1;
                if (length < 1024)
                {
                  numArray2 = new byte[length];
                  Array.Copy((Array) numArray1, (Array) numArray2, length);
                }
                int crc3 = (int) CRC.ComputeCRC(numArray2, 0, numArray2.Length, 0U);
                if (!this.m_eng.AddPacket(updateHandle, (uint) num1++, numArray2, CRC.ComputeCRC(numArray2, 0, numArray2.Length, 0U)))
                  return false;
                crc2 = CRC.ComputeCRC(numArray2, 0, numArray2.Length, crc2);
                if (hash != null)
                {
                  if (num1 == total)
                  {
                    hash.TransformFinalBlock(numArray2, 0, length);
                  }
                  else
                  {
                    byte[] outputBuffer = new byte[length];
                    hash.TransformBlock(numArray2, 0, length, outputBuffer, 0);
                  }
                }
                if (this.OnProgress != null)
                  this.OnProgress((long) num1, (long) total, string.Format(Resources.StatusFlashing, (object) num1));
              }
            }
            byte[] numArray3;
            if (signatureFormatter != null)
            {
              numArray3 = signatureFormatter.CreateSignature(hash);
            }
            else
            {
              numArray3 = new byte[4];
              using (MemoryStream output = new MemoryStream(numArray3))
              {
                using (BinaryWriter binaryWriter = new BinaryWriter((Stream) output))
                  binaryWriter.Write(crc2);
              }
            }
            if (this.m_eng.InstallUpdate(updateHandle, numArray3))
              return true;
          }
        }
        catch
        {
        }
      }
      return false;
    }

    public bool DeployUpdate(string comprFilePath) => this.m_eng.ConnectionSource == ConnectionSource.TinyCLR && this.DeployMFUpdate(comprFilePath);

    public bool Deploy(string filePath, string signatureFile, ref uint entryPoint)
    {
      entryPoint = 0U;
      if (!File.Exists(filePath))
        throw new FileNotFoundException(filePath);
      if (this.m_eng == null)
        throw new MFDeviceNoResponseException();
      if (this.CheckForMicroBooter())
        return this.DeploySREC(filePath, ref entryPoint);
      this.m_eng.TryToConnect(1, 100, true, ConnectionSource.Unknown);
      bool flag = File.Exists(signatureFile);
      FileInfo fileInfo = new FileInfo(filePath);
      ArrayList blocks = new ArrayList();
      entryPoint = SRecordFile.Parse(filePath, blocks, flag ? signatureFile : (string) null);
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
            if (block.data.Read(numArray, 0, count) <= 0 || !this.m_eng.WriteMemory(address, numArray))
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
            if (!this.m_eng.CheckSignature(block.signature == null || block.signature.Length == 0 ? numArray : block.signature, 0U))
              throw new MFSignatureFailureException(signatureFile);
          }
        }
      }
      return true;
    }

    public bool Execute(uint entryPoint)
    {
      if (this.m_eng == null)
        throw new MFDeviceNoResponseException();
      if (this.CheckForMicroBooter())
      {
        if (!this.m_execSrecHash.ContainsKey(entryPoint))
          return false;
        string s = this.m_execSrecHash[entryPoint];
        bool flag = false;
        for (int index = 0; index < 10; ++index)
        {
          this.m_eng.SendRawBuffer(Encoding.ASCII.GetBytes(s));
          this.m_eng.SendRawBuffer(Encoding.ASCII.GetBytes("\n"));
          if (this.m_evtMicroBooter.WaitOne(1000))
          {
            flag = true;
            break;
          }
        }
        return flag;
      }
      Commands.Monitor_Ping.Reply connectionSource = this.m_eng.GetConnectionSource();
      if (connectionSource == null)
        throw new MFDeviceNoResponseException();
      if (connectionSource.m_source == 1U)
        return this.m_eng.ExecuteMemory(entryPoint);
      this.m_eng.RebootDevice(Microsoft.SPOT.Debugger.Engine.RebootOption.RebootClrOnly);
      return true;
    }

    public void Reboot(bool coldBoot)
    {
      if (this.m_eng == null)
        throw new MFDeviceNoResponseException();
      this.m_eng.RebootDevice(coldBoot ? Microsoft.SPOT.Debugger.Engine.RebootOption.NoReconnect : Microsoft.SPOT.Debugger.Engine.RebootOption.RebootClrOnly);
      if (coldBoot)
        return;
      Thread.Sleep(200);
      this.ConnectTo(1000, true, ConnectionSource.TinyCLR);
    }

    public bool IsConnected => this.m_eng != null && this.m_eng.IsConnected;

    public void DoForEachAppDomain(MFDevice.AppDomainAction appDomainAction)
    {
      if (!this.m_eng.Capabilities.AppDomains)
        return;
      Commands.Debugging_TypeSys_AppDomains.Reply appDomains = this.m_eng.GetAppDomains();
      if (appDomains == null)
        return;
      foreach (uint num in appDomains.m_data)
      {
        Commands.Debugging_Resolve_AppDomain.Reply reply = this.m_eng.ResolveAppDomain(num);
        if (reply != null)
          appDomainAction((MFDevice.IAppDomainInfo) new MFDevice.AppDomainInfo(num, reply));
      }
    }

    public void DoForEachAssembly(MFDevice.AssemblyAction assemblyAction)
    {
      List<MFDevice.IAppDomainInfo> theDomains = new List<MFDevice.IAppDomainInfo>();
      this.DoForEachAppDomain((MFDevice.AppDomainAction) (adi => theDomains.Add(adi)));
      Commands.Debugging_Resolve_Assembly[] debuggingResolveAssemblyArray = this.m_eng.ResolveAllAssemblies();
      if (debuggingResolveAssemblyArray == null)
        return;
      foreach (Commands.Debugging_Resolve_Assembly dra in debuggingResolveAssemblyArray)
      {
        MFDevice.AssemblyInfoFromResolveAssembly ai = new MFDevice.AssemblyInfoFromResolveAssembly(dra);
        foreach (MFDevice.IAppDomainInfo adi in theDomains)
        {
          if (Array.IndexOf<uint>(adi.AssemblyIndicies, ai.Index) != -1)
            ai.AddDomain(adi);
        }
        assemblyAction((MFDevice.IAssemblyInfo) ai);
      }
    }

    public MFDevice.IMFDeviceInfo MFDeviceInfo
    {
      get
      {
        if (this.m_deviceInfoCache == null)
          this.m_deviceInfoCache = (MFDevice.IMFDeviceInfo) new MFDevice.MFDeviceInfoImpl(this);
        return this.m_deviceInfoCache;
      }
    }

    public delegate void OnProgressHandler(long value, long total, string status);

    public interface IAppDomainInfo
    {
      string Name { get; }

      uint ID { get; }

      uint[] AssemblyIndicies { get; }
    }

    public interface IAssemblyInfo
    {
      string Name { get; }

      System.Version Version { get; }

      uint Index { get; }

      List<MFDevice.IAppDomainInfo> InAppDomains { get; }
    }

    private class AppDomainInfo : MFDevice.IAppDomainInfo
    {
      private uint m_id;
      private Commands.Debugging_Resolve_AppDomain.Reply m_reply;

      public AppDomainInfo(uint id, Commands.Debugging_Resolve_AppDomain.Reply reply)
      {
        this.m_id = id;
        this.m_reply = reply;
      }

      public string Name => this.m_reply.Name;

      public uint ID => this.m_id;

      public uint[] AssemblyIndicies => this.m_reply.m_data;
    }

    private class AssemblyInfoFromResolveAssembly : MFDevice.IAssemblyInfo
    {
      private Commands.Debugging_Resolve_Assembly m_dra;
      private List<MFDevice.IAppDomainInfo> m_AppDomains = new List<MFDevice.IAppDomainInfo>();

      public AssemblyInfoFromResolveAssembly(Commands.Debugging_Resolve_Assembly dra) => this.m_dra = dra;

      public string Name => this.m_dra.m_reply.Name;

      public System.Version Version
      {
        get
        {
          Commands.Debugging_Resolve_Assembly.Version version = this.m_dra.m_reply.m_version;
          return new System.Version((int) version.iMajorVersion, (int) version.iMinorVersion, (int) version.iBuildNumber, (int) version.iRevisionNumber);
        }
      }

      public uint Index => this.m_dra.m_idx;

      public List<MFDevice.IAppDomainInfo> InAppDomains => this.m_AppDomains;

      public void AddDomain(MFDevice.IAppDomainInfo adi)
      {
        if (adi == null)
          return;
        this.m_AppDomains.Add(adi);
      }
    }

    public delegate void AppDomainAction(MFDevice.IAppDomainInfo adi);

    public delegate void AssemblyAction(MFDevice.IAssemblyInfo ai);

    public interface IMFDeviceInfo
    {
      bool Valid { get; }

      System.Version HalBuildVersion { get; }

      string HalBuildInfo { get; }

      byte OEM { get; }

      byte Model { get; }

      ushort SKU { get; }

      string ModuleSerialNumber { get; }

      string SystemSerialNumber { get; }

      System.Version ClrBuildVersion { get; }

      string ClrBuildInfo { get; }

      System.Version TargetFrameworkVersion { get; }

      System.Version SolutionBuildVersion { get; }

      string SolutionBuildInfo { get; }

      MFDevice.IAppDomainInfo[] AppDomains { get; }

      MFDevice.IAssemblyInfo[] Assemblies { get; }
    }

    private class MFDeviceInfoImpl : MFDevice.IMFDeviceInfo
    {
      private MFDevice m_self;
      private bool m_fValid;
      private List<MFDevice.IAppDomainInfo> m_Domains = new List<MFDevice.IAppDomainInfo>();
      private List<MFDevice.IAssemblyInfo> m_AssemblyInfos = new List<MFDevice.IAssemblyInfo>();

      public MFDeviceInfoImpl(MFDevice dev)
      {
        this.m_self = dev;
        if (!this.Dbg.IsConnectedToTinyCLR)
          return;
        this.m_self.DoForEachAppDomain((MFDevice.AppDomainAction) (adi => this.m_Domains.Add(adi)));
        this.m_self.DoForEachAssembly((MFDevice.AssemblyAction) (ai => this.m_AssemblyInfos.Add(ai)));
        this.m_fValid = true;
      }

      private Microsoft.SPOT.Debugger.Engine Dbg => this.m_self.DbgEngine;

      public bool Valid => this.m_fValid;

      public System.Version HalBuildVersion => this.Dbg.Capabilities.HalSystemInfo.halVersion;

      public string HalBuildInfo => this.Dbg.Capabilities.HalSystemInfo.halVendorInfo;

      public byte OEM => this.Dbg.Capabilities.HalSystemInfo.oemCode;

      public byte Model => this.Dbg.Capabilities.HalSystemInfo.modelCode;

      public ushort SKU => this.Dbg.Capabilities.HalSystemInfo.skuCode;

      public string ModuleSerialNumber => this.Dbg.Capabilities.HalSystemInfo.moduleSerialNumber;

      public string SystemSerialNumber => this.Dbg.Capabilities.HalSystemInfo.systemSerialNumber;

      public System.Version ClrBuildVersion => this.Dbg.Capabilities.ClrInfo.clrVersion;

      public string ClrBuildInfo => this.Dbg.Capabilities.ClrInfo.clrVendorInfo;

      public System.Version TargetFrameworkVersion => this.Dbg.Capabilities.ClrInfo.targetFrameworkVersion;

      public System.Version SolutionBuildVersion => this.Dbg.Capabilities.SolutionReleaseInfo.solutionVersion;

      public string SolutionBuildInfo => this.Dbg.Capabilities.SolutionReleaseInfo.solutionVendorInfo;

      public MFDevice.IAppDomainInfo[] AppDomains => this.m_Domains.ToArray();

      public MFDevice.IAssemblyInfo[] Assemblies => this.m_AssemblyInfos.ToArray();
    }
  }
}
