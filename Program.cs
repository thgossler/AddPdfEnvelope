// ------------------------------------------------------------------------
// Copyright 2022 Thomas Gossler
// Licensed under the AGPL license, Version 3 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//     https://www.gnu.org/licenses/#AGPL
// ------------------------------------------------------------------------

using System.CommandLine;
using System.Text.Json;
using AddPdfEnvelope;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Utils;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.Extensions.Configuration;

#region Workaround for commandLineArgs in WSL launch profile

// Workaround for not supported commandLineArgs in WSL launch profile with Visual Studio remote debugging

#if DEBUG
if (OperatingSystem.IsOSPlatform("Linux") && args.Length == 0 && Environment.CommandLine.Contains(".dll")) {
    var launchSettingsFile = Environment.CurrentDirectory.Split("/bin")[0] + "/Properties/launchSettings.json";
    var launchSettingsString = File.ReadAllText(launchSettingsFile);
    var launchSettings = JsonDocument.Parse(launchSettingsString);
    var commandLine = launchSettings.RootElement.GetProperty("profiles").GetProperty("ReplacePdfHyperlinks").GetProperty("commandLineArgs").ToString();
    args = CommandLine.Parse(commandLine);
}
#endif

#endregion

#region Read application settings

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();
var settings = new PdfEnvelopeSettings(); // to prevent trimming of types
settings = config.GetRequiredSection("PdfEnvelope").Get<PdfEnvelopeSettings>();

#endregion


#region Configure command line options

// Configure command line options

var command = new RootCommand("CLI tool to add a cover page, header and footer to a PDF file.");
Option<FileInfo?> inputFileOption = DefineInputFileOption();
command.AddOption(inputFileOption);
Option<FileInfo?> outputFileOption = DefineOutputFileOption();
command.AddOption(outputFileOption);
Option<bool?> overwriteOption = DefineOverwriteOption();
command.AddOption(overwriteOption);

#endregion


#region Command handling

// Command handler

command.SetHandler((inputFile, outputFile, overwrite) => {
    int exitCode = 0;

    var useInputFileAsOutputFile = false;

    if (outputFile == null) {
        // No output file specified but replace regex specified means input file shall be modified in-place
        if (overwrite.HasValue && overwrite.Value) {
            var tempOutputFilename = GetTempOutputFilename(inputFile);
            outputFile = new FileInfo(tempOutputFilename);
            useInputFileAsOutputFile = true;
        }
        else {
            Console.WriteLine($"In order to modify the input file in-place use option {overwriteOption.Name}.");
            exitCode = 1;
            return Task.FromResult(exitCode);
        }
    }
    else {
        // Avoid overwriting an existing output file without declared intent
        if (File.Exists(outputFile.FullName)) {
            if (!overwrite.HasValue || !overwrite.Value) {
                LogError($"Output file exists. Use option {overwriteOption.Name}.");
                exitCode = 1;
                return Task.FromResult(exitCode);
            }
            try {
                File.Delete(outputFile.FullName);
            }
            catch (Exception) {
                LogError($"Output file exists and cannot be overwritten. Ensure it is not opened in another application.");
                exitCode = 1;
                return Task.FromResult(exitCode);
            }
        }
    }

    Console.WriteLine($"Processing file: {inputFile.FullName}...");

    const float pageSideMargin = 50;
    const float pageTopMargin = 35;
    const float pageBottomMargin = 25;

    try {
        // Add a cover page to the document
        var coverTempFilename = $"{outputFile.Directory.FullName}{System.IO.Path.DirectorySeparatorChar}{outputFile.Name}-cover.pdf";
        using (var reader = new PdfReader(inputFile)) {
            using var writer = GetPdfWriter(outputFile);
            if (writer == null) {
                exitCode = 1;
                return Task.FromResult(exitCode);
            }
            using var inputPdf = new PdfDocument(reader);
            using var outputPdf = new PdfDocument(writer);
            // Create temporary cover page document
            using (var coverWriter = GetPdfWriter(new FileInfo(coverTempFilename))) {
                if (coverWriter == null) {
                    exitCode = 1;
                    return Task.FromResult(exitCode);
                }
                using var coverPdf = new PdfDocument(coverWriter);
                coverPdf.AddNewPage(new PageSize(inputPdf.GetFirstPage().GetPageSize()));

                using var coverDocument = new Document(coverPdf);
                PdfPage pdfPage = coverPdf.GetPage(1);
                var pageSize = pdfPage.GetPageSize();

                PdfCanvas pdfCanvas = new PdfCanvas(pdfPage.NewContentStreamBefore(), pdfPage.GetResources(), coverPdf);

                using (Canvas canvas = new Canvas(pdfCanvas, pdfPage.GetCropBox())) {
                    const float textSideMargin = 20;

                    var font = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
                    canvas.SetFont(font);

                    // Topic
                    canvas.Add(new Paragraph(ResolvePlaceholders(settings.CoverPage.Topic, coverPdf, 1, settings.PageNumberOffset))
                        .SetTextAlignment(TextAlignment.LEFT)
                        .SetFontSize(22)
                        .SetBold()
                        .SetFixedPosition(1, pageSideMargin + textSideMargin, pageSize.GetHeight() - pageTopMargin - 120, pageSize.GetWidth() - 2 * pageSideMargin - 2 * textSideMargin));
                    // Subtopic
                    canvas.Add(new Paragraph(ResolvePlaceholders(settings.CoverPage.Subtopic, coverPdf, 1, settings.PageNumberOffset))
                        .SetTextAlignment(TextAlignment.LEFT)
                        .SetFontSize(22)
                        .SetBold()
                        .SetFixedPosition(1, pageSideMargin + textSideMargin, pageSize.GetHeight() - pageTopMargin - 150, pageSize.GetWidth() - 2 * pageSideMargin - 2 * textSideMargin));
                    // Disclaimer
                    if (!string.IsNullOrWhiteSpace(settings.CoverPage.Disclaimer)) {
                        var disclaimerWidth = 180;
                        var disclaimerHeight = 40;
                        var disclaimerX = pageSize.GetWidth() - pageSideMargin - disclaimerWidth;
                        var disclaimerY = pageSize.GetHeight() - pageTopMargin - 50;
                        var disclaimerMargin = 5;
                        canvas.GetPdfCanvas().SetStrokeColor(ColorConstants.BLACK)
                            .SetLineWidth(1.0f)
                            .Rectangle(disclaimerX, disclaimerY, disclaimerWidth, -disclaimerHeight)
                            .Stroke();
                        canvas.Add(new Paragraph(settings.CoverPage.Disclaimer)
                            .SetTextAlignment(TextAlignment.LEFT)
                            .SetVerticalAlignment(VerticalAlignment.BOTTOM)
                            .SetFontSize(10)
                            .SetFixedPosition(1, disclaimerX + disclaimerMargin, disclaimerY - disclaimerHeight + disclaimerMargin, disclaimerWidth - 2 * disclaimerMargin));
                    }
                    // Title
                    canvas.Add(new Paragraph(ResolvePlaceholders(settings.CoverPage.Title, coverPdf, 1, settings.PageNumberOffset))
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFontSize(24)
                        .SetBold()
                        .SetFixedPosition(1, pageSideMargin + textSideMargin, pageSize.GetHeight() - pageTopMargin - 250, pageSize.GetWidth() - 2 * pageSideMargin - 2 * textSideMargin));
                    // Subtitle
                    canvas.Add(new Paragraph(ResolvePlaceholders(settings.CoverPage.Subtitle, coverPdf, 1, settings.PageNumberOffset))
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFontSize(18)
                        .SetBold()
                        .SetFixedPosition(1, pageSideMargin + textSideMargin, pageSize.GetHeight() - pageTopMargin - 280, pageSize.GetWidth() - 2 * pageSideMargin - 2 * textSideMargin));
                    // Version
                    canvas.Add(new Paragraph(ResolvePlaceholders(settings.CoverPage.Version, coverPdf, 1, settings.PageNumberOffset))
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFontSize(16)
                        .SetFixedPosition(1, pageSideMargin + textSideMargin, pageSize.GetHeight() - pageTopMargin - 370, pageSize.GetWidth() - 2 * pageSideMargin - 2 * textSideMargin));
                    // Author
                    canvas.Add(new Paragraph(ResolvePlaceholders(settings.CoverPage.Author, coverPdf, 1, settings.PageNumberOffset))
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFontSize(16)
                        .SetFixedPosition(1, pageSideMargin + textSideMargin, pageSize.GetHeight() - pageTopMargin - 400, pageSize.GetWidth() - 2 * pageSideMargin - 2 * textSideMargin));
                    // Date
                    canvas.Add(new Paragraph(ResolvePlaceholders(settings.CoverPage.Date, coverPdf, 1, settings.PageNumberOffset))
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFontSize(16)
                        .SetFixedPosition(1, pageSideMargin + textSideMargin, pageSize.GetHeight() - pageTopMargin - 430, pageSize.GetWidth() - 2 * pageSideMargin - 2 * textSideMargin));
                    // Frames
                    float orgTextSize = 14;
                    var orgTextWidth = !string.IsNullOrEmpty(settings.CoverPage.Organization) ? font.GetWidth(settings.CoverPage.Organization, orgTextSize) + 8 : 0;
                    var framesTopY = pageSize.GetHeight() - pageTopMargin - 170;
                    var mainBoxHeight = framesTopY - 20 - pageBottomMargin - 100;
                    canvas.GetPdfCanvas().SetStrokeColor(ColorConstants.BLACK)
                        .SetLineWidth(1.0f)
                        .SetFillColor(ColorConstants.BLACK)
                        .Rectangle(pageSideMargin + textSideMargin, framesTopY, pageSize.GetWidth() - 2 * pageSideMargin - 2 * textSideMargin - orgTextWidth, -15)
                        .FillStroke()
                        .Stroke();
                    canvas.GetPdfCanvas().SetStrokeColor(ColorConstants.BLACK)
                        .SetLineWidth(1.0f)
                        .Rectangle(pageSideMargin + textSideMargin, framesTopY - 20, pageSize.GetWidth() - 2 * pageSideMargin - 2 * textSideMargin, -mainBoxHeight)
                        .Stroke();
                    if (!string.IsNullOrEmpty(settings.CoverPage.Organization)) {
                        // Organization
                        canvas.Add(new Paragraph(ResolvePlaceholders(settings.CoverPage.Organization, coverPdf, 1, settings.PageNumberOffset))
                            .SetTextAlignment(TextAlignment.RIGHT)
                            .SetFontSize(orgTextSize)
                            .SetBold()
                            .SetFixedPosition(1, pageSize.GetWidth() - pageSideMargin - textSideMargin - orgTextWidth, framesTopY - 18, orgTextWidth));
                    }
                    if (settings.CoverPage.ShowSignatureArea) {
                        var sigAreaHeight = 80;
                        var sigAreaTop = framesTopY - 20 - mainBoxHeight + sigAreaHeight;
                        canvas.GetPdfCanvas().SetStrokeColor(ColorConstants.BLACK)
                            .SetLineWidth(1.0f)
                            .Rectangle(pageSideMargin + textSideMargin, sigAreaTop, (pageSize.GetWidth() - 2 * pageSideMargin - 2 * textSideMargin)/2, -sigAreaHeight)
                            .Stroke();
                        canvas.Add(new Paragraph(ResolvePlaceholders("Author", coverPdf, 1, settings.PageNumberOffset))
                            .SetTextAlignment(TextAlignment.LEFT)
                            .SetFontSize(12)
                            .SetFixedPosition(1, pageSideMargin + textSideMargin + 10, sigAreaTop - 20, pageSize.GetWidth() / 3));
                        canvas.GetPdfCanvas().SetStrokeColor(ColorConstants.BLACK)
                            .SetLineWidth(1.0f)
                            .Rectangle(pageSideMargin + textSideMargin + (pageSize.GetWidth() - 2 * pageSideMargin - 2 * textSideMargin) / 2, sigAreaTop, (pageSize.GetWidth() - 2 * pageSideMargin - 2 * textSideMargin) / 2, -sigAreaHeight)
                            .Stroke();
                        canvas.Add(new Paragraph(ResolvePlaceholders("Approver", coverPdf, 1, settings.PageNumberOffset))
                            .SetTextAlignment(TextAlignment.LEFT)
                            .SetFontSize(12)
                            .SetFixedPosition(1, pageSideMargin + textSideMargin + (pageSize.GetWidth() - 2 * pageSideMargin - 2 * textSideMargin) / 2 + 10, sigAreaTop - 20, pageSize.GetWidth() / 3));
                    }
                }
                coverPdf.SetFlushUnusedObjects(true);
                coverWriter.SetCompressionLevel(9);
            }

            // Add the cover page to document
            using (var coverPdf = new PdfDocument(new PdfReader(coverTempFilename))) {
                var merger = new PdfMerger(outputPdf);
                merger.Merge(coverPdf, 1, 1);
                merger.Merge(inputPdf, 1, inputPdf.GetNumberOfPages());
                merger.Close();
            }
        }
        try {
            // Delete temporary files
            if (File.Exists(coverTempFilename)) {
                File.Delete(coverTempFilename);
            }
        }
        catch (Exception) {
            LogError("Error: Temporary cover page file could not be deleted");
            Console.WriteLine("Continuing...");
        }
    }
    catch (Exception ex) {
        LogError($"Error: {ex.Message}");
        LogError($"Error: Cover page could not be generated");
        Console.WriteLine("Exiting...");
        exitCode = 1;
        return Task.FromResult(exitCode);
    }

    try {
        // Add header and footer to all pages except the cover page
        var tempOutput2Filename = GetTempOutputFilename(outputFile);
        var output2File = new FileInfo(tempOutput2Filename);
        using (var reader = new PdfReader(outputFile)) {
            using var writer = GetPdfWriter(output2File);
            if (writer == null) {
                exitCode = 1;
                return Task.FromResult(exitCode);
            }
            using var pdf = new PdfDocument(reader, writer);
            var numOfPages = pdf.GetNumberOfPages();
            using var document = new Document(pdf, pdf.GetDefaultPageSize(), false);
            const float textLineHeight = 15;
            const float fontSize = 9;

            var StyledParagraph = (string text) => {
                return new Paragraph(text)
                    .SetMargin(0)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN))
                    .SetFontSize(fontSize)
                    .SetFontColor(ColorConstants.BLACK);
            };

            for (int i = 1; i <= numOfPages; i++) {
                Console.Write($"Processing page {i}...\r");

                PdfPage pdfPage = pdf.GetPage(i);
                var pageSize = pdfPage.GetPageSize();

                PdfCanvas pdfCanvas = new PdfCanvas(pdfPage.NewContentStreamBefore(), pdfPage.GetResources(), pdf);

                using (Canvas canvas = new Canvas(pdfCanvas, pdfPage.GetCropBox())) {
                    if (i > 1 || !settings.PageHeader.ExcludeCoverPage) {
                        // Add page header - Left
                        Paragraph p = StyledParagraph(ResolvePlaceholders(settings.PageHeader.TextLeft1, pdf, i, settings.PageNumberOffset))
                            .SetTextAlignment(TextAlignment.LEFT)
                            .SetFixedPosition(i, pageSideMargin, pageSize.GetHeight() - pageTopMargin, pageSize.GetWidth() - 2 * pageSideMargin);
                        canvas.Add(p);
                        p = StyledParagraph(ResolvePlaceholders(settings.PageHeader.TextLeft2, pdf, i, settings.PageNumberOffset))
                            .SetTextAlignment(TextAlignment.LEFT)
                            .SetFixedPosition(i, pageSideMargin, pageSize.GetHeight() - pageTopMargin - textLineHeight, pageSize.GetWidth() - 2 * pageSideMargin);
                        canvas.Add(p);

                        // Add page header - Center
                        p = StyledParagraph(ResolvePlaceholders(settings.PageHeader.TextCenter1, pdf, i, settings.PageNumberOffset))
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetFixedPosition(i, pageSideMargin, pageSize.GetHeight() - pageTopMargin, pageSize.GetWidth() - 2 * pageSideMargin);
                        canvas.Add(p);
                        p = StyledParagraph(ResolvePlaceholders(settings.PageHeader.TextCenter2, pdf, i, settings.PageNumberOffset))
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetFixedPosition(i, pageSideMargin, pageSize.GetHeight() - pageTopMargin - textLineHeight, pageSize.GetWidth() - 2 * pageSideMargin);
                        canvas.Add(p);

                        // Add page header - Right
                        p = StyledParagraph(ResolvePlaceholders(settings.PageHeader.TextRight1, pdf, i, settings.PageNumberOffset))
                            .SetTextAlignment(TextAlignment.RIGHT)
                            .SetFixedPosition(i, pageSideMargin, pageSize.GetHeight() - pageTopMargin, pageSize.GetWidth() - 2 * pageSideMargin);
                        canvas.Add(p);
                        p = StyledParagraph(ResolvePlaceholders(settings.PageHeader.TextRight2, pdf, i, settings.PageNumberOffset))
                            .SetTextAlignment(TextAlignment.RIGHT)
                            .SetFixedPosition(i, pageSideMargin, pageSize.GetHeight() - pageTopMargin - textLineHeight, pageSize.GetWidth() - 2 * pageSideMargin);
                        canvas.Add(p);

                        // Draw header line
                        if (settings.PageHeader.DrawLine) {
                            canvas.GetPdfCanvas().SetStrokeColor(ColorConstants.BLACK)
                            .SetLineWidth(0.5f)
                            .MoveTo(pageSideMargin, pageSize.GetHeight() - pageTopMargin - textLineHeight - 3).LineTo(pageSize.GetWidth() - pageSideMargin, pageSize.GetHeight() - pageTopMargin - textLineHeight - 3)
                            .Stroke();
                        }
                    }

                    if (i > 1 || !settings.PageFooter.ExcludeCoverPage) {
                        // Add page number to the footer - Left
                        var p = StyledParagraph(ResolvePlaceholders(settings.PageFooter.TextLeft1, pdf, i, settings.PageNumberOffset))
                            .SetTextAlignment(TextAlignment.LEFT)
                            .SetFixedPosition(i, pageSideMargin, pageBottomMargin + textLineHeight, pageSize.GetWidth() - 2 * pageSideMargin);
                        canvas.Add(p);
                        p = StyledParagraph(ResolvePlaceholders(settings.PageFooter.TextLeft2, pdf, i, settings.PageNumberOffset))
                            .SetTextAlignment(TextAlignment.LEFT)
                            .SetFixedPosition(i, pageSideMargin, pageBottomMargin, pageSize.GetWidth() - 2 * pageSideMargin);
                        canvas.Add(p);

                        // Add page number to the footer - Center
                        p = StyledParagraph(ResolvePlaceholders(settings.PageFooter.TextCenter1, pdf, i, settings.PageNumberOffset))
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetFixedPosition(i, pageSideMargin, pageBottomMargin + textLineHeight, pageSize.GetWidth() - 2 * pageSideMargin);
                        canvas.Add(p);
                        p = StyledParagraph(ResolvePlaceholders(settings.PageFooter.TextCenter2, pdf, i, settings.PageNumberOffset))
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetFixedPosition(i, pageSideMargin, pageBottomMargin, pageSize.GetWidth() - 2 * pageSideMargin);
                        canvas.Add(p);

                        // Add page number to the footer - Right
                        p = StyledParagraph(ResolvePlaceholders(settings.PageFooter.TextRight1, pdf, i, settings.PageNumberOffset))
                            .SetTextAlignment(TextAlignment.RIGHT)
                            .SetFixedPosition(i, pageSideMargin, pageBottomMargin + textLineHeight, pageSize.GetWidth() - 2 * pageSideMargin);
                        canvas.Add(p);
                        p = StyledParagraph(ResolvePlaceholders(settings.PageFooter.TextRight2, pdf, i, settings.PageNumberOffset))
                            .SetTextAlignment(TextAlignment.RIGHT)
                            .SetFixedPosition(i, pageSideMargin, pageBottomMargin, pageSize.GetWidth() - 2 * pageSideMargin);
                        canvas.Add(p);

                        // Draw footer line
                        if (settings.PageFooter.DrawLine) {
                            canvas.GetPdfCanvas().SetStrokeColor(ColorConstants.BLACK)
                            .SetLineWidth(0.5f)
                            .MoveTo(pageSideMargin, pageBottomMargin + textLineHeight + fontSize + 5).LineTo(pageSize.GetWidth() - pageSideMargin, pageBottomMargin + textLineHeight + fontSize + 5)
                            .Stroke();
                        }
                    }
                    canvas.Flush();
                }
                pdfCanvas.Release();

                if (settings.RemoveAnnotationsOtherThanLinks) {
                    // Clear existing annotations/comments
                    var annotations = pdfPage.GetAnnotations();
                    foreach (var annotation in annotations) {
                        if (!annotation.GetSubtype().Equals(PdfName.Link)) {
                            pdfPage.RemoveAnnotation(annotation);
                        }
                    }
                }
            }
            Console.Write($"                                  \r");

            pdf.SetFlushUnusedObjects(true);
            writer.SetCompressionLevel(9);
        }
        File.Delete(outputFile.FullName);
        File.Move(output2File.FullName, outputFile.FullName);
    }
    catch (Exception ex) {
        LogError($"Error: {ex.Message}");
        LogError($"Error: Header and footer could not be added");
        Console.WriteLine("Exiting...");
        exitCode = 1;
        return Task.FromResult(exitCode);
    }

    #region Replace input file (if requested)
    if (useInputFileAsOutputFile) {
        // Apply the changes to the input file
        try {
            // Replace input file with temporary output file
            if (File.Exists(outputFile.FullName)) {
                File.Delete(inputFile.FullName);
                File.Move(outputFile.FullName, inputFile.FullName);
                outputFile = inputFile;
            }
        }
        catch (Exception ex) {
            LogError($"Error: {ex.Message}");
            LogError("The changes could not be applied to the input file (keeping temporary file)");
            exitCode = 1;
        }
    }
    #endregion

    #region Show some statistics

    Console.WriteLine($"Output file: {outputFile.FullName}");

    #endregion

    return Task.FromResult(exitCode);
},
inputFileOption, outputFileOption, overwriteOption);

return await command.InvokeAsync(args);

#endregion

#region Define command line options
// Define command line options

Option<FileInfo?> DefineInputFileOption()
{
    var inputFileOption = new Option<FileInfo?>(
        name: "--inputFile",
        description: "The PDF file to process.",
        parseArgument: result => {
            if (result.Tokens.Count == 0) {
                result.ErrorMessage = "Option '--inputFile' is required.";
                return null;
            }
            string? filePath = result.Tokens.Single().Value;
            if (!File.Exists(filePath)) {
                result.ErrorMessage = "Input file does not exist";
                return null;
            }
            else {
                return new FileInfo(filePath);
            }
        }) { IsRequired = true };
    inputFileOption.AddAlias("-f");
    return inputFileOption;
}

Option<FileInfo?> DefineOutputFileOption()
{
    var outputFileOption = new Option<FileInfo?>(
        name: "--outputFile",
        description: "The output PDF file path.");
    outputFileOption.AddAlias("-o");
    return outputFileOption;
}

Option<bool?> DefineOverwriteOption()
{
    var overwriteOption = new Option<bool?>(
        name: "--overwrite-yes",
        description: "Overwrite the existing output file without confirmation.",
        getDefaultValue: () => false);
    overwriteOption.AddAlias("-y");
    return overwriteOption;
}

#endregion

#region Helper functions

static void LogError(string message)
{
    var fgcolor = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(message);
    Console.ForegroundColor = fgcolor;
}

static PdfWriter? GetPdfWriter(FileInfo outputFile)
{
    PdfWriter? writer = null;
    FileStream? os = null;
    int retryCount = 3;
    int retryDelaySeconds = 5;
    do {
        try {
            os = new FileStream(outputFile.FullName, FileMode.OpenOrCreate, FileAccess.Write);
        }
        catch (Exception) {
            LogError($"Output file could not be opened for writing. Retrying {retryCount} times after {retryDelaySeconds} seconds...");
            Thread.Sleep(retryDelaySeconds * 1000);
        }
        retryCount--;
    } while (os == null && retryCount > 0);
    if (os != null) {
        writer = new PdfWriter(os);
    }
    return writer;
}

static string GetTempOutputFilename(FileInfo? baseFileInfo)
{
    if (baseFileInfo == null) {
        baseFileInfo = new FileInfo("TempOutput");
    }
    return baseFileInfo.Directory.FullName + System.IO.Path.DirectorySeparatorChar + baseFileInfo.Name + "-" +
        HashCode.Combine(baseFileInfo.FullName, DateTime.Now.Ticks.ToString()) + baseFileInfo.Extension;
}

static string? ResolvePlaceholders(string text, PdfDocument pdfDocument, int currentPageNumber, int pageNumberOffset = 0)
{
    if (string.IsNullOrWhiteSpace(text)) return string.Empty;

    string result = text;

    var placeholders = RegExs.PlaceholderRegEx.Matches(text);
    for (int i = 0; i < placeholders.Count; i++) {
        var placeholder = placeholders[i];
        var index = i.ToString();

        if (placeholder.Groups[0].Value.Contains("date")) {
            // {date[:<format>]}, see also: https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings
            result = result.Replace("date", index);
            try {
                result = String.Format(result, DateTime.Now);
            }
            catch (Exception) {
                LogError($"Invalid date format specified: {text}");
                Console.WriteLine($"Using default: dd.MM.yyyy");
                return String.Format("{" + index + ":dd.MM.yyyy}", DateTime.Now);
            }
        }
        else if (placeholder.Groups[0].Value.Contains("pageNum")) {
            // {pageNum}
            result = (currentPageNumber == 1) ? "{numOfPages} pages" : result.Replace("{pageNum}", (currentPageNumber + pageNumberOffset).ToString());
        }
        else if (placeholder.Groups[0].Value.Contains("numOfPages")) {
            // {numOfPages}
            result = result.Replace("{numOfPages}", (pdfDocument.GetNumberOfPages() + pageNumberOffset).ToString());
        }
    }

    return result;
}

#endregion
