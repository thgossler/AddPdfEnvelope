using System.Text.RegularExpressions;

namespace AddPdfEnvelope;
internal static class RegExs
{
    public static Regex PlaceholderRegEx = new Regex(@"{[^{}]+?}", RegexOptions.Compiled);
}
