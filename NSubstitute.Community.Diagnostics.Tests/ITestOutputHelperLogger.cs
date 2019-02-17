using Xunit.Abstractions;

namespace NSubstitute.Community.Diagnostics.Tests
{
    public class TestOutputHelperLogger : IDiagnosticsTracer
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public TestOutputHelperLogger(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        public void WriteLine(string line) => _testOutputHelper.WriteLine(line);
    }
}