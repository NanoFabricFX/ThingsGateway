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

using Newtonsoft.Json.Linq;

using Opc.Ua;

using ThingsGateway.Foundation.Extension.Generic;
using ThingsGateway.Foundation.OpcUa;
using ThingsGateway.Gateway.Application;

using TouchSocket.Core;

namespace ThingsGateway.Plugin.OpcUa;

/// <summary>
/// <inheritdoc/>
/// </summary>
public class OpcUaMaster : CollectBase
{
    private readonly OpcUaMasterProperty _driverPropertys = new();

    private ThingsGateway.Foundation.OpcUa.OpcUaMaster _plc;

    private CancellationToken _token;

    private volatile bool success = true;

    /// <inheritdoc/>
    public override Type DriverDebugUIType => typeof(ThingsGateway.Demo.OpcUaMaster);

    /// <inheritdoc/>
    public override CollectPropertyBase DriverPropertys => _driverPropertys;

    public override Type DriverUIType => null;

    /// <inheritdoc/>
    protected override IProtocol Protocol => null;

    public override string ToString()
    {
        return $"{_driverPropertys.OpcUrl}";
    }

    protected override bool IsSingleThread
    {
        get
        {
            return true;
        }
    }

    /// <inheritdoc/>
    public override void Init(IChannel? channel = null)
    {
        //载入配置
        OpcUaConfig config = new()
        {
            OpcUrl = _driverPropertys.OpcUrl,
            UpdateRate = _driverPropertys.UpdateRate,
            DeadBand = _driverPropertys.DeadBand,
            GroupSize = _driverPropertys.GroupSize,
            KeepAliveInterval = _driverPropertys.KeepAliveInterval,
            IsUseSecurity = _driverPropertys.IsUseSecurity,
            ActiveSubscribe = _driverPropertys.ActiveSubscribe,
            UserName = _driverPropertys.UserName,
            Password = _driverPropertys.Password,
            CheckDomain = _driverPropertys.CheckDomain,
            LoadType = _driverPropertys.LoadType,
        };
        if (_plc == null)
        {
            _plc = new();
            _plc.OpcStatusChange += _plc_OpcStatusChange;
            _plc.DataChangedHandler += DataChangedHandler;
        }
        _plc.OpcUaConfig = config;

        base.Init(channel);
    }

    /// <inheritdoc/>
    public override bool IsConnected() => _plc?.Connected == true;

    protected override async Task ProtectedBeforStartAsync(CancellationToken cancellationToken)
    {
        _token = cancellationToken;
        await _plc.ConnectAsync(cancellationToken);
        await base.ProtectedBeforStartAsync(cancellationToken);
    }

    protected override Task ProtectedAfterStopAsync()
    {
        _plc?.Disconnect();
        return base.ProtectedAfterStopAsync();
    }

    protected override string GetAddressDescription()
    {
        return _plc?.GetAddressDescription();
    }

    /// <inheritdoc/>
    protected override List<VariableSourceRead> ProtectedLoadSourceRead(List<VariableRunTime> deviceVariables)
    {
        if (deviceVariables.Count > 0)
        {
            var dataLists = deviceVariables.ChunkBetter(_driverPropertys.GroupSize);
            _plc.Variables = new();
            _plc.Variables.AddRange(dataLists.Select(a => a.Where(a => !a.RegisterAddress.IsNullOrEmpty()).Select(a => a.RegisterAddress!).ToList()).ToList());
            var dataResult = new List<VariableSourceRead>();
            foreach (var variable in dataLists)
            {
                var sourVars = new VariableSourceRead()
                {
                    TimeTick = new(_driverPropertys.UpdateRate),
                    RegisterAddress = "",
                };
                foreach (var item in variable)
                {
                    sourVars.AddVariable(item);
                }
                dataResult.Add(sourVars);
            }

            return dataResult;
        }
        else
        {
            return new();
        }
    }

    /// <inheritdoc/>
    protected override async Task<OperResult<byte[]>> ReadSourceAsync(VariableSourceRead deviceVariableSourceRead, CancellationToken cancellationToken)
    {
        if (IsSingleThread)
        {
            while (WriteLock.IsWaitting)
            {
                //等待写入完成
                await Task.Delay(100);
            }
        }
        try
        {
            var result = await _plc.ReadJTokenValueAsync(deviceVariableSourceRead.VariableRunTimes.Where(a => !a.RegisterAddress.IsNullOrEmpty()).Select(a => a.RegisterAddress!).ToArray(), cancellationToken);
            foreach (var data in result)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    var data1 = deviceVariableSourceRead.VariableRunTimes.Where(a => a.RegisterAddress == data.Item1);

                    foreach (var item in data1)
                    {
                        object value;
                        if (data.Item3 is JValue jValue)
                        {
                            value = jValue.Value;
                        }
                        else
                        {
                            value = data.Item3;
                        }
                        var isGood = StatusCode.IsGood(data.Item2.StatusCode);

                        var time = data.Item2.SourceTimestamp;
                        if (isGood)
                        {
                            item.SetValue(value, time);
                        }
                        else
                        {
                            item.SetValue(null, time, false);
                            item.VariableSource.LastErrorMessage = data.Item2.StatusCode.ToString();
                        }
                    }
                    LogMessage.Trace($"{ToString()} 状态变化:{Environment.NewLine}{data.Item1} : {data.Item3}");
                }
            }
            if (result.Any(a => StatusCode.IsBad(a.Item2.StatusCode)))
            {
                return new OperResult<byte[]>($"读取失败");
            }
            else
            {
                return OperResult.CreateSuccessResult<byte[]>(null);
            }
        }
        catch (Exception ex)
        {
            return new OperResult<byte[]>($"ReadSourceAsync Error：{Environment.NewLine}{ex}");
        }
    }

    /// <inheritdoc/>
    protected override async Task<Dictionary<string, OperResult>> WriteValuesAsync(Dictionary<VariableRunTime, JToken> writeInfoLists, CancellationToken cancellationToken)
    {
        try
        {
            if (IsSingleThread)
                await WriteLock.WaitAsync(cancellationToken);
            var result = await _plc.WriteNodeAsync(writeInfoLists.ToDictionary(a => a.Key.RegisterAddress!, a => a.Value), cancellationToken);
            return result.ToDictionary(a =>
            {
                return writeInfoLists.Keys.FirstOrDefault(b => b.RegisterAddress == a.Key)?.Name;
            }
            , a =>
            {
                if (!a.Value.Item1)
                    return new OperResult(a.Value.Item2);
                else
                    return new();
            });
        }
        finally
        {
            if (IsSingleThread)
                WriteLock.Release();
        }
    }

    protected override async Task ProtectedExecuteAsync(CancellationToken cancellationToken)
    {
        if (_driverPropertys.ActiveSubscribe)
        {
            //获取设备连接状态
            if (IsConnected())
            {
                //更新设备活动时间
                CurrentDevice.SetDeviceStatus(DateTimeUtil.TimerXNow, 0);
            }
            else
            {
                CurrentDevice.SetDeviceStatus(DateTimeUtil.TimerXNow, 999);
            }
        }
        else
        {
            await base.ProtectedExecuteAsync(cancellationToken);
        }
    }

    /// <inheritdoc/>
    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (_plc != null)
        {
            _plc.DataChangedHandler -= DataChangedHandler;
            _plc.OpcStatusChange -= _plc_OpcStatusChange;

            _plc.Disconnect();
            _plc.SafeDispose();
        }
        base.Dispose(disposing);
    }

    private void _plc_OpcStatusChange(object? sender, OpcUaStatusEventArgs e)
    {
        LogMessage?.Log((LogLevel)e.LogLevel, null, e.Text, null);
    }

    private void DataChangedHandler((VariableNode variableNode, DataValue dataValue, JToken jToken) data)
    {
        try
        {
            if (!CurrentDevice.KeepRun)
                return;
            if (_token.IsCancellationRequested)
                return;

            LogMessage.Trace($"{ToString()} 状态变化: {Environment.NewLine} {data.variableNode.NodeId} : {data.jToken?.ToString()}");

            if (!CurrentDevice.KeepRun)
            {
                return;
            }
            //尝试固定点位的数据类型
            var type = TypeInfo.GetSystemType(TypeInfo.GetBuiltInType(data.variableNode.DataType, _plc.Session.SystemContext.TypeTable), data.variableNode.ValueRank);

            var itemReads = CurrentDevice.VariableRunTimes.Where(it => it.RegisterAddress == data.variableNode.NodeId).ToList();

            object value;
            if (data.jToken is JValue jValue)
            {
                value = jValue.Value;
            }
            else
            {
                value = data.jToken;
            }
            var isGood = StatusCode.IsGood(data.dataValue.StatusCode);
            DateTime time = default;
            if (_driverPropertys.SourceTimestampEnable)
            {
                time = data.dataValue.SourceTimestamp.ToLocalTime();
            }
            foreach (var item in itemReads)
            {
                if (!CurrentDevice.KeepRun)
                    return;
                if (_token.IsCancellationRequested)
                    return;
                if (item.DataType == DataTypeEnum.Object)
                    if (type.Namespace.StartsWith("System"))
                    {
                        var enumValues = Enum.GetValues<DataTypeEnum>();
                        var stringList = enumValues.Select(e => e.ToString()).ToList();
                        if (stringList.Contains(type.Name))
                            try { item.DataType = Enum.Parse<DataTypeEnum>(type.Name); } catch { }
                    }
                if (isGood)
                {
                    item.SetValue(value, time);
                }
                else
                {
                    item.SetValue(null, time, false);
                    item.VariableSource.LastErrorMessage = data.Item2.StatusCode.ToString();
                }
            }
            success = true;
        }
        catch (Exception ex)
        {
            if (success)
                LogMessage?.LogWarning(ex);
            success = false;
        }
    }
}