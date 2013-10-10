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
  
  SetOutPath "$INSTDIR\Common"
  File "Common\Jypeli.Physics2d.dll"
  File "Common\Jypeli.Physics2d.xml"
  File "Common\Jypeli.SimplePhysics.dll"
  File "Common\Jypeli.SimplePhysics.xml"
  
  SetOutPath "$INSTDIR\WindowsGL"
  File "WindowsGL\Jypeli.dll"
  File "WindowsGL\Jypeli.xml"
  File "WindowsGL\Jypeli.MonoGame.Framework.dll"
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

Section "MonoJypeli for Windows Phone 8 (VS2012)"
  SetOutPath "$INSTDIR\WP8-ARM"
  File "WP8-ARM\*.*"
    
  SetOutPath "$INSTDIR\WP8-x86"
  File "WP8-x86\*.*"
    
  ReadEnvStr $0 VS110COMNTOOLS
  Push $0
  Call CopyWpTemplates
SectionEnd

Section "Project templates for VS2012"
  ReadEnvStr $0 VS110COMNTOOLS
  Push $0
  Call CopyVsTemplates
  
  DetailPrint "Installing project templates for VS2012 (may take a while)..."
  ReadEnvStr $0 VS110COMNTOOLS
  Push $0
  Call InstallVsTemplates
SectionEnd

Function CopyVsTemplates
   Pop $0
   
   IfFileExists "$0..\IDE\VCSExpress\*.*" 0 VsExpressForPhone
    StrCpy $1 "$0..\IDE\VCSExpress\ProjectTemplates\1033"
	SetOutPath $1
    File "..\projektimallit\VS2012\*.zip"
	Goto VsExpressForPhone
  
  VsExpressForPhone:
	ReadRegStr $5 HKLM "Software\Microsoft\VPDExpress\10.0" "InstallDir"
	StrCmp $5 "" VSPro 0	
    CreateDirectory "$5\VPDExpress\ProjectTemplates\Jypeli-MonoGame\1033"
	StrCpy $1 "$5\VPDExpress\ProjectTemplates\Jypeli-MonoGame\1033"
	SetOutPath $1
    File "..\projektimallit\VS2012\*.zip"
	Goto VSPro
	
  VSPro:
	IfFileExists "$0..\IDE\devenv.exe" 0 Done
      StrCpy $1 "$0..\IDE\ProjectTemplates\CSharp\Jypeli-MonoGame"
      CreateDirectory $1
      SetOutPath $1
      File "..\projektimallit\VS2012\*.zip"

  Done:
FunctionEnd

Function CopyWpTemplates
   Pop $0
   
   IfFileExists "$0..\IDE\VCSExpress\*.*" 0 VsExpressForPhone
    StrCpy $1 "$0..\IDE\VCSExpress\ProjectTemplates\1033"
	SetOutPath $1
    File "..\projektimallit\WP8\*.zip"
	Goto VsExpressForPhone
  
  VsExpressForPhone:
	ReadRegStr $5 HKLM "Software\Microsoft\VPDExpress\10.0" "InstallDir"
	StrCmp $5 "" VSPro 0	
    CreateDirectory "$5\VPDExpress\ProjectTemplates\Jypeli-MonoGame\Windows Phone 8\1033"
	StrCpy $1 "$5\VPDExpress\ProjectTemplates\Jypeli-MonoGame\Windows Phone 8\1033"
	SetOutPath $1
    File "..\projektimallit\WP8\*.zip"
	Goto VSPro
	
  VSPro:
	IfFileExists "$0..\IDE\devenv.exe" 0 Done
      StrCpy $1 "$0..\IDE\ProjectTemplates\CSharp\Jypeli-MonoGame\Windows Phone 8"
      CreateDirectory $1
      SetOutPath $1
      File "..\projektimallit\WP8\*.zip"

  Done:
FunctionEnd


Function InstallVsTemplates
   Pop $0
   
   IfFileExists "$0..\IDE\VCSExpress\*.*" 0 VsExpressForPhone
	 ExecWait '"$0..\IDE\vcsexpress.exe" /installvstemplates'
	 Goto VsExpressForPhone
  
  VsExpressForPhone:
	ReadRegStr $5 HKLM "Software\Microsoft\VPDExpress\10.0" "InstallDir"
	StrCmp $5 "" VSPro 0	
	ExecWait '"$5\VPDExpress.exe" /installvstemplates'    	
	Goto VSPro
	
  VSPro:
	IfFileExists "$0..\IDE\devenv.exe" 0 Done
      ExecWait '"$0..\IDE\devenv" /installvstemplates'

  Done:
FunctionEnd

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
