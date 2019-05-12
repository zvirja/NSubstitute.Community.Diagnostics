[![Build status](https://ci.appveyor.com/api/projects/status/7xumirowuxo3hfrv/branch/master?svg=true)](https://ci.appveyor.com/project/Zvirja/nsubstitute-community-diagnostics/branch/master) [![NuGet version](https://img.shields.io/nuget/vpre/NSubstitute.Community.Diagnostics.svg)](https://www.nuget.org/packages/NSubstitute.Community.Diagnostics)

# NSubstitute.Community.Diagnostics

A tool to quickly identify the NSubstitute related issues. It overrides the most critical parts of NSubstitute to log most important actions. Later log might be used to quickly figure out the reason of misbehavior.

# Installation and usage

To use any feature install the `NSubstitute.Community.Diagnostics` NuGet package to your test project first.

## NSubstitute exceptions diagnostics

This diagnostics allows to retrieve more context about NSubstitute exception. When installed, it intercepts all NSubstitute related exceptions and appends NSubstitute log to them, which allows to troubleshoot issues easier.

Ideally, you should install hook before any test run. NUnit and MSTest seem to support this feature natively. For xUnit you might want to use [Module Initializers feature](http://einaregilsson.com/module-initializers-in-csharp/) via [InjectModuleInitializer](https://github.com/kzu/InjectModuleInitializer) or [Fody ModuleInit](https://github.com/fody/moduleinit).

Run the following code to install the hook:

```c#
NSubstituteExceptionDiagnostics.Install();

```

Later whever NSubstitute exception occur, look for the `**  NSubstitute LOG (reversed)  **` section in the exception message for more clues.

[See output sample.](#nsubstitute-exception-message-with-enabled-exceptions-diagnostics)

##  NSubstitute logging

This diagnostics allows you to install global hook to monitor all the important NSubstitute library activity. You might use either logging or tracing mode, depending on amount of details you require.

To log NSubstitute wrap your test code with the diagnostics context:

```c#
using (NSubstituteDiagnosticsContext.InstallLogging(Console.WriteLine))
{
    var substitute = Substitute.For<ISut>();
    substitute.Echo(42).Returns(42);
}
```

To get more detailed output use tracing mode:

```c#
using (NSubstituteDiagnosticsContext.InstallTracing(Console.WriteLine))
{
    var substitute = Substitute.For<ISut>();
    substitute.Echo(42).Returns(42);
}
```

Then examine test output to get hints on the issue you have (or e.g. capture it and attach to the reported issue).

### xUnit

xUnit doesn't capture `Console.WriteLine` so use the `ITestOutputHelper` instead:

```c#
public class xUnitSample
{
    private readonly ITestOutputHelper _output;
    public xUnitSample(ITestOutputHelper output) => _output = output;

    [Fact]
    public void DiagnosticsSample()
    {
        using (NSubstituteDiagnosticsContext.InstallLogging(_output.WriteLine))
        {
            var substitute = Substitute.For<ISut>();
            substitute.Echo(42).Returns(42);
        }
    }
}
```

# Concurrency

Due to the nature of NSubstitute core the diagnostics context is global and affects all the threads while installed. In practice it means that you cannot install/uninstall the diagnostics concurrently in different tests, as it would lead to weird behavior.

If you need to troubleshoot a few tests or the whole test suite (e.g. issue happens only when all the tests are run concurrently), you can install the context once before any test run and log all the entries to the log file (don't forget to flush :blush:). In that case you might want to omit diagnostics context disposal, as no code is expected to run afterwards.

Notice, the concurrency limitation is related to the installation only - the code you run while the context is installed might be fully concurrent.

# Samples

## NSubstitute exception message with enabled exceptions diagnostics

```
NSubstitute.Exceptions.RedundantArgumentMatcherException
Some argument specifications (e.g. Arg.Is, Arg.Any) were left over after the last call.

... trimmed

Diagnostic information:

Remaining (non-bound) argument specifications:
    any Int32

All argument specifications:
    any Int32
    any Int32

************************************************************************
*********************  NSubstitute LOG (reversed)  *********************
************************************************************************

[Caller: ExceptionDiagnosticsTests.TestToTroubleshoot][Received call] Substitute: Substitute.ISut|00714c06 Call: Echo(Int32|0) Signature: Echo(Int32) -> Int32 Argument specifications: [<any Int32>, <any Int32>]
[Caller: ExceptionDiagnosticsTests.TestToTroubleshoot][Enqueue argument specification] Specification: <any Int32> 
[Caller: ExceptionDiagnosticsTests.TestToTroubleshoot][ProxyFactory] GenerateProxy(callRouter: <CallRouter|01af8d3f>, typeToProxy: Samples.ISut, additionalInterfaces: [], constructorArguments: []) => Substitute.ISut|00714c06
[Caller: ExceptionDiagnosticsTests.CorruptedTest][Enqueue argument specification] Specification: <any Int32> 
[Caller: ExceptionDiagnosticsTests.RunTests][DiagnosticsContextInstaller] Installed diagnostics context

************************************************************************
************************************************************************
```

## Tracing sample

Code:

```c#
public class xUnitSample
{
    private readonly ITestOutputHelper _output;
    public xUnitSample(ITestOutputHelper output) => _output = output;

    [Fact]
    public void DiagnosticsSample()
    {
        using (NSubstituteDiagnosticsContext.InstallLogging(_output.WriteLine))
        {
            var substitute = Substitute.For<ISut>();
            substitute.Echo(42).Returns(42);
        }
    }
}
```

Output:

```
[TID:15][Caller: xUnitSample.DiagnosticsSample][DiagnosticsContextInstaller] Installed diagnostics context
[TID:15][Caller: xUnitSample.DiagnosticsSample][ProxyFactory] GenerateProxy(callRouter: <CallRouter|03cc973e>, typeToProxy: Samples.ISut, additionalInterfaces: [], constructorArguments: []) => Substitute.ISut|000ef682
[TID:15][Caller: xUnitSample.DiagnosticsSample][ThreadLocalContext] DequeueAllArgumentSpecifications() => []
[TID:15][Caller: xUnitSample.DiagnosticsSample][CallRouter] Route(call: <[Substitute.ISut|000ef682].Echo(Int32|42) Signature: Echo(Int32) -> Int32>) [this: <CallRouter|03cc973e#Substitute.ISut|000ef682>]
    [TID:15][Caller: xUnitSample.DiagnosticsSample][ThreadLocalContext] SetLastCallRouter(callRouter: <CallRouter|03cc973e#Substitute.ISut|000ef682>)
    [TID:15][Caller: xUnitSample.DiagnosticsSample][ThreadLocalContext] UsePendingRaisingEventArgumentsFactory() => <null>
    [TID:15][Caller: xUnitSample.DiagnosticsSample][ThreadLocalContext] UseNextRoute(callRouter: <CallRouter|03cc973e#Substitute.ISut|000ef682>) => <null>
    [TID:15][Caller: xUnitSample.DiagnosticsSample][ThreadLocalContext.PendingSpecification] Clear()
    [TID:15][Caller: xUnitSample.DiagnosticsSample][ThreadLocalContext.PendingSpecification] SetLastCall(call: <[Substitute.ISut|000ef682].Echo(Int32|42) Signature: Echo(Int32) -> Int32>)
[TID:15][Caller: xUnitSample.DiagnosticsSample][ThreadLocalContext] LastCallShouldReturn(value: Int32|42, matchArgs: <as specified>)
[TID:15][Caller: xUnitSample.DiagnosticsSample][CallRouter] LastCallShouldReturn(value: Int32|42, matchArgs: <as specified>, pendingSpecInfo: <CALL|<[Substitute.ISut|000ef682].Echo(Int32|42) Signature: Echo(Int32) -> Int32>>) [this: <CallRouter|03cc973e#Substitute.ISut|000ef682>]
[TID:15][Caller: xUnitSample.DiagnosticsSample][DiagnosticsContextInstaller] Restored normal context
```

As you can see, the output logs all the major events happened to the NSubstitute. Now depending on the issue, either brief or precise look might be required to figure out what's going wrong.
