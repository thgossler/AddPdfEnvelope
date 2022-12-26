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
}

public class CoverPage
{
    public string Topic { get; set; }
    public string Subtopic { get; set; }
    public string Title { get; set; }
    public string Subtitle { get; set; }
    public string Organization { get; set; }
    public string Version { get; set; }
    public string Author { get; set; }
    public string Date { get; set; }
    public string Disclaimer { get; set; }
}

public class PageHeader
{
    public string TextLeft1 { get; set; }
    public string TextLeft2 { get; set; }
    public string TextCenter1 { get; set; }
    public string TextCenter2 { get; set; }
    public string TextRight1 { get; set; }
    public string TextRight2 { get; set; }
    public bool DrawLine { get; set; }
    public bool ExcludeCoverPage { get; set; }
}

public class PageFooter
{
    public string TextLeft1 { get; set; }
    public string TextLeft2 { get; set; }
    public string TextCenter1 { get; set; }
    public string TextCenter2 { get; set; }
    public string TextRight1 { get; set; }
    public string TextRight2 { get; set; }
    public bool DrawLine { get; set; }
    public bool ExcludeCoverPage { get; set; }
}
