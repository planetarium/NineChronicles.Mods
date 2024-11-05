$DOTENV_PATH = ".\.env.xml"

function Get-NineChroniclesDir {
    $defaultNineChroniclesDir = "C:\Users\$env:USERNAME\AppData\Roaming\Nine Chronicles\player\main"
    $caption = "Confirm Nine Chronicles Directory"
    $message = "Is this the correct path for Nine Chronicles? `n$defaultNineChroniclesDir"
    $options = @(
        [System.Management.Automation.Host.ChoiceDescription]::new("&Yes", "Confirm the path is correct.")
        [System.Management.Automation.Host.ChoiceDescription]::new("&No", "Enter a different path.")
    )
    $choice = $Host.UI.PromptForChoice($caption, $message, $options, 0)

    if ($choice -eq 0) {
        return $defaultNineChroniclesDir
    }
    else {
        $promptMessage = "Enter the path for Nine Chronicles:"
        $nineChroniclesDir = Read-Host -Prompt $promptMessage
        return $nineChroniclesDir
    }
}

function Write-Env ($content) {
    if (Test-Path -Path $DOTENV_PATH) {
        $caption = "Existing $DOTENV_PATH found"
        $message = "Overwrite it?"
        $options = @(
            [System.Management.Automation.Host.ChoiceDescription]::new("&Yes", "Remove existing $DOTENV_PATH and create a new $DOTENV_PATH")
            [System.Management.Automation.Host.ChoiceDescription]::new("&No", "Do nothing")
        )
        $choice = $Host.UI.PromptForChoice($caption, $message, $options, 1)
        if ($choice -eq 0) {
            Write-Host "Removing existing $DOTENV_PATH..."
            Remove-Item $DOTENV_PATH
        }
        else {
            return
        }
    }

    Write-Host "Writing new $DOTENV_PATH..."
    Set-Content $DOTENV_PATH $content
}

$nine_chronicles_dir = Get-NineChroniclesDir

$dotenv_content = @"
<Project>
	<PropertyGroup>
		<NINECHRONICLES_DIR>$nine_chronicles_dir</NINECHRONICLES_DIR>
	</PropertyGroup>
</Project>
"@
Write-Env($dotenv_content)
