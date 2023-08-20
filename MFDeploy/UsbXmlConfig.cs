// Decompiled with JetBrains decompiler
// Type: Microsoft.NetMicroFramework.Tools.MFDeployTool.UsbXMLConfigSet
// Assembly: MFDeploy, Version=4.3.1.0, Culture=neutral, PublicKeyToken=2670f5f21e7f4192
// MVID: 9518305E-2E58-43E8-9F45-41B78F83BC9E
// Assembly location: C:\Program Files (x86)\Microsoft .NET Micro Framework\v4.3\Tools\MFDeploy.exe

using System;
using System.Collections;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace Microsoft.NetMicroFramework.Tools.MFDeployTool
{
  public class UsbXMLConfigSet
  {
    private const byte USB_DISPLAY_STRING_NUM = 4;
    private const byte USB_FRIENDLY_STRING_NUM = 5;
    private const byte USB_END_DESCRIPTOR_MARKER = 0;
    private const byte USB_GENERIC_DESCRIPTOR_MARKER = 255;
    private const byte USB_DESCRIPTOR_HEADER_LENGTH = 4;
    private const byte HEADER_MARKER_OFFSET = 0;
    private const byte HEADER_INDEX_OFFSET = 1;
    private const byte HEADER_LENGTH_OFFSET = 2;
    private string FileName;
    private XmlTextReader reader;
    private bool countOnly;
    private byte[] data;
    private int dataIndex;
    private ArrayList stringList;
    private int autoIndex;

    public UsbXMLConfigSet(string fileName)
    {
      this.stringList = new ArrayList();
      this.FileName = fileName;
    }

    public byte[] Read()
    {
      FileStream input;
      try
      {
        input = File.OpenRead(this.FileName);
      }
      catch (Exception ex)
      {
        int num = (int) MessageBox.Show("XML file could not be opened due to exception: " + ex.Message);
        return (byte[]) null;
      }
      this.reader = new XmlTextReader((Stream) input);
      this.stringList.Clear();
      this.autoIndex = 1;
      this.countOnly = true;
      this.dataIndex = 0;
      try
      {
        if (!this.ParseXmlFile(this.reader))
        {
          this.reader.Close();
          return (byte[]) null;
        }
      }
      catch (Exception ex)
      {
        this.reader.Close();
        int num = (int) MessageBox.Show("Exception encountered while trying to parse XML file: " + ex.Message);
        return (byte[]) null;
      }
      this.DumpStrings();
      if (this.dataIndex == 0)
      {
        int num = (int) MessageBox.Show("XML file contained no useful information");
        return (byte[]) null;
      }
      this.dataIndex += 4;
      try
      {
        input.Seek(0L, SeekOrigin.Begin);
        this.reader = new XmlTextReader((Stream) input);
      }
      catch (Exception ex)
      {
        int num = (int) MessageBox.Show("Exception trying to Seek to beginning of file: " + ex.Message);
        return (byte[]) null;
      }
      this.countOnly = false;
      this.stringList.Clear();
      this.autoIndex = 1;
      this.data = new byte[this.dataIndex];
      this.dataIndex = 0;
      try
      {
        if (!this.ParseXmlFile(this.reader))
        {
          this.reader.Close();
          return (byte[]) null;
        }
      }
      catch (Exception ex)
      {
        this.reader.Close();
        int num = (int) MessageBox.Show("Exception encountered during second pass of XML file: " + ex.Message);
        return (byte[]) null;
      }
      this.DumpStrings();
      this.data[this.dataIndex] = (byte) 0;
      this.data[this.dataIndex + 1] = (byte) 0;
      this.data[this.dataIndex + 2] = (byte) 0;
      this.data[this.dataIndex + 2 + 1] = (byte) 0;
      this.reader.Close();
      return this.data;
    }

    private bool ParseXmlFile(XmlTextReader reader)
    {
      do
        ;
      while (reader.Read() && (reader.NodeType != XmlNodeType.Element || !(reader.Name == "UsbControllerConfiguration")));
      while (reader.Read())
      {
        if (reader.NodeType == XmlNodeType.Element)
        {
          if (reader.Name == "DeviceDescriptor")
          {
            if (!this.ParseDevice(reader))
              return false;
          }
          else if (reader.Name == "ConfigurationDescriptor")
          {
            if (!this.ParseConfiguration(reader))
              return false;
          }
          else if (reader.Name == "StringDescriptor")
          {
            if (!this.ParseString(reader))
              return false;
          }
          else if (reader.Name == "GenericDescriptor")
          {
            if (!this.ParseGeneric(reader))
              return false;
          }
          else
          {
            int num = (int) MessageBox.Show("Found unknown element (" + reader.Name + ") in UsbControllerConfiguration block of XML file");
            return false;
          }
        }
        else if (reader.NodeType == XmlNodeType.EndElement)
        {
          if (!(reader.Name != "UsbControllerConfiguration"))
            return true;
          int num = (int) MessageBox.Show("Found extraneous end element: " + reader.Name + " in XML file");
          return false;
        }
      }
      int num1 = (int) MessageBox.Show("Unexpected end of file while reading UsbControllerConfiguration block of XML file");
      return false;
    }

    private bool ParseDevice(XmlTextReader reader)
    {
      bool flag1 = true;
      ulong num1 = 0;
      bool flag2 = false;
      bool flag3 = false;
      bool flag4 = false;
      bool flag5 = false;
      if (!this.countOnly)
      {
        this.data[this.dataIndex] = (byte) 1;
        this.data[this.dataIndex + 1] = (byte) 0;
        this.data[this.dataIndex + 2] = (byte) 22;
        this.data[this.dataIndex + 2 + 1] = (byte) 0;
      }
      this.dataIndex += 4;
      if (!this.countOnly)
      {
        this.data[this.dataIndex] = (byte) 18;
        this.data[this.dataIndex + 1] = (byte) 1;
        this.data[this.dataIndex + 17] = (byte) 1;
        this.data[this.dataIndex + 14] = (byte) 0;
        this.data[this.dataIndex + 15] = (byte) 0;
        this.data[this.dataIndex + 16] = (byte) 0;
        this.data[this.dataIndex + 2] = (byte) 16;
        this.data[this.dataIndex + 2 + 1] = (byte) 1;
        this.data[this.dataIndex + 4] = (byte) 0;
        this.data[this.dataIndex + 5] = (byte) 0;
        this.data[this.dataIndex + 6] = (byte) 0;
      }
      if (reader.AttributeCount > 0)
      {
        int num2 = (int) MessageBox.Show("No attributes may be specified in the DeviceDescriptor");
        return false;
      }
      while (reader.Read())
      {
        if (!flag1)
          return false;
        if (reader.NodeType == XmlNodeType.Element)
        {
          if (reader.Name == "idVendor")
          {
            flag1 = this.ParseInteger(reader, out num1, 2);
            if (flag1)
            {
              flag1 = this.SaveInteger(reader.Name, num1, this.dataIndex + 8, 2);
              flag2 = true;
              continue;
            }
            continue;
          }
          if (reader.Name == "idProduct")
          {
            flag1 = this.ParseInteger(reader, out num1, 2);
            if (flag1)
            {
              flag1 = this.SaveInteger(reader.Name, num1, this.dataIndex + 10, 2);
              flag3 = true;
              continue;
            }
            continue;
          }
          if (reader.Name == "bcdDevice")
          {
            flag1 = this.ParseBcd(reader, (byte) 12);
            flag4 = true;
            continue;
          }
          if (reader.Name == "bMaxPacketSize0")
          {
            flag1 = this.ParseInteger(reader, out num1, 1);
            if (flag1)
            {
              flag1 = this.SaveInteger(reader.Name, num1, this.dataIndex + 7, 1);
              flag5 = true;
              continue;
            }
            continue;
          }
          if (reader.Name == "iManufacturer")
          {
            flag1 = this.ParseAutoString(reader, (byte) 14);
            continue;
          }
          if (reader.Name == "iProduct")
          {
            flag1 = this.ParseAutoString(reader, (byte) 15);
            continue;
          }
          if (reader.Name == "iSerialNumber")
          {
            flag1 = this.ParseAutoString(reader, (byte) 16);
            continue;
          }
          if (reader.Name == "bcdUSB")
          {
            flag1 = this.ParseBcd(reader, (byte) 2);
            continue;
          }
          if (reader.Name == "bDeviceClass")
          {
            flag1 = this.ParseInteger(reader, out num1, 1);
            if (flag1)
            {
              flag1 = this.SaveInteger(reader.Name, num1, this.dataIndex + 4, 1);
              continue;
            }
            continue;
          }
          if (reader.Name == "bDeviceSubClass")
          {
            flag1 = this.ParseInteger(reader, out num1, 1);
            if (flag1)
            {
              flag1 = this.SaveInteger(reader.Name, num1, this.dataIndex + 5, 1);
              continue;
            }
            continue;
          }
          if (reader.Name == "bDeviceProtocol")
          {
            flag1 = this.ParseInteger(reader, out num1, 1);
            if (flag1)
            {
              flag1 = this.SaveInteger(reader.Name, num1, this.dataIndex + 6, 1);
              continue;
            }
            continue;
          }
        }
        if (reader.NodeType == XmlNodeType.EndElement)
        {
          if (reader.Name != "DeviceDescriptor")
          {
            int num3 = (int) MessageBox.Show("Found extraneous end element: " + reader.Name + " in DeviceDescriptor of XML file");
            return false;
          }
          if (!flag2)
          {
            int num4 = (int) MessageBox.Show("No idVendor element found in DeviceDescriptor");
            return false;
          }
          if (!flag3)
          {
            int num5 = (int) MessageBox.Show("No idProduct element found in DeviceDescriptor");
            return false;
          }
          if (!flag4)
          {
            int num6 = (int) MessageBox.Show("No bcdDevice element found in DeviceDescriptor");
            return false;
          }
          if (!flag5)
          {
            int num7 = (int) MessageBox.Show("No bMaxPacketSize0 element found in DeviceDescriptor");
            return false;
          }
          this.dataIndex += 18;
          return true;
        }
      }
      int num8 = (int) MessageBox.Show("Unexpected end of file while reading DeviceDescriptor block of XML file");
      return false;
    }

    private bool ParseConfiguration(XmlTextReader reader)
    {
      int dataIndex = this.dataIndex;
      this.dataIndex += 13;
      byte num1 = 128;
      byte num2 = 0;
      byte num3 = 0;
      byte num4 = 0;
      if (reader.AttributeCount > 0)
      {
        reader.MoveToFirstAttribute();
        do
        {
          if (reader.Name == "self_powered")
            num1 |= (byte) 64;
          else if (reader.Name == "remote_wakeup")
          {
            num1 |= (byte) 32;
          }
          else
          {
            int num5 = (int) MessageBox.Show("ConfigurationDescriptor element does not recognize attribute '" + reader.Name + "'.");
            return false;
          }
        }
        while (reader.MoveToNextAttribute());
        reader.MoveToElement();
      }
      while (reader.Read())
      {
        if (reader.NodeType == XmlNodeType.Element)
        {
          if (reader.Name == "SelfPowered")
            num1 |= (byte) 64;
          else if (reader.Name == "RemoteWakeup")
            num1 |= (byte) 32;
          else if (reader.Name == "InterfaceDescriptor")
          {
            if (!this.ParseInterface(reader))
              return false;
            ++num2;
          }
          else if (reader.Name == "bMaxPower_mA")
          {
            ulong num6 = 0;
            if (!this.ParseInteger(reader, out num6, 2))
              return false;
            if (num6 < 2UL || num6 > 510UL)
            {
              int num7 = (int) MessageBox.Show("Value for bMaxPower_ma element was out of range (2-510)");
            }
            num3 = (byte) (num6 / 2UL);
          }
          else if (reader.Name == "iConfiguration")
          {
            if (!this.ParseAutoString(reader, (byte) 0))
              return false;
            if (!this.countOnly)
              num4 = this.data[this.dataIndex];
          }
          else
          {
            int num8 = (int) MessageBox.Show("The element name '" + reader.Name + "' was unexpected in a ConfigurationDescriptor element.");
            return false;
          }
        }
        else if (reader.NodeType == XmlNodeType.EndElement)
        {
          if (reader.Name != "ConfigurationDescriptor")
          {
            int num9 = (int) MessageBox.Show("Found extraneous end element: " + reader.Name + " in ConfigurationDescriptor of XML file");
            return false;
          }
          if (num3 == (byte) 0)
          {
            int num10 = (int) MessageBox.Show("There was no bMaxPower_mA element in the ConfigurationDescriptor element");
            return false;
          }
          if (num2 == (byte) 0)
          {
            int num11 = (int) MessageBox.Show("No InterfaceDescriptor elements were defined for the ConfigurationDescriptor");
            return false;
          }
          if (!this.countOnly)
          {
            this.data[dataIndex] = (byte) 2;
            this.data[dataIndex + 1] = (byte) 0;
            this.data[dataIndex + 2] = (byte) (this.dataIndex - dataIndex);
            this.data[dataIndex + 2 + 1] = (byte) (this.dataIndex - dataIndex >> 8);
            int index = dataIndex + 4;
            this.data[index] = (byte) 9;
            this.data[index + 1] = (byte) 2;
            this.data[index + 2] = (byte) (this.dataIndex - index);
            this.data[index + 2 + 1] = (byte) (this.dataIndex - index >> 8);
            this.data[index + 4] = num2;
            this.data[index + 5] = (byte) 1;
            this.data[index + 6] = num4;
            this.data[index + 7] = num1;
            this.data[index + 8] = num3;
          }
          return true;
        }
      }
      int num12 = (int) MessageBox.Show("Unexpected end of file while reading ConfigurationDescriptor block of XML file");
      return false;
    }

    private bool ParseInterface(XmlTextReader reader)
    {
      int dataIndex = this.dataIndex;
      ulong number = 0;
      this.dataIndex += 9;
      byte num1 = 0;
      byte num2 = 0;
      byte num3 = byte.MaxValue;
      byte num4 = 0;
      byte num5 = 0;
      byte num6 = 0;
      bool flag1 = false;
      bool flag2 = true;
      if (reader.AttributeCount > 0)
      {
        reader.MoveToFirstAttribute();
        while (!(reader.Name != "id"))
        {
          flag1 = true;
          if (this.ConvertInteger(reader.Value, 0, out number) <= 0)
            return false;
          num1 = (byte) number;
          if (!reader.MoveToNextAttribute())
          {
            reader.MoveToElement();
            goto label_39;
          }
        }
        int num7 = (int) MessageBox.Show("Unrecognized attribute (" + reader.Name + ") found in InterfaceDescriptor");
        return false;
      }
label_39:
      while (reader.Read())
      {
        if (!flag2)
          return false;
        if (reader.NodeType == XmlNodeType.Element)
        {
          if (reader.Name == "iInterface")
          {
            if (!this.ParseAutoString(reader, (byte) 0))
              return false;
            if (!this.countOnly)
              num2 = this.data[this.dataIndex];
          }
          else if (reader.Name == "bInterfaceClass")
          {
            flag2 = this.ParseInteger(reader, out number, 1);
            num3 = (byte) number;
          }
          else if (reader.Name == "bInterfaceSubClass")
          {
            flag2 = this.ParseInteger(reader, out number, 1);
            num4 = (byte) number;
          }
          else if (reader.Name == "bInterfaceProtocol")
          {
            flag2 = this.ParseInteger(reader, out number, 1);
            num5 = (byte) number;
          }
          else if (reader.Name == "ClassDescriptor")
          {
            if (num6 != (byte) 0)
            {
              int num8 = (int) MessageBox.Show("The ClassDescriptor must appear before any EndpointDescriptor in an InterfaceDescriptor");
              return false;
            }
            flag2 = this.ParseClass(reader);
          }
          else if (reader.Name == "EndpointDescriptor")
          {
            flag2 = this.ParseEndpoint(reader);
            ++num6;
          }
          else
          {
            int num9 = (int) MessageBox.Show("Unexpected element (" + reader.Name + ") found in InterfaceDescriptor");
            return false;
          }
        }
        else if (reader.NodeType == XmlNodeType.EndElement)
        {
          if (reader.Name != "InterfaceDescriptor")
          {
            int num10 = (int) MessageBox.Show("Found extraneous end element: " + reader.Name + " in InterfaceDescriptor of XML file");
            return false;
          }
          if (num6 == (byte) 0)
          {
            int num11 = (int) MessageBox.Show("InterfaceDescriptor has no EndpointDescriptor");
            return false;
          }
          if (!flag1)
          {
            int num12 = (int) MessageBox.Show("InterfaceDescriptor has no id attribute");
            return false;
          }
          if (!this.countOnly)
          {
            this.data[dataIndex] = (byte) 9;
            this.data[dataIndex + 1] = (byte) 4;
            this.data[dataIndex + 2] = num1;
            this.data[dataIndex + 3] = (byte) 0;
            this.data[dataIndex + 4] = num6;
            this.data[dataIndex + 5] = num3;
            this.data[dataIndex + 6] = num4;
            this.data[dataIndex + 7] = num5;
            this.data[dataIndex + 8] = num2;
          }
          return true;
        }
      }
      int num13 = (int) MessageBox.Show("Unexpected end of file while reading InterfaceDescriptor block of XML file");
      return false;
    }

    private bool ParseClass(XmlTextReader reader)
    {
      int dataIndex = this.dataIndex;
      ulong num1 = 0;
      this.dataIndex += 2;
      bool flag1 = false;
      bool flag2 = true;
      while (reader.Read())
      {
        if (!flag2)
          return false;
        if (reader.NodeType == XmlNodeType.Element)
        {
          if (reader.Name == "bDescriptorType")
          {
            if (!this.ParseInteger(reader, out num1, 1))
              return false;
            if (!this.countOnly)
              this.data[dataIndex + 1] = (byte) num1;
            flag1 = true;
          }
          else if (reader.Name == "bPadding")
          {
            flag2 = this.ParseInteger(reader, out num1, 1);
            int num2 = (int) (byte) num1;
            if (!this.countOnly && flag2)
            {
              for (int index = 0; index < num2; ++index)
                this.data[this.dataIndex + index] = (byte) 0;
            }
            this.dataIndex += num2;
          }
          else if (reader.Name == "bData")
            flag2 = this.ParseIntegers(reader, 1);
          else if (reader.Name == "wData")
            flag2 = this.ParseIntegers(reader, 2);
          else if (reader.Name == "dwData")
            flag2 = this.ParseIntegers(reader, 4);
          else if (reader.Name == "iData")
          {
            flag2 = this.ParseAutoString(reader, (byte) 0);
            ++this.dataIndex;
          }
          else if (reader.Name == "sData")
          {
            string str = reader.ReadInnerXml();
            if (this.countOnly)
            {
              this.dataIndex += str.Length;
            }
            else
            {
              for (int index = 0; index < str.Length; ++index)
                this.data[this.dataIndex++] = (byte) str[index];
            }
          }
          else if (reader.Name == "wsData")
          {
            string str = reader.ReadInnerXml();
            if (this.countOnly)
            {
              this.dataIndex += 2 * str.Length;
            }
            else
            {
              for (int index = 0; index < str.Length; ++index)
              {
                this.data[this.dataIndex++] = (byte) str[index];
                this.data[this.dataIndex++] = (byte) 0;
              }
            }
          }
          else
          {
            int num3 = (int) MessageBox.Show("Unexpected element (" + reader.Name + ") found in ClassDescriptor element.");
            return false;
          }
        }
        else if (reader.NodeType == XmlNodeType.EndElement)
        {
          if (reader.Name != "ClassDescriptor")
          {
            int num4 = (int) MessageBox.Show("Found extraneous end element: " + reader.Name + " in ClassDescriptor of XML file");
            return false;
          }
          if (!flag1)
          {
            int num5 = (int) MessageBox.Show("ClassDescriptor element requires a bDescriptorType element");
            return false;
          }
          int num6 = this.dataIndex - dataIndex;
          if (num6 < 3 || num6 > (int) byte.MaxValue)
          {
            int num7 = (int) MessageBox.Show("ClassDescriptor payload length is either too long or non-existent");
            return false;
          }
          if (!this.countOnly)
            this.data[dataIndex] = (byte) num6;
          return true;
        }
      }
      int num8 = (int) MessageBox.Show("Unexpected end of file while reading ClassDescriptor block of XML file");
      return false;
    }

    private bool ParseEndpoint(XmlTextReader reader)
    {
      bool flag1 = false;
      bool flag2 = false;
      byte num1 = 0;
      byte num2 = 0;
      if (!this.countOnly)
      {
        this.data[this.dataIndex] = (byte) 7;
        this.data[this.dataIndex + 1] = (byte) 5;
        this.data[this.dataIndex + 6] = (byte) 0;
      }
      bool flag3 = true;
      if (reader.AttributeCount > 0)
      {
        reader.MoveToFirstAttribute();
        do
        {
          if (reader.Name == "id")
          {
            ulong number;
            if (this.ConvertInteger(reader.Value, 0, out number) <= 0 || number > (ulong) sbyte.MaxValue || number == 0UL)
            {
              int num3 = (int) MessageBox.Show("Bad EndpointDescriptor id attribute value (must be less than 0x7F & non-zero.");
              return false;
            }
            num1 = (byte) ((ulong) (byte) ((uint) num1 & 128U) | number);
          }
          else if (reader.Name == "direction")
          {
            if (reader.Value == "in")
            {
              num1 |= (byte) 128;
              flag1 = true;
            }
            else if (reader.Value == "out")
            {
              flag1 = true;
            }
            else
            {
              int num4 = (int) MessageBox.Show("EndpointDescriptor direction attribute has bad value (" + reader.Value + ").");
              return false;
            }
          }
          else if (reader.Name == "transfer")
          {
            if (reader.Value == "bulk")
              num2 = (byte) 2;
            else if (reader.Value == "interrupt")
              num2 = (byte) 3;
            else if (reader.Value == "isochronous")
            {
              num2 = (byte) 1;
            }
            else
            {
              int num5 = (int) MessageBox.Show("EndpointDescriptor transfer attribute has bad value (" + reader.Value + ").");
              return false;
            }
          }
          else if (reader.Name == "usage")
          {
            if (!(reader.Value == "data"))
            {
              if (reader.Value == "implicit")
                num2 |= (byte) 32;
              else if (reader.Value == "feedback")
              {
                num2 |= (byte) 16;
              }
              else
              {
                int num6 = (int) MessageBox.Show("EndpointDescriptor usage attribute has bad value (" + reader.Value + ").");
                return false;
              }
            }
          }
          else if (reader.Name == "synchronization")
          {
            if (!(reader.Value == "none"))
            {
              if (reader.Value == "asynchronous")
                num2 |= (byte) 4;
              else if (reader.Value == "adaptive")
                num2 |= (byte) 8;
              else if (reader.Value == "synchronous")
              {
                num2 |= (byte) 12;
              }
              else
              {
                int num7 = (int) MessageBox.Show("EndpointDescriptor synchronization attribute has bad value (" + reader.Value + ").");
                return false;
              }
            }
          }
          else
          {
            int num8 = (int) MessageBox.Show("Unexpected attribute (" + reader.Name + ") found in EndpointDescriptor element.");
            return false;
          }
        }
        while (reader.MoveToNextAttribute());
        reader.MoveToElement();
      }
      if (((int) num1 & (int) sbyte.MaxValue) == 0)
      {
        int num9 = (int) MessageBox.Show("EndpointDescriptor contains no id or zero id attribute");
        return false;
      }
      if (!flag1)
      {
        int num10 = (int) MessageBox.Show("EndpointDescriptor " + ((int) num1 & (int) sbyte.MaxValue).ToString() + " has no direction attribute specified");
        return false;
      }
      if (((int) num2 & 3) == 0)
      {
        int num11 = (int) MessageBox.Show("EndpointDescriptor " + ((int) num1 & (int) sbyte.MaxValue).ToString() + " has no transfer attribute specified");
        return false;
      }
      while (reader.Read())
      {
        ulong num12 = 0;
        if (!flag3)
          return false;
        if (reader.NodeType == XmlNodeType.Element)
        {
          if (reader.Name == "wMaxPacketSize")
          {
            flag3 = this.ParseInteger(reader, out num12, 2);
            if (flag3)
            {
              flag3 = this.SaveInteger(reader.Name, num12, this.dataIndex + 4, 2);
              flag2 = true;
            }
          }
          else if (reader.Name == "bInterval")
          {
            flag3 = this.ParseInteger(reader, out num12, 1);
            if (flag3)
              flag3 = this.SaveInteger(reader.Name, num12, this.dataIndex + 6, 1);
          }
          else
          {
            int num13 = (int) MessageBox.Show("Unexpected element (" + reader.Name + ") found in EndpointDescriptor element.");
            return false;
          }
        }
        else if (reader.NodeType == XmlNodeType.EndElement)
        {
          if (reader.Name != "EndpointDescriptor")
          {
            int num14 = (int) MessageBox.Show("Found extraneous end element: " + reader.Name + " in EndpointDescriptor of XML file");
            return false;
          }
          if (!flag2)
          {
            int num15 = (int) MessageBox.Show("EndpointDescriptor " + ((int) num1 & (int) sbyte.MaxValue).ToString() + " has no maximum size specified");
            return false;
          }
          if (!this.countOnly)
          {
            this.data[this.dataIndex + 2] = num1;
            this.data[this.dataIndex + 3] = num2;
          }
          this.dataIndex += 7;
          return true;
        }
      }
      int num16 = (int) MessageBox.Show("Unexpected end of file while reading EndpointDescriptor block of XML file");
      return false;
    }

    private bool ParseString(XmlTextReader reader)
    {
      if (!reader.MoveToFirstAttribute() || reader.Name != "index")
      {
        int num = (int) MessageBox.Show("All String descriptors must have an index attribute");
        return false;
      }
      ulong number;
      if (this.ConvertInteger(reader.Value, 0, out number) <= 0 || number > (ulong) byte.MaxValue || number == 0UL)
      {
        int num = (int) MessageBox.Show("String index (" + reader.Value + ") is not legal.");
        return false;
      }
      reader.MoveToElement();
      return this.AddString(reader.ReadInnerXml(), (int) number) != 0;
    }

    private bool ParseGeneric(XmlTextReader reader)
    {
      int dataIndex = this.dataIndex;
      this.dataIndex += 10;
      byte num1 = 128;
      bool flag1 = false;
      if (!reader.MoveToFirstAttribute() || reader.Name != "type")
      {
        int num2 = (int) MessageBox.Show("The GenericDescriptor element must have a 'type' attribute");
        return false;
      }
      byte num3;
      if (reader.Value == "standard")
        num3 = num1;
      else if (reader.Value == "class")
        num3 = (byte) ((uint) num1 | 32U);
      else if (reader.Value == "vendor")
      {
        num3 = (byte) ((uint) num1 | 64U);
      }
      else
      {
        int num4 = (int) MessageBox.Show("The GenericDescriptor type attribute had the value '" + reader.Value + "' rather than 'standard', 'class', or 'vendor'");
        return false;
      }
      reader.MoveToElement();
      byte num5 = 6;
      uint num6 = 0;
      uint num7 = 0;
      ulong number = 0;
      bool flag2 = true;
      while (reader.Read())
      {
        if (!flag2)
          return false;
        if (reader.NodeType == XmlNodeType.Element)
        {
          if (reader.Name == "bRecipient")
          {
            string text = reader.ReadInnerXml();
            switch (text)
            {
              case "device":
                continue;
              case "interface":
                num3 |= (byte) 1;
                continue;
              case "endpoint":
                num3 |= (byte) 2;
                continue;
              case "other":
                num3 |= (byte) 3;
                continue;
              default:
                if (this.ConvertInteger(text, 0, out number) <= 0 || number > 31UL)
                {
                  int num8 = (int) MessageBox.Show("bRecipient value has bad characters or is too high (must be less than 0x20)");
                  return false;
                }
                num3 |= (byte) number;
                continue;
            }
          }
          else if (reader.Name == "wValue")
          {
            flag2 = this.ParseInteger(reader, out number, 2);
            num6 = (uint) number;
            flag1 = true;
          }
          else if (reader.Name == "bRequest")
          {
            flag2 = this.ParseInteger(reader, out number, 1);
            num5 = (byte) number;
          }
          else if (reader.Name == "wIndex")
          {
            flag2 = this.ParseInteger(reader, out number, 2);
            num7 = (uint) number;
          }
          else if (reader.Name == "bPadding")
          {
            flag2 = this.ParseInteger(reader, out number, 1);
            int num9 = (int) number;
            if (flag2 && !this.countOnly)
            {
              for (int index = 0; index < num9; ++index)
                this.data[this.dataIndex + index] = (byte) 0;
            }
            this.dataIndex += num9;
          }
          else if (reader.Name == "bData")
            flag2 = this.ParseIntegers(reader, 1);
          else if (reader.Name == "wData")
            flag2 = this.ParseIntegers(reader, 2);
          else if (reader.Name == "dwData")
            flag2 = this.ParseIntegers(reader, 4);
          else if (reader.Name == "iData")
          {
            flag2 = this.ParseAutoString(reader, (byte) 0);
            ++this.dataIndex;
          }
          else if (reader.Name == "sData")
          {
            string str = reader.ReadInnerXml();
            if (this.countOnly)
            {
              this.dataIndex += str.Length;
            }
            else
            {
              for (int index = 0; index < str.Length; ++index)
                this.data[this.dataIndex++] = (byte) str[index];
            }
          }
          else if (reader.Name == "wsData")
          {
            string str = reader.ReadInnerXml();
            if (this.countOnly)
            {
              this.dataIndex += 2 * str.Length;
            }
            else
            {
              for (int index = 0; index < str.Length; ++index)
              {
                this.data[this.dataIndex++] = (byte) str[index];
                this.data[this.dataIndex++] = (byte) 0;
              }
            }
          }
          else
          {
            int num10 = (int) MessageBox.Show("Element type '" + reader.Name + "' is not part of a GenericDescriptor element.");
            return false;
          }
        }
        else if (reader.NodeType == XmlNodeType.EndElement)
        {
          if (reader.Name != "GenericDescriptor")
          {
            int num11 = (int) MessageBox.Show("Found extraneous end element: " + reader.Name + " in GenericDescriptor of XML file");
            return false;
          }
          if (!flag1)
          {
            int num12 = (int) MessageBox.Show("GenericDescriptor element requires a wValue element");
            return false;
          }
          if (!this.countOnly)
          {
            this.data[dataIndex] = byte.MaxValue;
            this.data[dataIndex + 1] = (byte) 0;
            this.data[dataIndex + 2] = (byte) (this.dataIndex - dataIndex);
            this.data[dataIndex + 2 + 1] = (byte) (this.dataIndex - dataIndex >> 8);
            int index = dataIndex + 4;
            this.data[index] = num3;
            this.data[index + 1] = num5;
            this.data[index + 2] = (byte) num6;
            this.data[index + 2 + 1] = (byte) (num6 >> 8);
            this.data[index + 4] = (byte) num7;
            this.data[index + 4 + 1] = (byte) (num7 >> 8);
          }
          return true;
        }
      }
      int num13 = (int) MessageBox.Show("Unexpected end of file while reading GenericDescriptor block of XML file");
      return false;
    }

    private int ConvertInteger(string text, int offset, out ulong number)
    {
      int num1 = 0;
      bool flag = false;
      ulong num2 = 0;
      int num3 = 0;
      while (offset + num1 < text.Length)
      {
        char ch = text[offset + num1++];
        if (ch == ' ' || ch == '\t' || ch == '\r' || ch == '\n')
        {
          if (num3 != 0)
            break;
        }
        else if (ch == '#' && !flag && offset + num1 < text.Length - 1 && (text[offset + num1] == 'x' || text[offset + num1] == 'X'))
        {
          ++num1;
          flag = true;
        }
        else if (ch >= '0' && ch <= '9')
        {
          num2 = !flag ? num2 * 10UL + (ulong) ((int) ch - 48) : (num2 << 4) + (ulong) ((int) ch - 48);
          ++num3;
        }
        else if (flag && ch >= 'a' && ch <= 'f')
        {
          num2 = (num2 << 4) + (ulong) ((int) ch + 10 - 97);
          ++num3;
        }
        else if (flag && ch >= 'A' && ch <= 'F')
        {
          num2 = (num2 << 4) + (ulong) ((int) ch + 10 - 65);
          ++num3;
        }
        else
        {
          number = 0UL;
          return 1 - num1;
        }
      }
      if (num3 == 0)
      {
        number = 0UL;
        return 0;
      }
      number = num2;
      return num1;
    }

    private bool ParseInteger(XmlTextReader reader, out ulong value, int nBytes) => this.ParseInteger(reader.Name, reader.ReadInnerXml(), out value, nBytes);

    private bool ParseInteger(string name, string text, out ulong value, int nBytes)
    {
      ulong number;
      int offset = this.ConvertInteger(text, 0, out number);
      if (offset < 0)
      {
        int num = (int) MessageBox.Show("Illegal character (" + (object) text[-offset] + ") found in " + name + " element.");
        value = 0UL;
        return false;
      }
      if (offset == 0)
      {
        int num = (int) MessageBox.Show("No value was found in " + name + " element.");
        value = 0UL;
        return false;
      }
      value = number;
      if (this.ConvertInteger(text, offset, out number) != 0)
      {
        int num = (int) MessageBox.Show("Garbage discovered past end of number in " + name + " element.");
        return false;
      }
      if ((nBytes != 1 || value <= (ulong) byte.MaxValue) && (nBytes != 2 || value <= (ulong) ushort.MaxValue) && (nBytes != 4 || value <= (ulong) uint.MaxValue))
        return true;
      int num1 = (int) MessageBox.Show("Number value was too large in " + name + " element.");
      return false;
    }

    private bool ParseIntegers(XmlTextReader reader, int nBytes)
    {
      string name = reader.Name;
      string text = reader.ReadInnerXml();
      ulong number = 0;
      int offset = 0;
      int num1 = 0;
      int num2;
      do
      {
        num2 = this.ConvertInteger(text, offset, out number);
        if (num2 > 0)
        {
          if (!this.SaveInteger(name, number, this.dataIndex, nBytes))
            return false;
          ++num1;
          offset += num2;
          this.dataIndex += nBytes;
        }
      }
      while (num2 > 0);
      if (num2 == 0 && num1 == 0)
      {
        int num3 = (int) MessageBox.Show("No values were found in " + name + " element.");
        return false;
      }
      if (num2 >= 0)
        return true;
      int num4 = (int) MessageBox.Show("Illegal character (" + (object) text[offset - num2] + ") found in " + name + " element.");
      return false;
    }

    private bool ParseBcd(XmlTextReader reader, byte offset)
    {
      string name = reader.Name;
      string str = reader.ReadInnerXml();
      int num1 = 0;
      int num2 = 0;
      int num3 = 0;
      ulong num4 = 0;
      bool flag = false;
      while (num1 < str.Length)
      {
        char ch = str[num1++];
        if (ch == ' ' || ch == '\t' || ch == '\r' || ch == '\n')
        {
          if (num3 != 0 || flag)
          {
            if (num2 == 0)
            {
              if (flag)
                num4 <<= 4 * (2 - num3);
              else
                num4 <<= 8;
              if (!this.SaveInteger(name, num4, this.dataIndex + (int) offset, 2))
                return false;
              flag = false;
              num3 = 0;
              ++num2;
            }
            else
              break;
          }
        }
        else if (ch == '.' && !flag)
        {
          flag = true;
          num3 = 0;
        }
        else if (ch >= '0' && ch <= '9')
        {
          if (num3 > 1)
          {
            int num5 = (int) MessageBox.Show("Too many digits for BCD value in " + name + " element.");
            return false;
          }
          num4 = (num4 << 4) + (ulong) ((int) ch - 48);
          ++num3;
        }
        else
        {
          int num6 = (int) MessageBox.Show("Non-numeric character (" + (object) ch + ") found in " + name + " BCD element.");
          return false;
        }
      }
      if (num3 != 0 || flag)
      {
        if (num2 != 0)
        {
          int num7 = (int) MessageBox.Show("More than one numeric value found in " + name + " element");
          return false;
        }
        ulong num8 = !flag ? num4 << 8 : num4 << 4 * (2 - num3);
        if (!this.SaveInteger(name, num8, this.dataIndex + (int) offset, 2))
          return false;
        ++num2;
      }
      if (num2 != 0)
        return true;
      int num9 = (int) MessageBox.Show("Empty " + name + " element. Expected a BCD value.");
      return false;
    }

    private bool SaveInteger(string name, ulong value, int index, int nBytes)
    {
      switch (nBytes)
      {
        case 1:
          if (value <= (ulong) byte.MaxValue)
          {
            if (!this.countOnly)
              this.data[index] = (byte) value;
            return true;
          }
          break;
        case 2:
          if (value <= (ulong) ushort.MaxValue)
          {
            if (!this.countOnly)
            {
              this.data[index] = (byte) value;
              this.data[index + 1] = (byte) (value >> 8);
            }
            return true;
          }
          break;
        case 4:
          if (value <= (ulong) uint.MaxValue)
          {
            if (!this.countOnly)
            {
              this.data[index] = (byte) value;
              this.data[index + 1] = (byte) (value >> 8);
              this.data[index + 2] = (byte) (value >> 16);
              this.data[index + 3] = (byte) (value >> 24);
            }
            return true;
          }
          break;
        default:
          int num1 = (int) MessageBox.Show("Programming error: nBytes has value of " + nBytes.ToString() + " in " + name + " element.");
          return false;
      }
      int num2 = (int) MessageBox.Show("Integer value too large in " + name + " element.");
      return false;
    }

    private bool ParseAutoString(XmlTextReader reader, byte offset)
    {
      string name = reader.Name;
      int num = this.AddString(reader.ReadInnerXml(), 0);
      if (num == 0)
        return false;
      if (!this.countOnly)
        this.data[this.dataIndex + (int) offset] = (byte) num;
      return true;
    }

    private int AddString(string text, int index)
    {
      if (index == 0)
      {
        index = this.autoIndex;
        ++this.autoIndex;
        if (this.autoIndex == 4 || this.autoIndex == 5)
          this.autoIndex = 6;
      }
      if (index < 1 || index > (int) byte.MaxValue)
      {
        int num = (int) MessageBox.Show("Specified string descriptor index (" + index.ToString() + ") is too large.");
        return 0;
      }
      if (text.Length >= 126)
      {
        int num = (int) MessageBox.Show("Specified string (" + text + ") is too long.  It must have fewer than 126 characters.");
        return 0;
      }
      this.stringList.Add((object) new UsbXMLConfigSet.StringDescriptor(index, text));
      return index;
    }

    private void DumpStrings()
    {
      for (int index1 = 0; index1 < this.stringList.Count; ++index1)
      {
        UsbXMLConfigSet.StringDescriptor stringDescriptor = (UsbXMLConfigSet.StringDescriptor) this.stringList[index1];
        if (!this.countOnly)
        {
          int num = stringDescriptor.Text.Length * 2 + 2 + 4;
          this.data[this.dataIndex] = (byte) 3;
          this.data[this.dataIndex + 1] = (byte) stringDescriptor.Index;
          this.data[this.dataIndex + 2] = (byte) num;
          this.data[this.dataIndex + 2 + 1] = (byte) (num >> 8);
        }
        this.dataIndex += 4;
        if (!this.countOnly)
        {
          this.data[this.dataIndex] = (byte) (stringDescriptor.Text.Length * 2 + 2);
          this.data[this.dataIndex + 1] = (byte) 3;
        }
        this.dataIndex += 2;
        for (int index2 = 0; index2 < stringDescriptor.Text.Length; ++index2)
        {
          if (!this.countOnly)
          {
            this.data[this.dataIndex] = (byte) stringDescriptor.Text[index2];
            this.data[this.dataIndex + 1] = (byte) 0;
          }
          this.dataIndex += 2;
        }
      }
    }

    private class StringDescriptor
    {
      public int Index;
      public string Text;

      public StringDescriptor(int index, string text)
      {
        this.Index = index;
        this.Text = text;
      }
    }
  }
}
