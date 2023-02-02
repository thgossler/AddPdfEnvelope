public class PdfEnvelopeSettings
{
    public PdfEnvelopeSettings()
    {
        CoverPage = new CoverPage();
        PageHeader = new PageHeader();
        PageFooter = new PageFooter();
    }

    public CoverPage CoverPage { get; set; }
    public PageHeader PageHeader { get; set; }
    public PageFooter PageFooter { get; set; }

    public int PageNumberOffset { get; set; } = -1;

    public bool RemoveAnnotationsOtherThanLinks { get; set; }
}

public class CoverPage
{
    public string Topic { get; set; } = string.Empty;
    public string Subtopic { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Disclaimer { get; set; } = string.Empty;
    public bool ShowSignatureArea { get; set; } = false;
}

public class PageHeader
{
    public string TextLeft1 { get; set; } = string.Empty;
    public string TextLeft2 { get; set; } = string.Empty;
    public string TextCenter1 { get; set; } = string.Empty;
    public string TextCenter2 { get; set; } = string.Empty;
    public string TextRight1 { get; set; } = string.Empty;
    public string TextRight2 { get; set; } = string.Empty;
    public bool DrawLine { get; set; }
    public bool ExcludeCoverPage { get; set; }
}

public class PageFooter
{
    public string TextLeft1 { get; set; } = string.Empty;
    public string TextLeft2 { get; set; } = string.Empty;
    public string TextCenter1 { get; set; } = string.Empty;
    public string TextCenter2 { get; set; } = string.Empty;
    public string TextRight1 { get; set; } = string.Empty;
    public string TextRight2 { get; set; } = string.Empty;
    public bool DrawLine { get; set; }
    public bool ExcludeCoverPage { get; set; }
}
