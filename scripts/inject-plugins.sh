#!/bin/bash

dotenvPath="./.env.xml"
solutionDirectory=$(pwd)
publishConfiguration="Release"
projects=("NineChronicles.Mods.Athena" "NineChronicles.Modules.BlockSimulation")

function get_ninechronicles_dir_from_env() {
    nineChroniclesDir=$(xmllint --xpath 'string(//Project/PropertyGroup/NINECHRONICLES_DIR)' "$dotenvPath")
    if [ -z "$nineChroniclesDir" ]; then
        echo "NINECHRONICLES_DIR not found in $dotenvPath"
        exit 1
    fi
    echo $nineChroniclesDir
}

function publish_project() {
    local projectName="$1"
    local solutionDirectory="$2"
    local publishConfiguration="$3"
    local projectPath="$solutionDirectory/$projectName/$projectName.csproj"
    local publishOutputPath="$solutionDirectory/$projectName/bin/$publishConfiguration/netstandard2.1"

    dotnet publish "$projectPath" -c "$publishConfiguration" -o "$publishOutputPath"
    local lastExitCode=$?
    echo "dotnet publish exit code: $lastExitCode"
    if [ $lastExitCode -ne 0 ]; then
        echo "dotnet publish failed"
        exit 1
    fi

    echo "Publish output path: $publishOutputPath"
    echo $publishOutputPath
}

function copy_dll_to_plugin_dir() {
    local sourcePath="$1"
    local dllFile="$2"
    local nineChroniclesDir="$3"
    local targetPath="$nineChroniclesDir/BepInEx/plugins"
    local sourceDllPath="$sourcePath/$dllFile"

    echo "Attempting to copy from $sourceDllPath to $targetPath"

    if [ ! -f "$sourceDllPath" ]; then
        echo "$dllFile not found in $sourcePath"
        return
    fi

    if [ ! -d "$targetPath" ]; then
        mkdir -p "$targetPath"
    fi

    cp -f "$sourceDllPath" "$targetPath"
    echo "$dllFile copied to $targetPath"
}

nineChroniclesDir=$(get_ninechronicles_dir_from_env)

for projectName in "${projects[@]}"; do
    echo "Processing project: $projectName"
    publishOutputPath=$(publish_project "$projectName" "$solutionDirectory" "$publishConfiguration")
    copy_dll_to_plugin_dir "$projectName/bin/$publishConfiguration/netstandard2.1" "${projectName}.dll" "$nineChroniclesDir"
done
