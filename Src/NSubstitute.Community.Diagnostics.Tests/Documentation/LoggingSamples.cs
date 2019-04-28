using System;
using Samples;
using Xunit;
using Xunit.Abstractions;

namespace NSubstitute.Community.Diagnostics.Tests.Documentation
{
    public class LoggingSamples
    {
        public void WritingToConsoleLoggingSample()
        {
            using (NSubstituteDiagnosticsContext.InstallLoggingContext(Console.WriteLine))
            {
                var substitute = Substitute.For<ISut>();
                substitute.Echo(42).Returns(42);
            }
        }

        public void WritingToConsoleTracingSample()
        {
            using (NSubstituteDiagnosticsContext.InstallTracingContext(Console.WriteLine))
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
                using (NSubstituteDiagnosticsContext.InstallTracingContext(_output.WriteLine))
                {
                    var substitute = Substitute.For<ISut>();
                    substitute.Echo(42).Returns(42);
                }
            }
        }
    }
}