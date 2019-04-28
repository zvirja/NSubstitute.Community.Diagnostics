using NSubstitute.ClearExtensions;
using NSubstitute.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace NSubstitute.Community.Diagnostics.Tests
{
    public class IntegrationTests
    {
        private readonly ITestOutputHelper _output;

        public IntegrationTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Test()
        {
            using (NSubstituteDiagnosticsContext.InstallTracingContext(_output.WriteLine))
            {
                ClearOptions xx = ClearOptions.All;
                xx.ToString();

                var subs = Substitute.For<ITargetInterface>();
                var res = Substitute.For<ITargetInterface>();
                subs.Configure().GetSelf(Arg.Any<long>()).Returns(res);

                subs.ClearSubstitute(ClearOptions.ReturnValues | ClearOptions.ReceivedCalls);

                var result = subs.Echo("42");
            }
        }
    }
}