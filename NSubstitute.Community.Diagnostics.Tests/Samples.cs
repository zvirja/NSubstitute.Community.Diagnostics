using NSubstitute;
using NSubstitute.Community.Diagnostics;
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
            using (NSubstituteDiagnosticsContext.InstallLoggingContext(_output.WriteLine))
            {
                var sut = Substitute.For<ISut>();
                sut.Configure().Echo(Arg.Any<int>()).Returns(42);
                //sut.Echo(42).Returns(42);
                sut.Echo((byte)42);

                sut.Prop = 42;
            }
        }
    }
}