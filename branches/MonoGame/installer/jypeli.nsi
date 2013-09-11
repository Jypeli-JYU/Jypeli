;
; Installs Jypeli.
;

Name "The Jypeli Game Programming Library with MonoGame"

OutFile "MonoJypeli_setup.exe"

InstallDir $PROGRAMFILES\MonoJypeli

InstallDirRegKey HKLM "Software\MonoJypeli" "Install_Dir"

RequestExecutionLevel admin

;--------------------------------

; Pages

Page components
Page directory
Page instfiles

UninstPage uninstConfirm
UninstPage instfiles

;--------------------------------

Section "MonoJypeli for VS2012"
  SectionIn RO
  
  SetOutPath "$INSTDIR\WindowsGL"
  
  File "WindowsGL\Jypeli.dll"
  File "WindowsGL\Jypeli.xml"
  File "WindowsGL\Jypeli.Physics2d.dll"
  File "WindowsGL\Jypeli.Physics2d.xml"
  File "WindowsGL\Jypeli.SimplePhysics.dll"
  File "WindowsGL\Jypeli.SimplePhysics.xml"
  File "WindowsGL\MonoGame.Framework.dll"
  File "WindowsGL\Lidgren.Network.dll"
  File "WindowsGL\Tao.Sdl.dll"
  File "WindowsGL\OpenTK.dll"
  File "WindowsGL\SDL.dll"
    
  WriteRegStr HKLM "SOFTWARE\MonoJypeli" "Install_Dir" "$INSTDIR"
  
  ; Write the uninstall keys for Windows
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MonoJypeli" "DisplayName" "Jypeli with MonoGame"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MonoJypeli" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MonoJypeli" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MonoJypeli" "NoRepair" 1

  WriteUninstaller "uninstall.exe"
SectionEnd

Section "Project templates for VS2012"
  SetOutPath "$DOCUMENTS\Visual Studio 2012\Templates\ProjectTemplates\Visual C#\Jypeli-MonoGame"
  File "..\projektimallit\VS2012\*.zip"
SectionEnd

;--------------------------------


Section "Uninstall"
  
  ; Register values
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MonoJypeli"
  DeleteRegKey HKLM "SOFTWARE\MonoJypeli"

  ; Installation dir
  RMDir /r /REBOOTOK $INSTDIR

  ; Project templates
  ReadEnvStr $0 VS100COMNTOOLS
  StrCpy $1 "$DOCUMENTS\Visual Studio 2012\Templates\ProjectTemplates\Visual C#\Jypeli-MonoGame"
  Delete "$1\*.zip"
  RMDir "$1"
  
SectionEnd
