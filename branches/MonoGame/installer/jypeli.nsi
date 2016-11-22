;
; Installs Jypeli.
;

Name "MonoJypeli 6.8.5"

OutFile "MonoJypeli_setup.exe"

InstallDir $PROGRAMFILES\MonoJypeli

InstallDirRegKey HKLM "Software\MonoJypeli" "Install_Dir"

RequestExecutionLevel admin

; Headers
!include LogicLib.nsh
!include nsDialogs.nsh


; Globals
Var MonoGameInstalled


; Pages
Page custom ChkPrqCreate ChkPrqLeave
Page components
Page directory
Page instfiles

UninstPage uninstConfirm
UninstPage instfiles


Function ChkPrqHelper
	StrCpy $1 "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MonoGame"
	ReadRegStr $2 HKLM "$1" "DisplayName"
	IfErrors CheckWOW32 MonoGameDetected

	CheckWOW32:
	StrCpy $1 "SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\MonoGame"
	ReadRegStr $2 HKLM "$1" "DisplayName"
	IfErrors NoMonoGame MonoGameDetected

	NoMonoGame:
	GetDlgItem $1 $hwndparent 1
	StrCpy $MonoGameInstalled 0
	${NSD_SetText} $1 "&Try again"
	goto ChkPrqHelper_Return

	MonoGameDetected:
	GetDlgItem $1 $hwndparent 1
	${NSD_SetText} $1 "$(^NextBtn)"
	StrCpy $MonoGameInstalled 1
	
	ChkPrqHelper_Return:
FunctionEnd


Function ChkPrqLeave
	call ChkPrqHelper
	${If} $MonoGameInstalled < 1
	Abort
	${EndIf}
FunctionEnd


Function ChkPrqCreate
	nsDialogs::Create 1018
	Pop $0

	${NSD_CreateLabel} 10 0 100% 12u "The installer could not find MonoGame on your system."
	Pop $0

	${NSD_CreateLabel} 10 20 100% 12u "This version of Jypeli depends heavily on a working installation of MonoGame."
	Pop $0

	${NSD_CreateLink} 10 50 100% 12u "Click here to go to the download site"
	Pop $0
	${NSD_OnClick} $0 onClickMonogameLink

	${NSD_CreateLabel} 10 150 100% 20u "You may leave this window open during the MonoGame installation and continue when it's complete."
	Pop $0

	Call ChkPrqHelper
	${If} $MonoGameInstalled < 1
		nsDialogs::Show
	${EndIf}
FunctionEnd


Function onClickMonogameLink
	Pop $0 ; don't forget to pop HWND of the stack
	ExecShell "open" "http://monogame.net/downloads/"
FunctionEnd
	

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

Section "MonoJypeli for Windows, DirectX 11"
  SetOutPath "$INSTDIR\Windows"
  File "..\Compiled\Windows-AnyCPU\Jypeli.dll"
  File "..\Compiled\Windows-AnyCPU\Jypeli.xml"
  File "..\Compiled\Windows-AnyCPU\Jypeli.Physics2d.dll"
  File "..\Compiled\Windows-AnyCPU\Jypeli.Physics2d.xml"
  File "..\Compiled\Windows-AnyCPU\Jypeli.SimplePhysics.dll"
  File "..\Compiled\Windows-AnyCPU\Jypeli.SimplePhysics.xml"
SectionEnd

Section "MonoJypeli for Windows, OpenGL"
  SetOutPath "$INSTDIR\WindowsGL"
  File "..\Compiled\WindowsGL-AnyCPU\Jypeli.dll"
  File "..\Compiled\WindowsGL-AnyCPU\Jypeli.xml"
  File "..\Compiled\WindowsGL-AnyCPU\Jypeli.Physics2d.dll"
  File "..\Compiled\WindowsGL-AnyCPU\Jypeli.Physics2d.xml"
  File "..\Compiled\WindowsGL-AnyCPU\Jypeli.SimplePhysics.dll"
  File "..\Compiled\WindowsGL-AnyCPU\Jypeli.SimplePhysics.xml"
SectionEnd

Section "MonoJypeli for Android"
  SetOutPath "$INSTDIR\Android"
  File "..\Compiled\Android-AnyCPU\Jypeli.dll"
  File "..\Compiled\Android-AnyCPU\Jypeli.pdb"
  File "..\Compiled\Android-AnyCPU\Jypeli.xml"
  File "..\Compiled\Android-AnyCPU\Jypeli.Physics2d.dll"
  File "..\Compiled\Android-AnyCPU\Jypeli.Physics2d.pdb"
  File "..\Compiled\Android-AnyCPU\Jypeli.Physics2d.xml"
  File "..\Compiled\Android-AnyCPU\Jypeli.SimplePhysics.dll"
  File "..\Compiled\Android-AnyCPU\Jypeli.SimplePhysics.pdb"
  File "..\Compiled\Android-AnyCPU\Jypeli.SimplePhysics.xml"
SectionEnd

Section "MonoJypeli for Windows Universal platform"
  SetOutPath "$INSTDIR\WindowsUniversal"
  File "..\Compiled\WindowsUniversal-AnyCPU\Jypeli.dll"
  File "..\Compiled\WindowsUniversal-AnyCPU\Jypeli.xml"
  File "..\Compiled\WindowsUniversal-AnyCPU\Jypeli.Physics2d.dll"
  File "..\Compiled\WindowsUniversal-AnyCPU\Jypeli.Physics2d.xml"
  File "..\Compiled\WindowsUniversal-AnyCPU\Jypeli.SimplePhysics.dll"
  File "..\Compiled\WindowsUniversal-AnyCPU\Jypeli.SimplePhysics.xml"
SectionEnd

Section "MonoJypeli for Windows Phone 8.1"
  SetOutPath "$INSTDIR\WP81"
  File "..\Compiled\WindowsPhone81-AnyCPU\Jypeli.dll"
  File "..\Compiled\WindowsPhone81-AnyCPU\Jypeli.xml"
  File "..\Compiled\WindowsPhone81-AnyCPU\Jypeli.Physics2d.dll"
  File "..\Compiled\WindowsPhone81-AnyCPU\Jypeli.Physics2d.xml"
  File "..\Compiled\WindowsPhone81-AnyCPU\Jypeli.SimplePhysics.dll"
  File "..\Compiled\WindowsPhone81-AnyCPU\Jypeli.SimplePhysics.xml"
SectionEnd


Section "MonoJypeli for Windows Store 8"
  SetOutPath "$INSTDIR\Win8"
  File "..\Compiled\Windows8-AnyCPU\Jypeli.dll"
  File "..\Compiled\Windows8-AnyCPU\Jypeli.xml"
  File "..\Compiled\Windows8-AnyCPU\Jypeli.Physics2d.dll"
  File "..\Compiled\Windows8-AnyCPU\Jypeli.Physics2d.xml"
  File "..\Compiled\Windows8-AnyCPU\Jypeli.SimplePhysics.dll"
  File "..\Compiled\Windows8-AnyCPU\Jypeli.SimplePhysics.xml"
SectionEnd

Section "MonoJypeli for Linux"
  SetOutPath "$INSTDIR\Linux"
  File "..\Compiled\Linux-AnyCPU\Jypeli.dll"
  File "..\Compiled\Linux-AnyCPU\Jypeli.xml"
  File "..\Compiled\Linux-AnyCPU\Jypeli.Physics2d.dll"
  File "..\Compiled\Linux-AnyCPU\Jypeli.Physics2d.xml"
  File "..\Compiled\Linux-AnyCPU\Jypeli.SimplePhysics.dll"
  File "..\Compiled\Linux-AnyCPU\Jypeli.SimplePhysics.xml"
SectionEnd

Section "MonoJypeli content extensions"
  SetOutPath "C:\Program Files (x86)\MonoJypeli\ContentExtensions"
  File "..\Compiled\ContentExtensions\*"
SectionEnd

Section "OpenAL" OpenAL
  SetOutPath "$INSTDIR\WindowsGL"
  File '..\MonoGame\ThirdParty\Dependencies\oalinst.exe'
  ExecWait '"$INSTDIR\WindowsGL\oalinst.exe /S"'
SectionEnd

SubSection "Visual Studio 2015 project templates"

Section "Windows DirectX"
  ReadEnvStr $R0 VS140COMNTOOLS
  
  ${If} $R0 != ""
    Push $R0
    Call CopyDxTemplates
  ${Else}
    DetailPrint "Could not find Visual Studio 2015, skipping template installation."
  ${Endif}
SectionEnd

Section "Windows OpenGL"
  ReadEnvStr $R0 VS140COMNTOOLS
  
  ${If} $R0 != ""
    Push $R0
    Call CopyGLTemplates
  ${Else}
    DetailPrint "Could not find Visual Studio 2015, skipping template installation."
  ${Endif}
SectionEnd

Section "Android"
  ReadEnvStr $R0 VS140COMNTOOLS
  ${If} $R0 != ""
    Push $R0
    Call CopyAndroidTemplates
  ${Else}
    DetailPrint "Could not find Visual Studio 2015, skipping template installation."
  ${Endif}
SectionEnd

Section "Windows Universal App"
  ReadEnvStr $R0 VS140COMNTOOLS
  ${If} $R0 != ""
    Push $R0
    Call CopyUniversalTemplates
  ${Else}
    DetailPrint "Could not find Visual Studio 2015, skipping template installation."
  ${Endif}
SectionEnd

Section "Windows Phone 8.1"
  ReadEnvStr $R0 VS140COMNTOOLS
  ${If} $R0 != ""
    Push $R0
    Call CopyWp81Templates
  ${Else}
    DetailPrint "Could not find Visual Studio 2015, skipping template installation."
  ${Endif}
SectionEnd

Section "Windows Store 8"
  ReadEnvStr $R0 VS140COMNTOOLS
  ${If} $R0 != ""
    Push $R0
    Call CopyRTTemplates
  ${Else}
    DetailPrint "Could not find Visual Studio 2015, skipping template installation."
  ${Endif}
SectionEnd

Section "Run template installer"
  ReadEnvStr $R0 VS140COMNTOOLS
  ${If} $R0 != ""
    DetailPrint "Installing project templates for VS2015 (may take a while)..."
    Push $R0
    Call InstallVsTemplates
  ${Else}
    DetailPrint "Could not find Visual Studio 2015, skipping template installation."
  ${Endif}
SectionEnd

SubSectionEnd

SubSection "Visual Studio 2013 project templates"

Section "Windows DirectX"
  ReadEnvStr $R0 VS120COMNTOOLS
  
  ${If} $R0 != ""
    Push $R0
    Call CopyDxTemplates
  ${Else}
    DetailPrint "Could not find Visual Studio 2013, skipping template installation."
  ${Endif}
SectionEnd

Section "Windows OpenGL"
  ReadEnvStr $R0 VS120COMNTOOLS
  
  ${If} $R0 != ""
    Push $R0
    Call CopyGLTemplates
  ${Else}
    DetailPrint "Could not find Visual Studio 2013, skipping template installation."
  ${Endif}
SectionEnd

Section "Android"
  ReadEnvStr $R0 VS120COMNTOOLS
  ${If} $R0 != ""
    Push $R0
    Call CopyAndroidTemplates
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

Section "Windows Store 8"
  ReadEnvStr $R0 VS120COMNTOOLS
  ${If} $R0 != ""
    Push $R0
    Call CopyRTTemplates
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

Section "Windows DirectX"
  ReadEnvStr $R0 VS110COMNTOOLS
  ${If} $R0 != ""
    Push $R0
    Call CopyDxTemplates
  ${Else}
    DetailPrint "Could not find Visual Studio 2012, skipping template installation."
  ${Endif}
SectionEnd

Section "WindowsGL"
  ReadEnvStr $R0 VS110COMNTOOLS
  ${If} $R0 != ""
    Push $R0
    Call CopyGLTemplates
  ${Else}
    DetailPrint "Could not find Visual Studio 2012, skipping template installation."
  ${Endif}
SectionEnd

Section "Windows Store 8"
  ReadEnvStr $R0 VS110COMNTOOLS
  ${If} $R0 != ""
    Push $R0
    Call CopyRTTemplates
  ${Else}
    DetailPrint "Could not find Visual Studio 2012, skipping template installation."
  ${Endif}
SectionEnd

SubSectionEnd

Function CopyDxTemplates
   Pop $0
   
   IfFileExists "$0..\IDE\VCSExpress\*.*" 0 VSPro
    StrCpy $1 "$0..\IDE\VCSExpress\ProjectTemplates\1033"
    SetOutPath $1
    File "..\projektimallit\Windows\*.zip"
    Goto VSPro
    
  VSPro:
    IfFileExists "$0..\IDE\devenv.exe" 0 Done
      StrCpy $1 "$0..\IDE\ProjectTemplates\CSharp\Jypeli-MonoGame\DirectX 11"
      CreateDirectory $1
      SetOutPath $1
      File "..\projektimallit\Windows\*.zip"

  Done:
FunctionEnd

Function CopyGLTemplates
   Pop $0
   
   IfFileExists "$0..\IDE\VCSExpress\*.*" 0 VSPro
    StrCpy $1 "$0..\IDE\VCSExpress\ProjectTemplates\1033"
    SetOutPath $1
    File "..\projektimallit\WindowsGL\*.zip"
    Goto VSPro
    
  VSPro:
    IfFileExists "$0..\IDE\devenv.exe" 0 Done
      StrCpy $1 "$0..\IDE\ProjectTemplates\CSharp\Jypeli-MonoGame"
      CreateDirectory $1
      SetOutPath $1
      File "..\projektimallit\WindowsGL\*.zip"

  Done:
FunctionEnd

Function CopyAndroidTemplates
   Pop $0
   
   IfFileExists "$0..\IDE\VCSExpress\*.*" 0 VSPro
    StrCpy $1 "$0..\IDE\VCSExpress\ProjectTemplates\1033"
    SetOutPath $1
    File "..\projektimallit\Android\*.zip"
    Goto VSPro
    
  VSPro:
    IfFileExists "$0..\IDE\devenv.exe" 0 Done
      StrCpy $1 "$0..\IDE\ProjectTemplates\CSharp\Jypeli-MonoGame\Android"
      CreateDirectory $1
      SetOutPath $1
      File "..\projektimallit\Android\*.zip"

  Done:
FunctionEnd

Function CopyUniversalTemplates
   Pop $0
   
   IfFileExists "$0..\IDE\VCSExpress\*.*" 0 VSPro
    StrCpy $1 "$0..\IDE\VCSExpress\ProjectTemplates\1033"
    SetOutPath $1
    File "..\projektimallit\WindowsUniversal\*.zip"
    Goto VSPro
    
  VSPro:
    IfFileExists "$0..\IDE\devenv.exe" 0 Done
      StrCpy $1 "$0..\IDE\ProjectTemplates\CSharp\Jypeli-MonoGame\Windows Universal"
      CreateDirectory $1
      SetOutPath $1
      File "..\projektimallit\WindowsUniversal\*.zip"

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
      RMDir /r /REBOOTOK "$0..\IDE\ProjectTemplates\CSharp\Jypeli-MonoGame\Windows 8"
      StrCpy $1 "$0..\IDE\ProjectTemplates\CSharp\Jypeli-MonoGame\Windows 8.1"
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
  Delete "$0\Android\*.zip"
  Delete "$0\DirectX 11\*.zip"
  Delete "$0\Windows OpenGL\*.zip"
  Delete "$0\Linux\*.zip"
  Delete "$0\Windows 8\*.zip"
  Delete "$0\Windows 8.1\*.zip"
  Delete "$0\Windows Phone 8\*.zip"
  Delete "$0\Windows Phone 8.1\*.zip"
  Delete "$0\Windows Universal\*.zip"

  RMDir "$0\Android"
  RMDir "$0\DirectX 11"
  RMDir "$0\Windows OpenGL"
  RMDir "$0\Linux"
  RMDir "$0\Windows 8"
  RMDir "$0\Windows 8.1"
  RMDir "$0\Windows Phone 8"
  RMDir "$0\Windows Phone 8.1"
  RMDir "$0\Windows Universal"
  RMDir "$0"  
FunctionEnd

Function un.InstallVsTemplates
   Pop $0
   
   IfFileExists "$0..\IDE\VCSExpress\*.*" 0 VSPro
     ExecWait '"$0..\IDE\vcsexpress.exe" /installvstemplates'
     Goto VSPro
    
  VSPro:
    IfFileExists "$0..\IDE\devenv.exe" 0 Done
      ExecWait '"$0..\IDE\devenv" /installvstemplates'

  Done:
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

  ; VS2015 project templates
  ReadEnvStr $R1 VS140COMNTOOLS
  StrCpy $R0 "$R1\..\IDE\ProjectTemplates\CSharp\Jypeli-MonoGame"
  Push $R0
  Call un.RemoveVsTemplateDir

  Push "$DOCUMENTS\Visual Studio 2015\Templates\ProjectTemplates\Visual C#\Jypeli-MonoGame"
  Call un.RemoveVsTemplateDir
  
  ; Update VS template caches
  ReadEnvStr $R1 VS110COMNTOOLS
  DetailPrint "Updating Visual Studio 2012 templates (may take a while)..."
  Call un.InstallVSTemplates
  ReadEnvStr $R1 VS120COMNTOOLS
  DetailPrint "Updating Visual Studio 2013 templates (may take a while)..."
  Call un.InstallVSTemplates
  ReadEnvStr $R1 VS140COMNTOOLS
  DetailPrint "Updating Visual Studio 2015 templates (may take a while)..."
  Call un.InstallVSTemplates  

  
SectionEnd
