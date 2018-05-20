;
; Installs Jypeli.
;

Name "MonoJypeli 6.9.1"

OutFile "MonoJypeli_setup.exe"

InstallDir $PROGRAMFILES\MonoJypeli

InstallDirRegKey HKLM "Software\MonoJypeli" "Install_Dir"

RequestExecutionLevel admin

; Headers
!include LogicLib.nsh
!include nsDialogs.nsh
!include sections.nsh

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

	${NSD_CreateLabel} 10 20 100% 12u "This version of Jypeli depends on a working installation of MonoGame."
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

  WriteUninstaller "$INSTDIR\uninstall.exe"
SectionEnd

Section "MonoJypeli for Windows, DirectX 11" SECTION_WINDOWS_DIRECTX
  SetOutPath "$INSTDIR\Windows"
  File "..\Compiled\Windows-AnyCPU\Jypeli.dll"
  File "..\Compiled\Windows-AnyCPU\Jypeli.xml"
  File "..\Compiled\Windows-AnyCPU\Jypeli.Physics2d.dll"
  File "..\Compiled\Windows-AnyCPU\Jypeli.Physics2d.xml"
  File "..\Compiled\Windows-AnyCPU\Jypeli.SimplePhysics.dll"
  File "..\Compiled\Windows-AnyCPU\Jypeli.SimplePhysics.xml"
SectionEnd

;Section "MonoJypeli for Windows, OpenGL"
;  SetOutPath "$INSTDIR\WindowsGL"
;  File "..\Compiled\WindowsGL-AnyCPU\Jypeli.dll"
;  File "..\Compiled\WindowsGL-AnyCPU\Jypeli.xml"
;  File "..\Compiled\WindowsGL-AnyCPU\Jypeli.Physics2d.dll"
;  File "..\Compiled\WindowsGL-AnyCPU\Jypeli.Physics2d.xml"
;  File "..\Compiled\WindowsGL-AnyCPU\Jypeli.SimplePhysics.dll"
;  File "..\Compiled\WindowsGL-AnyCPU\Jypeli.SimplePhysics.xml"
;SectionEnd

Section "MonoJypeli for Android" SECTION_ANDROID
  SetOutPath "$INSTDIR\Android"
  File "..\Compiled\Android-AnyCPU\Jypeli.dll"
;  File "..\Compiled\Android-AnyCPU\Jypeli.pdb"
  File "..\Compiled\Android-AnyCPU\Jypeli.xml"
  File "..\Compiled\Android-AnyCPU\Jypeli.Physics2d.dll"
;  File "..\Compiled\Android-AnyCPU\Jypeli.Physics2d.pdb"
  File "..\Compiled\Android-AnyCPU\Jypeli.Physics2d.xml"
;  File "..\Compiled\Android-AnyCPU\Jypeli.SimplePhysics.dll"
;  File "..\Compiled\Android-AnyCPU\Jypeli.SimplePhysics.pdb"
;  File "..\Compiled\Android-AnyCPU\Jypeli.SimplePhysics.xml"
SectionEnd

;Section "MonoJypeli for Windows Universal platform"
;  SetOutPath "$INSTDIR\WindowsUniversal"
;  File "..\Compiled\WindowsUniversal-AnyCPU\Jypeli.dll"
;  File "..\Compiled\WindowsUniversal-AnyCPU\Jypeli.xml"
;  File "..\Compiled\WindowsUniversal-AnyCPU\Jypeli.Physics2d.dll"
;  File "..\Compiled\WindowsUniversal-AnyCPU\Jypeli.Physics2d.xml"
;  File "..\Compiled\WindowsUniversal-AnyCPU\Jypeli.SimplePhysics.dll"
;  File "..\Compiled\WindowsUniversal-AnyCPU\Jypeli.SimplePhysics.xml"
;SectionEnd
;
;Section "MonoJypeli for Windows Phone 8.1"
;  SetOutPath "$INSTDIR\WP81"
;  File "..\Compiled\WindowsPhone81-AnyCPU\Jypeli.dll"
;  File "..\Compiled\WindowsPhone81-AnyCPU\Jypeli.xml"
;  File "..\Compiled\WindowsPhone81-AnyCPU\Jypeli.Physics2d.dll"
;  File "..\Compiled\WindowsPhone81-AnyCPU\Jypeli.Physics2d.xml"
;  File "..\Compiled\WindowsPhone81-AnyCPU\Jypeli.SimplePhysics.dll"
;  File "..\Compiled\WindowsPhone81-AnyCPU\Jypeli.SimplePhysics.xml"
;SectionEnd
;
;
;Section "MonoJypeli for Windows Store 8"
;  SetOutPath "$INSTDIR\Win8"
;  File "..\Compiled\Windows8-AnyCPU\Jypeli.dll"
;  File "..\Compiled\Windows8-AnyCPU\Jypeli.xml"
;  File "..\Compiled\Windows8-AnyCPU\Jypeli.Physics2d.dll"
;  File "..\Compiled\Windows8-AnyCPU\Jypeli.Physics2d.xml"
;  File "..\Compiled\Windows8-AnyCPU\Jypeli.SimplePhysics.dll"
;  File "..\Compiled\Windows8-AnyCPU\Jypeli.SimplePhysics.xml"
;SectionEnd
;
;Section "MonoJypeli for Linux"
;  SetOutPath "$INSTDIR\Linux"
;  File "..\Compiled\Linux-AnyCPU\Jypeli.dll"
;  File "..\Compiled\Linux-AnyCPU\Jypeli.xml"
;  File "..\Compiled\Linux-AnyCPU\Jypeli.Physics2d.dll"
;  File "..\Compiled\Linux-AnyCPU\Jypeli.Physics2d.xml"
;  File "..\Compiled\Linux-AnyCPU\Jypeli.SimplePhysics.dll"
;  File "..\Compiled\Linux-AnyCPU\Jypeli.SimplePhysics.xml"
;SectionEnd

Section "MonoJypeli content extensions"
  SetOutPath "C:\Program Files (x86)\MonoJypeli\ContentExtensions"
  File "..\Compiled\ContentExtensions\TextFileContentExtension.dll"
  File "..\Compiled\ContentExtensions\TextFileContentExtension.pdb"
SectionEnd

Section "OpenAL" OpenAL
  SetOutPath "$INSTDIR\WindowsGL"
  File '..\MonoGame\ThirdParty\Dependencies\oalinst.exe'
  ExecWait '"$INSTDIR\WindowsGL\oalinst.exe /S"'
SectionEnd

Section "Visual Studio 2017 Project Templates"
  ; Visual Studio 2017 doesn't create an environment variable, so we need to use vswhere.exe instead
  ; (the link below talks about C, but it's the same thing with C#)
  ; https://blogs.msdn.microsoft.com/vcblog/2017/03/06/finding-the-visual-c-compiler-tools-in-visual-studio-2017/
  ; https://github.com/Microsoft/vswhere
  DetailPrint "Requested template installation for Visual Studio 2017."
  IfFileExists "$PROGRAMFILES32\Microsoft Visual Studio\Installer\vswhere.exe" Find17
    DetailPrint "vswhere.exe not found, template installation failed."
    Goto Done17
  Goto Find17
  
  Find17:
  ; nsExec documentation: http://nsis.sourceforge.net/Docs/nsExec/nsExec.txt
  ; for vswhere.exe command line arguments, run vswhere.exe /?
  ; TODO Check if this also works when multiple VS2017 editions are installed (like Community and Enterprise)
  DetailPrint "Looking up the installation directory of Visual Studio 2017."
  nsExec::ExecToStack '"$PROGRAMFILES32\Microsoft Visual Studio\Installer\vswhere.exe" -nologo -version [15.0,16.0) -property installationPath'
  pop $0
  pop $0
  ${If} $0 == "error"
  ${OrIf} $0 == "timeout"
    DetailPrint "Executing vswhere.exe failed, template installation aborted."
	Goto Done17
  ${Endif}
  
  ; vswhere includes a line-end (CR LF) at the end of the path, so let's cut if off
  ; it could be safer to use a string trim function instead
  StrCpy $0 $0 -2
  
  IfFileExists "$0\Common7\IDE\devenv.exe" Install17
  
  ; Sometimes vswhere.exe can apparently fail to report the correct directory.
  ; If that's the case, we'll check the default directories.
  StrCpy $0 "$PROGRAMFILES32\Microsoft Visual Studio\2017\Community"
  IfFileExists "$0\Common7\IDE\devenv.exe" Install17
  
  StrCpy $0 "$PROGRAMFILES32\Microsoft Visual Studio\2017\Enterprise"
  IfFileExists "$0\Common7\IDE\devenv.exe" 0 Error17
  
  Install17:
  
  ; MSBuild
  CreateDirectory "$PROGRAMFILES32\MSBuild\Jypeli"
  SetOutPath "$PROGRAMFILES32\MSBuild\Jypeli"
  File "..\MGCBTask\bin\Debug\Jypeli.MGCBTask.dll"
  File "..\MSBuildExtension\MonoJypeli.targets"
  
  ; Windows
  ${If} ${SectionIsSelected} ${SECTION_WINDOWS_DIRECTX}
  	DetailPrint "Found VS2017, installing templates..."
  	StrCpy $1 "$0\Common7\IDE\ProjectTemplates\CSharp\Jypeli-Windows"
  	CreateDirectory $1
  	SetOutPath $1
	; Delete outdated .zip templates
	Delete "$1\*.zip"
  	File /r /x *.zip /x Jypeli-Windows.vstman "..\Projektimallit\Windows\"
  	SetOutPath "$0\Common7\IDE\ProjectTemplates"
  	File "..\Projektimallit\Windows\Jypeli-Windows.vstman"
  ${Endif}
  
  ; Android
  ${If} ${SectionIsSelected} ${SECTION_ANDROID}
  	StrCpy $1 "$0\Common7\IDE\ProjectTemplates\CSharp\Jypeli-Android"
  	CreateDirectory $1
  	SetOutPath $1
	; Android can still use ZIP files without issues, since we can't compile
	; Android projects at the university anyway
  	File /r /x *.zip /x Jypeli-Android.vstman "..\Projektimallit\Android\"
  	SetOutPath "$0\Common7\IDE\ProjectTemplates"
  	File "..\Projektimallit\Android\Jypeli-Android.vstman"
  ${Endif}
  
  DetailPrint "Updating template cache, please wait. This could take a couple of minutes."
  ExecWait '"$0\Common7\IDE\devenv.exe" /InstallVSTemplates'
  Goto Done17
  
  Error17:
  DetailPrint "devenv.exe for VS2017 not found!"
  
  Done17:
SectionEnd

;SubSection "Visual Studio 2015 Project Templates" SECTION_VS2015
;
;Section "Windows DirectX" SECTION_VS2015_WINDX
;  ReadEnvStr $R0 VS140COMNTOOLS
;  
;  ${If} $R0 != ""
;    Push $R0
;    Call CopyDxTemplates
;  ${Else}
;    DetailPrint "Could not find Visual Studio 2015, skipping template installation."
;  ${Endif}
;SectionEnd

;Section "Windows OpenGL"
;  ReadEnvStr $R0 VS140COMNTOOLS
;  
;  ${If} $R0 != ""
;    Push $R0
;    Call CopyGLTemplates
;  ${Else}
;    DetailPrint "Could not find Visual Studio 2015, skipping template installation."
;  ${Endif}
;SectionEnd

;Section "Android" SECTION_VS2015_ANDROID
;  ReadEnvStr $R0 VS140COMNTOOLS
;  ${If} $R0 != ""
;    Push $R0
;    Call CopyAndroidTemplates
;  ${Else}
;    DetailPrint "Could not find Visual Studio 2015, skipping template installation."
;  ${Endif}
;SectionEnd

;Section "Windows Universal App"
;  ReadEnvStr $R0 VS140COMNTOOLS
;  ${If} $R0 != ""
;    Push $R0
;    Call CopyUniversalTemplates
;  ${Else}
;    DetailPrint "Could not find Visual Studio 2015, skipping template installation."
;  ${Endif}
;SectionEnd

;Section "Run template installer" SECTION_VS2015_TI
;  ReadEnvStr $R0 VS140COMNTOOLS
;  ${If} $R0 != ""
;    DetailPrint "Installing project templates for VS2015 (may take a while)..."
;    Push $R0
;    Call InstallVsTemplates
;  ${Else}
;    DetailPrint "Could not find Visual Studio 2015, skipping template installation."
;  ${Endif}
;SectionEnd
;
;SubSectionEnd

;Function CopyDxTemplates
;   Pop $0
;   
;   IfFileExists "$0..\IDE\VCSExpress\*.*" 0 VSPro
;    StrCpy $1 "$0..\IDE\VCSExpress\ProjectTemplates\1033"
;    SetOutPath $1
;    File "..\projektimallit\Windows\*.zip"
;    Goto VSPro
;    
;  VSPro:
;    IfFileExists "$0..\IDE\devenv.exe" 0 Done
;      StrCpy $1 "$0..\IDE\ProjectTemplates\CSharp\Jypeli-MonoGame\DirectX 11"
;      CreateDirectory $1
;      SetOutPath $1
;      File "..\projektimallit\Windows\*.zip"
;
;  Done:
;FunctionEnd
;
;Function CopyGLTemplates
;   Pop $0
;   
;   IfFileExists "$0..\IDE\VCSExpress\*.*" 0 VSPro
;    StrCpy $1 "$0..\IDE\VCSExpress\ProjectTemplates\1033"
;    SetOutPath $1
;    File "..\projektimallit\WindowsGL\*.zip"
;    Goto VSPro
;    
;  VSPro:
;    IfFileExists "$0..\IDE\devenv.exe" 0 Done
;      StrCpy $1 "$0..\IDE\ProjectTemplates\CSharp\Jypeli-MonoGame"
;      CreateDirectory $1
;      SetOutPath $1
;      File "..\projektimallit\WindowsGL\*.zip"
;
;  Done:
;FunctionEnd
;
;Function CopyAndroidTemplates
;   Pop $0
;   
;   IfFileExists "$0..\IDE\VCSExpress\*.*" 0 VSPro
;    StrCpy $1 "$0..\IDE\VCSExpress\ProjectTemplates\1033"
;    SetOutPath $1
;    File "..\projektimallit\Android\*.zip"
;    Goto VSPro
;    
;  VSPro:
;    IfFileExists "$0..\IDE\devenv.exe" 0 Done
;      StrCpy $1 "$0..\IDE\ProjectTemplates\CSharp\Jypeli-MonoGame\Android"
;      CreateDirectory $1
;      SetOutPath $1
;      File "..\projektimallit\Android\*.zip"
;
;  Done:
;FunctionEnd
;
;Function CopyUniversalTemplates
;   Pop $0
;   
;   IfFileExists "$0..\IDE\VCSExpress\*.*" 0 VSPro
;    StrCpy $1 "$0..\IDE\VCSExpress\ProjectTemplates\1033"
;    SetOutPath $1
;    File "..\projektimallit\WindowsUniversal\*.zip"
;    Goto VSPro
;    
;  VSPro:
;    IfFileExists "$0..\IDE\devenv.exe" 0 Done
;      StrCpy $1 "$0..\IDE\ProjectTemplates\CSharp\Jypeli-MonoGame\Windows Universal"
;      CreateDirectory $1
;      SetOutPath $1
;      File "..\projektimallit\WindowsUniversal\*.zip"
;
;  Done:
;FunctionEnd
;
;Function InstallVsTemplates
;   Pop $0
;   
;   IfFileExists "$0..\IDE\VCSExpress\*.*" 0 VSPro
;     ExecWait '"$0..\IDE\vcsexpress.exe" /installvstemplates'
;     Goto VSPro
;    
;  VSPro:
;    IfFileExists "$0..\IDE\devenv.exe" 0 Done
;      ExecWait '"$0..\IDE\devenv" /installvstemplates'
;
;  Done:
;FunctionEnd

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

  ; VS 2017 product templates
  DetailPrint "Uninstalling Visual Studio 2017 templates."
  IfFileExists "$PROGRAMFILES32\Microsoft Visual Studio\Installer\vswhere.exe" Find17
    DetailPrint "vswhere.exe not found, skipping template uninstallation."
    Goto Done17
  Goto Find17
  
  Find17:
  ; nsExec documentation: http://nsis.sourceforge.net/Docs/nsExec/nsExec.txt
  ; for vswhere.exe command line arguments, run vswhere.exe /?
  ; TODO Check if this also works when multiple VS2017 editions are installed (like Community and Enterprise)
  DetailPrint "Looking up the installation directory of Visual Studio 2017."
  nsExec::ExecToStack '"$PROGRAMFILES32\Microsoft Visual Studio\Installer\vswhere.exe" -nologo -version [15.0,16.0) -property installationPath'
  pop $0
  pop $0
  ${If} $0 == "error"
  ${OrIf} $0 == "timeout"
    DetailPrint "Executing vswhere.exe failed, template uninstallation aborted."
	Goto Done17
  ${Endif}
  
  ; vswhere includes a line-end (CR LF) at the end of the path, so let's cut if off
  ; it could be safer to use a string trim function instead
  StrCpy $0 $0 -2
  
  IfFileExists "$0\Common7\IDE\devenv.exe" Uninstall17
  
  ; Sometimes vswhere.exe can apparently fail to report the correct directory.
  ; If that's the case, we'll check the default directories.
  StrCpy $0 "$PROGRAMFILES32\Microsoft Visual Studio\2017\Community"
  IfFileExists "$0\Common7\IDE\devenv.exe" Uninstall17
  
  StrCpy $0 "$PROGRAMFILES32\Microsoft Visual Studio\2017\Enterprise"
  IfFileExists "$0\Common7\IDE\devenv.exe" 0 Error17
  
  Uninstall17:
  
  DetailPrint "Found VS2017, uninstalling templates..."
  RMDir /r "$0\Common7\IDE\ProjectTemplates\CSharp\Jypeli-Windows"
  RMDir /r "$0\Common7\IDE\ProjectTemplates\CSharp\Jypeli-Android"
  RMDir /r "$PROGRAMFILES32\MSBuild\Jypeli"
  Delete "$0\Common7\IDE\ProjectTemplates\Jypeli-Windows.vstman"
  Delete "$0\Common7\IDE\ProjectTemplates\Jypeli-Android.vstman"
  DetailPrint "Updating Visual Studio 2017 templates (may take a while)..."
  ExecWait '"$0\Common7\IDE\devenv.exe" /InstallVSTemplates'
  Goto Done17
  
  Error17:
  DetailPrint "devenv.exe for VS2017 not found!"
  
  Done17:
  
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
  ReadEnvStr $R0 VS110COMNTOOLS
  ${If} $R0 != ""
    DetailPrint "Updating Visual Studio 2012 templates (may take a while)..."
    Push $R0
    Call un.InstallVSTemplates
  ${EndIf}}

  ReadEnvStr $R0 VS120COMNTOOLS
  ${If} $R0 != ""
    DetailPrint "Updating Visual Studio 2013 templates (may take a while)..."
    Push $R0
    Call un.InstallVSTemplates
  ${EndIf}}
  
  ReadEnvStr $R0 VS140COMNTOOLS
  ${If} $R0 != ""
    DetailPrint "Updating Visual Studio 2015 templates (may take a while)..."
    Push $R0
    Call un.InstallVSTemplates
  ${EndIf}}

  
SectionEnd

; Since we no longer support VS2015, we don't need this anymore
;Function .onInit
;	ReadEnvStr $R0 VS140COMNTOOLS
;	
;	; If VS 2015 not found, hide and uncheck the related section
;	${If} $R0 == ""
;		IntOp $0 0 | ${SF_EXPAND}
;		SectionSetFlags ${SECTION_VS2015} $0
;		SectionSetFlags ${SECTION_VS2015_WINDX} 0
;		SectionSetFlags ${SECTION_VS2015_ANDROID} 0
;		SectionSetFlags ${SECTION_VS2015_TI} 0
;		SectionSetText ${SECTION_VS2015} ""
;		SectionSetText ${SECTION_VS2015_WINDX} ""
;		SectionSetText ${SECTION_VS2015_ANDROID} ""
;		SectionSetText ${SECTION_VS2015_TI} ""
;	${Endif}
;FunctionEnd
