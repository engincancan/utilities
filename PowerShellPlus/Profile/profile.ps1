function edit-file([string]$path) {
	$paths = @(resolve-path $path -ErrorAction "SilentlyContinue")
	if ($paths.count -gt 1) {
		if ($psplus.Confirm("You are about to open the below files. Are you sure?", "Open multiple files", $paths) -eq $FALSE) {
			break
		} 
	} elseif ($paths.count -eq 0) {
		if ($psplus.Confirm("No files matched your request. Do you want to create a new file?", "Create New File", $path) -eq $TRUE) 
		{
			[Environment]::CurrentDirectory = (Get-Location)
			trap { Write-Error $_.Exception.Message; continue}
			. {
				$path = [System.IO.Path]::GetFullPath($path)
				if (!($path.Contains("."))) {
					$path += ".ps1"
				}
				"" > $path
				$paths = @($path)
			}
		} else {
			break
		}
	}

	foreach ($path in $paths) 
	{ 
	$psplus.OpenFile((get-item $path).FullName) 
	}
}

function tabIdentification{

            # This is the default function to use for tab expansion. It handles simple
            # member expansion on variables, variable name expansion and parameter completion
            # on commands. It doesn't understand strings so strings containing ; | ( or { may
            # cause expansion to fail.

            param($line, $lastWord)

        $line, $lastword
}

Push-Location
$privateprofile = (Split-Path -Parent $MyInvocation.MyCommand.Path) # + "\"


################ Start of PowerTab TabCompletion Code ########################


& "$privateprofile\Init-TabExpansion.ps1" -ConfigurationLocation "$privateprofile"


################ End of PowerTab TabCompletion Code ##########################

Pop-Location