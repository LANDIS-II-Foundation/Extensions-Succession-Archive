#define PackageName      "Century Succession"
#define PackageNameLong  "Century Succession Extension"
#define Version          "1.1"
#define ReleaseType      "official"
#define ReleaseNumber    "1"

#define CoreVersion      "5.1"
#define CoreReleaseAbbr  ""

#include "package (Setup section).iss"

#if ReleaseType != "official"
  #define Configuration  "debug"
#else
  #define Configuration  "release"
#endif

[Files]
; Auxiliary libs
Source: {#LandisBuildDir}\Landis.Library.Cohorts.LeafBiomass.dll; DestDir: {app}\bin; Flags: replacesameversion
Source: {#LandisBuildDir}\Landis.Succession.dll; DestDir: {app}\bin; Flags: uninsneveruninstall replacesameversion
Source: {#LandisBuildDir}\Landis.Library.Climate.dll; DestDir: {app}\bin; Flags: uninsneveruninstall replacesameversion

; Century Succession
Source: {#LandisBuildDir}\Landis.Extension.Succession.Century.dll; DestDir: {app}\bin; Flags: replacesameversion
Source: docs\LANDIS-II Century Succession v1.0 User Guide.pdf; DestDir: {app}\docs
Source: examples\*; DestDir: {app}\examples\century-succession

#define CenturySucc "Century Succession 1.1.txt"
Source: {#CenturySucc}; DestDir: {#LandisPlugInDir}

[Run]
;; Run plug-in admin tool to add an entry for the plug-in
#define PlugInAdminTool  CoreBinDir + "\Landis.PlugIns.Admin.exe"

Filename: {#PlugInAdminTool}; Parameters: "remove ""Century Succession"" "; WorkingDir: {#LandisPlugInDir}
Filename: {#PlugInAdminTool}; Parameters: "add ""{#CenturySucc}"" "; WorkingDir: {#LandisPlugInDir}

[UninstallRun]
;; Run plug-in admin tool to remove the entry for the plug-in
; Filename: {#PlugInAdminTool}; Parameters: "remove ""Century Succession"" "; WorkingDir: {#LandisPlugInDir}

[Code]
#include "package (Code section) v3.iss"

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
