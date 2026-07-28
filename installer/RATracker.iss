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

; Always write a setup log, without the user needing to pass /LOG. If anything goes wrong the log
; is folded into a report saved to the user's Desktop, so diagnosing a failed install never depends
; on the person running a command line.
SetupLogging=yes

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

{ ---------------------------------------------------------------------------------------------
  Failure reporting.

  The people testing this are not going to run anything from a command prompt, so the installer has
  to produce its own diagnostics. On failure it writes a single plain-text report to the Desktop
  containing the reason, the machine details, the Velopack payload's own log and the setup log, then
  opens it. The user's entire job is "send me the file on your Desktop".
  --------------------------------------------------------------------------------------------- }

const
  ReportFileName = 'RATracker-Install-Report.txt';

function PayloadLogPath: String;
begin
  Result := ExpandConstant('{tmp}\velopack-install.log');
end;

{ Reads a text file if it exists; returns a placeholder rather than failing. }
function ReadTextFileSafe(const Path: String): String;
var
  Contents: AnsiString;
begin
  if not FileExists(Path) then
  begin
    Result := '(not created)';
    Exit;
  end;

  if LoadStringFromFile(Path, Contents) then
    Result := String(Contents)
  else
    Result := '(could not be read)';
end;

{ Reads setup's own log.

  It cannot be read directly: setup still has it open, so LoadStringFromFile fails and the most
  useful part of the report comes back empty. FileCopy opens with read sharing, so copy it aside
  first and read the copy. }
function ReadSetupLog: String;
var
  Original: String;
  Copy: String;
begin
  Original := ExpandConstant('{log}');
  if Original = '' then
  begin
    Result := '(logging is disabled)';
    Exit;
  end;

  Copy := ExpandConstant('{tmp}\setup-log-copy.txt');
  if FileCopy(Original, Copy, False) then
    Result := ReadTextFileSafe(Copy)
  else
    Result := '(could not be copied from ' + Original + ')';
end;

{ Writes the report somewhere the user can actually find it and opens it. Returns where it landed. }
function SaveFailureReport(const Reason: String): String;
var
  Report: String;
  Target: String;
  ErrorCode: Integer;
begin
  Report :=
    '{#AppName} - installation report' + #13#10 +
    '===========================================' + #13#10 +
    'Please send this whole file to the developer.' + #13#10 + #13#10 +
    'Version:        {#AppVersion}' + #13#10 +
    'Date:           ' + GetDateTimeString('yyyy-mm-dd hh:nn:ss', '-', ':') + #13#10 +
    'Windows:        ' + GetWindowsVersionString + #13#10 +
    'Install folder: ' + ExpandConstant('{app}') + #13#10 + #13#10 +
    'WHAT WENT WRONG' + #13#10 +
    '---------------' + #13#10 +
    Reason + #13#10 + #13#10 +
    'APPLICATION INSTALLER LOG' + #13#10 +
    '-------------------------' + #13#10 +
    ReadTextFileSafe(PayloadLogPath) + #13#10 + #13#10 +
    'SETUP LOG' + #13#10 +
    '---------' + #13#10 +
    ReadSetupLog;

  Target := ExpandConstant('{userdesktop}\' + ReportFileName);
  if not SaveStringToFile(Target, AnsiString(Report), False) then
  begin
    { Desktop may be redirected or locked down; fall back somewhere always writable. }
    Target := ExpandConstant('{tmp}\' + ReportFileName);
    if not SaveStringToFile(Target, AnsiString(Report), False) then
    begin
      Result := '';
      Exit;
    end;
  end;

  { Open it so the user sees exactly which file to send. }
  ShellExec('open', Target, '', '', SW_SHOW, ewNoWait, ErrorCode);
  Result := Target;
end;

{ Reports a failure to the user in plain language, then aborts setup with a non-zero exit code. }
procedure FailWithReport(const Reason: String; const Advice: String);
var
  Saved: String;
  Message: String;
begin
  Saved := SaveFailureReport(Reason);

  Message := '{#AppName} could not be installed.' + #13#10 + #13#10 + Advice;

  if Saved <> '' then
    Message := Message + #13#10 + #13#10 +
               'A report has been saved and opened for you:' + #13#10 +
               Saved + #13#10 + #13#10 +
               'Please send that file to the developer.'
  else
    Message := Message + #13#10 + #13#10 + '(A report could not be saved.)';

  SuppressibleMsgBox(Message, mbCriticalError, MB_OK, IDOK);
  RaiseException(Reason);
end;

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

  { The payload was extracted moments ago. If it has vanished, something removed it. }
  if not FileExists(Payload) then
  begin
    FailWithReport(
      'The bundled application installer was missing from ' + Payload + ' immediately after being ' +
      'extracted. Something removed it.',
      'Your antivirus most likely deleted part of the installer.' + #13#10 +
      'Allow this installer in your antivirus, then run it again.');
    Exit;
  end;

  Log('Running Velopack payload: ' + Payload);

  { --log makes the payload record its own diagnosis, which is folded into the report. }
  if not Exec(Payload,
              '--silent --installto "' + ExpandConstant('{app}') + '" --log "' + PayloadLogPath + '"',
              '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    FailWithReport(
      'The bundled application installer could not be started at all.',
      'Your antivirus most likely blocked it from running.' + #13#10 +
      'Allow this installer in your antivirus, then run it again.');
    Exit;
  end;

  Log('Velopack payload exit code: ' + IntToStr(ResultCode));

  if ResultCode <> 0 then
  begin
    FailWithReport(
      'The bundled application installer ran but failed, exiting with code ' + IntToStr(ResultCode) + '.',
      'This is usually antivirus interference, or the chosen folder being unwritable.' + #13#10 +
      'Try allowing the installer in your antivirus, or choosing a different folder.');
    Exit;
  end;

  { Exit code 0 is necessary but not sufficient: files can be quarantined right after extraction. }
  if not FileExists(InstalledExe) then
  begin
    Detail := 'The application installer reported success, but no program was actually written to ' +
              ExpandConstant('{app}') + '. The files were removed after being created.';
    FailWithReport(
      Detail,
      'Your antivirus almost certainly quarantined the application right after it was installed.' + #13#10 +
      'Allow the installer and the installed folder in your antivirus, then run it again.');
    Exit;
  end;

  Log('Install verified at: ' + InstalledExe);
end;
