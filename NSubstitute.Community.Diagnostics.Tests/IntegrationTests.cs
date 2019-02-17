using System;
using System.Runtime.CompilerServices;
using System.Transactions;
using NSubstitute.ClearExtensions;
using NSubstitute.Extensions;
using NSubstitute.ReturnsExtensions;
using Xunit;
using Xunit.Abstractions;

namespace NSubstitute.Community.Diagnostics.Tests
{
    public class IntegrationTests
    {
        private IDiagnosticsTracer _output;

        public IntegrationTests(ITestOutputHelper output)
        {
            _output = new TestOutputHelperLogger(output);
        }

        [Fact]
        public void Test()
        {
            using (new NSubstituteDiagnosticsContext(_output))
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