; Basic installer script for hamnt
!include "MUI2.nsh"
!include "VersionCompare.nsh"

; Update ALL version references (change these for each version)
!define VERSION "0.3.0"

; General Settings  
Name "Hamnt"
OutFile "hamnt-${VERSION}-setup.exe"
InstallDir "$PROGRAMFILES\Hamnt"
InstallDirRegKey HKCU "Software\Hamnt" ""
RequestExecutionLevel admin

; Version Information
VIProductVersion "${VERSION}.0"
VIAddVersionKey "ProductName" "Hamnt"
VIAddVersionKey "FileDescription" "HyperAggressively Minimal Note Taking app"
VIAddVersionKey "LegalCopyright" "© tezoatlipoca@gmail.com"
VIAddVersionKey "FileVersion" "${VERSION}"

; Interface Settings
!define MUI_ABORTWARNING
!define MUI_ICON "hamnt.ico" ; Optional: Replace with your icon path

; Variables
Var InstallScope

; Pages - PROPER ORDER IS CRITICAL
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
Page Custom ScopePageCreate ScopePageLeave  ; Custom page with proper navigation
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

; Languages
!insertmacro MUI_LANGUAGE "English"

; Custom page functions
Function ScopePageCreate
  ; Initialize scope to ALL by default
  StrCpy $InstallScope "ALL"
  
  ; Create the scope selection dialog
  nsDialogs::Create 1018
  Pop $0
  ${If} $0 == error
    Abort
  ${EndIf}

  ; Add explanatory label
  ${NSD_CreateLabel} 0u 0u 100% 15u "Your %PATH% environment variable makes sure hamnt can be found:"
  Pop $3

  ; Create a group box
  ${NSD_CreateGroupBox} 10u 25u 80% 50u "Modify PATH environment variable for:"
  Pop $6

  ; Add radio buttons
  ${NSD_CreateRadioButton} 15u 40u 70% 12u "All users (requires admin privileges)"
  Pop $1
  ${NSD_SetState} $1 ${BST_CHECKED}
  ${NSD_OnClick} $1 ScopeAllClicked

  ${NSD_CreateRadioButton} 15u 55u 70% 12u "Current user only"
  Pop $2
  ${NSD_OnClick} $2 ScopeCurrentClicked

  nsDialogs::Show
FunctionEnd

Function ScopePageLeave
  ; This function is called when leaving the page
  ; You can add validation here if needed
  DetailPrint "Leaving scope page with: $InstallScope"
FunctionEnd

Function ScopeAllClicked
  StrCpy $InstallScope "ALL"
  DetailPrint "Selected scope: ALL"
FunctionEnd

Function ScopeCurrentClicked
  StrCpy $InstallScope "CURRENT"
  DetailPrint "Selected scope: CURRENT"
FunctionEnd

; Installer Sections
Section "Install"
  SetOutPath "$INSTDIR"

  ; Add files
  File /oname=hamnt.exe "..\bin\Release\net8.0\win-x64\publish\hamnt.exe"

  ; Check for existing version in BOTH registry locations
  ReadRegStr $R0 HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Hamnt" "DisplayVersion"
  ${If} $R0 == ""
    ReadRegStr $R0 HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\Hamnt" "DisplayVersion"
  ${EndIf}
  
  StrCpy $R1 "${VERSION}" ; This installer's version

  ${If} $R0 != ""
    ${VersionCompare} $R0 $R1 $R2
    ${If} $R2 == "1" ; R0 > R1 (installed version is newer)
      MessageBox MB_ICONSTOP "A newer version ($R0) of Hamnt is already installed.$\nCurrent installer: $R1$\nInstaller will now close."
      Quit
    ${ElseIf} $R2 == "0" ; R0 == R1 (same version)
      MessageBox MB_ICONQUESTION|MB_YESNO "Hamnt version $R0 is already installed. Do you want to reinstall?" IDNO quit_installer
    ${Else}
      MessageBox MB_ICONQUESTION|MB_YESNO "Upgrading Hamnt from version $R0 to ${VERSION}. Continue?" IDNO quit_installer
    ${EndIf}
  ${EndIf}

  Goto continue_install
  
  quit_installer:
    Quit
  
  continue_install:

  ; Create Start Menu shortcuts
  CreateDirectory "$SMPROGRAMS\Hamnt"
  CreateShortcut "$SMPROGRAMS\Hamnt\Hamnt.lnk" "$INSTDIR\hamnt.exe"
  CreateShortcut "$SMPROGRAMS\Hamnt\Uninstall.lnk" "$INSTDIR\uninstall.exe"

  ; Create uninstaller
  WriteUninstaller "$INSTDIR\uninstall.exe"

  ; CRITICAL: Debug the scope value
  ;DetailPrint "DEBUG: Install scope is: '$InstallScope'"
  ;MessageBox MB_OK "DEBUG: Install scope is: '$InstallScope'"

  ${If} $InstallScope == "ALL"
    DetailPrint "Installing for ALL users - using HKLM and system PATH"
    ;MessageBox MB_OK "Going to modify SYSTEM PATH"
    
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Hamnt" "DisplayName" "Hamnt"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Hamnt" "UninstallString" "$\"$INSTDIR\uninstall.exe$\""
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Hamnt" "DisplayVersion" "${VERSION}"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Hamnt" "Publisher" "tezoatlipoca"
    WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Hamnt" "InstallLocation" "$INSTDIR"
    
    ; Add install directory to system PATH
    ReadRegStr $0 HKLM "SYSTEM\CurrentControlSet\Control\Session Manager\Environment" "Path"
    ${If} $0 == ""
      WriteRegStr HKLM "SYSTEM\CurrentControlSet\Control\Session Manager\Environment" "Path" "$INSTDIR"
    ${Else}
      WriteRegStr HKLM "SYSTEM\CurrentControlSet\Control\Session Manager\Environment" "Path" "$INSTDIR;$0"
    ${EndIf}
    DetailPrint "Added $INSTDIR to SYSTEM PATH"
  ${Else}
    DetailPrint "Installing for CURRENT user - using HKCU and user PATH"
    ;MessageBox MB_OK "Going to modify USER PATH"
    
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\Hamnt" "DisplayName" "Hamnt"
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\Hamnt" "UninstallString" "$\"$INSTDIR\uninstall.exe$\""
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\Hamnt" "DisplayVersion" "${VERSION}"
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\Hamnt" "Publisher" "tezoatlipoca"
    WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\Hamnt" "InstallLocation" "$INSTDIR"
    
    ; Add install directory to user PATH
    ReadRegStr $0 HKCU "Environment" "Path"
    ${If} $0 == ""
      WriteRegStr HKCU "Environment" "Path" "$INSTDIR"
    ${Else}
      WriteRegStr HKCU "Environment" "Path" "$INSTDIR;$0"
    ${EndIf}
    DetailPrint "Added $INSTDIR to USER PATH"
  ${EndIf}

  ; Broadcast WM_SETTINGCHANGE to update environment variables
  System::Call 'user32::SendMessageTimeoutA(i 0xffff, i ${WM_SETTINGCHANGE}, i 0, t "Environment", i 0, i 1000, *i .r0)'
SectionEnd

; Uninstaller Section
Section "Uninstall"
  ; Remove files and folders
  Delete "$INSTDIR\hamnt.exe"
  Delete "$INSTDIR\uninstall.exe"
  
  ; Remove shortcuts
  Delete "$SMPROGRAMS\Hamnt\Hamnt.lnk"
  Delete "$SMPROGRAMS\Hamnt\Uninstall.lnk"
  RMDir "$SMPROGRAMS\Hamnt"
  
  ; Remove from PATH using custom function
  ; System PATH
  ReadRegStr $0 HKLM "SYSTEM\CurrentControlSet\Control\Session Manager\Environment" "Path"
  Push "$0"
  Push "$INSTDIR;"
  Call un.RemoveFromPath
  Pop $1
  WriteRegStr HKLM "SYSTEM\CurrentControlSet\Control\Session Manager\Environment" "Path" "$1"
  
  ; User PATH  
  ReadRegStr $0 HKCU "Environment" "Path"
  Push "$0"
  Push "$INSTDIR;"
  Call un.RemoveFromPath
  Pop $1
  WriteRegStr HKCU "Environment" "Path" "$1"
  
  ; Remove directories
  RMDir "$INSTDIR"
  
  ; Remove registry keys from BOTH possible locations
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Hamnt"
  DeleteRegKey HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\Hamnt"
  
  ; Broadcast WM_SETTINGCHANGE to update environment variables
  System::Call 'user32::SendMessageTimeoutA(i 0xffff, i ${WM_SETTINGCHANGE}, i 0, t "Environment", i 0, i 1000, *i .r0)'
SectionEnd



; Function to remove string from PATH
Function un.RemoveFromPath
  Exch $0 ; string to remove
  Exch
  Exch $1 ; input string
  Push $2
  Push $3
  Push $4
  Push $5
  
  StrLen $2 $0
  StrCpy $3 ""
  StrCpy $4 0
  
  loop:
    StrCpy $5 $1 $2 $4
    StrCmp $5 $0 remove
    StrCpy $5 $1 1 $4
    StrCpy $3 "$3$5"
    IntOp $4 $4 + 1
    StrCpy $5 $1 1 $4
    StrCmp $5 "" done
    Goto loop
    
  remove:
    IntOp $4 $4 + $2
    Goto loop
    
  done:
    StrCpy $0 $3
    Pop $5
    Pop $4
    Pop $3
    Pop $2
    Pop $1
    Exch $0
FunctionEnd