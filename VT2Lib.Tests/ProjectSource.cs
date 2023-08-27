using System.Runtime.CompilerServices;

namespace VT2Lib.Tests;

// Because I really don't want to copy gigabytes of data to my output directory, thank you very much.
// https://stackoverflow.com/a/75577224/
internal static class ProjectSource
{
    public static string ProjectDirectory => Path.GetDirectoryName(CallerFilePath())!;

    private static string CallerFilePath([CallerFilePath] string? callerFilePath = null)
    {
        return callerFilePath ?? throw new ArgumentNullException(nameof(callerFilePath));
    }
}