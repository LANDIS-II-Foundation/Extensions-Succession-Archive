#define PackageName      "Century Succession"
#define PackageNameLong  "Century Succession Extension"
#define Version          "3.1.1"
#define ReleaseType      "official"
#define ReleaseNumber    "3"

#define CoreVersion      "6.0"
#define CoreReleaseAbbr  ""

#include "J:\Scheller\LANDIS-II\deploy\package (Setup section) v6.0.iss"
#define BuildDir "C:\Program Files\LANDIS-II\v6\bin\extensions"
#define AppDir "C:\Program Files\LANDIS-II\v6"

[Files]
; Auxiliary libs
Source: ..\src\bin\Debug\Landis.Library.AgeOnlyCohorts.dll; DestDir: {#BuildDir}; Flags: replacesameversion
Source: ..\src\bin\Debug\Landis.Library.Cohorts.dll; DestDir: {#BuildDir}; Flags: replacesameversion
Source: ..\src\bin\Debug\Landis.Library.LeafBiomassCohorts.dll; DestDir: {#AppDir}; Flags: replacesameversion
Source: ..\src\bin\Debug\Landis.Library.Succession.dll; DestDir: {#BuildDir}; Flags: uninsneveruninstall replacesameversion
Source: ..\src\bin\Debug\Landis.Library.Climate.dll; DestDir: {#BuildDir}; Flags: uninsneveruninstall replacesameversion

; Century Succession
Source: ..\src\bin\Debug\Landis.Extension.Succession.Century.dll; DestDir: {#BuildDir}; Flags: replacesameversion

; Supporting documents
Source: docs\LANDIS-II Century Succession v3.1 User Guide.pdf; DestDir: {#AppDir}\docs
Source: docs\Century-succession-log-metadata.xlsx; DestDir: {#AppDir}\docs
Source: examples\*; DestDir: {#AppDir}\examples\century-succession

#define CenturySucc "Century Succession 3.1.txt"
Source: {#CenturySucc}; DestDir: {#LandisPlugInDir}

[Run]
;; Run plug-in admin tool to add an entry for the plug-in
#define PlugInAdminTool  CoreBinDir + "\Landis.PlugIns.Admin.exe"

Filename: {#PlugInAdminTool}; Parameters: "remove ""Century Succession"" "; WorkingDir: {#LandisPlugInDir}
Filename: {#PlugInAdminTool}; Parameters: "add ""{#CenturySucc}"" "; WorkingDir: {#LandisPlugInDir}

[UninstallRun]

[Code]
#include "J:\Scheller\LANDIS-II\deploy\package (Code section) v3.iss"

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
