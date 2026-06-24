; Inno Setup script that packages clipory into a small per-user installer.
;
; Build it with:
;   "ISCC.exe" /DAppVersion=1.0.0 installer.iss
; The self-contained clipory.exe must already be published to dist\win-x64
; (run tools\publish.ps1 first). The release.ps1 script does both in order.
;
; Per-user install (PrivilegesRequired=lowest) is deliberate: it never raises a
; UAC prompt, so the in-app auto-update can run the installer without the user
; having to approve an elevation every time.

#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif

#define AppName "clipory"
#define AppPublisher "Volkan Turhan"
#define AppUrl "https://github.com/volkanturhan/clipory"
#define AppExe "clipory.exe"

[Setup]
; A fixed AppId ties upgrades and the uninstall entry together across versions.
; Never change it once shipped.
AppId={{B6F8E2D4-3A7C-4F1E-9C5B-2E8A1D6F0B3C}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppUrl}
AppSupportURL={#AppUrl}
WizardStyle=modern
PrivilegesRequired=lowest
DefaultDirName={localappdata}\Programs\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
; Let Inno's Restart Manager close a running clipory before overwriting its exe.
CloseApplications=yes
SetupIconFile=..\clipory\Assets\clipory.ico
UninstallDisplayIcon={app}\{#AppExe}
OutputDir=..\dist\installer
OutputBaseFilename=clipory-setup-v{#AppVersion}
Compression=lzma2/max
SolidCompression=yes

[Languages]
Name: "en"; MessagesFile: "compiler:Default.isl"
Name: "tr"; MessagesFile: "compiler:Languages\Turkish.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\dist\win-x64\{#AppExe}"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExe}"
Name: "{group}\{cm:UninstallProgram,{#AppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Run]
; Offer to launch clipory when the wizard finishes (after a manual install).
Filename: "{app}\{#AppExe}"; Description: "{cm:LaunchProgram,{#AppName}}"; Flags: nowait postinstall skipifsilent
