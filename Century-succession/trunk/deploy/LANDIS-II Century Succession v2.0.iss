#define PackageName      "Century Succession"
#define PackageNameLong  "Century Succession Extension"
#define Version          "2.0"
#define ReleaseType      "official"
#define ReleaseNumber    "2"

#define CoreVersion      "6.0"
#define CoreReleaseAbbr  ""

#include AddBackslash(GetEnv("LANDIS_DEPLOY")) + "package (Setup section) v6.0.iss"

[Files]
; Auxiliary libs
#define BuildDir "C:\Program Files\LANDIS-II\6.0\bin"
Source: {#BuildDir}\Landis.Library.LeafBiomassCohorts.dll; DestDir: {app}\bin; Flags: replacesameversion
Source: {#BuildDir}\Landis.Library.Succession.dll; DestDir: {app}\bin; Flags: uninsneveruninstall replacesameversion
Source: {#BuildDir}\Landis.Library.Climate.dll; DestDir: {app}\bin; Flags: uninsneveruninstall replacesameversion

; Century Succession
Source: {#BuildDir}\Landis.Extension.Succession.Century.dll; DestDir: {app}\bin; Flags: replacesameversion
Source: docs\LANDIS-II Century Succession v2.0 User Guide.pdf; DestDir: {app}\docs
Source: docs\Century-succession-log-metadata.xlsx; DestDir: {app}\docs
Source: examples\*; DestDir: {app}\examples\century-succession

#define CenturySucc "Century Succession 2.0.txt"
Source: {#CenturySucc}; DestDir: {#LandisPlugInDir}

[Run]
;; Run plug-in admin tool to add an entry for the plug-in
#define PlugInAdminTool  CoreBinDir + "\Landis.PlugIns.Admin.exe"

Filename: {#PlugInAdminTool}; Parameters: "remove ""Century Succession"" "; WorkingDir: {#LandisPlugInDir}
Filename: {#PlugInAdminTool}; Parameters: "add ""{#CenturySucc}"" "; WorkingDir: {#LandisPlugInDir}

[UninstallRun]

[Code]
#include AddBackslash(GetEnv("LANDIS_DEPLOY")) + "package (Code section) v3.iss"

//-----------------------------------------------------------------------------

function CurrentVersion_PostUninstall(currentVersion: TInstalledVersion): Integer;
begin
    Result := 0;
end;

//-----------------------------------------------------------------------------

function InitializeSetup_FirstPhase(): Boolean;
begin
  CurrVers_PostUninstall := @CurrentVersion_PostUninstall
  Result := True
end;
