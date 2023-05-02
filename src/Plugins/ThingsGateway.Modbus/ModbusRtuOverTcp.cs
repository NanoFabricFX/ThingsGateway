﻿using Microsoft.Extensions.DependencyInjection;

using ThingsGateway.Foundation;
using ThingsGateway.Web.Foundation;

namespace ThingsGateway.Modbus;

public class ModbusRtuOverTcp : CollectBase, IDisposable
{

    private ThingsGateway.Foundation.Adapter.Modbus.ModbusRtuOverTcp _plc;
    private ModbusRtuOverTcpProperty driverPropertys = new ModbusRtuOverTcpProperty();

    public ModbusRtuOverTcp(IServiceScopeFactory scopeFactory) : base(scopeFactory)
    {
    }
    public override CollectDriverPropertyBase DriverPropertys => driverPropertys;
    public override IThingsGatewayBitConverter ThingsGatewayBitConverter { get => _plc?.ThingsGatewayBitConverter; }

    public override void AfterStop()
    {
        _plc?.Disconnect();
    }

    public override async Task BeforStartAsync()
    {
        await _plc?.ConnectAsync();
    }

    public override void Dispose()
    {
        _plc?.Disconnect();
    }

    public override void InitDataAdapter()
    {
        _plc.SetDataAdapter();
    }

    public override OperResult IsConnected()
    {
        return _plc?.TGTcpClient?.CanSend == true ? OperResult.CreateSuccessResult() : new OperResult("失败");
    }
    public override bool IsSupportAddressRequest()
    {
        return true;
    }

    public override OperResult<List<DeviceVariableSourceRead>> LoadSourceRead(List<CollectVariableRunTime> deviceVariables)
    {
        return deviceVariables.LoadSourceRead(_logger, ThingsGatewayBitConverter, driverPropertys.MaxPack);
    }

    public override async Task<OperResult> WriteValueAsync(CollectVariableRunTime deviceVariable, string value)
    {
        return await _plc.WriteAsync(deviceVariable.DataType, deviceVariable.VariableAddress, value);
    }

    protected override void Init(CollectDeviceRunTime device, object client = null)
    {
        if (client == null)
        {
            TouchSocketConfig.SetRemoteIPHost(new IPHost($"{driverPropertys.IP}:{driverPropertys.Port}"))
                .SetBufferLength(1024);
            client = TouchSocketConfig.Container.Resolve<TGTcpClient>();
            ((TGTcpClient)client).Setup(TouchSocketConfig);
        }
        //载入配置
        _plc = new((TGTcpClient)client);
        _plc.Crc16CheckEnable = driverPropertys.Crc16CheckEnable;
        _plc.DataFormat = driverPropertys.DataFormat;
        _plc.ConnectTimeOut = driverPropertys.ConnectTimeOut;
        _plc.Station = driverPropertys.Station;
        _plc.TimeOut = driverPropertys.TimeOut;
    }
    protected override Task<OperResult<byte[]>> ReadAsync(string address, int length, CancellationToken cancellationToken)
    {
        return _plc.ReadAsync(address, length);
    }

}

public class ModbusRtuOverTcpProperty : CollectDriverPropertyBase
{
    [DeviceProperty("IP", "")]
    public override string IP { get; set; } = "127.0.0.1";

    [DeviceProperty("端口", "")]
    public override int Port { get; set; } = 502;

    [DeviceProperty("默认解析顺序", "")]
    public DataFormat DataFormat { get; set; }
    [DeviceProperty("默认站号", "")]
    public byte Station { get; set; } = 1;
    [DeviceProperty("连接超时时间", "")]
    public ushort ConnectTimeOut { get; set; } = 3000;
    [DeviceProperty("最大打包长度", "")]
    public ushort MaxPack { get; set; } = 100;
    [DeviceProperty("CRC检测", "")]
    public bool Crc16CheckEnable { get; set; } = true;
    [DeviceProperty("读写超时时间", "")]
    public ushort TimeOut { get; set; } = 3000;

    [DeviceProperty("共享链路", "")]
    public override bool IsShareChannel { get; set; } = false;
    public override ShareChannelEnum ShareChannel => ShareChannelEnum.TcpClient;
}
