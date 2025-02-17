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

using Furion.Logging.Extensions;

using Mapster;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 网关更新服务
/// </summary>
public class UpdatesWorker : BackgroundService
{
    protected IServiceScope _serviceScope;
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly ILogger _logger;

    /// <inheritdoc cref="AlarmWorker"/>
    public UpdatesWorker(IServiceScopeFactory serviceScopeFactory, IHostApplicationLifetime appLifetime)
    {
        _serviceScope = serviceScopeFactory.CreateScope();
        _logger = _serviceScope.ServiceProvider.GetService<ILoggerFactory>().CreateLogger("网关更新服务");
        _appLifetime = appLifetime;
    }

    #region worker服务

    private EasyLock _easyLock = new();

    /// <inheritdoc/>
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger?.LogInformation("更新服务启动");
        await _easyLock.WaitAsync();
        _appLifetime.ApplicationStarted.Register(() => { _easyLock.Release(); _easyLock = null; });
        await base.StartAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger?.LogInformation("更新服务停止");
        return base.StopAsync(cancellationToken);
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _easyLock?.WaitAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(60000, stoppingToken);
            }
            catch (TaskCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }

    #endregion worker服务
}