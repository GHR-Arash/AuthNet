Set-StrictMode -Version Latest

$AuthNetPackProjects = @(
    'src\AuthNet.Core\AuthNet.Core.csproj',
    'src\AuthNet.ExternalProviders\AuthNet.ExternalProviders.csproj',
    'src\AuthNet.Persistence.EntityFrameworkCore\AuthNet.Persistence.EntityFrameworkCore.csproj',
    'src\AuthNet.Persistence.Postgres\AuthNet.Persistence.Postgres.csproj',
    'src\AuthNet.Persistence.SqlServer\AuthNet.Persistence.SqlServer.csproj',
    'src\AuthNet.UI.Razor\AuthNet.UI.Razor.csproj',
    'src\AuthNet.Api\AuthNet.Api.csproj',
    'src\AuthNet.AspNetCore\AuthNet.AspNetCore.csproj'
)

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
[xml] $directoryBuildProps = Get-Content (Join-Path $repoRoot 'Directory.Build.props')
$AuthNetPackageVersion = $directoryBuildProps.Project.PropertyGroup.VersionPrefix

$AuthNetPackageIds = @($AuthNetPackProjects | ForEach-Object {
    [System.IO.Path]::GetFileNameWithoutExtension($_)
})

$AuthNetExpectedPackageFiles = @($AuthNetPackageIds | ForEach-Object { "$_.$AuthNetPackageVersion.nupkg" })
