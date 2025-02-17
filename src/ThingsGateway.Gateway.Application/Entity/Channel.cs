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

using System.IO.Ports;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 通道表
/// </summary>
[SugarTable("channel", TableDescription = "通道表")]
[Tenant(SqlSugarConst.DB_Custom)]
[SugarIndex("unique_device_name", nameof(Device.Name), OrderByType.Asc, true)]
public class Channel : PrimaryIdEntity, IChannelData
{
    /// <summary>
    /// 通道名称
    /// </summary>
    [SugarColumn(ColumnDescription = "名称", Length = 200)]
    [DataTable(Order = 0, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public virtual string Name { get; set; }

    /// <inheritdoc/>
    [SugarColumn(ColumnDescription = "通道类型", IsNullable = false)]
    [DataTable(Order = 1, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public virtual ChannelTypeEnum ChannelType { get; set; }

    /// <summary>
    /// 使能
    /// </summary>
    [SugarColumn(ColumnDescription = "使能")]
    [DataTable(Order = 1, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public virtual bool Enable { get; set; }

    /// <summary>
    /// 远程地址，可由<see cref="IPHost.IPHost(string)"/>与<see href="IPHost.ToString()"/>相互转化
    /// </summary>
    [SugarColumn(ColumnDescription = "远程地址", Length = 200, IsNullable = true)]
    [DataTable(Order = 2, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public string? RemoteUrl { get; set; }

    /// <summary>
    /// 本地地址，可由<see cref="IPHost.IPHost(string)"/>与<see href="IPHost.ToString()"/>相互转化
    /// </summary>
    [SugarColumn(ColumnDescription = "本地地址", Length = 200, IsNullable = true)]
    [DataTable(Order = 3, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public string? BindUrl { get; set; }

    /// <summary>
    /// COM
    /// </summary>
    [SugarColumn(ColumnDescription = "COM", IsNullable = true)]
    [DataTable(Order = 4, IsShow = true, Sortable = true, CellClass = " table-text-truncate ")]
    public string? PortName { get; set; }

    /// <summary>
    /// 波特率
    /// </summary>
    [SugarColumn(ColumnDescription = "波特率", IsNullable = true)]
    [DataTable(Order = 5, IsShow = true, DefaultFilter = true, Sortable = true, CellClass = " table-text-truncate ")]
    public int? BaudRate { get; set; }

    /// <summary>
    /// 数据位
    /// </summary>
    [SugarColumn(ColumnDescription = "数据位", IsNullable = true)]
    [DataTable(Order = 6, IsShow = true, DefaultFilter = true, Sortable = true, CellClass = " table-text-truncate ")]
    public int? DataBits { get; set; }

    /// <summary>
    /// 校验位
    /// </summary>
    [SugarColumn(ColumnDescription = "校验位", IsNullable = true)]
    [DataTable(Order = 7, IsShow = true, DefaultFilter = true, Sortable = true, CellClass = " table-text-truncate ")]
    public Parity? Parity { get; set; }

    /// <summary>
    /// 停止位
    /// </summary>
    [SugarColumn(ColumnDescription = "停止位", IsNullable = true)]
    [DataTable(Order = 8, IsShow = true, DefaultFilter = true, Sortable = true, CellClass = " table-text-truncate ")]
    public StopBits? StopBits { get; set; }

    /// <summary>
    /// DtrEnable
    /// </summary>
    [SugarColumn(ColumnDescription = "DtrEnable", IsNullable = true)]
    [DataTable(Order = 9, IsShow = true, DefaultFilter = true, Sortable = true, CellClass = " table-text-truncate ")]
    public bool? DtrEnable { get; set; }

    /// <summary>
    /// RtsEnable
    /// </summary>
    [SugarColumn(ColumnDescription = "RtsEnable", IsNullable = true)]
    [DataTable(Order = 9, IsShow = true, DefaultFilter = true, Sortable = true, CellClass = " table-text-truncate ")]
    public bool? RtsEnable { get; set; }

    /// <summary>
    /// LogEnable
    /// </summary>
    [SugarColumn(ColumnDescription = "调试日志")]
    [DataTable(Order = 9, IsShow = true, DefaultFilter = true, Sortable = true, CellClass = " table-text-truncate ")]
    public bool LogEnable { get; set; }

    /// <summary>
    /// 导入验证专用
    /// </summary>
    [IgnoreExcel]
    [SugarColumn(IsIgnore = true)]
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    internal bool IsUp { get; set; }
}