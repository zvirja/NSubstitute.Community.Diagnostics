using NSubstitute;
using NSubstitute.ClearExtensions;
using NSubstitute.Community.Diagnostics;
using NSubstitute.Community.Diagnostics.Tests;
using NSubstitute.Extensions;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable once CheckNamespace
namespace Samples
{
    public class IssueSample
    {
        private readonly ITestOutputHelper _output;
        public IssueSample(ITestOutputHelper output) => _output = output;

        [Fact]
        public void IssueDemo()
        {
//            using (NSubstituteDiagnosticsContext.CreateTracingContext(_output.WriteLine))
            using (NSubstituteDiagnosticsContext.InstallLogging(_output.WriteLine))
            {
                var sut = Substitute.For<ISut>();
                sut.Configure().Echo(Arg.Any<int>()).Returns(42);
                //sut.Echo(42).Returns(42);
                sut.Echo((byte)42);

                sut.Prop = 42;
            }
        }
        
        [Fact]
        public void ComplexScenario()
        {
            using (NSubstituteDiagnosticsContext.InstallTracing(_output.WriteLine))
            {
                ClearOptions xx = ClearOptions.All;
                xx.ToString();

                var subs = Substitute.For<ISut>();
                var res = Substitute.For<ISut>();
                subs.Configure().GetSelf(Arg.Any<long>()).Returns(res);

                subs.ClearSubstitute(ClearOptions.ReturnValues | ClearOptions.ReceivedCalls);

                var result = subs.Echo(42);
            }
        }
    }
}