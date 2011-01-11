#define PackageName      "Biomass Succession"
#define PackageNameLong  "Biomass Succession Extension"
#define Version          "3.0"
#define ReleaseType      "official"
#define ReleaseNumber    "3"
; #define LandisDeployDir  "c:\users\rob\landis-ii\deploy"

#define CoreVersion      "6.0"
#define CoreReleaseAbbr  ""

#include AddBackslash(GetEnv("LANDIS_DEPLOY")) + "package (Setup section).iss"
; #include "c:\users\rob\landis-ii\deploy\package (Setup section).iss"

[Files]

Source: C:\Program Files\LANDIS-II\6.0\bin\Landis.Extension.Succession.Biomass.dll; DestDir: {app}\bin; Flags: replacesameversion
Source: docs\LANDIS-II Biomass Succession v3.0 User Guide.pdf; DestDir: {app}\docs
Source: examples\*; DestDir: {app}\examples\biomass-succession

#define BioSucc3 "Biomass Succession 3.0.txt"
Source: {#BioSucc3}; DestDir: {#LandisPlugInDir}

; Until the the latest version of that library is released for the LANDIS-II main
; package, the library is included in this installer.  It's marked as
; uninstallable because if the package is uninstalled and this version
; of the Succession library is removed, then age-only succession will
; break
Source: C:\Program Files\LANDIS-II\6.0\bin\Landis.Library.Succession.dll; DestDir: {app}\bin; Flags: replacesameversion uninsneveruninstall

; Cohort Libraries
Source: C:\Program Files\LANDIS-II\6.0\bin\Landis.Library.BiomassCohorts.dll; DestDir: {app}\bin; Flags: replacesameversion uninsneveruninstall


[Run]
;; Run plug-in admin tool to add an entry for the plug-in
#define PlugInAdminTool  CoreBinDir + "\Landis.PlugIns.Admin.exe"
Filename: {#PlugInAdminTool}; Parameters: "remove ""Biomass Succession"" "; WorkingDir: {#LandisPlugInDir}
Filename: {#PlugInAdminTool}; Parameters: "add ""{#BioSucc3}"" "; WorkingDir: {#LandisPlugInDir}

[UninstallRun]
;; Run plug-in admin tool to remove the entry for the plug-in
; Filename: {#PlugInAdminTool}; Parameters: "remove ""Biomass Succession v3"" "; WorkingDir: {#LandisPlugInDir}
[Code]

#include AddBackslash(LandisDeployDir) + "package (Code section) v3.iss"
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
