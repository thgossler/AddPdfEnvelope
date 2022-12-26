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

This is a simple .NET command line (CLI) tool for Windows and Linux to add a cover page, header and footer to a PDF document. The tool uses the iText7 library for PDF processing which is licensed under AGPL for non-commercial use. Hence, this tool is also published under the AGPL license.

[![ReplacePdfHyperlinks screen shot][product-screenshot]]([https://github.com/thgossler/AddPdfEnvelope/])

> _**Note:** This tool was written by me in my spare time and will be developed only sporadically._


### Built With

* [.NET 7 (C#)](https://dotnet.microsoft.com/en-us/)
* [System.CommandLine](https://github.com/dotnet/command-line-api)
* [iText7 for .NET](https://github.com/itext/itext7-dotnet)


## Getting Started

### Prerequisites

* Latest .NET SDK
  ```sh
  winget install -e --id Microsoft.dotnet
  ```


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
[license-url]: https://github.com/thgossler/AddPdfEnvelope/blob/main/LICENSE
[product-screenshot]: images/screenshot.png
