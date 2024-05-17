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

    public static string? FromFSharpOption(this FSharpOption<string> value)
    {
        if (FSharpOption<string>.get_IsNone(value))
        {
            return null;
        }

        return value.Value;
    }
}
