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

using Opc.Ua;

using ThingsGateway.Gateway.Application;

namespace ThingsGateway.Plugin.OpcUa;

/// <inheritdoc/>
public class OpcUaServerProperty : BusinessPropertyBase
{
    [DynamicProperty("是否选择全部变量", "")]
    public bool IsAllVariable { get; set; } = false;

    /// <summary>
    /// 服务地址
    /// </summary>
    [DynamicProperty("服务地址", "分号分割数组，可设置多个url")]
    public string OpcUaStringUrl { get; set; } = "opc.tcp://127.0.0.1:49321";

    /// <summary>
    /// SubjectName
    /// </summary>
    [DynamicProperty("SubjectName", "")]
    public string BigTextSubjectName { get; set; } = "CN=ThingsGateway OPCUAServer, C=CN, S=GUANGZHOU, O=ThingsGateway, DC=" + System.Net.Dns.GetHostName();

    /// <summary>
    /// ApplicationUri
    /// </summary>
    [DynamicProperty("ApplicationUri", "")]
    public string BigTextApplicationUri { get; set; } = Utils.Format(@"urn:{0}:thingsgatewayopcuaserver", System.Net.Dns.GetHostName());

    /// <summary>
    /// 安全策略
    /// </summary>
    [DynamicProperty("安全策略", "")]
    public bool SecurityPolicy { get; set; }

    /// <summary>
    /// 接受不受信任的证书
    /// </summary>
    [DynamicProperty("自动接受不受信任的证书", "")]
    public bool AutoAcceptUntrustedCertificates { get; set; } = true;
}