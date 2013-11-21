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

Section "Jypeli"
  SectionIn RO
  WriteRegStr HKLM "SOFTWARE\MonoJypeli" "Install_Dir" "$INSTDIR"
  
  ; Write the uninstall keys for Windows
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MonoJypeli" "DisplayName" "Jypeli with MonoGame"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MonoJypeli" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MonoJypeli" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MonoJypeli" "NoRepair" 1

  WriteUninstaller "uninstall.exe"
SectionEnd

Section "MonoJypeli for Windows"
  SetOutPath "$INSTDIR\WindowsGL"
  File "..\Compiled\WindowsGL-x86\*.dll"
  File "..\Compiled\WindowsGL-x86\*.xml"
SectionEnd

Section "MonoJypeli for Windows Phone 8"
  SetOutPath "$INSTDIR\WP8-ARM"
  File "..\Compiled\WP8-ARM\*.dll"
  File "..\Compiled\WP8-ARM\*.xml"
    
  SetOutPath "$INSTDIR\WP8-x86"
  File "..\Compiled\WP8-x86\*.dll"
  File "..\Compiled\WP8-x86\*.xml"
SectionEnd

Section "MonoJypeli for Windows Store 8 / WinRT"
  SetOutPath "$INSTDIR\Win8-ARM"
  File "..\Compiled\Win8-ARM\*.dll"
  File "..\Compiled\Win8-ARM\*.xml"
    
  SetOutPath "$INSTDIR\Win8-x86"
  File "..\Compiled\Win8-x86\*.dll"
  File "..\Compiled\Win8-x86\*.xml"
SectionEnd

Section "MonoJypeli for Linux"
  SetOutPath "$INSTDIR\Linux"
  File "..\Compiled\Linux-x86\*.dll"
  File "..\Compiled\Linux-x86\*.xml"
SectionEnd

SubSection "Visual Studio 2012 project templates"

Section "Windows"
  ReadEnvStr $0 VS110COMNTOOLS
  Push $0
  Call CopyVsTemplates
SectionEnd

Section "Windows 8 Store / RT"
  ReadEnvStr $0 VS110COMNTOOLS
  Push $0
  Call CopyRTTemplates
SectionEnd

Section "Windows Phone 8"
  ReadEnvStr $0 VS110COMNTOOLS
  Push $0
  Call CopyWpTemplates
SectionEnd

Section "Run template installer"
  DetailPrint "Installing project templates for VS2012 (may take a while)..."
  ReadEnvStr $0 VS110COMNTOOLS
  Push $0
  Call InstallVsTemplates
SectionEnd

SubSectionEnd

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

Function CopyRTTemplates
   Pop $0
   
   IfFileExists "$0..\IDE\VCSExpress\*.*" 0 VsExpressForPhone
    StrCpy $1 "$0..\IDE\VCSExpress\ProjectTemplates\1033"
	SetOutPath $1
    File "..\projektimallit\Win8\*.zip"
	Goto VsExpressForPhone
  
  VsExpressForPhone:
	ReadRegStr $5 HKLM "Software\Microsoft\VPDExpress\10.0" "InstallDir"
	StrCmp $5 "" VSPro 0	
    CreateDirectory "$5\VPDExpress\ProjectTemplates\Jypeli-MonoGame\Windows 8\1033"
	StrCpy $1 "$5\VPDExpress\ProjectTemplates\Jypeli-MonoGame\Windows 8\1033"
	SetOutPath $1
    File "..\projektimallit\Win8\*.zip"
	Goto VSPro
	
  VSPro:
	IfFileExists "$0..\IDE\devenv.exe" 0 Done
      StrCpy $1 "$0..\IDE\ProjectTemplates\CSharp\Jypeli-MonoGame\Windows 8"
      CreateDirectory $1
      SetOutPath $1
      File "..\projektimallit\Win8\*.zip"

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
