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

namespace ThingsGateway.Foundation.Adapter.OPCDA.Da;

/// <summary>
/// 值变化
/// </summary>
/// <param name="opcItems"></param>
public delegate void OnDataChangedHandler(List<ItemReadResult> opcItems);

/// <summary>
/// 读取
/// </summary>
/// <param name="opcItems"></param>
public delegate void OnReadCompletedHandler(List<ItemReadResult> opcItems);

/// <summary>
/// 写入
/// </summary>
/// <param name="opcItems"></param>
internal delegate void OnWriteCompletedHandler(List<ItemWriteResult> opcItems);

/// <summary>
/// 返回结果
/// </summary>
public class ItemReadResult
{
    /// <summary>
    /// ID
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Quality
    /// </summary>
    public short Quality { get; set; }

    /// <summary>
    /// TimeStamp
    /// </summary>
    public DateTime TimeStamp { get; set; }

    /// <summary>
    /// Value
    /// </summary>
    public object Value { get; set; } = 0;
}

internal class ItemWriteResult
{
    internal int Exception { get; set; } = 0;
    internal string Name { get; set; } = "";
}