;
; Installs Jypeli.
;

Name "MonoJypeli 7.0.3"

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

  ; MSBuild
  CreateDirectory "$PROGRAMFILES32\MSBuild\Jypeli"
  SetOutPath "$PROGRAMFILES32\MSBuild\Jypeli"
  File "..\MSBuildExtension\MGCBTask\bin\Debug\Jypeli.MGCBTask.dll"
  File "..\MSBuildExtension\MonoJypeli.targets"
  
  CreateDirectory $INSTDIR
  ClearErrors
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

Section "MonoJypeli for Android" SECTION_ANDROID
  SetOutPath "$INSTDIR\Android"
  File "..\Compiled\Android-AnyCPU\Jypeli.dll"
  File "..\Compiled\Android-AnyCPU\Jypeli.xml"
  File "..\Compiled\Android-AnyCPU\Jypeli.Physics2d.dll"
  File "..\Compiled\Android-AnyCPU\Jypeli.Physics2d.xml"
SectionEnd

Section "MonoJypeli content extensions"
  SetOutPath "C:\Program Files (x86)\MonoJypeli\ContentExtensions"
  File "..\Compiled\ContentExtensions\TextFileContentExtension.dll"
  File "..\Compiled\ContentExtensions\TextFileContentExtension.pdb"
SectionEnd

Section "OpenAL" OpenAL
  SetOutPath "$INSTDIR\WindowsGL"
  File '..\Precompiled\oalinst.exe'
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

Section "Visual Studio 2019 Project Templates"
  DetailPrint "Requested template installation for Visual Studio 2019."
  
  StrCpy $1 "$DOCUMENTS\Visual Studio 2019\Templates\ProjectTemplates\Visual C#"
  IfFileExists "$1\*.*" Install19 Error19
  
  Install19:
  DetailPrint "Found VS2019, installing templates..."
  CreateDirectory "$1\Jypeli"
  
  ; Windows
  CreateDirectory "$1\Jypeli\Windows"
  SetOutPath "$1\Jypeli\Windows"
  File /r /x *.zip /x Jypeli-Windows.vstman "..\Projektimallit\Windows\"
  
  ; Android
  CreateDirectory "$1\Jypeli\Android"
  SetOutPath "$1\Jypeli\Android"
  File /r /x *.zip /x Jypeli-Android.vstman "..\Projektimallit\Android\"
  
  Goto Done19
  
  Error19:
  DetailPrint "Visual Studio 2019 template directory not found! (Documents\Visual Studio 2019\Templates\ProjectTemplates\Visual C#)"
  
  Done19:
SectionEnd

;--------------------------------

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
  
  ; VS2019 project templates
  StrCpy $1 "$DOCUMENTS\Visual Studio 2019\Templates\ProjectTemplates\Visual C#\Jypeli"
  IfFileExists "$1\*.*" Uninstall19 Error19
  
  Uninstall19:
  DetailPrint "Visual Studio 2019 templates found, deleting them."
  RMDir /r "$1"
  
  Goto Done19
  
  Error19:
  DetailPrint "Visual Studio 2019 templates not found."
  
  Done19:
  DetailPrint "Uninstallation complete."
SectionEnd

