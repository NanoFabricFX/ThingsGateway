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

using System.Text;

namespace ThingsGateway.Foundation.Dlt645;

/// <summary>
/// Dlt645_2007Address
/// </summary>
public class Dlt645_2007Address
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public Dlt645_2007Address()
    {
    }

    /// <summary>
    /// 数据标识
    /// </summary>
    public byte[] DataId { get; set; } = new byte[0];

    /// <summary>
    /// 反转解析
    /// </summary>
    public bool Reverse { get; set; } = true;

    /// <summary>
    /// 站号信息
    /// </summary>
    public byte[] Station { get; set; } = new byte[0];

    /// <summary>
    /// 作为Slave时需提供的SocketId，用于分辨Socket客户端，通常对比的是初始链接时的注册包
    /// </summary>
    public string? SocketId { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        StringBuilder stringGeter = new();
        if (Station.Length > 0)
        {
            stringGeter.Append($"s={Station.Reverse().ToArray().ToHexString()};");
        }
        if (DataId.Length > 0)
        {
            stringGeter.Append($"{DataId.Reverse().ToArray().ToHexString()};");
        }
        if (!Reverse)
        {
            stringGeter.Append($"s={Reverse};");
        }
        if (!string.IsNullOrEmpty(SocketId))
        {
            stringGeter.Append($"id={SocketId};");
        }
        return stringGeter.ToString();
    }
}