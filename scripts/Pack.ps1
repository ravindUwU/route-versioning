<#
.SYNOPSIS
	Generates fresh builds of NuGet packages into the `publish` directory. Uses Process-ReadmeTemplate
	to generate READMEs, in the process.

.PARAMETER IgnoreUncommittedChanges
	If true, continues packing despite uncommitted changes present in the repository.
#>
[CmdletBinding()]
param (
	[switch] $IgnoreUncommittedChanges = $false
)

$projects = @(
	'RouteVersioning'
	'RouteVersioning.OpenApi'
)

try {
	Push-Location .
	Set-Location (Join-Path $PSScriptRoot '..' -Resolve)

	if (-not $IgnoreUncommittedChanges) {
		git diff --exit-code | Out-Null
		if ($LASTEXITCODE -ne 0) {
			throw 'Uncommitted changes present'
		}

		git diff --cached --exit-code | Out-Null
		if ($LASTEXITCODE -ne 0) {
			throw 'Uncommitted changes present'
		}
	}

	Write-Host "Cleaning publish directory" -ForegroundColor Cyan
	Remove-Item 'publish' -Recurse -Force -ErrorAction Ignore

	foreach ($project in $projects) {
		try {
			Push-Location .
			Set-Location $project

			Write-Host "Generating $project README" -ForegroundColor Cyan
			& "$PSScriptRoot/Process-ReadmeTemplate.ps1" 'README.template.md'

			Write-Host "Packing $project" -ForegroundColor Cyan
			Remove-Item 'bin' -Recurse -Force -ErrorAction Ignore
			Remove-Item 'obj' -Recurse -Force -ErrorAction Ignore

			dotnet pack --output '../publish' --verbosity detailed
		}
		finally {
			Pop-Location
		}
	}

	Write-Host 'Done' -ForegroundColor Green
}
finally {
	Pop-Location
}
