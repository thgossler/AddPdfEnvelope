#!/usr/bin/env pwsh
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, ParameterSetName = 'Major')]
    [switch] $Major,

    [Parameter(Mandatory = $true, ParameterSetName = 'Minor')]
    [switch] $Minor,

    [Parameter(Mandatory = $true, ParameterSetName = 'Patch')]
    [switch] $Patch
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$projectPath = Join-Path $PSScriptRoot 'AddPdfEnvelope.csproj'
$projectContent = [System.IO.File]::ReadAllText($projectPath)
$projectXml = [System.Xml.Linq.XDocument]::Parse($projectContent)
$versionElement = $projectXml.Descendants('Version') | Select-Object -First 1

if ($null -eq $versionElement) {
    throw "The project does not contain a Version property: $projectPath"
}

$semVerPattern = '(?<![0-9A-Za-z-])(?<Major>0|[1-9][0-9]*)\.(?<Minor>0|[1-9][0-9]*)\.(?<Patch>0|[1-9][0-9]*)(?<Suffix>(?:-[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*)?(?:\+[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*)?)(?![0-9A-Za-z-])'
$versionMatch = [regex]::Match($versionElement.Value, $semVerPattern)

if (-not $versionMatch.Success) {
    throw "The Version property is not a supported SemVer value: $($versionElement.Value)"
}

$majorNumber = [int64]::Parse($versionMatch.Groups['Major'].Value)
$minorNumber = [int64]::Parse($versionMatch.Groups['Minor'].Value)
$patchNumber = [int64]::Parse($versionMatch.Groups['Patch'].Value)

if ($Major) {
    $majorNumber++
    $minorNumber = 0
    $patchNumber = 0
}
elseif ($Minor) {
    $minorNumber++
    $patchNumber = 0
}
else {
    $patchNumber++
}

$newVersion = "$majorNumber.$minorNumber.$patchNumber"
$versionPropertyNames = @(
    'Version',
    'VersionPrefix',
    'PackageVersion',
    'AssemblyVersion',
    'FileVersion',
    'InformationalVersion',
    'AssemblyInformationalVersion'
)
$updatedPropertyCount = 0
$updatedContent = $projectContent

foreach ($propertyName in $versionPropertyNames) {
    $propertyPattern = "(?s)(?<open><$propertyName\s*>)(?<value>[^<]*)(?<close></$propertyName\s*>)"
    $propertyMatchCount = [regex]::Matches($updatedContent, $propertyPattern).Count
    $updatedPropertyCount += $propertyMatchCount

    if ($propertyMatchCount -eq 0) {
        continue
    }

    $updatedContent = [regex]::Replace($updatedContent, $propertyPattern, {
        param($match)

        $value = $match.Groups['value'].Value
        $propertyVersionMatch = [regex]::Match($value, $semVerPattern)

        if (-not $propertyVersionMatch.Success) {
            throw "The $propertyName property is not a supported SemVer value: $value"
        }

        $updatedValue = [regex]::Replace($value, $semVerPattern, $newVersion)
        return $match.Groups['open'].Value + $updatedValue + $match.Groups['close'].Value
    })
}

if ($updatedPropertyCount -eq 0) {
    throw "No version properties were updated in $projectPath"
}

[System.IO.File]::WriteAllText($projectPath, $updatedContent, [System.Text.UTF8Encoding]::new($false))
Write-Host "Updated $projectPath from $($versionMatch.Value) to $newVersion in $updatedPropertyCount property occurrence(s)."
