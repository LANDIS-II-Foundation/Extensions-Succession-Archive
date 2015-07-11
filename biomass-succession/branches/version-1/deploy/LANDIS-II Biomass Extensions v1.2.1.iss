#define PackageName      "Biomass Extensions"
;#define PackageNameLong  "Base Fire Extension"
#define Version          "1.2.1"
#define ReleaseType      "official"
;#define ReleaseNumber    "4"

#define CoreVersion      "5.1"
#define CoreReleaseAbbr  ""

#include "..\package (Setup section).iss"


[Files]

; Biomass Succession v1.2 plug-in and auxiliary libs (cohorts v1.1, dead v1.2)
Source: {#LandisBuildDir}\libs\biomass-cohort\build\release\Landis.Biomass.Cohorts.dll; DestDir: {app}\bin
Source: {#LandisBuildDir}\libs\dead-biomass\build\release\Landis.Biomass.Dead.dll; DestDir: {app}\bin
Source: {#LandisBuildDir}\plug-ins\biomass-succession\build\release\Landis.Biomass.Succession.dll; DestDir: {app}\bin

; For version of 1.2 of this package, the Succession library (v2.3) was updated
; It's marked as uninstallable because if the package is uninstalled and this
; version of the Succession library is removed, then age-only succession will
; break
Source: {#LandisBuildDir}\libs\succession\build\release\Landis.Succession.dll; DestDir: {app}\bin; Flags: uninsneveruninstall

Source: doc\LANDIS-II Biomass Succession v1.2 User Guide.pdf; DestDir: {app}\doc

#define BiomassSuccession "Biomass Succession 1.2.txt"
Source: {#BiomassSuccession}; DestDir: {#LandisPlugInDir}

; Biomass Output v1.2 plug-in
Source: {#LandisBuildDir}\plug-ins\output-biomass\build\release\Landis.Output.Biomass.dll; DestDir: {app}\bin
Source: doc\LANDIS-II Biomass Output v1.2 User Guide.pdf; DestDir: {app}\doc

#define BiomassOutput "Output Biomass 1.2.txt"
Source: {#BiomassOutput}; DestDir: {#LandisPlugInDir}

; Biomass Reclass v1.0 plug-in
Source: {#LandisBuildDir}\plug-ins\biomass-reclass\build\release\Landis.Output.BiomassReclass.dll; DestDir: {app}\bin
Source: doc\LANDIS-II Biomass Reclass Output v1.0 User Guide.pdf; DestDir: {app}\doc

#define BiomassReclass "Biomass Reclass 1.0.txt"
Source: {#BiomassReclass}; DestDir: {#LandisPlugInDir}

; All the example input-files for the 3 plug-ins are in examples\biomass
Source: examples\*; DestDir: {app}\examples; Flags: recursesubdirs


[Run]
;; Run plug-in admin tool to add entries for each plug-in
#define PlugInAdminTool  CoreBinDir + "\Landis.PlugIns.Admin.exe"

Filename: {#PlugInAdminTool}; Parameters: "add ""{#BiomassSuccession}"" "; WorkingDir: {#LandisPlugInDir}
Filename: {#PlugInAdminTool}; Parameters: "add ""{#BiomassOutput}"" "; WorkingDir: {#LandisPlugInDir}
Filename: {#PlugInAdminTool}; Parameters: "add ""{#BiomassReclass}"" "; WorkingDir: {#LandisPlugInDir}


[UninstallRun]
;; Run plug-in admin tool to remove entries for each plug-in
Filename: {#PlugInAdminTool}; Parameters: "remove ""Biomass Succession"" "; WorkingDir: {#LandisPlugInDir}
Filename: {#PlugInAdminTool}; Parameters: "remove ""Output Biomass"" "; WorkingDir: {#LandisPlugInDir}
Filename: {#PlugInAdminTool}; Parameters: "remove ""Biomass Reclass"" "; WorkingDir: {#LandisPlugInDir}


[Code]
#include "..\package (Code section).iss"

//-----------------------------------------------------------------------------

function CurrentVersion_PostUninstall(currentVersion: TInstalledVersion): Integer;
var
  plugInNames: Array of String;
  i: Integer;
begin
  // Version 1.0 doesn't remove plug-in names from database
  if currentVersion.Version = '1.0' then
    begin
    plugInNames := [ 'Biomass Succession', 'Output Biomass', 'Biomass Reclass' ];
    for i := 0 to GetArrayLength(plugInNames)-1 do
      Exec('{#PlugInAdminTool}', 'remove "' + plugInNames[i] + '"',
           ExtractFilePath('{#PlugInAdminTool}'),
		   SW_HIDE, ewWaitUntilTerminated, Result);
	end
  else
    Result := 0;
end;

//-----------------------------------------------------------------------------

function InitializeSetup_FirstPhase(): Boolean;
begin
  CurrVers_PostUninstall := @CurrentVersion_PostUninstall
  Result := True
end;
