// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine.MFApplicationDeployment
// Assembly: MFDeployEngine, Version=2.0.0.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: 7FFE591E-9FC8-4A2A-9A07-642B2A02EB3C
// Assembly location: D:\Androvil\Visual Studio 2022\Projects\MfDeploy\Tools\MFDeployEngine.dll

using Microsoft.SPOT.Debugger.WireProtocol;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool.Engine
{
  public class MFApplicationDeployment
  {
    private MFDevice m_device;

    public MFApplicationDeployment(MFDevice device) => this.m_device = device;

    private bool CreateDeploymentReadHelper(
      BackgroundWorker backgroundWorker,
      DoWorkEventArgs doWorkEventArgs,
      Microsoft.SPOT.Debugger.Engine engine,
      ref uint address,
      uint addressStart,
      uint addressEnd,
      uint bytes,
      out byte[] data)
    {
      data = new byte[bytes];
      uint index = 0;
      while (bytes > 0U)
      {
        uint length = Math.Min(1024U, bytes);
        byte[] buf;
        if (!engine.ReadMemory(address, length, out buf))
          throw new ApplicationException("Cannot read data");
        buf.CopyTo((Array) data, (long) index);
        address += length;
        index += length;
        bytes -= length;
        if (backgroundWorker.CancellationPending)
        {
          doWorkEventArgs.Cancel = true;
          return false;
        }
      }
      return true;
    }

    public void CreateDeploymentData(
      BackgroundWorker backgroundWorker,
      DoWorkEventArgs doWorkEventArgs)
    {
      Microsoft.SPOT.Debugger.Engine dbgEngine = (doWorkEventArgs.Argument as MFDevice).DbgEngine;
      Commands.Monitor_FlashSectorMap.Reply flashSectorMap = dbgEngine.GetFlashSectorMap();
      MemoryStream readStream = new MemoryStream();
      int index1 = -1;
      int num1 = 0;
      uint addressStart = 0;
      uint addressEnd = 0;
      for (int index2 = 0; index2 < flashSectorMap.m_map.Length; ++index2)
      {
        Commands.Monitor_FlashSectorMap.FlashSectorData flashSectorData = flashSectorMap.m_map[index2];
        if (((int) flashSectorData.m_flags & 240) == 80)
        {
          if (index1 < 0)
          {
            index1 = index2;
            addressStart = flashSectorData.m_address;
          }
          num1 = index2;
          addressEnd = flashSectorData.m_address + flashSectorData.m_size;
        }
      }
      if (index1 < 0)
        throw new ApplicationException("Could not find deployment sectors");
      uint address = addressStart;
      int index3 = index1;
label_10:
      uint num2;
      while (true)
      {
        do
        {
          if (backgroundWorker.WorkerReportsProgress)
          {
            int percentProgress = (int) (100.0 * (double) address / (double) addressEnd);
            backgroundWorker.ReportProgress(percentProgress);
          }
          uint bytes1 = (uint) Marshal.SizeOf(typeof (MFApplicationDeployment.CLR_RECORD_ASSEMBLY));
          byte[] data1 = (byte[]) null;
          byte[] data2 = (byte[]) null;
          if (address + bytes1 < addressEnd)
          {
            num2 = address;
            if (!this.CreateDeploymentReadHelper(backgroundWorker, doWorkEventArgs, dbgEngine, ref address, addressStart, addressEnd, bytes1, out data1))
              return;
            GCHandle gcHandle = GCHandle.Alloc((object) data1, GCHandleType.Pinned);
            MFApplicationDeployment.CLR_RECORD_ASSEMBLY structure = (MFApplicationDeployment.CLR_RECORD_ASSEMBLY) Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), typeof (MFApplicationDeployment.CLR_RECORD_ASSEMBLY));
            gcHandle.Free();
            bool flag = structure.marker == 13920295833523021L;
            if (flag)
            {
              uint headerCrc = structure.headerCRC;
              int num3 = 8;
              int length = 4;
              Array.Clear((Array) data1, num3, length);
              uint crc = Microsoft.SPOT.Debugger.CRC.ComputeCRC(data1, 0U);
              Array.Copy((Array) BitConverter.GetBytes(headerCrc), 0, (Array) data1, num3, length);
              flag = (int) headerCrc == (int) crc;
            }
            if (flag)
            {
              uint bytes2 = structure.startOfTables[15] - bytes1;
              if (address + bytes2 < addressEnd)
              {
                if (!this.CreateDeploymentReadHelper(backgroundWorker, doWorkEventArgs, dbgEngine, ref address, addressStart, addressEnd, bytes2, out data2))
                  return;
                flag = (int) Microsoft.SPOT.Debugger.CRC.ComputeCRC(data2, 0U) == (int) structure.assemblyCRC;
              }
              else
                goto label_29;
            }
            if (flag)
            {
              readStream.Write(data1, 0, data1.Length);
              readStream.Write(data2, 0, data2.Length);
            }
            else
              goto label_27;
          }
          else
            goto label_29;
        }
        while (address % 4U == 0U);
        byte[] buffer = new byte[(4U - address % 4U)];
        readStream.Write(buffer, 0, buffer.Length);
        address += 4U - address % 4U;
      }
label_27:
      for (; index3 < num1; ++index3)
      {
        Commands.Monitor_FlashSectorMap.FlashSectorData flashSectorData = flashSectorMap.m_map[index3];
        if (num2 >= flashSectorData.m_address && num2 < flashSectorData.m_address + flashSectorData.m_size)
        {
          address = flashSectorData.m_address + flashSectorData.m_size;
          break;
        }
      }
      goto label_10;
label_29:
      MFApplicationDeployment.MFApplicationDeploymentData applicationDeploymentData = new MFApplicationDeployment.MFApplicationDeploymentData();
      long count1 = readStream.Seek(0L, SeekOrigin.Current);
      MemoryStream writeStream = new MemoryStream();
      readStream.Seek(0L, SeekOrigin.Begin);
      applicationDeploymentData.BinaryData = new byte[(int) count1];
      readStream.Read(applicationDeploymentData.BinaryData, 0, (int) count1);
      readStream.Seek(0L, SeekOrigin.Begin);
      new MFApplicationDeployment.BinToSrec().DoConversion((Stream) readStream, (Stream) writeStream, flashSectorMap.m_map[index1].m_address);
      long count2 = writeStream.Seek(0L, SeekOrigin.Current);
      applicationDeploymentData.HexData = new byte[count2];
      writeStream.Seek(0L, SeekOrigin.Begin);
      writeStream.Read(applicationDeploymentData.HexData, 0, (int) count2);
      doWorkEventArgs.Result = (object) applicationDeploymentData;
    }

    public MFApplicationDeployment.MFApplicationDeploymentData CreateDeploymentData()
    {
      BackgroundWorker backgroundWorker = new BackgroundWorker();
      DoWorkEventArgs doWorkEventArgs = new DoWorkEventArgs((object) this.m_device);
      this.CreateDeploymentData(backgroundWorker, doWorkEventArgs);
      return doWorkEventArgs.Result as MFApplicationDeployment.MFApplicationDeploymentData;
    }

    public class MFApplicationDeploymentData
    {
      public byte[] BinaryData;
      public byte[] HexData;
    }

    [StructLayout(LayoutKind.Sequential)]
    private class CLR_RECORD_VERSION
    {
      public ushort iMajorVersion;
      public ushort iMinorVersion;
      public ushort iBuildNumber;
      public ushort iRevisionNumber;
    }

    [StructLayout(LayoutKind.Sequential, Size = 124)]
    private class CLR_RECORD_ASSEMBLY
    {
      public const int TBL_EndOfAssembly = 15;
      public const int TBL_MAX = 16;
      public const long MARKER_ASSEMBLY_V1 = 13920295833523021;
      public long marker;
      public uint headerCRC;
      public uint assemblyCRC;
      public uint flags;
      public uint nativeMethodsChecksum;
      public uint patchEntryOffset;
      public MFApplicationDeployment.CLR_RECORD_VERSION version;
      public ushort assemblyName;
      public ushort stringTableVersion;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
      public uint[] startOfTables;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
      public byte[] extra;
    }

    public class BinToSrec
    {
      private const int c_nHeaderLength = 12;
      private const int c_nTailLength = 2;
      private const int c_nBaseLength = 14;

      public void DoConversion(Stream readStream, Stream writeStream, uint nBaseAddress)
      {
        int count = 16;
        byte[] numArray = new byte[count];
        uint nCurrentAddress = nBaseAddress;
        int nRead;
        while ((nRead = readStream.Read(numArray, 0, count)) != 0)
        {
          char[] chArray = this.ConstructSrecRecord("S3", numArray, nRead, nCurrentAddress);
          if (chArray == null)
            throw new Exception("Problem in Code");
          for (int index = 0; index < chArray.Length; ++index)
            writeStream.WriteByte((byte) chArray[index]);
          writeStream.WriteByte((byte) 13);
          writeStream.WriteByte((byte) 10);
          nCurrentAddress += 16U;
        }
      }

      public byte[] GetBinaryData(string hexFile)
      {
        List<byte> byteList = new List<byte>();
        TextReader textReader = (TextReader) File.OpenText(hexFile);
        string str;
        while ((str = textReader.ReadLine()) != null)
        {
          for (int startIndex = 12; startIndex < str.Length - 2; startIndex += 2)
            byteList.Add(byte.Parse(str.Substring(startIndex, 2), NumberStyles.HexNumber));
        }
        return byteList.ToArray();
      }

      private char[] ConstructSrecRecord(
        string szType,
        byte[] arrByteInput,
        int nRead,
        uint nCurrentAddress)
      {
        char[] arrToWriteTo = new char[14 + nRead * 2];
        if (szType == null || szType.Length != 2)
          return (char[]) null;
        arrToWriteTo[0] = szType[0];
        arrToWriteTo[1] = szType[1];
        byte num1 = (byte) ((arrToWriteTo.Length - 4) / 2);
        string str1 = string.Format("{0:X2}", (object) num1);
        arrToWriteTo[2] = str1[0];
        arrToWriteTo[3] = str1[1];
        int num2 = (int) num1;
        string str2 = string.Format("{0:X8}", (object) nCurrentAddress);
        for (int index = 0; index < str2.Length; ++index)
          arrToWriteTo[4 + index] = str2[index];
        for (int index = 0; index < 4; ++index)
        {
          num2 = (int) ((long) num2 + (long) (nCurrentAddress & (uint) byte.MaxValue));
          nCurrentAddress >>= 8;
        }
        for (int index = 0; index < nRead; ++index)
          num2 += (int) arrByteInput[index];
        int num3 = int.MaxValue - num2;
        this.ConvertToHexAscii(arrToWriteTo, 12, arrByteInput, nRead);
        string str3 = string.Format("{0:X8}", (object) num3);
        arrToWriteTo[arrToWriteTo.Length - 2] = str3[str3.Length - 2];
        arrToWriteTo[arrToWriteTo.Length - 1] = str3[str3.Length - 1];
        return arrToWriteTo;
      }

      private void ConvertToHexAscii(
        char[] arrToWriteTo,
        int nIndexToStart,
        byte[] arrInput,
        int nRead)
      {
        for (int index = 0; index < nRead; ++index)
        {
          string str = string.Format("{0:X2}", (object) arrInput[index]);
          arrToWriteTo[nIndexToStart] = str[0];
          arrToWriteTo[nIndexToStart + 1] = str[1];
          nIndexToStart += 2;
        }
      }
    }
  }
}
