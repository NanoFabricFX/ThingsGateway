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

using Microsoft.AspNetCore.Components.Forms;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// 插件添加DTO
/// </summary>
public class PluginAddInput
{
    /// <summary>
    /// 主程序集
    /// </summary>
    [Description("主程序集")]
    [Required(ErrorMessage = "主程序集不能为空")]
    public IBrowserFile? MainFile { get; set; }

    /// <summary>
    /// 附属程序集
    /// </summary>
    [Description("附属程序集")]
    public List<IBrowserFile?>? OtherFiles { get; set; }
}

/// <summary>
/// 插件分页
/// </summary>
public class PluginPageInput : BasePageInput
{
    /// <summary>
    /// 插件名称
    /// </summary>
    [Description("插件名称")]
    public string Name { get; set; }

    /// <summary>
    /// 文件名称
    /// </summary>
    [Description("文件名称")]
    public string FileName { get; set; }

    [Description("插件类型")]
    public PluginTypeEnum? PluginType { get; set; }
}