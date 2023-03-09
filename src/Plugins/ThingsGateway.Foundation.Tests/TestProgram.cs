using Furion.Xunit;

using Xunit;
using Xunit.Abstractions;

[assembly: TestFramework("ThingsGateway.Foundation.Tests.TestProgram", "ThingsGateway.Foundation.Tests")]
namespace ThingsGateway.Foundation.Tests;

/// <summary>
/// ��Ԫ����������
/// </summary>

public class TestProgram : TestStartup
{
    public TestProgram(IMessageSink messageSink) : base(messageSink)
    {
        // ��ʼ�� Furion
        Serve.Run(silence: true);
    }
}