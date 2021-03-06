#define PackageName      "Biomass Succession"
#define PackageNameLong  "Biomass Succession Extension"
#define Version          "3.1.2"
#define ReleaseType      "beta"
#define ReleaseNumber    "3"

#define CoreVersion      "6.0"
#define CoreReleaseAbbr  ""

#include "C:\BRM\LANDIS_II\Code\SDK\Deploy\package (Setup section) v6.0.iss"
#define ExtDir "C:\Program Files\LANDIS-II\v6\bin\extensions"
#define AppDir "C:\Program Files\LANDIS-II\v6\"


[Files]

Source: C:\Program Files\LANDIS-II\v6\bin\extensions\Landis.Extension.Succession.Biomass.dll; DestDir: {#ExtDir}; Flags: replacesameversion
Source: C:\Program Files\LANDIS-II\v6\bin\extensions\Landis.Library.Succession.dll; DestDir: {#ExtDir}; Flags: replacesameversion uninsneveruninstall
Source: C:\Program Files\LANDIS-II\v6\bin\extensions\Landis.Library.BiomassCohorts.dll; DestDir: {#ExtDir}; Flags: replacesameversion uninsneveruninstall
Source: C:\Program Files\LANDIS-II\v6\bin\extensions\Landis.Library.AgeOnlyCohorts.dll; DestDir: {#ExtDir}; Flags: replacesameversion uninsneveruninstall
Source: C:\Program Files\LANDIS-II\v6\bin\extensions\Landis.Library.Cohorts.dll; DestDir: {#ExtDir}; Flags: replacesameversion uninsneveruninstall

Source: docs\LANDIS-II Biomass Succession v3.1 User Guide.pdf; DestDir: {#AppDir}\docs
Source: examples\*.txt; DestDir: {#AppDir}\examples\biomass-succession
Source: examples\*.gis; DestDir: {#AppDir}\examples\biomass-succession
Source: examples\*.bat; DestDir: {#AppDir}\examples\biomass-succession

#define BioSucc3 "Biomass Succession 3.1.txt"
Source: {#BioSucc3}; DestDir: {#LandisPlugInDir}

[Run]
;; Run plug-in admin tool to add an entry for the plug-in
#define PlugInAdminTool  CoreBinDir + "\Landis.PlugIns.Admin.exe"

Filename: {#PlugInAdminTool}; Parameters: "remove ""Biomass Succession"" "; WorkingDir: {#LandisPlugInDir}
Filename: {#PlugInAdminTool}; Parameters: "add ""{#BioSucc3}"" "; WorkingDir: {#LandisPlugInDir}

[Code]

#include "C:\BRM\LANDIS_II\Code\SDK\Deploy\package (Code section) v3.iss"

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
