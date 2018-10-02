$ErrorActionPreference = "Stop"
Push-Location AK.Listor.WebClient
Write-Host "Installing web client dependencies..."
npm install
If ($LastExitCode -Ne 0) { Throw "npm install failed." }
Write-Host "Building web client..."
npm run build
If ($LastExitCode -Ne 0) { Throw "npm run build failed." }
Pop-Location
Write-Host "Deleting existing client files..."
[System.IO.Directory]::GetFiles("AK.Listor\Client", "*.*") | Where-Object {
	[System.IO.Path]::GetFileName($_) -Ne ".gitignore"
} | ForEach-Object {
	$File = [System.IO.Path]::GetFullPath($_)
	[System.IO.File]::Delete($File)
}
[System.IO.Directory]::GetDirectories("AK.Listor\Client") | ForEach-Object {
	$Directory = [System.IO.Path]::GetFullPath($_)
	[System.IO.Directory]::Delete($Directory, $True)
}
Write-Host "Copying new files..."
[System.IO.Directory]::GetFiles("AK.Listor.WebClient\build", "*.*", "AllDirectories") | Where-Object {
	-Not ($_.EndsWith("service-worker.js"))
} | ForEach-Object {
	$SourceFile = [System.IO.Path]::GetFullPath($_)
	$TargetFile = [System.IO.Path]::GetFullPath([System.IO.Path]::Combine("AK.Listor\Client", $_.Substring(26)))
	Write-Host "$SourceFile --> $TargetFile"
	$TargetDirectory = [System.IO.Path]::GetDirectoryName($TargetFile)
	If (-Not [System.IO.Directory]::Exists($TargetDirectory)) {
		[System.IO.Directory]::CreateDirectory($TargetDirectory) | Out-Null
	}
	[System.IO.File]::Copy($SourceFile, $TargetFile, $True) | Out-Null
}
Write-Host "Building application..."
dotnet build
If ($LastExitCode -Ne 0) { Throw "dotnet build failed." }
