# NSubstitute.Community.Diagnostics

A tool to quickly identify the NSubstitute related issues. It overrides the most critical parts of NSubstitute to log actions which happen. Later those logs might be used to quickly figure out the misbehaving reason.

# Installation and usage

Install the `NSubstitute.Community.Diagnostics` NuGet package to your test project. After that wrap your test code with the diagnostics context and run it:

```c#
using (new NSubstituteDiagnosticsContext(Console.WriteLine))
{
    var substitute = Substitute.For<ISut>();
    substitute.Echo(42).Returns(42);
}

```

Examine test output to get hints on the issue you have (or e.g. capture it and attach to the reported issue).

## xUnit

xUnit doesn't capture `Console.WriteLine` and you are required to use the `ITestOutputHelper` instance:

```c#
public class xUnitSample
{
    private readonly ITestOutputHelper _output;
    public xUnitSample(ITestOutputHelper output) => _output = output;

    [Fact]
    public void DiagnosticsSample()
    {
        using (new NSubstituteDiagnosticsContext(_output.WriteLine))
        {
            var substitute = Substitute.For<ISut>();
            substitute.Echo(42).Returns(42);
        }
    }
}
```

## Concurrency

Due to the nature of NSubstitute core the diagnostics context is global and affects all the threads while installed. In practice it means that you cannot install/uninstall the diagnostics concurrently in different tests, as it would lead to weird behavior.

If you need to troubleshoot a few tests or the whole test suite (e.g. issue happens only when all the tests are run concurrently), you can install the context once before any test run and log all the entries to the log file (don't forget to flush :blush:). In that case you might want to omit diagnostics context disposal, as no code is expected to run afterwards.

Notice, the concurrency limitation is related to the installation only - the code you run while the context is installed might be fully concurrent.

# Sample

Here is the sample of diagnostics code and output.

Code:
```c#
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
            sut.Echo(42).Returns(42);
        }
    }
}
```

Output:
```
[TID:13][DiagnosticsContextInstaller] Installed diagnostics context
[TID:13][ProxyFactory] GenerateProxy(callRouter: <CallRouter|015c71b8>, typeToProxy: Samples.IssueSample+ISut, additionalInterfaces: [], constructorArguments: []) => Substitute.ISut|02210f78
[TID:13][ThreadLocalContext] DequeueAllArgumentSpecifications() => []
[TID:13][CallRouter] Route(call: <Substitute.ISut|02210f78.Echo(Int32|42) : Echo(Int32) -> Int32>) [this: <CallRouter|015c71b8#Substitute.ISut|02210f78>]
[TID:13][ThreadLocalContext] SetLastCallRouter(callRouter: <CallRouter|015c71b8#Substitute.ISut|02210f78>)
[TID:13][ThreadLocalContext] UsePendingRaisingEventArgumentsFactory() => <null>
[TID:13][ThreadLocalContext] UseNextRoute(callRouter: <CallRouter|015c71b8#Substitute.ISut|02210f78>) => <null>
[TID:13][ThreadLocalContext.PendingSpecification] Clear()
[TID:13][ThreadLocalContext.PendingSpecification] SetLastCall(call: <Substitute.ISut|02210f78.Echo(Int32|42) : Echo(Int32) -> Int32>)
[TID:13][ThreadLocalContext] LastCallShouldReturn(value: Int32|42, matchArgs: <as specified>)
[TID:13][CallRouter] LastCallShouldReturn(value: Int32|42, matchArgs: <as specified>, pendingSpecInfo: <CALL|<Substitute.ISut|02210f78.Echo(Int32|42) : Echo(Int32) -> Int32>>) [this: <CallRouter|015c71b8#Substitute.ISut|02210f78>]
[TID:13][DiagnosticsContextInstaller] Restored normal context
```

As you can see, the output logs all the major events happened to the NSubstitute. Now depending on the issue, either brief or precise look is required to figure out what's going wrong :wink:
