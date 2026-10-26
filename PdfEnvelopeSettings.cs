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

    public Dictionary<string, string?> Variables { get; set; } = new();

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
    public bool? AddSignTags { get; set; }
    public string? SignTagPattern { get; set; }
    public List<SignatureRole>? Roles { get; set; }

    // Legacy keys, only honored when the corresponding generic keys above are not configured
    public bool AddAdobeSignTags { get; set; } = false;
    public string AdobeSignTagPattern { get; set; } = "";
    public string? RoleAuthor { get; set; }
    public string? RoleApprover { get; set; }

    public const int MaxSignatureFields = 5;
    public const string DefaultSignTagPattern = "{{SigB_es_:signer{sigNum}:signatureblock}}";

    public bool ShouldAddSignTags() => AddSignTags ?? AddAdobeSignTags;

    public string GetSignTagPattern()
    {
        if (!string.IsNullOrWhiteSpace(SignTagPattern)) return SignTagPattern;
        if (!string.IsNullOrWhiteSpace(AdobeSignTagPattern)) return AdobeSignTagPattern;
        return DefaultSignTagPattern;
    }

    /// <summary>
    /// Returns the configured signature roles. If <see cref="Roles"/> is not configured, the legacy
    /// fixed two-role configuration is mapped to the generic representation.
    /// </summary>
    public IReadOnlyList<SignatureRole> GetSignatureRoles(bool rolesKeyPresent)
    {
        if (Roles != null && Roles.Count > 0) return Roles;
        if (rolesKeyPresent) return [];
        return [
            new SignatureRole { DocRole = "Author", ContentRole = RoleAuthor ?? "AR" },
            new SignatureRole { DocRole = "Approver", ContentRole = RoleApprover ?? "AR" }
        ];
    }
}

public class SignatureRole
{
    public string DocRole { get; set; } = string.Empty;
    public string ContentRole { get; set; } = string.Empty;

    public string GetLabel()
    {
        var docRole = DocRole?.Trim() ?? string.Empty;
        var contentRole = ContentRole?.Trim() ?? string.Empty;
        if (docRole.Length == 0) return contentRole;
        return contentRole.Length == 0 ? docRole : $"{docRole} ({contentRole})";
    }
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
