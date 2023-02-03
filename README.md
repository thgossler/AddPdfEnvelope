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

The tool uses the iText7 library for PDF processing which is licensed under AGPL for non-commercial use. Hence, this tool is also published under the AGPL license.

The command line interface is very simple as shown in the following picture. Here are also a few example command lines.

```bash
# Don't forget to make the file executable under Linux, e.g.
sudo chmod +x ./AddPdfEnvelope-linux-x64

./AddPdfEnvelope-linux-x64 --help
./AddPdfEnvelope-linux-x64 -f ../../../Test.pdf -o ../../../Test-result.pdf -y
```

[![AddPdfEnvelope screen shot][product-screenshot]]([https://github.com/thgossler/AddPdfEnvelope/])

The content of the added cover page, the header and the footer are configurable via `appsettings.json` file. Here is an example:

```json
{
  "PdfEnvelope": {
    "CoverPage": {
      "Topic": "Product Name",
      "Subtopic": "Version 2022",
      "Title": "Feature Name",
      "Subtitle": "Architecture Specification",
      "Organization": "Department",
      "Version": "Revision 0.1 (for review)",
      "Author": "Author: Surname, Given name",
      "Date": "Date: {date:dd.MM.yyyy}",
      "Disclaimer": "This printed copy is not subject to any change control.",
      "ShowSignatureArea": true
    },
    "PageHeader": {
      "TextLeft1": "Product Name, Version 2022",
      "TextLeft2": "Architecture Specification",
      "TextCenter1": "",
      "TextCenter2": "",
      "TextRight1": "",
      "TextRight2": "Feature Name, Revision 0.1",
      "DrawLine": true,
      "ExcludeCoverPage": true
    },
    "PageFooter": {
      "TextLeft1": "",
      "TextLeft2": "Department",
      "TextCenter1": "© My Company {date:yyyy}. All rights reserved.",
      "TextCenter2": "Restricted (for internal use only).",
      "TextRight1": "",
      "TextRight2": "Page {pageNum} of {numOfPages}",
      "DrawLine": true,
      "ExcludeCoverPage": false
    },
    "PageNumberOffset": -1,
    "RemoveAnnotationsOtherThanLinks": true
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

The result looks like that:

[![AddPdfEnvelope screen shot][result-screenshot]]([https://github.com/thgossler/AddPdfEnvelope/])


### Built With

* [.NET 7 (C#)](https://dotnet.microsoft.com/en-us/)
* [System.CommandLine](https://github.com/dotnet/command-line-api)
* [iText7 for .NET](https://github.com/itext/itext7-dotnet)


## Getting Started

### Prerequisites

* Latest .NET SDK


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
