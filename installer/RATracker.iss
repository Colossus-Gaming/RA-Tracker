; Inno Setup front-end for the Velopack installer.
;
; Velopack's Setup.exe has no folder-picker UI -- it always installs to a fixed per-user location.
; This wrapper adds the standard "choose install location" wizard page and then hands the chosen
; path to Velopack via its --installto argument, so the user gets a real choice while Velopack keeps
; ownership of installation, shortcuts, uninstall registration and silent self-updating.
;
; Build:  ISCC.exe /DVelopackSetup="<path to *-Setup.exe>" /DAppVersion=1.9.1 RATracker.iss

#ifndef VelopackSetup
  #error VelopackSetup must be defined, e.g. /DVelopackSetup="..\releases\ColossusGaming.RATracker-win-Setup.exe"
#endif

#ifndef AppVersion
  #define AppVersion "0.0.0"
#endif

#define AppName "Retro Achievement Tracker"
#define AppPublisher "Colossus Gaming"

[Setup]
AppId={{8F2B6C41-9E4D-4A7B-B3C8-2D5E1A9F4C60}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
WizardStyle=modern
DisableWelcomePage=no
DisableProgramGroupPage=yes

; Per-user install, no UAC. Velopack updates in place without elevation, so the app must live
; somewhere the user can write to. Requesting elevation here would let people pick Program Files
; and silently break every future update.
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=

DefaultDirName={localappdata}\ColossusGaming.RATracker
DirExistsWarning=no
AppendDefaultDirName=no

; Velopack registers its own Add/Remove Programs entry and owns uninstallation. Creating a second
; uninstaller here would leave the user with two entries, one of which would not work.
Uninstallable=no
CreateAppDir=yes

OutputBaseFilename=RATracker-Setup-{#AppVersion}
Compression=lzma2/max
SolidCompression=yes
; The payload is an already-compressed self-contained build; recompressing it wastes build time for
; almost no size gain.
InternalCompressLevel=fast

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Messages]
WelcomeLabel2=This will install {#AppName} on your computer.%n%nYou can choose where it goes on the next page. Pick a folder your Windows account can write to, so the app can keep itself up to date without asking for administrator rights.
SelectDirLabel3=Setup will install {#AppName} into the following folder.
SelectDirBrowseLabel=To continue, click Next. To choose a different folder, click Browse.

[Files]
; Extracted to a temp folder and run; not installed alongside the app.
Source: "{#VelopackSetup}"; DestDir: "{tmp}"; DestName: "VelopackSetup.exe"; Flags: deleteafterinstall ignoreversion

; NOTE: the payload is deliberately NOT run from a [Run] entry. A [Run] entry ignores the child
; process's exit code, so if Velopack failed the wizard still reported success and the user was left
; with "it said it installed, but nothing installed". It is executed from CurStepChanged below,
; where the exit code is checked and the result verified on disk.

[Code]
{ Reject locations the user cannot write to. Installing into Program Files would appear to succeed
  and then break every silent update, because Velopack updates without elevation. }
function IsWritableLocation(Path: String): Boolean;
var
  ProgramFiles: String;
  ProgramFilesX86: String;
  WindowsDir: String;
  Lower: String;
begin
  Lower := Lowercase(Path);
  ProgramFiles := Lowercase(ExpandConstant('{commonpf}'));
  ProgramFilesX86 := Lowercase(ExpandConstant('{commonpf32}'));
  WindowsDir := Lowercase(ExpandConstant('{win}'));

  Result := True;
  if (Pos(ProgramFiles, Lower) = 1) or (Pos(ProgramFilesX86, Lower) = 1) or (Pos(WindowsDir, Lower) = 1) then
    Result := False;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
  Result := True;
  if CurPageID = wpSelectDir then
  begin
    if not IsWritableLocation(WizardDirValue) then
    begin
      MsgBox('That location needs administrator rights to write to, which would stop ' +
             '{#AppName} from updating itself.' + #13#10#13#10 +
             'Please choose a folder inside your user profile, or another drive.',
             mbError, MB_OK);
      Result := False;
    end;
  end;
end;

{ Runs for silent installs too, unlike NextButtonClick, so /VERYSILENT /DIR=... cannot bypass the
  writable-location rule. Returning a non-empty string aborts setup with that message. }
function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  Result := '';
  if not IsWritableLocation(ExpandConstant('{app}')) then
    Result := 'The chosen folder requires administrator rights, which would prevent automatic updates. ' +
              'Choose a folder inside your user profile or on another drive.';
end;

{ Runs the bundled Velopack installer and reports honestly on the result.

  This is the whole reason the payload is not a [Run] entry: Exec gives us the exit code, and we
  additionally confirm the application executable actually exists afterwards. Without both checks a
  failed or blocked payload (antivirus quarantine, a locked directory, a half-extracted download)
  produced a wizard that cheerfully announced success while installing nothing. }
procedure CurStepChanged(CurStep: TSetupStep);
var
  ResultCode: Integer;
  Payload: String;
  InstalledExe: String;
  Detail: String;
begin
  if CurStep <> ssPostInstall then
    Exit;

  Payload := ExpandConstant('{tmp}\VelopackSetup.exe');
  InstalledExe := ExpandConstant('{app}\RATracker.WPF.exe');

  if not FileExists(Payload) then
  begin
    RaiseException('The installer payload is missing. The download may be incomplete or was ' +
                   'partially removed by antivirus. Please download the installer again.');
    Exit;
  end;

  Log('Running Velopack payload: ' + Payload);

  if not Exec(Payload, '--silent --installto "' + ExpandConstant('{app}') + '"',
              '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    RaiseException('{#AppName} could not be installed: the bundled installer would not start.' + #13#10 +
                   'This is usually antivirus blocking it. Allow the installer and try again.');
    Exit;
  end;

  Log('Velopack payload exit code: ' + IntToStr(ResultCode));

  if ResultCode <> 0 then
  begin
    RaiseException('{#AppName} could not be installed. The bundled installer exited with code ' +
                   IntToStr(ResultCode) + '.' + #13#10#13#10 +
                   'Antivirus software blocking the installer is the most common cause. ' +
                   'Re-run with /LOG="%TEMP%\ratracker-install.log" and share that file to diagnose.');
    Exit;
  end;

  { Exit code 0 is necessary but not sufficient - confirm something is actually on disk. }
  if not FileExists(InstalledExe) then
  begin
    Detail := 'The installer reported success but no application was written to:' + #13#10 +
              ExpandConstant('{app}') + #13#10#13#10 +
              'This usually means antivirus removed the files immediately after extraction.';
    RaiseException(Detail);
    Exit;
  end;

  Log('Install verified at: ' + InstalledExe);
end;
