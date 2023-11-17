using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace VT2Lib.Tests;

internal static class TestOutputHelperExtensions
{
    private static MethodInfo? _queueTestOutputMethod;

    public static void Write(this ITestOutputHelper output, string message)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (output is TestOutputHelper outputImpl)
        {
            _queueTestOutputMethod ??= typeof(TestOutputHelper).GetMethod("QueueTestOutput", BindingFlags.NonPublic | BindingFlags.Instance)!;
            _queueTestOutputMethod.Invoke(outputImpl, new object[] { message });
        }
        else
        {
            // don't know the implementation, can't reflection it.
            output.WriteLine(message);
        }
    }
}