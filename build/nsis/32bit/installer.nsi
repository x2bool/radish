!include "MUI2.nsh"

Name "Radish"
OutFile "radish-setup.exe"

InstallDir "$APPDATA\Radish"

InstallDirRegKey HKCU "Software\Radish32" ""

RequestExecutionLevel highest

!define MUI_ABORTWARNING

!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

!insertmacro MUI_LANGUAGE "English"
!insertmacro MUI_LANGUAGE "Russian"

Section "Radish" SecInstall

  SetOutPath "$INSTDIR"
  
  File /r ".\*"
  
  ;Store installation folder
  WriteRegStr HKCU "Software\Radish32" "" $INSTDIR
  
  ;Create uninstaller
  WriteUninstaller "$INSTDIR\Uninstall.exe"

SectionEnd

Section "Shortcut"
  CreateShortCut "$DESKTOP\Radish.lnk" "$INSTDIR\radish.exe"
SectionEnd

Section "Uninstall"

  Delete "$INSTDIR\Uninstall.exe"

  RMDir "$INSTDIR"

  DeleteRegKey /ifempty HKCU "Software\Radish32"

SectionEnd

Function .onInit
  SectionSetFlags 0 17
FunctionEnd
