#define PackageName      "Age-only Succession"
#define PackageNameLong  "Age-only Succession Extension"
#define Version          "4.1"
#define ReleaseType      "official"
#define ReleaseNumber    ""

#define CoreVersion      "6.0"
#define CoreReleaseAbbr  ""

#include GetEnv("LANDIS_DEPLOY") + "\package (Setup section) v6.0.iss"
;#include "C:\BRM\LANDIS_II\code\deploy\package (Setup section) v6.0.iss"
#define ExtDir "C:\Program Files\LANDIS-II\v6\bin\extensions"
#define AppDir "C:\Program Files\LANDIS-II\v6\"

[Files]

; Install Files and Necessary Libraries
Source: ..\src\bin\debug\Landis.Extension.Succession.AgeOnly.dll; DestDir: {#ExtDir}; Flags: replacesameversion
Source: ..\src\bin\debug\Landis.Library.Succession.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall replacesameversion
Source: ..\src\bin\debug\Landis.Library.Cohorts.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall replacesameversion
Source: ..\src\bin\debug\Landis.Library.AgeOnlyCohorts.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall replacesameversion

; Ancillary Files
Source: docs\LANDIS-II Age-Only Succession v4.1 User Guide.pdf; DestDir: {#AppDir}\docs
Source: examples\ecoregions.gis; DestDir: {#AppDir}\examples\age-only-succession
Source: examples\initial-communities.gis; DestDir: {#AppDir}\examples\age-only-succession
Source: examples\*.txt; DestDir: {#AppDir}\examples\age-only-succession
Source: examples\*.bat; DestDir: {#AppDir}\examples\age-only-succession

#define AgeOnlySucc "Age-only Succession 4.1.txt"
Source: {#AgeOnlySucc}; DestDir: {#LandisPlugInDir}

[Run]
;; Run plug-in admin tool to add an entry for the plug-in
#define PlugInAdminTool  CoreBinDir + "\Landis.PlugIns.Admin.exe"

Filename: {#PlugInAdminTool}; Parameters: "remove ""Age-only Succession"" "; WorkingDir: {#LandisPlugInDir}
Filename: {#PlugInAdminTool}; Parameters: "add ""{#AgeOnlySucc}"" "; WorkingDir: {#LandisPlugInDir}

[UninstallRun]
;; Run plug-in admin tool to remove the entry for the plug-in

[Code]
#include GetEnv("LANDIS_DEPLOY")+"\package (Code section) v3.iss"

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
