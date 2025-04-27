; Basic installer script for hamnt
!include "MUI2.nsh"

; General Settings
Name "Hamnt"
OutFile "hamnt-0.2.0-setup.exe"
InstallDir "$PROGRAMFILES\Hamnt"
InstallDirRegKey HKCU "Software\Hamnt" ""
RequestExecutionLevel admin

; Version Information
VIProductVersion "0.2.0.0"
VIAddVersionKey "ProductName" "Hamnt"
VIAddVersionKey "FileDescription" "HyperAggressively Minimal Note Taking app"
VIAddVersionKey "LegalCopyright" "© tezoatlipoca@gmail.com"
VIAddVersionKey "FileVersion" "0.2.0"

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

; Installer Sections
Section "Install"
  SetOutPath "$INSTDIR"
  
  ; Show the path we're looking for the binary in
  DetailPrint "Looking for binary at: bin\Release\net8.0\win-x64\publish\hamnt.exe"
  
  ; Add files
  File /oname=hamnt.exe "..\bin\Release\net8.0\win-x64\publish\hamnt.exe"
  ; File "path/to/other/files/*.*" ; Add any other files your app needs
  
  ; Create Start Menu shortcuts
  CreateDirectory "$SMPROGRAMS\Hamnt"
  CreateShortcut "$SMPROGRAMS\Hamnt\Hamnt.lnk" "$INSTDIR\hamnt.exe"
  CreateShortcut "$SMPROGRAMS\Hamnt\Uninstall.lnk" "$INSTDIR\uninstall.exe"
  
  ; Create uninstaller
  WriteUninstaller "$INSTDIR\uninstall.exe"
  
  ; Write registry keys for uninstall
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Hamnt" \
                   "DisplayName" "Hamnt"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Hamnt" \
                   "UninstallString" "$\"$INSTDIR\uninstall.exe$\""
  
  ; Add install directory to user PATH
  ReadRegStr $0 HKCU "Environment" "Path"
  StrCpy $1 "$INSTDIR;$0"
  WriteRegStr HKCU "Environment" "Path" "$1"

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