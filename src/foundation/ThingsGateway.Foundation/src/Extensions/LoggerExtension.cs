#region copyright

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

using System.Text.RegularExpressions;

namespace ThingsGateway.Foundation;

/// <summary>
/// <inheritdoc/>
/// </summary>
public static class LoggerExtension
{
    /// <summary>
    /// 获取DEBUG日志路径
    /// </summary>
    /// <param name="channelName"></param>
    /// <returns></returns>
    public static string GetDebugLogPath(this string channelName)
    {
        return GetDebugLogBasePath().CombinePath(channelName).FileNameReplace();
    }

    /// <summary>
    /// 获取日志路径
    /// </summary>
    /// <param name="channelName"></param>
    /// <returns></returns>
    public static string GetLogPath(this string channelName)
    {
        return GetLogBasePath().CombinePath(channelName).FileNameReplace();
    }

    /// <summary>
    /// 替换名称中不符合文件路径规则的字符为_
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static string FileNameReplace(this string fileName)
    {
        // 定义文件名称规则的正则表达式模式
        string pattern = @"[^a-zA-Z0-9_./\\-]";
        // 使用正则表达式将不符合规则的部分替换为下划线
        string sanitizedFileName = Regex.Replace(fileName, pattern, "_");
        return sanitizedFileName;
    }

    /// <summary>
    /// GetLogBasePath
    /// </summary>
    public static string GetLogBasePath()
    {
        return "wwwroot".CombinePath("logs");
    }

    /// <summary>
    /// GetDebugLogBasePath
    /// </summary>
    public static string GetDebugLogBasePath()
    {
        return "wwwroot".CombinePath("debugLogs");
    }

    #region 日志

    /// <summary>
    /// 输出错误日志
    /// </summary>
    public static void LogError(this ILog logger, Exception ex, string msg)
    {
        logger.Log(TouchSocket.Core.LogLevel.Error, null, msg, ex);
    }

    /// <summary>
    /// 输出错误日志
    /// </summary>
    public static void LogError(this ILog logger, Exception ex)
    {
        logger.Log(TouchSocket.Core.LogLevel.Error, null, ex.Message, ex);
    }

    /// <summary>
    /// 输出警示日志
    /// </summary>
    public static void LogWarning(this ILog logger, Exception ex, string msg)
    {
        logger.Log(TouchSocket.Core.LogLevel.Warning, null, msg, ex);
    }

    /// <summary>
    /// 输出警示日志
    /// </summary>
    public static void LogWarning(this ILog logger, Exception ex)
    {
        logger.Log(TouchSocket.Core.LogLevel.Warning, null, ex.Message, ex);
    }

    /// <summary>
    /// 输出警示日志
    /// </summary>
    public static void LogWarning(this ILog logger, string msg)
    {
        logger.Log(TouchSocket.Core.LogLevel.Warning, null, msg, null);
    }

    /// <summary>
    /// 输出Trace日志
    /// </summary>
    public static void LogTrace(this ILog logger, string msg)
    {
        logger.Log(TouchSocket.Core.LogLevel.Trace, null, msg, null);
    }

    /// <summary>
    /// 输出提示日志
    /// </summary>
    public static void LogInformation(this ILog logger, string msg)
    {
        logger.Log(TouchSocket.Core.LogLevel.Info, null, msg, null);
    }

    #endregion 日志
}