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

namespace ThingsGateway.Gateway.Blazor;

/// <summary>
/// 硬件信息页面
/// </summary>
public partial class HardwareInfoPage
{
    private readonly PeriodicTimer _periodicTimer = new(TimeSpan.FromSeconds(5));

    private HardwareInfoWorker _hardwareInfoWorker { get; set; }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        _periodicTimer?.Dispose();
        base.Dispose(disposing);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    protected override void OnInitialized()
    {
        _hardwareInfoWorker = WorkerUtil.GetWoker<HardwareInfoWorker>();
        _ = Task.Run(GetOption);
        _ = RunTimerAsync();
        base.OnInitialized();
    }

    private async Task RunTimerAsync()
    {
        while (await _periodicTimer.WaitForNextTickAsync())
        {
            try
            {
                GetOption();
                await InvokeAsync(StateHasChanged);
                await Task.Delay(5000);
            }
            catch
            {
            }
        }
    }

    private object eChartsOption = new();

    private void GetOption()
    {
        var hisDatas = _hardwareInfoWorker.GetHis();

        eChartsOption = new
        {
            tooltip = new { trigger = "axis" },
            legend = new
            {
                data = new[] {
                AppService.I18n.T("CPU使用率"),
                AppService.I18n.T("内存使用率"),
                AppService.I18n.T("磁盘使用率"),
                AppService.I18n.T("温度"),
                AppService.I18n.T("电池"),
                AppService.I18n.T("上载速度"),
                AppService.I18n.T("下载速度")
            },
                left = "5%"
            },
            grid = new { left = "3%", right = "4%", bottom = "3%", containLabel = true },
            toolbox = new { feature = new { saveAsImage = new { } } },
            xAxis = new
            {
                type = "category",
                boundaryGap = false,
                data = hisDatas.Select(a => a.Date.ToString("yyyy-MM-dd HH:mm")).ToArray()
            },
            yAxis = new { type = "value" },
            series = new[]
    {
        new { name =AppService.I18n.T( "CPU使用率"), type = "line",
                data = hisDatas.Select(a=>a.CpuUsage).ToArray(),
            smooth=true
        },
        new { name = AppService.I18n.T("内存使用率"), type = "line",
                data = hisDatas.Select(a=>a.MemoryUsage).ToArray(),
            smooth=true
        },
        new { name = AppService.I18n.T("磁盘使用率"), type = "line",
                data = hisDatas.Select(a=>a.DriveUsage).ToArray(),
            smooth=true
        },
        new { name = AppService.I18n.T("温度"), type = "line",
                data = hisDatas.Select(a=>a.Temperature).ToArray(),
            smooth=true
        },
                new { name = AppService.I18n.T("电池"), type = "line",
                data = hisDatas.Select(a=>a.Battery).ToArray(),
            smooth=true
        },
        new { name = AppService.I18n.T("上载速度"), type = "line",
                data = hisDatas.Select(a=>a.UplinkSpeed).ToArray(),
            smooth=true
        },
        new { name = AppService.I18n.T("下载速度"), type = "line",
                data = hisDatas.Select(a=>a.DownlinkSpeed).ToArray(),
            smooth=true
        },
    }
        };
    }
}