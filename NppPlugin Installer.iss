#define COMPANY ""
#define NAME "NppPlugin"
#define URL "" 
#ifndef VERSION
  #define VERSION "1.0.0"
#endif

[Setup]
AppId={{29AC398A-E58C-4FF0-86D9-791109DEF303}
AppName={#NAME}
AppVersion={#VERSION}
AppPublisher={#COMPANY}
AppPublisherURL={#URL}
AppSupportURL={#URL}
AppUpdatesURL={#URL}
DefaultDirName={pf}\{#NAME}
DefaultGroupName={#COMPANY}
DisableProgramGroupPage=yes
OutputBaseFilename={#NAME} Installer v{#VERSION}
Compression=lzma
SolidCompression=yes
DisableWelcomePage=True
AlwaysShowDirOnReadyPage=True
RestartIfNeededByRun=False
ShowLanguageDialog=no
UninstallDisplayName={#NAME}
UninstallDisplayIcon={app}\Promega Debugger.exe

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
;Notepad++ Plug-In
Source: "Output\NppPlugin.dll"; DestDir: "{code:GetNppLocation}"; AfterInstall: InstallNppLangs


[ThirdParty]
UseRelativePaths=True

[Code] 
function GetNppInstallPath(out strout: WideString): Integer;
  external 'GetNppInstallPath@files:NppPlugin.dll stdcall setuponly';
function InstallCustomNppLanguages(): Integer;
  external 'InstallCustomNppLanguages@files:NppPlugin.dll stdcall setuponly';

var
  NppLocation: WideString;
  RetVal: Integer;

function FindNotepadInstallation(): Boolean;
begin                       
  RetVal:=GetNppInstallPath(NppLocation);
  NppLocation := NppLocation + '\plugins';
  Result:=(RetVal=0);
end;

function GetNppLocation(Param: String): String;
begin
  Result:=NppLocation;
end;

procedure InstallNppLangs();
begin
  InstallCustomNppLanguages();
end;
