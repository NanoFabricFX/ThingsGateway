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

namespace ThingsGateway.Admin.Application;

/// <summary>
/// 权限按钮服务
/// </summary>
public interface IButtonService : ISugarService, ITransient
{
    /// <summary>
    /// 添加按钮
    /// </summary>
    /// <param name="input">添加参数</param>
    /// <returns></returns>
    Task AddAsync(ButtonAddInput input);

    /// <summary>
    /// 批量添加
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<List<long>> AddBatchAsync(ButtonAddInput input);

    /// <summary>
    /// 删除按钮
    /// </summary>
    /// <param name="input">删除参数</param>
    /// <returns></returns>
    Task DeleteAsync(List<BaseIdInput> input);

    /// <summary>
    /// 编辑按钮
    /// </summary>
    /// <param name="input">编辑参数</param>
    /// <returns></returns>
    Task EditAsync(ButtonEditInput input);

    /// <summary>
    /// 按钮分页查询
    /// </summary>
    /// <param name="input">查询条件</param>
    /// <returns>按钮分页列表</returns>
    Task<SqlSugarPagedList<SysResource>> PageAsync(ButtonPageInput input);
}