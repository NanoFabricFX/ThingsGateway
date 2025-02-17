﻿#region copyright

//------------------------------------------------------------------------------
//  此代码版权声明为全文件覆盖，如有原作者特别声明，会在下方手动补充
//  此代码版权（除特别声明外的代码）归作者本人Diego所有
//  源代码使用协议遵循本仓库的开源协议及附加协议
//  Gitee源代码仓库：https://gitee.com/diego2098/ThingsGateway
//  Github源代码仓库：https://github.com/kimdiego2098/ThingsGateway
//  使用文档：https://diego2098.gitee.io/thingsgateway-docs/
//  QQ群：605534569
//------------------------------------------------------------------------------

#endregion

using ThingsGateway.Foundation.Extension.Generic;

namespace ThingsGateway.Foundation.Modbus;

internal class ModbusHelper
{
    /// <summary>
    /// 添加Crc16
    /// </summary>
    internal static byte[] AddCrc(byte[] command)
    {
        return CRC16Utils.CRC16(command);
    }

    private static ThingsGatewayBitConverter ThingsGatewayBitConverter = new(EndianType.Big);

    internal static void ModbusServerAnalysisAddressValue(IModbusServerMessage request, byte[] response, OperResult<byte[], FilterResult> result, int offset)
    {
        //解析01 03 00 00 00 0A
        var station = ThingsGatewayBitConverter.ToByte(response, 0 + offset);
        var function = ThingsGatewayBitConverter.ToByte(response, 1 + offset);
        int addressStart = ThingsGatewayBitConverter.ToInt16(response, 2 + offset);
        if (addressStart == -1)
        {
            addressStart = 65535;
        }
        if (function > 4)
        {
            if (function > 6)
            {
                request.ModbusAddress = new ModbusAddress()
                {
                    Station = station,
                    Address = addressStart.ToString(),
                    WriteFunction = function,
                    ReadFunction = (byte)(function == 16 ? 3 : function == 15 ? 1 : 3),
                };
                request.Length = ThingsGatewayBitConverter.ToByte(response, 5 + offset);
                request.Content = result.Content.RemoveBegin(7);
            }
            else
            {
                request.ModbusAddress = new ModbusAddress()
                {
                    Station = station,
                    Address = addressStart.ToString(),
                    WriteFunction = function,
                    ReadFunction = (byte)(function == 6 ? 3 : function == 5 ? 1 : 3),
                };
                request.Length = 1;
                request.Content = result.Content.RemoveBegin(4);
            }
        }
        else
        {
            request.ModbusAddress = new ModbusAddress()
            {
                Station = station,
                Address = addressStart.ToString(),
                ReadFunction = function,
            };
            request.Length = ThingsGatewayBitConverter.ToByte(response, 5 + offset);
        }
    }

    /// <summary>
    /// 获取modbus写入数据区内容
    /// </summary>
    /// <param name="response">返回数据</param>
    /// <returns></returns>
    internal static OperResult<byte[], FilterResult> GetModbusWriteData(byte[] response)
    {
        try
        {
            var func = response[1];
            if (func == 1 || func == 2 || func == 3 || func == 4 || func == 5 || func == 6)
            {
                if (response.Length == 6)
                    return new() { Content = response, Content2 = FilterResult.Success };
            }
            else if (func == 15 || func == 16)
            {
                var length = response[6];
                if (response.Length == 7 + length)
                {
                    return new() { Content = response, Content2 = FilterResult.Success };
                }
                if (response.Length > 7 + length)
                {
                    return new(string.Format(FoundationConst.DataLengthError, response.Length)) { Content2 = FilterResult.Success };
                }
                else
                {
                    return new(string.Format(FoundationConst.DataLengthError, response.Length)) { Content2 = FilterResult.Cache };
                }
            }

            return new(string.Format(FoundationConst.DataLengthError, response.Length)) { Content2 = FilterResult.Success };
        }
        catch (Exception ex)
        {
            return new(ex) { Content2 = FilterResult.Success };
        }
    }

    /// <summary>
    /// 添加ModbusTcp报文头
    /// </summary>
    internal static byte[] AddModbusTcpHead(byte[] modbus, ushort id)
    {
        byte[] tcp = new byte[modbus.Length + 6];
        tcp[0] = BitConverter.GetBytes(id)[1];
        tcp[1] = BitConverter.GetBytes(id)[0];
        tcp[4] = BitConverter.GetBytes(modbus.Length)[1];
        tcp[5] = BitConverter.GetBytes(modbus.Length)[0];
        modbus.CopyTo(tcp, 6);
        return tcp;
    }

    /// <summary>
    /// modbus地址格式说明
    /// </summary>
    /// <returns></returns>
    internal static string GetAddressDescription()
    {
        return ModbusConst.AddressDes;
    }

    /// <summary>
    /// 通过错误码来获取到对应的文本消息
    /// </summary>
    internal static string GetDescriptionByErrorCode(byte code)
    {
        return code switch
        {
            1 => ModbusConst.ModbusError1,
            2 => ModbusConst.ModbusError2,
            3 => ModbusConst.ModbusError3,
            4 => ModbusConst.ModbusError4,
            5 => ModbusConst.ModbusError5,
            6 => ModbusConst.ModbusError6,
            8 => ModbusConst.ModbusError8,
            10 => ModbusConst.ModbusError10,
            11 => ModbusConst.ModbusError11,
            _ => FoundationConst.UnknowError,
        };
    }

    /// <summary>
    /// 获取modbus数据区内容，返回数据需去除Crc和报文头，例如：01 03 02 00 01，发送数据需报文头
    /// </summary>
    /// <param name="send">发送数据</param>
    /// <param name="response">返回数据</param>
    /// <returns></returns>
    internal static OperResult<byte[], FilterResult> GetModbusData(byte[] send, byte[] response)
    {
        try
        {
            if (response.Length < 3)
                return new OperResult<byte[], FilterResult>(string.Format(FoundationConst.LengthShortError, response.ToHexString())) { Content2 = FilterResult.Cache };

            if (response[1] >= 0x80)//错误码
                return new OperResult<byte[], FilterResult>(GetDescriptionByErrorCode(response[2])) { Content2 = FilterResult.Success };
            if (response[1] <= 0x05)
            {
                if ((response.Length < response[2] + 3))
                    return new OperResult<byte[], FilterResult>(string.Format(FoundationConst.LengthShortError, response.ToHexString())) { Content2 = FilterResult.Cache };
            }
            else
            {
                if ((response.Length < 6))
                    return new OperResult<byte[], FilterResult>(string.Format(FoundationConst.LengthShortError, response.ToHexString())) { Content2 = FilterResult.Cache };
            }

            if (send == null)
            {
                var result = new OperResult<byte[], FilterResult>() { Content = response.RemoveBegin(3), Content2 = FilterResult.Success };
                result.ErrorMessage = FoundationConst.NotActiveQueryError;
                return result;
            }
            if (send.Length == 0)
            {
                return new() { Content = response.RemoveBegin(3), Content2 = FilterResult.Success };
            }
            if (send[0] != response[0])
                return new OperResult<byte[], FilterResult>(string.Format(ModbusConst.StationNotSame, send[0], response[0])) { Content2 = FilterResult.Success };
            if (send[1] != response[1])
                return new OperResult<byte[], FilterResult>(string.Format(ModbusConst.FunctionNotSame, send[1], response[1])) { Content2 = FilterResult.Success };
            return new() { Content = response.RemoveBegin(3), Content2 = FilterResult.Success };
        }
        catch (Exception ex)
        {
            return new OperResult<byte[], FilterResult>(ex) { Content2 = FilterResult.Success };
        }
    }

    /// <summary>
    /// 去除Crc，返回modbus数据区
    /// </summary>
    /// <param name="send"></param>
    /// <param name="response"></param>
    /// <param name="crcCheck"></param>
    /// <returns></returns>
    internal static OperResult<byte[], FilterResult> GetModbusRtuData(byte[] send, byte[] response, bool crcCheck = true)
    {
        if (response.Length < 3)
            return new OperResult<byte[], FilterResult>(string.Format(FoundationConst.LengthShortError, response.ToHexString())) { Content2 = FilterResult.Cache };

        if (response[1] >= 0x80)//错误码
            return new OperResult<byte[], FilterResult>(GetDescriptionByErrorCode(response[2])) { Content2 = FilterResult.Success };
        if (response[1] <= 0x04)
        {
            if ((response.Length < response[2] + 5))
                return new OperResult<byte[], FilterResult>(string.Format(FoundationConst.LengthShortError, response.ToHexString())) { Content2 = FilterResult.Cache };
        }
        else
        {
            if ((response.Length < 8))
                return new OperResult<byte[], FilterResult>(string.Format(FoundationConst.LengthShortError, response.ToHexString())) { Content2 = FilterResult.Cache };
        }

        var data = response.SelectMiddle(0, response[1] <= 0x04 ? response[2] != 0 ? response[2] + 5 : 8 : 8);
        if (crcCheck && !CRC16Utils.CheckCRC16(data))
            return new OperResult<byte[], FilterResult>("Crc校验失败" + DataTransUtil.ByteToHexString(data, ' ')) { Content2 = FilterResult.Success };
        return GetModbusData(send, data.RemoveLast(2));
    }

    /// <summary>
    /// 获取读取报文
    /// </summary>
    internal static byte[] GetReadModbusCommand(string address, ushort length, byte station)
    {
        var mAddress = ModbusAddressHelper.ParseFrom(address, station);
        return GetReadModbusCommand(mAddress, length);
    }

    /// <summary>
    /// 获取写入字报文，根据地址识别功能码
    /// </summary>
    internal static byte[] GetWriteModbusCommand(string address, byte[] value, byte station)
    {
        var mAddress = ModbusAddressHelper.ParseFrom(address, station);
        value = value.ArrayExpandToLengthEven();
        //功能码或实际长度
        if (value.Length > 2 || mAddress.WriteFunction == 16)
            return GetWriteModbusCommand(mAddress, value);
        else
            return GetWriteOneModbusCommand(mAddress, value);
    }

    #region 报文构建

    /// <summary>
    /// 获取读取报文
    /// </summary>
    internal static byte[] GetReadModbusCommand(ModbusAddress mAddress, ushort length)
    {
        byte[] array = new byte[6]
        {
        (byte) mAddress.Station,
        (byte) mAddress.ReadFunction,
        BitConverter.GetBytes(mAddress.AddressStart)[1],
        BitConverter.GetBytes(mAddress.AddressStart)[0],
        BitConverter.GetBytes(length)[1],
        BitConverter.GetBytes(length)[0]
        };
        return array;
    }

    /// <summary>
    /// 获取05写入布尔量报文
    /// </summary>
    internal static byte[] GetWriteBoolModbusCommand(ModbusAddress mAddress, bool value)
    {
        byte[] array = new byte[6]
        {
    (byte) mAddress.Station,
    (byte)5,
    BitConverter.GetBytes(mAddress.AddressStart)[1],
    BitConverter.GetBytes(mAddress.AddressStart)[0],
     0,
     0
        };
        if (value)
        {
            array[4] = 0xFF;
            array[5] = 0;
        }
        else
        {
            array[4] = 0;
            array[5] = 0;
        }
        return array;
    }

    /// <summary>
    /// 获取15写入布尔量报文
    /// </summary>
    internal static byte[] GetWriteBoolModbusCommand(ModbusAddress mAddress, bool[] values, ushort length)
    {
        byte[] numArray1 = values.BoolArrayToByte();
        byte[] numArray2 = new byte[7 + numArray1.Length];
        numArray2[0] = (byte)mAddress.Station;
        numArray2[1] = (byte)15;
        numArray2[2] = BitConverter.GetBytes(mAddress.AddressStart)[1];
        numArray2[3] = BitConverter.GetBytes(mAddress.AddressStart)[0];
        numArray2[4] = (byte)(length / 256);
        numArray2[5] = (byte)(length % 256);
        numArray2[6] = (byte)numArray1.Length;
        numArray1.CopyTo(numArray2, 7);
        return numArray2;
    }

    /// <summary>
    /// 获取16写入字报文
    /// </summary>
    internal static byte[] GetWriteModbusCommand(ModbusAddress mAddress, byte[] values)
    {
        byte[] numArray = new byte[7 + values.Length];
        numArray[0] = (byte)mAddress.Station;
        numArray[1] = (byte)16;
        numArray[2] = BitConverter.GetBytes(mAddress.AddressStart)[1];
        numArray[3] = BitConverter.GetBytes(mAddress.AddressStart)[0];
        numArray[4] = (byte)(values.Length / 2 / 256);
        numArray[5] = (byte)(values.Length / 2 % 256);
        numArray[6] = (byte)values.Length;
        values.CopyTo(numArray, 7);
        return numArray;
    }

    /// <summary>
    /// 获取6写入字报文
    /// </summary>
    internal static byte[] GetWriteOneModbusCommand(ModbusAddress mAddress, byte[] values)
    {
        byte[] numArray = new byte[4 + values.Length];
        numArray[0] = (byte)mAddress.Station;
        numArray[1] = (byte)6;
        numArray[2] = BitConverter.GetBytes(mAddress.AddressStart)[1];
        numArray[3] = BitConverter.GetBytes(mAddress.AddressStart)[0];
        values.CopyTo(numArray, 4);
        return numArray;
    }

    #endregion 报文构建
}