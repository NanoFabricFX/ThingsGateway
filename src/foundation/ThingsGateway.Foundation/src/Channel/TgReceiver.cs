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

namespace ThingsGateway.Foundation;

/// <summary>
/// TgReceiver
/// </summary>
public class TgReceiver : DisposableObject, IReceiver
{
    /// <summary>
    /// TgReceiver
    /// </summary>
    ~TgReceiver()
    {
        this.Dispose(false);
    }

    private readonly IClient m_client;
    private readonly EasyLock m_resetEventForComplateRead = new EasyLock(false);
    private readonly AsyncAutoResetEvent m_resetEventForRead = new AsyncAutoResetEvent(false);
    private ByteBlock m_byteBlock;
    private IRequestInfo m_requestInfo;

    /// <summary>
    /// TgReceiver
    /// </summary>
    /// <param name="client"></param>
    public TgReceiver(IClient client)
    {
        this.m_client = client;
    }

    /// <inheritdoc/>
    public async Task<ReceiverResult> ReadAsync(CancellationToken token)
    {
        this.ThrowIfDisposed();
        await this.m_resetEventForRead.WaitOneAsync(token).ConfigureFalseAwait();
        return new ReceiverResult(this.ComplateRead, this.m_byteBlock, this.m_requestInfo);
    }

#if NET6_0_OR_GREATER
    /// <inheritdoc/>
    public async ValueTask<ReceiverResult> ValueReadAsync(CancellationToken token)
    {
        this.ThrowIfDisposed();
        await this.m_resetEventForRead.WaitOneAsync(token).ConfigureFalseAwait();
        return new ReceiverResult(this.ComplateRead, this.m_byteBlock, this.m_requestInfo);
    }
#endif

    /// <inheritdoc/>
    public bool TryInputReceive(ByteBlock byteBlock, IRequestInfo requestInfo)
    {
        if (this.DisposedValue)
        {
            return false;
        }
        this.m_byteBlock = byteBlock;
        this.m_requestInfo = requestInfo;
        this.m_resetEventForRead.Set();
        if (byteBlock == null && requestInfo == null)
        {
            return true;
        }
        if (this.m_resetEventForComplateRead.Wait(TimeSpan.FromSeconds(10), CancellationToken.None))
        {
            return true;
        }
        return false;
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.m_client.ClearReceiver();
        }
        else
        {
            this.m_resetEventForRead.SafeDispose();
        }
        this.m_byteBlock = null;
        base.Dispose(disposing);
    }

    private void ComplateRead()
    {
        this.m_byteBlock = default;
        this.m_requestInfo = default;
        this.m_resetEventForComplateRead.Release();
    }
}