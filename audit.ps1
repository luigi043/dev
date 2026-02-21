Write-Host "==============================="
Write-Host ".NET Project Audit"
Write-Host "==============================="

$root = Get-Location
Write-Host ""
Write-Host "Project Root: $root"
Write-Host ""

# -----------------------------
# Check for Solution File
# -----------------------------
Write-Host "Checking for .sln file..."
$sln = Get-ChildItem -Path . -Filter *.sln -Recurse -ErrorAction SilentlyContinue

if ($sln) {
    Write-Host "Solution file(s) found:"
    foreach ($file in $sln) {
        Write-Host " - $($file.FullName)"
    }
} else {
    Write-Host "No .sln file found."
}

Write-Host ""

# -----------------------------
# Find .csproj files
# -----------------------------
Write-Host "Scanning for .csproj files..."
$projects = Get-ChildItem -Path . -Filter *.csproj -Recurse -ErrorAction SilentlyContinue

if (!$projects) {
    Write-Host "No .csproj files found."
} else {
    Write-Host "$($projects.Count) project(s) found:"
    foreach ($proj in $projects) {
        Write-Host " - $($proj.FullName)"
    }
}

Write-Host ""

# -----------------------------
# Check Project References
# -----------------------------
Write-Host "Checking project references..."

foreach ($proj in $projects) {

    Write-Host ""
    Write-Host "Project: $($proj.Name)"

    [xml]$xml = Get-Content $proj.FullName

    $refs = $xml.Project.ItemGroup.ProjectReference

    if ($refs) {
        foreach ($ref in $refs) {

            $refPath = Join-Path $proj.DirectoryName $ref.Include

            if (Test-Path $refPath) {
                Write-Host "   OK -> $($ref.Include)"
            } else {
                Write-Host "   MISSING -> $($ref.Include)"
            }
        }
    } else {
        Write-Host "   No project references."
    }
}

Write-Host ""
Write-Host "==============================="
Write-Host "Audit Complete"
Write-Host "==============================="