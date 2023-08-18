// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Program
// Assembly: MFDeploy, Version=2.0.0.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: 2BA2704E-D6D9-4FE0-ACF3-88D9CCA0E272
// Assembly location: D:\Androvil\Visual Studio 2022\Projects\MfDeploy\Tools\MFDeploy.exe

using Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine;
using Microsoft.NetMicroFramework.Tools.MFDeployTool.Properties;
using System;
using System.Text;
using System.Windows.Forms;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool
{
  public static class Program
  {
    [STAThread]
    public static void Main(params string[] args)
    {
      bool flag = true;
      MFPortDefinition mfPortDefinition1 = (MFPortDefinition) null;
      MFPortDefinition mfPortDefinition2 = (MFPortDefinition) null;
      if (args.Length > 0)
      {
        try
        {
          StringBuilder stringBuilder = new StringBuilder();
          foreach (string str1 in args)
          {
            if (str1.ToUpper().StartsWith(ArgumentParser.Commands.Deploy.ToString().ToUpper() + (object) ':'))
            {
              string str2 = str1.Replace(':'.ToString(), ':'.ToString() + "\"").Replace(';'.ToString(), "\"" + (object) ';' + "\"");
              stringBuilder.AppendFormat("{0}\"", (object) str2);
            }
            else
              stringBuilder.AppendFormat("{0} ", (object) str1);
          }
          string args1 = stringBuilder.ToString().Trim();
          ArgumentParser argumentParser = new ArgumentParser();
          string error;
          if (!argumentParser.ValidateArgs(args1, out error))
          {
            flag = false;
            Console.WriteLine(Resources.ErrorPrefix + error);
          }
          else if (argumentParser.Command != ~ArgumentParser.Commands.Erase)
          {
            flag = false;
            argumentParser.Execute();
          }
          else if (argumentParser.Interface != null)
          {
            if (argumentParser.Interface.Transport != ~TransportType.Serial)
            {
              mfPortDefinition1 = argumentParser.Interface;
              mfPortDefinition2 = argumentParser.TinybtrInterface;
            }
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine(Resources.ErrorPrefix + ex.Message);
        }
      }
      if (!flag)
        return;
      NativeMethods.FreeConsole();
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run((Form) new Form1()
      {
        Transport = mfPortDefinition1,
        TransportTinyBooter = mfPortDefinition2
      });
    }
  }
}
