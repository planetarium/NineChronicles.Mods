# 스크립트 설정
$dotenvPath = ".\.env.xml"
$projectName = "NineChronicles.Mods.Athena"
$solutionDirectory = Get-Location
$publishConfiguration = "Release"

function Get-NineChroniclesDirFromEnv {
    [xml]$xml = Get-Content $dotenvPath
    $nineChroniclesDir = $xml.Project.PropertyGroup.NINECHRONICLES_DIR
    if (-not $nineChroniclesDir) {
        Write-Error "NINECHRONICLES_DIR not found in $dotenvPath"
        exit
    }
    return $nineChroniclesDir
}

function Publish-Project {
    param (
        [string]$projectName,
        [string]$solutionDirectory,
        [string]$publishConfiguration
    )
    $projectPath = Join-Path -Path $solutionDirectory -ChildPath "$projectName\$projectName.csproj"
    $publishOutputPath = Join-Path -Path $solutionDirectory -ChildPath "$projectName\bin\$publishConfiguration\netstandard2.1"

    dotnet publish $projectPath -c $publishConfiguration -o $publishOutputPath
    Write-Host "dotnet publish exit code: $LASTEXITCODE"
    if ($LASTEXITCODE -ne 0) {
        Write-Error "dotnet publish failed"
        exit
    }

    Write-Host "Publish output path: $publishOutputPath"
    return $publishOutputPath
}

function Copy-DllToPluginDir {
    param (
        [string]$sourcePath,
        [string]$dllFile,
        [string]$nineChroniclesDir
    )
    $targetPath = Join-Path -Path $nineChroniclesDir -ChildPath "BepInEx\plugins"
    $sourceDllPath = Join-Path -Path $sourcePath -ChildPath $dllFile

    Write-Host "Attempting to copy from $sourceDllPath to $targetPath"

    if (-not (Test-Path -Path $sourceDllPath)) {
        Write-Error "$dllFile not found in $sourcePath"
        return
    }

    if (-not (Test-Path -Path $targetPath)) {
        New-Item -ItemType Directory -Force -Path $targetPath
    }

    Copy-Item -Path $sourceDllPath -Destination $targetPath -Force
    Write-Host "$dllFile copied to $targetPath"
}

$nineChroniclesDir = Get-NineChroniclesDirFromEnv
$publishOutputPath = Publish-Project -projectName $projectName -solutionDirectory $solutionDirectory -publishConfiguration $publishConfiguration
Copy-DllToPluginDir -sourcePath "$projectName\bin\$publishConfiguration\netstandard2.1" -dllFile "NineChronicles.Mods.Athena.dll" -nineChroniclesDir $nineChroniclesDir
Copy-DllToPluginDir -sourcePath "$projectName\bin\$publishConfiguration\netstandard2.1" -dllFile "NineChronicles.Mods.Illusionist.dll" -nineChroniclesDir $nineChroniclesDir
Copy-DllToPluginDir -sourcePath "$projectName\bin\$publishConfiguration\netstandard2.1" -dllFile "NineChronicles.Modules.BlockSimulation.dll" -nineChroniclesDir $nineChroniclesDir
