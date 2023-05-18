using Microsoft.FSharp.Core;

namespace PomodoroWindowsTimer.WpfClient.Extensions;

public static class StringExtensions
{
    public static FSharpOption<string> ToFSharpOption(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return FSharpOption<string>.None;
        }

        return FSharpOption<string>.Some(value);
    }
}
