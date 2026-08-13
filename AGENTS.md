# AGENTS.md

## Tools & commands
- Use the .NET 10 SDK.
- Run `dotnet build --nologo` to compile the project.
- Run `dotnet run -- --help` to verify CLI startup and option wiring.
- For PDF changes, smoke-test with `dotnet run -- -f Test.pdf -o /tmp/AddPdfEnvelope-result.pdf -y` and remove the generated file afterward.
- Run `./publish.sh` for the six-platform self-contained release packages; use `publish.cmd` on Windows.

## Workflow requirements
- The repository has no automated test project; use the build, CLI help, and PDF smoke check as the baseline validation.
- Keep `appsettings.json` available beside the executable when running it. The application loads it by relative path, and the project copies it to output.
- When changing configuration keys or placeholder behavior, update `PdfEnvelopeSettings`, `appsettings.json`, and the README example together.
- Do not use `-y` without `-o` unless intentionally testing in-place replacement of the input PDF.