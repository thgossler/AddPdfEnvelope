namespace AddPdfEnvelope;

public static class Globals
{
    public static int CurrentSigNum = 1;
    public static Dictionary<string, string> Variables = new(StringComparer.OrdinalIgnoreCase);
    public static HashSet<string> ReportedUnknownPlaceholders = new(StringComparer.Ordinal);
}

