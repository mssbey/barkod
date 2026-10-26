param([switch]$SelfTest)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
$source = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'EtiketAtolyesi.cs') -Raw -Encoding UTF8
Add-Type -TypeDefinition $source -ReferencedAssemblies System.Windows.Forms,System.Drawing
if ($SelfTest) { [EtiketAtolyesi.App]::SelfTest($PSScriptRoot); exit }
[System.Windows.Forms.Application]::EnableVisualStyles()
[System.Windows.Forms.Application]::Run([EtiketAtolyesi.App]::new())
