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

using Mapster;

using ThingsGateway.Foundation;

using Yitter.IdGenerator;

namespace ThingsGateway.Plugin.TDengineDB;

/// <summary>
/// TDengineDBProducer
/// </summary>
public partial class TDengineDBProducer : BusinessBaseWithCacheInterval<TDengineDBHistoryValue>
{
    private readonly TDengineDBProducerVariableProperty _variablePropertys = new();
    internal readonly TDengineDBProducerProperty _driverPropertys = new();

    public override VariablePropertyBase VariablePropertys => _variablePropertys;

    protected override BusinessPropertyWithCacheInterval _businessPropertyWithCacheInterval => _driverPropertys;

    /// <inheritdoc/>
    public override Type DriverUIType => typeof(TDengineDBPage);

    /// <inheritdoc/>
    public override bool IsConnected() => success;

    /// <inheritdoc/>
    public override string ToString()
    {
        return $" {nameof(TDengineDBProducer)}";
    }

    public override void Init(IChannel? channel = null)
    {
        base.Init(channel);

        #region 初始化

        _config = new TypeAdapterConfig();
        _config.ForType<VariableRunTime, TDengineDBHistoryValue>()
            .Map(dest => dest.Value, src => src.Value == null ? string.Empty : src.Value.ToString() ?? string.Empty)
            .Map(dest => dest.Id, src => YitIdHelper.NextId())
            ;//注意sqlsugar插入时无时区，直接utc时间

        #endregion 初始化
    }

    protected override async Task ProtectedBeforStartAsync(CancellationToken cancellationToken)
    {
        var db = BusinessDatabaseUtil.GetDb(_driverPropertys.DbType, _driverPropertys.BigTextConnectStr);
        db.DbMaintenance.CreateDatabase();
        db.CodeFirst.InitTables(typeof(TDengineDBHistoryValue));
        await base.ProtectedBeforStartAsync(cancellationToken);
    }

    protected override async Task ProtectedExecuteAsync(CancellationToken cancellationToken)
    {
        await UpdateTMemory(cancellationToken);
        await UpdateTCache(cancellationToken);

        await Delay(CurrentDevice.IntervalTime, cancellationToken);
    }
}