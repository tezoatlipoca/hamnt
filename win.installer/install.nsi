; Basic installer script for hamnt
!include "MUI2.nsh"

; General Settings
Name "Hamnt"
OutFile "hamnt-0.2.1-setup.exe"
InstallDir "$PROGRAMFILES\Hamnt"
InstallDirRegKey HKCU "Software\Hamnt" ""
RequestExecutionLevel admin

; Version Information
VIProductVersion "0.2.1.0"
VIAddVersionKey "ProductName" "Hamnt"
VIAddVersionKey "FileDescription" "HyperAggressively Minimal Note Taking app"
VIAddVersionKey "LegalCopyright" "© tezoatlipoca@gmail.com"
VIAddVersionKey "FileVersion" "0.2.1"

; Interface Settings
!define MUI_ABORTWARNING
!define MUI_ICON "hamnt.ico" ; Optional: Replace with your icon path

; Pages
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

; Languages
!insertmacro MUI_LANGUAGE "English"

; Add a page to ask for install scope
Var InstallScope

Page Custom ScopePageCreate ScopePageLeave

Function ScopePageCreate
    nsDialogs::Create 1018
    Pop $0

    ${If} $0 == error
        Abort
    ${EndIf}

    ${NSD_CreateRadioButton} 0u 0u 100% 12u "Install for all users (requires admin)"
    Pop $1
    ${NSD_SetState} $1 ${BST_CHECKED}
    StrCpy $InstallScope "ALL"

    ${NSD_OnClick} $1 ScopeAllClicked

    ${NSD_CreateRadioButton} 0u 14u 100% 12u "Install for current user only"
    Pop $2
    ${NSD_OnClick} $2 ScopeCurrentClicked

    nsDialogs::Show
FunctionEnd

Function ScopeAllClicked
    StrCpy $InstallScope "ALL"
FunctionEnd

Function ScopeCurrentClicked
    StrCpy $InstallScope "CURRENT"
FunctionEnd

Function ScopePageLeave
    ; nothing needed, $InstallScope is set
FunctionEnd

; Installer Sections
Section "Install"
  SetOutPath "$INSTDIR"

  ; Path to the binary we want to package
  DetailPrint "Packaging binary at: hamnt.exe"

  ; Check if the binary exists before proceeding
  IfFileExists "hamnt.exe" +2 0
    MessageBox MB_ICONSTOP "ERROR: Binary not found at hamnt.exe. Aborting installation."
    Abort

  ; Add files
  File /oname=hamnt.exe "hamnt.exe"

  ; Check for existing version
  ReadRegStr $R0 HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Hamnt" "DisplayVersion"
  StrCpy $R1 "0.2.1" ; This installer's version

  ; Compare versions (simple string compare; for more complex versioning, use a plugin)
  ${If} $R0 != ""
    ${If} $R0 > $R1
      MessageBox MB_ICONSTOP "A newer version ($R0) of Hamnt is already installed. Installation aborted."
      Abort
    ${EndIf}
  ${EndIf}

 

  ; Create Start Menu shortcuts
  CreateDirectory "$SMPROGRAMS\Hamnt"
  CreateShortcut "$SMPROGRAMS\Hamnt\Hamnt.lnk" "$INSTDIR\hamnt.exe"
  CreateShortcut "$SMPROGRAMS\Hamnt\Uninstall.lnk" "$INSTDIR\uninstall.exe"

  ; Create uninstaller
  WriteUninstaller "$INSTDIR\uninstall.exe"

  ; Write registry keys for uninstall
  ${If} $InstallScope == "ALL"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Hamnt" "DisplayName" "Hamnt"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Hamnt" "UninstallString" "$\"$INSTDIR\uninstall.exe$\""
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Hamnt" "DisplayVersion" "0.2.1"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Hamnt" "Publisher" "tezoatlipoca"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Hamnt" "InstallLocation" "$INSTDIR"
    ; Add install directory to system PATH
    ReadRegStr $0 HKLM "SYSTEM\CurrentControlSet\Control\Session Manager\Environment" "Path"
    StrCpy $1 "$INSTDIR;$0"
    WriteRegStr HKLM "SYSTEM\CurrentControlSet\Control\Session Manager\Environment" "Path" "$1"
  ${Else}
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\Hamnt" "DisplayName" "Hamnt"
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\Hamnt" "UninstallString" "$\"$INSTDIR\uninstall.exe$\""
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\Hamnt" "DisplayVersion" "0.2.1"
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\Hamnt" "Publisher" "tezoatlipoca"
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\Hamnt" "InstallLocation" "$INSTDIR"
    ; Add install directory to user PATH
    ReadRegStr $0 HKCU "Environment" "Path"
    StrCpy $1 "$INSTDIR;$0"
    WriteRegStr HKCU "Environment" "Path" "$1"
  ${EndIf}

  ; Broadcast WM_SETTINGCHANGE to update environment variables
  System::Call 'user32::SendMessageTimeoutA(i 0xffff, i ${WM_SETTINGCHANGE}, i 0, t "Environment", i 0, i 1000, *i .r0)'
SectionEnd

; Uninstaller Section
Section "Uninstall"
  ; Remove files and folders
  Delete "$INSTDIR\hamnt.exe"
  Delete "$INSTDIR\uninstall.exe"
    ; Add commands to delete any other files
  
  ; Remove shortcuts
  Delete "$SMPROGRAMS\Hamnt\Hamnt.lnk"
  Delete "$SMPROGRAMS\Hamnt\Uninstall.lnk"
  RMDir "$SMPROGRAMS\Hamnt"
  
  ; Remove directories
  RMDir "$INSTDIR"
  
  ; Remove registry keys
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Hamnt"
SectionEnd