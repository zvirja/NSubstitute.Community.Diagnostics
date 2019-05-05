using System;
using Xunit;
using Xunit.Abstractions;

namespace NSubstitute.Community.Diagnostics.Tests.Documentation
{
    public class LoggingSamples
    {
        [Fact]
        public void WritingToConsoleLoggingSample()
        {
            using (NSubstituteDiagnosticsContext.InstallLogging(Console.WriteLine))
            {
                var substitute = Substitute.For<ISut>();
                substitute.Echo(42).Returns(42);
            }
        }

        [Fact]
        public void WritingToConsoleTracingSample()
        {
            using (NSubstituteDiagnosticsContext.InstallTracing(Console.WriteLine))
            {
                var substitute = Substitute.For<ISut>();
                substitute.Echo(42).Returns(42);
            }
        }

        public class xUnitSample
        {
            private readonly ITestOutputHelper _output;
            public xUnitSample(ITestOutputHelper output) => _output = output;

            [Fact]
            public void DiagnosticsSample()
            {
                using (NSubstituteDiagnosticsContext.InstallTracing(_output.WriteLine))
                {
                    var substitute = Substitute.For<ISut>();
                    substitute.Echo(42).Returns(42);
                }
            }
        }
    }
}