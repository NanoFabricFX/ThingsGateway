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

using ThingsGateway.Gateway.Application;

using TouchSocket.Sockets;

namespace ThingsGateway.Plugin.Siemens;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class SiemensS7Master : CollectBase
{
    private readonly SiemensS7MasterProperty _driverPropertys = new();

    private ThingsGateway.Foundation.SiemensS7.SiemensS7Master _plc;

    /// <inheritdoc/>
    public override Type DriverDebugUIType => typeof(ThingsGateway.Demo.SiemensS7Master);

    /// <inheritdoc/>
    public override CollectPropertyBase DriverPropertys => _driverPropertys;

    public override Type DriverUIType => null;

    /// <inheritdoc/>
    protected override IProtocol Protocol => _plc;

    /// <inheritdoc/>
    public override void Init(IChannel? channel = null)
    {
        ArgumentNullException.ThrowIfNull(channel, "通道配置不能为null");
        //载入配置
        _plc = new(channel)
        {
            DataFormat = _driverPropertys.DataFormat,
            SendDelayTime = _driverPropertys.SendDelayTime,
            CacheTimeout = _driverPropertys.CacheTimeout,
            ConnectTimeout = _driverPropertys.ConnectTimeout,
            SiemensType = _driverPropertys.SiemensType,
            Timeout = _driverPropertys.Timeout,
            LocalTSAP = _driverPropertys.LocalTSAP,
            Rack = _driverPropertys.Rack,
            Slot = _driverPropertys.Slot,
        };
        base.Init(channel);
    }

    protected override bool IsSingleThread
    {
        get
        {
            return true;
        }
    }

    /// <inheritdoc/>
    protected override List<VariableSourceRead> ProtectedLoadSourceRead(List<VariableRunTime> deviceVariables)
    {
        try { _plc.Channel.Connect(_driverPropertys.ConnectTimeout, CancellationToken.None); } catch { }
        try
        {
            return _plc.LoadSourceRead<VariableSourceRead>(deviceVariables, 0, CurrentDevice.IntervalTime);
        }
        finally { _plc.Channel.Close(); }
    }

    /// <summary>
    /// <see cref="SiemensS7PLC.ReadDateAsync(string,CancellationToken)"/>
    /// </summary>
    /// <returns></returns>
    [DynamicMethod("ReadWriteDateAsync", "读写日期格式")]
    public async Task<OperResult<System.DateTime>> ReadWriteDateAsync(string address, System.DateTime? value = null, CancellationToken cancellationToken = default)
    {
        if (value == null)
            return await _plc?.ReadDateAsync(address, cancellationToken);
        else
            return new(await _plc?.WriteDateAsync(address, value.Value, cancellationToken));
    }

    /// <summary>
    /// <see cref="SiemensS7PLC.ReadDateTimeAsync(string,CancellationToken)"/>
    /// </summary>
    /// <param name="address"></param>
    /// <param name="value"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [DynamicMethod("ReadWriteDateTimeAsync", "读写日期时间格式")]
    public async Task<OperResult<System.DateTime>> ReadWriteDateTimeAsync(string address, System.DateTime? value = null, CancellationToken cancellationToken = default)
    {
        if (value == null)
            return await _plc?.ReadDateTimeAsync(address, cancellationToken);
        else
            return new(await _plc?.WriteDateTimeAsync(address, value.Value, cancellationToken));
    }
}