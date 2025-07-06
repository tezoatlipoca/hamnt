$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url        = 'https://github.com/tezoatlipoca/hamnt/releases/download/v0.3.0/hamnt_0.3.0_win-x64.zip'

Install-ChocolateyZipPackage 'hamnt' $url $toolsDir