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
/// 关系服务
/// </summary>
public interface IRelationService : ISugarService, ITransient
{
    /// <summary>
    /// 根据分类获取关系表信息
    /// </summary>
    /// <param name="category">分类名称</param>
    /// <returns>关系表</returns>
    Task<List<SysRelation>> GetRelationByCategoryAsync(string category);

    /// <summary>
    /// 通过对象ID和分类获取关系列表
    /// </summary>
    /// <param name="objectId">对象ID</param>
    /// <param name="category">分类</param>
    /// <returns></returns>
    Task<List<SysRelation>> GetRelationListByObjectIdAndCategoryAsync(long objectId, string category);

    /// <summary>
    /// 通过对象ID列表和分类获取关系列表
    /// </summary>
    /// <param name="objectIds">对象ID</param>
    /// <param name="category">分类</param>
    /// <returns></returns>
    Task<List<SysRelation>> GetRelationListByObjectIdListAndCategoryAsync(List<long> objectIds, string category);

    /// <summary>
    /// 通过目标ID和分类获取关系列表
    /// </summary>
    /// <param name="targetId">目标ID</param>
    /// <param name="category">分类</param>
    /// <returns></returns>
    Task<List<SysRelation>> GetRelationListByTargetIdAndCategoryAsync(string targetId, string category);

    /// <summary>
    /// 通过目标ID列表和分类获取关系列表
    /// </summary>
    /// <param name="targetIds"></param>
    /// <param name="category"></param>
    /// <returns></returns>
    Task<List<SysRelation>> GetRelationListByTargetIdListAndCategoryAsync(List<string> targetIds, string category);

    /// <summary>
    /// 获取关系表用户工作台
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>关系表数据</returns>
    Task<SysRelation> GetWorkbenchAsync(long userId);

    /// <summary>
    /// 获取关系表用户主页
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>关系表数据</returns>
    Task<SysRelation> GetDefaultRazorAsync(long userId);

    /// <summary>
    /// 更新缓存
    /// </summary>
    /// <param name="category">分类</param>
    /// <returns></returns>
    Task RefreshCacheAsync(string category);

    /// <summary>
    /// 保存关系
    /// </summary>
    /// <param name="category">分类</param>
    /// <param name="objectId">对象ID</param>
    /// <param name="targetId">目标ID</param>
    /// <param name="extJson">拓展信息</param>
    /// <param name="clear">是否清除老的数据</param>
    /// <param name="refreshCache">是否刷新缓存</param>
    /// <returns></returns>
    Task SaveRelationAsync(string category, long objectId, string targetId, string extJson, bool clear, bool refreshCache = true);

    /// <summary>
    /// 批量保存关系
    /// </summary>
    /// <param name="category">分类</param>
    /// <param name="objectId">对象ID</param>
    /// <param name="targetIds">目标ID列表</param>
    /// <param name="extJsons">拓展信息列表</param>
    /// <param name="clear">是否清除老的数据</param>
    /// <returns></returns>
    Task SaveRelationBatchAsync(string category, long objectId, List<string> targetIds, List<string> extJsons, bool clear);
}