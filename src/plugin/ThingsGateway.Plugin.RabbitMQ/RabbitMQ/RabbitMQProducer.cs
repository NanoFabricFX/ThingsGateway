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

using RabbitMQ.Client;

using ThingsGateway.Foundation;

using TouchSocket.Core;

namespace ThingsGateway.Plugin.RabbitMQ;

/// <summary>
/// RabbitMQProducer
/// </summary>
public partial class RabbitMQProducer : BusinessBaseWithCacheIntervalScript<VariableData, DeviceData, AlarmVariable>
{
    private readonly RabbitMQProducerVariableProperty _variablePropertys = new();
    private readonly RabbitMQProducerProperty _driverPropertys = new();

    public override VariablePropertyBase VariablePropertys => _variablePropertys;

    protected override BusinessPropertyWithCacheIntervalScript _businessPropertyWithCacheIntervalScript => _driverPropertys;

    /// <inheritdoc/>
    public override bool IsConnected() => success;

    /// <inheritdoc/>
    public override string ToString()
    {
        return $" {nameof(RabbitMQProducer)} IP:{_driverPropertys.IP} Port:{_driverPropertys.Port}";
    }

    public override void Init(IChannel? channel = null)
    {
        base.Init(channel);

        #region 初始化

        _connectionFactory = new ConnectionFactory
        {
            HostName = _driverPropertys.IP,
            Port = _driverPropertys.Port,
            UserName = _driverPropertys.UserName,
            Password = _driverPropertys.Password,
            VirtualHost = _driverPropertys.VirtualHost,
        };

        #endregion 初始化
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        _model?.SafeDispose();
        _connection?.SafeDispose();
        base.Dispose(disposing);
    }

    protected override async Task ProtectedExecuteAsync(CancellationToken cancellationToken)
    {
        if (_model == null)
        {
            try
            {
                // 创建连接
                _connection ??= _connectionFactory.CreateConnection();
                // 创建通道
                _model ??= _connection.CreateModel();
                // 声明路由队列
                if (_driverPropertys.IsQueueDeclare)
                {
                    _model?.QueueDeclare(_driverPropertys.VariableTopic, true, false, false);
                    _model?.QueueDeclare(_driverPropertys.DeviceTopic, true, false, false);
                    _model?.QueueDeclare(_driverPropertys.AlarmTopic, true, false, false);
                }
                success = true;
            }
            catch (Exception ex)
            {
                if (success)
                {
                    LogMessage?.LogWarning(ex);
                    success = false;
                }
            }
        }
        else
        {
            await Update(cancellationToken);
        }

        await Delay(CurrentDevice.IntervalTime, cancellationToken);
    }
}