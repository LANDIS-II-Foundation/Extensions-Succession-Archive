#define PackageName      "Age-only Succession"
#define PackageNameLong  "Age-only Succession Extension"
#define Version          "1.3"
#define ReleaseType      "official"
#define ReleaseNumber    ""

#define CoreVersion      "5.1"
#define CoreReleaseAbbr  ""

#include AddBackslash(GetEnv("LANDIS_DEPLOY")) + "package (Setup section).iss"

#if ReleaseType != "official"
  #define Configuration  "debug"
#else
  #define Configuration  "release"
#endif

[Files]

; Base Harvest (v1.0)
Source: {#LandisBuildDir}\successionextensions\age-only-succession\build\{#Configuration}\Landis.AgeOnly.Succession.dll; DestDir: {app}\bin
Source: docs\*; DestDir: {app}\docs
Source: examples\*; DestDir: {app}\examples

#define AgeOnlySucc "Age-only Succession 1.3.txt"
Source: {#AgeOnlySucc}; DestDir: {#LandisPlugInDir}

; Until the the latest version of that library is released for the LANDIS-II main
; package, the library is included in this installer.  It's marked as
; uninstallable because if the package is uninstalled and this version
; of the Succession library is removed, then age-only succession will
; break
Source: {#LandisBuildDir}\successionextensions\succession\build\release\Landis.Succession.dll; DestDir: {app}\bin; Flags: uninsneveruninstall


[Run]
;; Run plug-in admin tool to add an entry for the plug-in
#define PlugInAdminTool  CoreBinDir + "\Landis.PlugIns.Admin.exe"

Filename: {#PlugInAdminTool}; Parameters: "add ""{#AgeOnlySucc}"" "; WorkingDir: {#LandisPlugInDir}

[UninstallRun]
;; Run plug-in admin tool to remove the entry for the plug-in
Filename: {#PlugInAdminTool}; Parameters: "remove ""Age-only Succession"" "; WorkingDir: {#LandisPlugInDir}

[Code]
#include AddBackslash(LandisDeployDir) + "package (Code section).iss"

//-----------------------------------------------------------------------------

function CurrentVersion_PostUninstall(currentVersion: TInstalledVersion): Integer;
begin
  // Alpha and beta releases of version 1.0 don't remove the plug-in name from
  // database
  if StartsWith(currentVersion.Version, '2.0') then
    begin
      Exec('{#PlugInAdminTool}', 'remove "Age-only Succession"',
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
