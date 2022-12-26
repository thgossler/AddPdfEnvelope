// ------------------------------------------------------------------------
// Copyright 2022 Thomas Gossler
// This file is licensed under the MIT license.
// ------------------------------------------------------------------------

using System.Text;

/// <summary>
/// Helper functions to parse a command line string into a command line argument array in
/// the same way as .NET does. Tested with .NET 6 on Windows and Ubuntu Linux.
/// </summary>
internal static class CommandLine
{
    public static string[] Parse(string commandLine)
    {
        return Split(commandLine)
            .Select(arg => arg.Trim().TrimMatchingQuotes('\"'))
            .Select(arg => arg.Trim().TrimMatchingQuotes('\''))
            .ToArray();
    }

    public static IEnumerable<string> Split(string commandLine)
    {
        var result = new StringBuilder();

        var quoted = false;
        var escaped = false;
        var started = false;
        var allowcaret = false;
        for (int i = 0; i < commandLine.Length; i++) {
            var chr = commandLine[i];

            if (chr == '^' && !quoted) {
                if (allowcaret) {
                    result.Append(chr);
                    started = true;
                    escaped = false;
                    allowcaret = false;
                }
                else if (i + 1 < commandLine.Length && commandLine[i + 1] == '^') {
                    allowcaret = true;
                }
                else if (i + 1 == commandLine.Length) {
                    result.Append(chr);
                    started = true;
                    escaped = false;
                }
            }
            else if (escaped) {
                result.Append(chr);
                started = true;
                escaped = false;
            }
            else if (chr == '"') {
                quoted = !quoted;
                started = true;
            }
            else if (chr == '\\' && i + 1 < commandLine.Length && commandLine[i + 1] == '"') {
                escaped = true;
            }
            else if (chr == ' ' && !quoted) {
                if (started) {
                    yield return result.ToString();
                }
                result.Clear();
                started = false;
            }
            else {
                result.Append(chr);
                started = true;
            }
        }

        if (started) {
            yield return result.ToString();
        }
    }

    public static string TrimMatchingQuotes(this string input, char quote)
    {
        if ((input.Length >= 2) && (input[0] == quote) && (input[input.Length - 1] == quote)) {
            return input.Substring(1, input.Length - 2);
        }
        return input;
    }
}
