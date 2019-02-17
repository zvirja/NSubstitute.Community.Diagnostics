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

        public interface ISut
        {
            int Echo(int msg);
        }

        [Fact]
        public void IssueDemo()
        {
            using (new NSubstituteDiagnosticsContext(_output.WriteLine))
            {
                var sut = Substitute.For<ISut>();
                sut.Configure().Echo(42).Returns(42);
            }
        }
    }
}