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

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 插件配置项
/// <para></para>
/// 约定：
/// 如果需要密码输入，属性名称中需包含Password字符串
/// <br></br>
/// 使用<see cref="DevicePropertyAttribute"/> 标识所需的配置属性
/// </summary>
public abstract class CollectPropertyBase
{
    /// <summary>
    /// 离线后恢复运行的间隔时间 /s，默认30s
    /// </summary>
    [DynamicProperty("离线恢复间隔", "离线后恢复运行的间隔时间s，默认30s，最大600s")]
    public virtual int ReIntervalTime { get; set; } = 30;

    /// <summary>
    /// 失败重试次数，默认3
    /// </summary>
    [DynamicProperty("失败重试次数", "失败重试次数，默认3")]
    public virtual int RetryCount { get; set; } = 3;

    /// <summary>
    /// 最大并发数量
    /// </summary>
    [DynamicProperty("最大并发数量", "最大读写并发数量")]
    public virtual int ConcurrentCount { get; set; } = 1;
}