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

using Mapster;

using Microsoft.Extensions.Logging;

using Opc.Ua;
using Opc.Ua.Configuration;

using System.Collections.Concurrent;

using ThingsGateway.Foundation.Extension.ConcurrentQueue;
using ThingsGateway.Gateway.Application;

using TouchSocket.Core;

namespace ThingsGateway.Plugin.OpcUa;

/// <summary>
/// OPCUA服务端
/// </summary>
public partial class OpcUaServer : BusinessBase
{
    private readonly OpcUaServerProperty _driverPropertys = new();
    private readonly OpcUaServerVariableProperty _variablePropertys = new();
    private ApplicationInstance m_application;
    private ApplicationConfiguration m_configuration;
    private ThingsGatewayServer m_server;
    private volatile bool success = true;

    /// <inheritdoc/>
    public override VariablePropertyBase VariablePropertys => _variablePropertys;

    /// <inheritdoc/>
    protected override BusinessPropertyBase _businessPropertyBase => _driverPropertys;

    protected override IProtocol? Protocol => null;
    private ConcurrentQueue<VariableData> CollectVariableRunTimes { get; set; } = new();

    public override void Init(IChannel? channel = null)
    {
        base.Init(channel);
        if (_driverPropertys.IsAllVariable)
        {
            CurrentDevice.VariableRunTimes = GlobalData.AllVariables.ToList();
            CollectDevices = GlobalData.CollectDevices.ToList();
        }

        ApplicationInstance.MessageDlg = new ApplicationMessageDlg(LogMessage);//默认返回true

        //Utils.SetLogger(new OPCUALogger(LogMessage)); //调试用途
        m_application = new ApplicationInstance();
        m_configuration = GetDefaultConfiguration();
        m_configuration.Validate(ApplicationType.Server).GetAwaiter().GetResult();
        m_application.ApplicationConfiguration = m_configuration;
        if (m_configuration.SecurityConfiguration.AutoAcceptUntrustedCertificates)
        {
            m_configuration.CertificateValidator.CertificateValidation += (s, e) =>
            {
                e.Accept = (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted);
            };
        }

        m_server = new(this);

        CollectVariableRunTimes.Clear();
        CurrentDevice.VariableRunTimes.ForEach(a =>
        {
            VariableValueChange(a);
            a.VariableValueChange += VariableValueChange;
        });
    }

    /// <inheritdoc/>
    public override bool IsConnected() => m_server?.CurrentInstance.CurrentState == Opc.Ua.ServerState.Running;

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        CurrentDevice?.VariableRunTimes?.ForEach(a =>
        {
            a.VariableValueChange -= VariableValueChange;
        });
        m_server?.SafeDispose();
        CollectVariableRunTimes.Clear();
        base.Dispose(disposing);
    }

    protected override Task ProtectedAfterStopAsync()
    {
        m_application.Stop();
        return base.ProtectedAfterStopAsync();
    }

    protected override async Task ProtectedBeforStartAsync(CancellationToken cancellationToken)
    {
        // 启动服务器。
        await m_application.CheckApplicationInstanceCertificate(false, 0, 1200);
        await m_application.Start(m_server);
        await base.ProtectedBeforStartAsync(cancellationToken);
    }

    protected override async Task ProtectedExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            ////变化推送
            var varList = CollectVariableRunTimes.ToListWithDequeue();
            if (varList?.Count != 0)
            {
                foreach (var item in varList)
                {
                    try
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            m_server?.NodeManager?.UpVariable(item);
                        }
                        else
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogMessage.LogWarning(ex);
                    }
                }
            }
            success = true;
        }
        catch (Exception ex)
        {
            if (success)
                LogMessage.LogWarning(ex);
            success = false;
        }

        await Delay(CurrentDevice.IntervalTime, cancellationToken);
    }

    private ApplicationConfiguration GetDefaultConfiguration()
    {
        ApplicationConfiguration config = new();
        var urls = _driverPropertys.OpcUaStringUrl.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
        // 签名及加密验证
        ServerSecurityPolicyCollection policies = new();
        var userTokens = new UserTokenPolicyCollection();
        if (_driverPropertys.SecurityPolicy)
        {
            policies.Add(new ServerSecurityPolicy()
            {
                SecurityMode = MessageSecurityMode.Sign,
                SecurityPolicyUri = SecurityPolicies.Basic128Rsa15
            });
            policies.Add(new ServerSecurityPolicy()
            {
                SecurityMode = MessageSecurityMode.Sign,
                SecurityPolicyUri = SecurityPolicies.Basic256
            });
            policies.Add(new ServerSecurityPolicy()
            {
                SecurityMode = MessageSecurityMode.Sign,
                SecurityPolicyUri = SecurityPolicies.Basic256Sha256
            });
            policies.Add(new ServerSecurityPolicy()
            {
                SecurityMode = MessageSecurityMode.Sign,
                SecurityPolicyUri = SecurityPolicies.Aes128_Sha256_RsaOaep
            });
            policies.Add(new ServerSecurityPolicy()
            {
                SecurityMode = MessageSecurityMode.Sign,
                SecurityPolicyUri = SecurityPolicies.Aes256_Sha256_RsaPss
            });
            policies.Add(new ServerSecurityPolicy()
            {
                SecurityMode = MessageSecurityMode.SignAndEncrypt,
                SecurityPolicyUri = SecurityPolicies.Basic128Rsa15
            });
            policies.Add(new ServerSecurityPolicy()
            {
                SecurityMode = MessageSecurityMode.SignAndEncrypt,
                SecurityPolicyUri = SecurityPolicies.Basic256
            });
            policies.Add(new ServerSecurityPolicy()
            {
                SecurityMode = MessageSecurityMode.SignAndEncrypt,
                SecurityPolicyUri = SecurityPolicies.Basic256Sha256
            });
            policies.Add(new ServerSecurityPolicy()
            {
                SecurityMode = MessageSecurityMode.SignAndEncrypt,
                SecurityPolicyUri = SecurityPolicies.Aes128_Sha256_RsaOaep
            });
            policies.Add(new ServerSecurityPolicy()
            {
                SecurityMode = MessageSecurityMode.SignAndEncrypt,
                SecurityPolicyUri = SecurityPolicies.Aes256_Sha256_RsaPss
            });

            userTokens.Add(new UserTokenPolicy(UserTokenType.UserName));
        }
        else
        {
            userTokens.Add(new UserTokenPolicy(UserTokenType.UserName));
            policies.Add(new ServerSecurityPolicy()
            {
                SecurityMode = MessageSecurityMode.None,
                SecurityPolicyUri = SecurityPolicies.None
            });
            userTokens.Add(new UserTokenPolicy(UserTokenType.Anonymous));
        }

        config.ApplicationName = "ThingsGateway OPCUAServer";
        config.ApplicationType = ApplicationType.Server;
        config.ApplicationUri = _driverPropertys.BigTextApplicationUri;

        config.ServerConfiguration = new ServerConfiguration()
        {
            // 配置登录的地址
            BaseAddresses = urls,

            SecurityPolicies = policies,
            UserTokenPolicies = userTokens,
            ShutdownDelay = 1,

            DiagnosticsEnabled = false,           // 是否启用诊断
            MaxSessionCount = 1000,               // 最大打开会话数
            MinSessionTimeout = 10000,            // 允许该会话在与客户端断开时（单位毫秒）仍然保持连接的最小时间
            MaxSessionTimeout = 60000,            // 允许该会话在与客户端断开时（单位毫秒）仍然保持连接的最大时间
            MaxBrowseContinuationPoints = 1000,   // 用于Browse / BrowseNext操作的连续点的最大数量。
            MaxQueryContinuationPoints = 1000,    // 用于Query / QueryNext操作的连续点的最大数量
            MaxHistoryContinuationPoints = 500,   // 用于HistoryRead操作的最大连续点数。
            MaxRequestAge = 1000000,              // 传入请求的最大年龄（旧请求被拒绝）。
            MinPublishingInterval = 100,          // 服务器支持的最小发布间隔（以毫秒为单位）
            MaxPublishingInterval = 3600000,      // 服务器支持的最大发布间隔（以毫秒为单位）1小时
            PublishingResolution = 50,            // 支持的发布间隔（以毫秒为单位）的最小差异
            MaxSubscriptionLifetime = 3600000,    // 订阅将在没有客户端发布的情况下保持打开多长时间 1小时
            MaxMessageQueueSize = 100,            // 每个订阅队列中保存的最大消息数
            MaxNotificationQueueSize = 100,       // 为每个被监视项目保存在队列中的最大证书数
            MaxNotificationsPerPublish = 1000,    // 每次发布的最大通知数
            MinMetadataSamplingInterval = 1000,   // 元数据的最小采样间隔
            MaxRegistrationInterval = -1,   // 两次注册尝试之间的最大时间（以毫秒为单位）//不提供注册
        };
        config.SecurityConfiguration = new SecurityConfiguration()
        {
            AddAppCertToTrustedStore = true,
            AutoAcceptUntrustedCertificates = _driverPropertys.AutoAcceptUntrustedCertificates,
            RejectSHA1SignedCertificates = false,
            MinimumCertificateKeySize = 1024,
            SuppressNonceValidationErrors = true,
            ApplicationCertificate = new CertificateIdentifier()
            {
                StoreType = CertificateStoreType.X509Store,
                StorePath = "CurrentUser\\UAServer_ThingsGateway",
                SubjectName = _driverPropertys.BigTextSubjectName,
                //ValidationOptions = CertificateValidationOptions.SuppressHostNameInvalid,
            },

            TrustedPeerCertificates = new CertificateTrustList()
            {
                StoreType = CertificateStoreType.Directory,
                StorePath = AppContext.BaseDirectory + @"OPCUAServerCertificate\pki\trustedPeer",
            },

            TrustedIssuerCertificates = new CertificateTrustList()
            {
                StoreType = CertificateStoreType.Directory,
                StorePath = AppContext.BaseDirectory + @"OPCUAServerCertificate\pki\trustedIssuer",
            },

            RejectedCertificateStore = new CertificateStoreIdentifier()
            {
                StoreType = CertificateStoreType.Directory,
                StorePath = AppContext.BaseDirectory + @"OPCUAServerCertificate\pki\rejected",
            },
            UserIssuerCertificates = new CertificateTrustList
            {
                StoreType = CertificateStoreType.Directory,
                StorePath = AppContext.BaseDirectory + @"OPCUAServerCertificate\pki\issuerUser",
            },
            TrustedUserCertificates = new CertificateTrustList
            {
                StoreType = CertificateStoreType.Directory,
                StorePath = AppContext.BaseDirectory + @"OPCUAServerCertificate\pki\trustedUser",
            }
        };

        config.TransportConfigurations = new TransportConfigurationCollection();
        config.TransportQuotas = new TransportQuotas { OperationTimeout = 15000 };
        config.ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 };
        config.TraceConfiguration = new TraceConfiguration();

        config.CertificateValidator = new CertificateValidator();
        config.CertificateValidator.Update(config).GetAwaiter().GetResult();
        config.Extensions = new XmlElementCollection();

        return config;
    }

    private void VariableValueChange(VariableRunTime variableRunTime)
    {
        if (!CurrentDevice.KeepRun)
            return;
        CollectVariableRunTimes.Enqueue(variableRunTime.Adapt<VariableData>());
    }
}