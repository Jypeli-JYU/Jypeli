;
; Installs Jypeli.
;

Name "MonoJypeli 6.2.1"

OutFile "MonoJypeli_setup.exe"

InstallDir $PROGRAMFILES\MonoJypeli

InstallDirRegKey HKLM "Software\MonoJypeli" "Install_Dir"

RequestExecutionLevel admin


; Headers
!include LogicLib.nsh


; Pages

Page components
Page directory
Page instfiles

UninstPage uninstConfirm
UninstPage instfiles


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
  File "..\Compiled\WindowsGL-AnyCPU\*.dll"
  File "..\Compiled\WindowsGL-AnyCPU\*.xml"
SectionEnd

Section "MonoJypeli for Windows Phone 8.1"
  SetOutPath "$INSTDIR\WP81"
  File "..\Compiled\WindowsPhone81-AnyCPU\*.dll"
  File "..\Compiled\WindowsPhone81-AnyCPU\*.pri"
  File "..\Compiled\WindowsPhone81-AnyCPU\*.xml"
  SetOutPath "$INSTDIR\WP81\MonoGame.Framework"
  File "..\Compiled\WindowsPhone81-AnyCPU\MonoGame.Framework\MonoGame.Framework.xr.xml"
  SetOutPath "$INSTDIR\WP81\MonoGame.Framework\Themes"
  File "..\Compiled\WindowsPhone81-AnyCPU\MonoGame.Framework\Themes\generic.xbf"
SectionEnd


Section "MonoJypeli for Windows Phone 8.0"
  SetOutPath "$INSTDIR\WP8-ARM"
  File "..\Compiled\WindowsPhone-ARM\*.dll"
  File "..\Compiled\WindowsPhone-ARM\*.xml"
  File "..\Compiled\WindowsPhone-ARM\*.winmd"
    
  SetOutPath "$INSTDIR\WP8-x86"
  File "..\Compiled\WindowsPhone-x86\*.dll"
  File "..\Compiled\WindowsPhone-x86\*.xml"
  File "..\Compiled\WindowsPhone-x86\*.winmd"
SectionEnd

Section "MonoJypeli for Windows Store 8 / WinRT"
  SetOutPath "$INSTDIR\Win8"
  File "..\Compiled\Windows8-AnyCPU\*.dll"
  File "..\Compiled\Windows8-AnyCPU\*.pri"
  File "..\Compiled\Windows8-AnyCPU\*.xml"
SectionEnd

Section "MonoJypeli for Linux"
  SetOutPath "$INSTDIR\Linux"
  File "..\Compiled\Linux-AnyCPU\*.dll"
  File "..\Compiled\Linux-AnyCPU\*.xml"
SectionEnd

SubSection "Visual Studio 2013 project templates"

Section "Windows"
  ReadEnvStr $R0 VS120COMNTOOLS
  
  ${If} $R0 != ""
    Push $R0
    Call CopyVsTemplates
  ${Else}
    DetailPrint "Could not find Visual Studio 2013, skipping template installation."
  ${Endif}
SectionEnd

Section "Windows 8 Store / RT"
  ReadEnvStr $R0 VS120COMNTOOLS
  ${If} $R0 != ""
    Push $R0
    Call CopyRTTemplates
  ${Else}
    DetailPrint "Could not find Visual Studio 2013, skipping template installation."
  ${Endif}
SectionEnd

Section "Windows Phone 8.0"
  ReadEnvStr $R0 VS120COMNTOOLS
  ${If} $R0 != ""
    Push $R0
    Call CopyWpTemplates
  ${Else}
    DetailPrint "Could not find Visual Studio 2013, skipping template installation."
  ${Endif}
SectionEnd

Section "Windows Phone 8.1"
  ReadEnvStr $R0 VS120COMNTOOLS
  ${If} $R0 != ""
    Push $R0
    Call CopyWp81Templates
  ${Else}
    DetailPrint "Could not find Visual Studio 2013, skipping template installation."
  ${Endif}
SectionEnd


Section "Run template installer"
  ReadEnvStr $R0 VS120COMNTOOLS
  ${If} $R0 != ""
    DetailPrint "Installing project templates for VS2013 (may take a while)..."
    Push $R0
    Call InstallVsTemplates
  ${Else}
    DetailPrint "Could not find Visual Studio 2013, skipping template installation."
  ${Endif}
SectionEnd

SubSectionEnd

SubSection "Visual Studio 2012 project templates"

Section "Windows"
  ReadEnvStr $R0 VS110COMNTOOLS
  ${If} $R0 != ""
    Push $R0
    Call CopyVsTemplates
  ${Else}
    DetailPrint "Could not find Visual Studio 2012, skipping template installation."
  ${Endif}
SectionEnd

Section "Windows 8 Store / RT"
  ReadEnvStr $R0 VS110COMNTOOLS
  ${If} $R0 != ""
    Push $R0
    Call CopyRTTemplates
  ${Else}
    DetailPrint "Could not find Visual Studio 2012, skipping template installation."
  ${Endif}
SectionEnd

Section "Windows Phone 8.0"
  ReadEnvStr $R0 VS110COMNTOOLS
  ${If} $R0 != ""
    Push $R0
    Call CopyWpTemplates
  ${Else}
    DetailPrint "Could not find Visual Studio 2012, skipping template installation."
  ${Endif}
SectionEnd

Section "Run template installer"
  ReadEnvStr $R0 VS110COMNTOOLS
  ${If} $R0 != ""
    DetailPrint "Installing project templates for VS2012 (may take a while)..."
    Push $R0
    Call InstallVsTemplates
  ${Else}
    DetailPrint "Could not find Visual Studio 2012, skipping template installation."
  ${Endif}
SectionEnd

SubSectionEnd

Function CopyVsTemplates
   Pop $0
   
   IfFileExists "$0..\IDE\VCSExpress\*.*" 0 VSPro
    StrCpy $1 "$0..\IDE\VCSExpress\ProjectTemplates\1033"
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
   
   IfFileExists "$0..\IDE\VCSExpress\*.*" 0 VSPro
    StrCpy $1 "$0..\IDE\VCSExpress\ProjectTemplates\1033"
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

Function CopyWp81Templates
   Pop $0
   
   IfFileExists "$0..\IDE\VCSExpress\*.*" 0 VSPro
    StrCpy $1 "$0..\IDE\VCSExpress\ProjectTemplates\1033"
	SetOutPath $1
    File "..\projektimallit\WP81\*.zip"
	Goto VSPro
	
  VSPro:
	IfFileExists "$0..\IDE\devenv.exe" 0 Done
      StrCpy $1 "$0..\IDE\ProjectTemplates\CSharp\Jypeli-MonoGame\Windows Phone 8.1"
      CreateDirectory $1
      SetOutPath $1
      File "..\projektimallit\WP81\*.zip"

  Done:
FunctionEnd

Function CopyRTTemplates
   Pop $0
   
   IfFileExists "$0..\IDE\VCSExpress\*.*" 0 VSPro
    StrCpy $1 "$0..\IDE\VCSExpress\ProjectTemplates\1033"
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
   
   IfFileExists "$0..\IDE\VCSExpress\*.*" 0 VSPro
     ExecWait '"$0..\IDE\vcsexpress.exe" /installvstemplates'
     Goto VSPro
	
  VSPro:
    IfFileExists "$0..\IDE\devenv.exe" 0 Done
      ExecWait '"$0..\IDE\devenv" /installvstemplates'

  Done:
FunctionEnd

;--------------------------------

Function un.RemoveVsTemplateDir
  Pop $0
  Delete "$0\*.zip"
  Delete "$0\Windows 8\*.zip"
  Delete "$0\Windows Phone 8\*.zip"
  Delete "$0\Windows Phone 8.1\*.zip"
  RMDir "$0\Windows 8"
  RMDir "$0\Windows Phone 8"
  RMDir "$0\Windows Phone 8.1"
  RMDir "$0"  
FunctionEnd

Section "Uninstall"
  
  ; Registry values
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\MonoJypeli"
  DeleteRegKey HKLM "SOFTWARE\MonoJypeli"

  ; Installation dir
  RMDir /r /REBOOTOK $INSTDIR

  ; VS2012 project templates
  ReadEnvStr $R1 VS110COMNTOOLS
  StrCpy $R0 "$R1\..\IDE\ProjectTemplates\CSharp\Jypeli-MonoGame"
  Push $R0  
  Call un.RemoveVsTemplateDir

  Push "$DOCUMENTS\Visual Studio 2012\Templates\ProjectTemplates\Visual C#\Jypeli-MonoGame"
  Call un.RemoveVsTemplateDir

  ; VS2013 project templates
  ReadEnvStr $R1 VS120COMNTOOLS
  StrCpy $R0 "$R1\..\IDE\ProjectTemplates\CSharp\Jypeli-MonoGame"
  Push $R0
  Call un.RemoveVsTemplateDir

  Push "$DOCUMENTS\Visual Studio 2013\Templates\ProjectTemplates\Visual C#\Jypeli-MonoGame"
  Call un.RemoveVsTemplateDir
  
SectionEnd



