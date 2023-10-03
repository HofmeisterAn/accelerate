namespace Accelerate.Extensions;

public static class StringExtension
{
    public static string EscapeLineEndings(this string value)
    {
        return value.ReplaceLineEndings(Regex.Escape(Environment.NewLine));
    }

    public static string RemoveInvalidChars(this string value)
    {
        return string.Concat(value.Split(Path.GetInvalidPathChars()));
    }
}