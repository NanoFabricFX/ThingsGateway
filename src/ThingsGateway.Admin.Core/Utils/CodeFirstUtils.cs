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

using System.Collections;
using System.Reflection;

namespace ThingsGateway.Admin.Core;

/// <summary>
/// CodeFirst功能类
/// </summary>
[SuppressSniffer]
public static class CodeFirstUtils
{
    /// <summary>
    ///  CodeFirst生成数据库表结构和种子数据
    /// </summary>
    /// <param name="options">codefirst选项</param>
    /// <param name="assemblyName">程序集名称</param>
    public static void CodeFirst(string assemblyName)
    {
        InitTable(assemblyName);
        InitSeedData(assemblyName);
    }

    /// <summary>
    /// 初始化数据库表结构
    /// </summary>
    /// <param name="assemblyName">程序集名称</param>
    private static void InitTable(string assemblyName)
    {
        // 获取所有实体表-初始化表结构
        var entityTypes = App.EffectiveTypes.Where(u =>
            !u.IsInterface && !u.IsAbstract && u.IsClass && u.IsDefined(typeof(SugarTable), false) && u.Assembly.FullName == assemblyName);
        if (!entityTypes.Any()) return;//没有就退出
        foreach (var entityType in entityTypes)
        {
            var tenantAtt = entityType.GetCustomAttribute<TenantAttribute>();//获取Sqlsugar多租户特性
            var config = DbContext.DbConfigs.FirstOrDefault(u => u.ConfigId.ToString() == tenantAtt.configId.ToString());//获取数据库配置
            if (!config.InitTable) continue;
            var ignoreInit = entityType.GetCustomAttribute<IgnoreInitTableAttribute>();//获取忽略初始化特性
            if (ignoreInit != null) continue;//如果有忽略初始化特性
            if (tenantAtt == null) continue;//如果没有租户特性就下一个
            var db = DbContext.Db.GetConnectionScope(tenantAtt.configId.ToString());//获取数据库对象
            var splitTable = entityType.GetCustomAttribute<SplitTableAttribute>();//获取自动分表特性
            if (splitTable == null)//如果特性是空
                db.CodeFirst.InitTables(entityType);//普通创建
            else
                db.CodeFirst.SplitTables().InitTables(entityType);//自动分表创建
        }
    }

    /// <summary>
    /// 初始化种子数据
    /// </summary>
    /// <param name="assemblyName">程序集名称</param>
    private static void InitSeedData(string assemblyName)
    {
        // 获取所有种子配置-初始化数据
        var seedDataTypes = App.EffectiveTypes.Where(u => !u.IsInterface && u is { IsAbstract: false, IsClass: true }
            && u.GetInterfaces().Any(i => i.HasImplementedRawGeneric(typeof(ISqlSugarEntitySeedData<>))) && u.Assembly.FullName == assemblyName);
        if (!seedDataTypes.Any()) return;
        foreach (var seedType in seedDataTypes)//遍历种子类
        {
            //使用与指定参数匹配程度最高的构造函数来创建指定类型的实例。
            var instance = Activator.CreateInstance(seedType);
            //获取SeedData方法
            var hasDataMethod = seedType.GetMethod("SeedData");
            //判断是否有种子数据
            var seedData = ((IEnumerable)hasDataMethod?.Invoke(instance, null))?.Cast<object>();
            if (seedData == null) continue;//没有种子数据就下一个
            var entityType = seedType.GetInterfaces().First().GetGenericArguments().First();//获取实体类型
            var tenantAtt = entityType.GetCustomAttribute<TenantAttribute>();//获取sqlsugar租户特性
            if (tenantAtt == null) continue;//如果没有租户特性就下一个
            var db = DbContext.Db.GetConnectionScope(tenantAtt.configId.ToString());//获取数据库对象
            var config = DbContext.DbConfigs.FirstOrDefault(u => u.ConfigId.ToString() == tenantAtt.configId.ToString());//获取数据库配置
            if (!config.InitSeedData) continue;
            // var seedDataTable = seedData.ToList().ToDataTable();//获取种子数据:已弃用
            var entityInfo = db.EntityMaintenance.GetEntityInfo(entityType);
            // seedDataTable.TableName = db.EntityMaintenance.GetEntityInfo(entityType).DbTableName;//获取表名
            var ignoreAdd = hasDataMethod.GetCustomAttribute<IgnoreSeedDataAddAttribute>();//读取忽略插入特性
            var ignoreUpdate = hasDataMethod.GetCustomAttribute<IgnoreSeedDataUpdateAttribute>();//读取忽略更新特性
            if (entityInfo.Columns.Any(u => u.IsPrimarykey))//判断种子数据是否有主键
            {
                // 按主键进行批量增加和更新
                var storage = db.StorageableByObject(seedData.ToList()).ToStorage();
                if (ignoreAdd == null) storage.AsInsertable.ExecuteCommand();//执行插入

                if (ignoreUpdate == null && config.IsUpdateSeedData) storage.AsUpdateable.ExecuteCommand();//只有没有忽略更新的特性才执行更新
            }
            else// 没有主键或者不是预定义的主键(有重复的可能)
            {
                //全量插入
                // 无主键则只进行插入
                if (!db.Queryable(entityInfo.DbTableName, entityInfo.DbTableName).Any() && ignoreAdd == null)
                    db.InsertableByObject(seedData.ToList()).ExecuteCommand();
            }
        }
    }

    /// <summary>
    /// 排除SqlSugar忽略的列
    /// </summary>
    /// <param name="pi"></param>
    /// <returns></returns>
    private static bool IsIgnoreColumn(PropertyInfo pi)
    {
        var sc = pi.GetCustomAttributes<SugarColumn>(false).FirstOrDefault(u => u.IsIgnore == true);
        return sc != null;
    }

    private static bool IsJsonColumn(PropertyInfo pi)
    {
        var sc = pi.GetCustomAttributes<SugarColumn>(false).FirstOrDefault(u => u.IsJson == true);
        return sc != null;
    }
}