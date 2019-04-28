using Samples;
using Xunit;

namespace NSubstitute.Community.Diagnostics.Tests.Documentation
{
    public class ExceptionDiagnosticSamples
    {
        [Fact]
        public void RunTests()
        {
            using (NSubstituteExceptionDiagnostics.Install())
            {
                CorruptedTest();
                TestToTroubleshoot();
            }
        }

        private void TestToTroubleshoot()
        {
            var sut = Substitute.For<ISut>();
            sut.Echo(Arg.Any<int>()).Returns(42);
        }

        private void CorruptedTest()
        {
            Arg.Any<int>();
        }
    }
}