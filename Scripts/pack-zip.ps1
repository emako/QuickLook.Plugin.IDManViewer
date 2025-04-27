Remove-Item ..\QuickLook.Plugin.IDManViewer.qlplugin -ErrorAction SilentlyContinue

$files = Get-ChildItem -Path ..\Build\Release\ -Exclude *.pdb,*.xml
Compress-Archive $files ..\QuickLook.Plugin.IDManViewer.zip
Move-Item ..\QuickLook.Plugin.IDManViewer.zip ..\QuickLook.Plugin.IDManViewer.qlplugin