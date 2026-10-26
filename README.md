<div align="center">

[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![AGPL License][license-shield]][license-url]

</div>

<!-- PROJECT LOGO -->
<br />
<div align="center">
  <h1 align="center">AddPdfEnvelope</h1>

  <p align="center">
    A CLI tool for Windows and Linux to add a cover page, header and footer to a PDF document.
    <br />
    <a href="https://github.com/thgossler/AddPdfEnvelope/issues">Report Bug</a>
    ·
    <a href="https://github.com/thgossler/AddPdfEnvelope/issues">Request Feature</a>
    ·
    <a href="https://github.com/thgossler/AddPdfEnvelope#contributing">Contribute</a>
    ·
    <a href="https://github.com/sponsors/thgossler">Sponsor project</a>
  </p>
</div>


## About The Project

This is a simple .NET command line (CLI) tool for Windows and Linux which helps adding a cover page, a header and a footer to an existing PDF document. 

The tool uses the iText9 library for PDF processing which is licensed under AGPL for non-commercial use. Hence, this tool is also published under the AGPL license.

The command line interface is very simple as shown in the following picture. Here are also a few example command lines.

```bash
# Don't forget to make the file executable under Linux, e.g.
sudo chmod +x ./AddPdfEnvelope-linux-x64

./AddPdfEnvelope-linux-x64 --help
./AddPdfEnvelope-linux-x64 -f ../../../Test.pdf -o ../../../Test-result.pdf -y
```

[![AddPdfEnvelope screen shot][product-screenshot]]([https://github.com/thgossler/AddPdfEnvelope/])

The content of the added cover page, the header and the footer are configurable via `appsettings.json` file. The file may contain JSON comments (`// ...` and `/* ... */`) and trailing commas. Here is an example:

```jsonc
{
  "PdfEnvelope": {
    // Placeholders supported in all texts: {date} or {date:<.NET date format>}, e.g. {date:dd.MM.yyyy},
    // {pageNum}, {numOfPages}, any {<name>} defined in Variables; {sigNum} is only meaningful in SignTagPattern
    // User-defined values to avoid repeating texts; values may contain built-in placeholders like {date:yyyy}
    "Variables": {
      "product": "Product Name",
      "release": "2022",
      "feature": "Feature Name",
      "docType": "Architecture Specification",
      "version": "0.1",
      "department": "Department"
    },
    "CoverPage": {
      // Large bold text lines in the top-left corner of the cover page
      "Topic": "{product}",
      "Subtopic": "Release {release}",
      // Centered inside the main frame
      "Title": "{feature}",
      "Subtitle": "{docType}",
      // Bold text right-aligned next to the black bar above the main frame (empty = bar spans full width)
      "Organization": "{department}",
      // Centered text lines below the subtitle
      "Version": "Revision {version} (for review)",
      "Author": "Author: Surname, Given name",
      "Date": "Date: {date:dd.MM.yyyy}",
      // Framed box in the top-right corner (empty = no box)
      "Disclaimer": "This printed copy is not subject to any change control.",
      // Signature fields at the bottom of the main frame: 1-2 roles are full-width rows, 3-5 use two columns
      "ShowSignatureArea": true,
      // Adds an e-signature text tag to each signature field
      "AddSignTags": true,
      // {sigNum} is replaced with the sequential field number (1, 2, ...); the font shrinks to keep the tag on one line
      "SignTagPattern": "{{Sig{sigNum}_es_:signer{sigNum}:signatureblock}}",
      // None or up to 5 roles are supported (an empty array shows no signature fields)
      // Field label is "DocRole (ContentRole)"; ContentRole is optional
      "Roles": [
        { "DocRole": "Author", "ContentRole": "AR" },
        { "DocRole": "Approver", "ContentRole": "AR" }
      ]
    },
    // Line 1 is the top line and line 2 the line below it; empty texts are not shown
    "PageHeader": {
      "TextLeft1": "{product}, Release {release}",
      "TextLeft2": "{docType}",
      "TextCenter1": "",
      "TextCenter2": "",
      "TextRight1": "",
      "TextRight2": "{feature}, Revision {version}",
      // Line below the header text
      "DrawLine": true,
      // Don't show the header on the cover page
      "ExcludeCoverPage": true
    },
    // Line 1 is above line 2; on the cover page, a text containing {pageNum} is shown as "<numOfPages> pages"
    "PageFooter": {
      "TextLeft1": "",
      "TextLeft2": "{department}",
      "TextCenter1": "© My Company {date:yyyy}. All rights reserved.",
      "TextCenter2": "Restricted (for internal use only).",
      "TextRight1": "",
      "TextRight2": "Page {pageNum} of {numOfPages}",
      // Line above the footer text
      "DrawLine": true,
      // Don't show the footer on the cover page
      "ExcludeCoverPage": false
    },
    // Added to {pageNum} and {numOfPages}; -1 leaves the cover page uncounted so the first content page is page 1
    "PageNumberOffset": -1,
    // Removes comments and other annotations from all pages, but keeps hyperlinks
    "RemoveAnnotationsOtherThanLinks": true
  },
  // Currently not used by the tool
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

All texts support the built-in placeholders `{date}` or `{date:<format>}` (using [.NET date and time format strings](https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings)), `{pageNum}` and `{numOfPages}`. In addition, each entry in the optional `Variables` section can be referenced as `{<name>}` (case-insensitive) in any text, so that values like a version number are defined only once. Variable values may contain built-in placeholders but no other variables. Unknown placeholders are kept as-is and reported as a warning.

When `ShowSignatureArea` is enabled, one signature field is drawn for each entry in `Roles` (up to 5; additional entries are ignored with a warning). Each field is labeled `DocRole (ContentRole)`, or only `DocRole` if `ContentRole` is empty. One or two fields are stacked as full-width rows; three to five fields are arranged in two columns, with an odd last field spanning the full width. An empty `Roles` array shows no signature fields.

When `AddSignTags` is enabled, `SignTagPattern` is added to each signature field and each application of the pattern receives the next signature number. Multiple `{sigNum}` placeholders within one pattern application use the same number, while separate applications receive distinct numbers. The font size of a sign tag is reduced if necessary so that it fits on a single line within its field.

Configuration files using the previous keys `AddAdobeSignTags`, `AdobeSignTagPattern`, `RoleAuthor` and `RoleApprover` continue to work: if `Roles` is not configured, the two fields `Author` and `Approver` are shown using the legacy role keys, and the legacy tag keys are used if `AddSignTags` or `SignTagPattern` are not configured.

The result looks like that:

[![AddPdfEnvelope screen shot][result-screenshot]]([https://github.com/thgossler/AddPdfEnvelope/])


### Built With

* [.NET 10 (C#)](https://dotnet.microsoft.com/en-us/)
* [System.CommandLine](https://github.com/dotnet/command-line-api)
* [iText for .NET](https://github.com/itext/itext-dotnet)


## Getting Started

### Prerequisites

* Latest .NET 10 SDK


### Installation as Tool for Use

1. Download the self-contained single-file executables from the [releases](https://github.com/thgossler/AddPdfEnvelope/releases) section

2. Copy it to a location where you can easily call it and rename it as desired (e.g. to `AddPdfEnvelope`)

3. Open a command prompt or PowerShell and type `AddPdfEnvelope --help`


### Installation from Source for Development

1. Clone the repo
   ```sh
   git clone https://github.com/thgossler/AddPdfEnvelope.git
   ```
2. Build
   ```sh
   dotnet build
   ```
3. Run without arguments to get help
   ```sh
   dotnet run
   ```

Alternatively, you can open the folder in [VS Code](https://code.visualstudio.com/) or the solution (.sln file) in the [Microsoft Visual Studio IDE](https://visualstudio.microsoft.com/vs/) and press F5.


### Versioning and Releases

The `<Version>` property in `AddPdfEnvelope.csproj` is the source of truth for release versions. Use the cross-platform PowerShell script to update the project version and its related version properties:

```powershell
pwsh ./bump-version.ps1 -Patch
pwsh ./bump-version.ps1 -Minor
pwsh ./bump-version.ps1 -Major
```

Push a tag matching the updated version, such as `v0.6.2`, to build the six self-contained platform packages and publish them as a GitHub release:

```bash
git tag v0.6.2
git push origin v0.6.2
```


## Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".
Don't forget to give the project a star :wink: Thanks!

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request


## License

Distributed under the AGPL License. See [`LICENSE`](https://github.com/thgossler/AddPdfEnvelope/blob/main/LICENSE) for more information.



<!-- MARKDOWN LINKS & IMAGES (https://www.markdownguide.org/basic-syntax/#reference-style-links) -->
[contributors-shield]: https://img.shields.io/github/contributors/thgossler/AddPdfEnvelope.svg
[contributors-url]: https://github.com/thgossler/AddPdfEnvelope/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/thgossler/AddPdfEnvelope.svg
[forks-url]: https://github.com/thgossler/AddPdfEnvelope/network/members
[stars-shield]: https://img.shields.io/github/stars/thgossler/AddPdfEnvelope.svg
[stars-url]: https://github.com/thgossler/AddPdfEnvelope/stargazers
[issues-shield]: https://img.shields.io/github/issues/thgossler/AddPdfEnvelope.svg
[issues-url]: https://github.com/thgossler/AddPdfEnvelope/issues
[license-shield]: https://img.shields.io/github/license/thgossler/AddPdfEnvelope.svg
[license-url]: https://github.com/thgossler/AddPdfEnvelope/blob/master/LICENSE.txt
[product-screenshot]: images/screenshot.png
[result-screenshot]: images/screenshot2.png
