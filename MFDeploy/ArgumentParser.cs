// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.ArgumentParser
// Assembly: MFDeploy, Version=4.3.1.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: 9518305E-2E58-43E8-9F45-41B78F83BC9E
// Assembly location: C:\Program Files (x86)\Microsoft .NET Micro Framework\v4.3\Tools\MFDeploy.exe

using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties;
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool
{
  internal class ArgumentParser
  {
    internal const string FlagPrefix = "[/-]";
    internal const char ArgSeparator = ':';
    internal const char FileSeparator = ';';
    private ArgumentParser.CommandMap[] m_commandMap;
    private ArgumentParser.InterfaceMap[] m_interfaceMap;
    private MFPortDefinition[] m_transports = new MFPortDefinition[2];
    private ArgumentParser.Commands m_cmd = ~ArgumentParser.Commands.Erase;
    private string[] m_flashFiles = new string[0];
    private bool m_fWarmReboot;

    internal MFPortDefinition Interface => this.m_transports[0];

    internal MFPortDefinition TinybtrInterface => this.m_transports[1];

    internal ArgumentParser.Commands Command => this.m_cmd;

    internal ArgumentParser()
    {
      string str1 = "((\"[\\w\\W]+\")*|([\\\\d\\w\\._\\-\\:]+))";
      string str2 = str1 + "(" + (object) ';' + str1 + ")*";
      this.m_commandMap = new ArgumentParser.CommandMap[6]
      {
        new ArgumentParser.CommandMap(ArgumentParser.Commands.Erase, "", string.Format("{0,-35}{1}", (object) "Erase", (object) Resources.HelpCommandErase)),
        new ArgumentParser.CommandMap(ArgumentParser.Commands.Deploy, ':'.ToString() + str2, string.Format("{0,-35}{1}", (object) "Deploy:<image file>[;<image file>]*", (object) Resources.HelpCommandFlash)),
        new ArgumentParser.CommandMap(ArgumentParser.Commands.Ping, "", string.Format("{0,-35}{1}", (object) "Ping", (object) Resources.HelpCommandPing)),
        new ArgumentParser.CommandMap(ArgumentParser.Commands.Reboot, "(" + (object) ':' + "warm)?", string.Format("{0,-35}{1}", (object) "Reboot[:Warm]", (object) Resources.HelpCommandReboot)),
        new ArgumentParser.CommandMap(ArgumentParser.Commands.List, "", string.Format("{0,-35}{1}", (object) "List", (object) Resources.HelpCommandList)),
        new ArgumentParser.CommandMap(ArgumentParser.Commands.Help, "", string.Format("{0,-35}{1}", (object) "Help", (object) Resources.HelpCommandHelp))
      };
      this.m_interfaceMap = new ArgumentParser.InterfaceMap[3]
      {
        new ArgumentParser.InterfaceMap(TransportType.Serial, ':'.ToString() + "\\d+", string.Format("{0,-35}{1}", (object) "Serial:<port_num>", (object) Resources.HelpInterfaceCom)),
        new ArgumentParser.InterfaceMap(TransportType.USB, ':'.ToString() + "[\\W\\w]+", string.Format("{0,-35}{1}", (object) "USB:<usb_name>", (object) Resources.HelpInterfaceUsb)),
        new ArgumentParser.InterfaceMap(TransportType.TCPIP, ':'.ToString() + "\\d\\d?\\d?\\.\\d\\d?\\d?\\.\\d\\d?\\d?\\.\\d\\d?\\d?", string.Format("{0,-35}{1}", (object) "TCPIP:<ip_addr>", (object) Resources.HelpInterfaceTcpIp))
      };
    }

    internal string ValidCommandExpression
    {
      get
      {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("(");
        for (int index = 0; index < this.m_commandMap.Length; ++index)
        {
          if (index > 0)
            stringBuilder.Append("|");
          stringBuilder.Append("(");
          stringBuilder.Append(this.m_commandMap[index].Command.ToString());
          stringBuilder.Append(this.m_commandMap[index].CommandArgExpr);
          stringBuilder.Append(")");
        }
        stringBuilder.Append(")");
        return stringBuilder.ToString();
      }
    }

    internal string ValidInterfaceExpression
    {
      get
      {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("[/-]");
        stringBuilder.Append('I');
        stringBuilder.Append(':'.ToString() + "(");
        for (int index = 0; index < this.m_interfaceMap.Length; ++index)
        {
          if (index > 0)
            stringBuilder.Append("|");
          stringBuilder.Append("(");
          stringBuilder.Append(this.m_interfaceMap[index].Interface.ToString());
          stringBuilder.Append(this.m_interfaceMap[index].InterfaceArgExpr);
          stringBuilder.Append(")");
        }
        stringBuilder.Append("){1}");
        return stringBuilder.ToString();
      }
    }

    internal bool ValidateArgs(string args, out string error)
    {
      bool flag = true;
      error = "";
      try
      {
        MatchCollection matchCollection1 = new Regex(this.ValidCommandExpression, RegexOptions.IgnoreCase).Matches(args);
        if (matchCollection1.Count != 1)
        {
          if (new Regex("[/-]\\?|[/-]h", RegexOptions.IgnoreCase).IsMatch(args))
            this.m_cmd = ArgumentParser.Commands.Help;
          else
            error += matchCollection1.Count == 0 ? Resources.ErrorCommandMissing : Resources.ErrorCommandsMultiple + "\r\n";
        }
        else if (matchCollection1[0].Groups[0].Success)
        {
          string[] strArray = matchCollection1[0].Groups[0].Value.Trim().Split(new char[1]
          {
            ':'
          }, 2);
          this.m_cmd = (ArgumentParser.Commands) Enum.Parse(typeof (ArgumentParser.Commands), strArray[0], true);
          switch (this.m_cmd)
          {
            case ArgumentParser.Commands.Deploy:
              strArray[1] = strArray[1].Replace("\"", "");
              this.m_flashFiles = strArray[1].Trim().Split(new char[1]
              {
                ';'
              }, StringSplitOptions.RemoveEmptyEntries);
              break;
            case ArgumentParser.Commands.Reboot:
              this.m_fWarmReboot = strArray.Length > 1;
              break;
          }
          args = args.Replace(matchCollection1[0].Groups[0].Value, "");
        }
        if (this.m_cmd != ArgumentParser.Commands.Help)
        {
          if (this.m_cmd != ArgumentParser.Commands.List)
          {
            MatchCollection matchCollection2 = new Regex(this.ValidInterfaceExpression, RegexOptions.IgnoreCase).Matches(args);
            if (matchCollection2.Count <= 0 || matchCollection2.Count > 2)
            {
              error = matchCollection2.Count == 0 ? Resources.ErrorInterfaceMissing : Resources.ErrorInterfaceMultiple + "\r\n";
              flag = false;
            }
            else
            {
              for (int i = 0; i < matchCollection2.Count; ++i)
              {
                if (matchCollection2[i].Groups[0].Success)
                {
                  string[] strArray = matchCollection2[i].Groups[0].Value.Trim().Split(':');
                  switch ((TransportType) Enum.Parse(typeof (TransportType), strArray[1], true))
                  {
                    case TransportType.Serial:
                      this.m_transports[i] = (MFPortDefinition) new MFSerialPort("COM" + strArray[2], "\\\\.\\COM" + strArray[2]);
                      break;
                    case TransportType.USB:
                      this.m_transports[i] = (MFPortDefinition) new MFUsbPort(strArray[2]);
                      break;
                    case TransportType.TCPIP:
                      this.m_transports[i] = (MFPortDefinition) new MFTcpIpPort(strArray[2], "");
                      break;
                  }
                }
                args = args.Replace(matchCollection2[i].Groups[0].Value, "");
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        flag = false;
        ref string local = ref error;
        local = local + ex.ToString() + "\r\n";
      }
      if (this.m_cmd != ArgumentParser.Commands.Help && this.m_cmd != ArgumentParser.Commands.List && flag)
      {
        args = args.Trim();
        if (args.Length != 0)
        {
          flag = false;
          error = Resources.ErrorArgumentsInvalid + args;
        }
      }
      return flag;
    }

    internal void OnStatus(long current, long total, string txt) => Console.WriteLine(txt);

    internal void OnDeployStatus(long current, long total, string txt) => Console.Write("\r{0}%", (object) (int) (100.0 * ((double) current / (double) total)));

    internal void Execute()
    {
      MFDeploy mfDeploy = new MFDeploy();
      try
      {
        switch (this.m_cmd)
        {
          case ArgumentParser.Commands.Erase:
            Console.Write(Resources.StatusErasing);
            try
            {
              MFDevice mfDevice = mfDeploy.Connect(this.m_transports[0]);
              mfDevice.OnProgress += new MFDevice.OnProgressHandler(this.OnStatus);
              Console.WriteLine(mfDevice.Erase() ? Resources.ResultSuccess : Resources.ResultFailure);
              mfDevice.OnProgress -= new MFDevice.OnProgressHandler(this.OnStatus);
              break;
            }
            catch (Exception ex)
            {
              Console.WriteLine(Resources.ErrorPrefix + ex.Message);
              break;
            }
          case ArgumentParser.Commands.Deploy:
            try
            {
              bool flag = false;
              uint entryPoint1 = 0;
              MFDevice mfDevice = mfDeploy.Connect(this.m_transports[0]);
              foreach (string flashFile in this.m_flashFiles)
              {
                uint entryPoint2 = 0;
                FileInfo fileInfo = new FileInfo(flashFile);
                string str1 = flashFile;
                if (fileInfo.Extension != null || fileInfo.Extension.Length > 0)
                {
                  int startIndex = flashFile.LastIndexOf(fileInfo.Extension);
                  str1 = flashFile.Remove(startIndex, fileInfo.Extension.Length);
                }
                string str2 = str1 + ".sig";
                if (!File.Exists(flashFile))
                {
                  Console.WriteLine(string.Format(Resources.ErrorFileNotFound, (object) flashFile));
                  break;
                }
                if (!File.Exists(str2))
                {
                  Console.WriteLine(string.Format(Resources.ErrorFileNotFound, (object) str2));
                  break;
                }
                Console.WriteLine(string.Format(Resources.StatusFlashing, (object) flashFile));
                mfDevice.OnProgress += new MFDevice.OnProgressHandler(this.OnDeployStatus);
                flag = mfDevice.Deploy(flashFile, str2, ref entryPoint2);
                mfDevice.OnProgress -= new MFDevice.OnProgressHandler(this.OnDeployStatus);
                Console.WriteLine();
                Console.WriteLine(flag ? Resources.ResultSuccess : Resources.ResultFailure);
                if (entryPoint2 != 0U && entryPoint1 == 0U || flashFile.ToLower().Contains("\\er_flash"))
                  entryPoint1 = entryPoint2;
              }
              if (!flag)
                break;
              Console.WriteLine(string.Format(Resources.StatusExecuting, (object) entryPoint1));
              mfDevice.Execute(entryPoint1);
              break;
            }
            catch (Exception ex)
            {
              Console.WriteLine(Resources.ErrorPrefix + ex.Message);
              break;
            }
          case ArgumentParser.Commands.Ping:
            Console.Write(Resources.StatusPinging);
            try
            {
              Console.WriteLine(mfDeploy.Connect(this.m_transports[0]).Ping().ToString());
              break;
            }
            catch (Exception ex)
            {
              Console.WriteLine(Resources.ErrorPrefix + ex.Message);
              break;
            }
          case ArgumentParser.Commands.Reboot:
            try
            {
              MFDevice mfDevice = mfDeploy.Connect(this.m_transports[0]);
              Console.WriteLine(Resources.StatusRebooting);
              mfDevice.Reboot(!this.m_fWarmReboot);
              break;
            }
            catch (Exception ex)
            {
              Console.WriteLine(Resources.ErrorPrefix + ex.Message);
              break;
            }
          case ArgumentParser.Commands.List:
            IEnumerator enumerator = mfDeploy.DeviceList.GetEnumerator();
            try
            {
              while (enumerator.MoveNext())
                Console.WriteLine(((MFPortDefinition) enumerator.Current).Name);
              break;
            }
            finally
            {
              if (enumerator is IDisposable disposable)
                disposable.Dispose();
            }
          case ArgumentParser.Commands.Help:
            Console.WriteLine();
            Console.WriteLine(Resources.HelpBanner);
            Console.WriteLine();
            Console.WriteLine(Resources.HelpDescription);
            Console.WriteLine();
            Console.WriteLine(Resources.HelpUsage);
            Console.WriteLine();
            Console.WriteLine(Resources.HelpCommand);
            foreach (ArgumentParser.CommandMap command in this.m_commandMap)
              Console.WriteLine("  " + command.HelpString);
            Console.WriteLine();
            Console.WriteLine(Resources.HelpInterface);
            foreach (ArgumentParser.InterfaceMap interfaceMap in this.m_interfaceMap)
              Console.WriteLine("  " + interfaceMap.InterfaceHelp);
            Console.WriteLine();
            Console.WriteLine(Resources.HelpInterfaceSpecial);
            Console.WriteLine();
            break;
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(string.Format(Resources.ErrorFailure, (object) ex.Message));
      }
      finally
      {
        mfDeploy?.Dispose();
      }
    }

    internal enum Commands
    {
      Erase,
      Deploy,
      Ping,
      Reboot,
      List,
      Help,
    }

    internal class CommandMap
    {
      internal ArgumentParser.Commands Command;
      internal string CommandArgExpr;
      internal string HelpString;

      internal CommandMap(ArgumentParser.Commands cmd, string cmdArgs, string hlp)
      {
        this.Command = cmd;
        this.CommandArgExpr = cmdArgs;
        this.HelpString = hlp;
      }
    }

    internal class InterfaceMap
    {
      internal TransportType Interface;
      internal string InterfaceArgExpr;
      internal string InterfaceHelp;

      internal InterfaceMap(TransportType itf, string itfArgs, string itfHelp)
      {
        this.Interface = itf;
        this.InterfaceArgExpr = itfArgs;
        this.InterfaceHelp = itfHelp;
      }
    }
  }
}
