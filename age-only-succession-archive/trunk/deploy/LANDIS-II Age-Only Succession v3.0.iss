#define PackageName      "Age-only Succession"
#define PackageNameLong  "Age-only Succession Extension"
#define Version          "3.0"
#define ReleaseType      "official"
#define ReleaseNumber    ""

#define CoreVersion      "6.0"
#define CoreReleaseAbbr  ""

#include AddBackslash(GetEnv("LANDIS_DEPLOY")) + "package (Setup section) v6.0.iss"

#if ReleaseType != "official"
  #define Configuration  "debug"
#else
  #define Configuration  "release"
#endif

[Files]

; Base Harvest (v1.0)
Source: C:\Program Files\LANDIS-II\6.0\bin\Landis.Extension.Succession.AgeOnly.dll; DestDir: {app}\bin; Flags: replacesameversion
Source: docs\LANDIS-II Age-Only Succession v3.0 User Guide.pdf; DestDir: {app}\docs
Source: examples\*; DestDir: {app}\examples\age-only-succession

#define AgeOnlySucc "Age-only Succession 3.0.txt"
Source: {#AgeOnlySucc}; DestDir: {#LandisPlugInDir}

; Necessary libraries
Source: C:\Program Files\LANDIS-II\6.0\bin\Landis.Library.Succession.dll; DestDir: {app}\bin; Flags: uninsneveruninstall replacesameversion
Source: C:\Program Files\LANDIS-II\6.0\bin\Landis.Library.Cohorts.dll; DestDir: {app}\bin; Flags: uninsneveruninstall replacesameversion
Source: C:\Program Files\LANDIS-II\6.0\bin\Landis.Library.AgeOnlyCohorts.dll; DestDir: {app}\bin; Flags: uninsneveruninstall replacesameversion


[Run]
;; Run plug-in admin tool to add an entry for the plug-in
#define PlugInAdminTool  CoreBinDir + "\Landis.PlugIns.Admin.exe"

Filename: {#PlugInAdminTool}; Parameters: "remove ""Age-only Succession"" "; WorkingDir: {#LandisPlugInDir}
Filename: {#PlugInAdminTool}; Parameters: "add ""{#AgeOnlySucc}"" "; WorkingDir: {#LandisPlugInDir}

[UninstallRun]
;; Run plug-in admin tool to remove the entry for the plug-in

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
