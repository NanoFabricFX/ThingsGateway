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

using System.ComponentModel;

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 会话信息
/// </summary>
public class VerificatInfo : PrimaryIdEntity
{
    /// <summary>
    /// 客户端ID列表
    /// </summary>
    [Newtonsoft.Json.JsonIgnore]
    public List<string> ClientIds { get; set; } = new List<string>();

    /// <summary>
    /// 设备
    /// </summary>
    [Description("设备")]
    [DataTable(Order = 1, IsShow = true, Sortable = true)]
    public string Device { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    [Description("过期时间")]
    public int Expire { get; set; }

    /// <summary>
    /// 验证Id
    /// </summary>
    [Description("验证Id")]
    [DataTable(Order = 3, IsShow = true, Sortable = true)]
    public override long Id { get; set; }

    /// <summary>
    /// 在线状态
    /// </summary>
    [Description("在线状态")]
    [DataTable(Order = 2, IsShow = true, Sortable = true)]
    public bool IsOnline => ClientIds.Count > 0;

    /// <summary>
    /// 连接数量
    /// </summary>
    [Description("连接数量")]
    [DataTable(Order = 4, IsShow = true, Sortable = true)]
    public int OnlineNum => ClientIds.Count;

    /// <summary>
    /// verificat剩余有效期
    /// </summary>
    [Description("有效期")]
    [DataTable(Order = 5, IsShow = true, Sortable = true)]
    public string VerificatRemain { get; set; }

    /// <summary>
    /// 超时时间
    /// </summary>
    [Description("超时时间")]
    public DateTime VerificatTimeout { get; set; }
}