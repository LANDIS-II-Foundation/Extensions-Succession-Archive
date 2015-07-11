//  Copyright 2007-2010 Portland State University, University of Wisconsin-Madison
//  Author: Robert Scheller, Ben Sulman

//using Landis.Cohorts;
using Landis.Core;
using Landis.SpatialModeling;
using Edu.Wisc.Forest.Flel.Util;
using System.IO;
using System;


namespace Landis.Extension.Succession.Century
{
    public class Outputs
    {
    
        private static StreamWriter log;
        private static StreamWriter logMonthly;
        public static StreamWriter CalibrateLog;

        //---------------------------------------------------------------------
        public static void Initialize(IInputParameters parameters)
        {
        
            string logFileName   = "Century-succession-log.csv";
            PlugIn.ModelCore.Log.WriteLine("   Opening Century-succession log file \"{0}\" ...", logFileName);
            try {
                log = new StreamWriter(logFileName);
            }
            catch (Exception err) {
                string mesg = string.Format("{0}", err.Message);
                throw new System.ApplicationException(mesg);
            }
            
            log.AutoFlush = true;
            log.Write("Time, Ecoregion, NumSites,");
            log.Write("NEEC, SOMTC, AGB, ");
            log.Write("AG_NPPC, BG_NPPC, LitterfallC, AgeMortality, ");
            log.Write("MineralN, TotalN, GrossMineralization, ");
            log.Write("C:LeafFRoot, C:WoodCRoot, C:DeadWood, C:DeadCRoot, ");
            log.Write("C:SurfStruc, C:SurfMeta, C:SoilStruc, C:SoilMeta, ");
            log.Write("C:SOM1surf, C:SOM1soil, C:SOM2, C:SOM3, ");
            log.Write("N:CohortLeaf, N:CohortWood, N:DeadWood, N:DeadRoot, ");
            log.Write("N:SurfStruc, N:SurfMeta, N:SoilStruc, N:SoilMeta, ");
            log.Write("N:SOM1surf, N:SOM1soil, N:SOM2, N:SOM3, ");
            log.Write("SurfStrucNetMin, SurfMetaNetMin, SoilStrucNetMin, SoilMetaNetMin, ");
            log.Write("SOM1surfNetMin, SOM1soilNetMin, SOM2NetMin, SOM3NetMin, ");
            log.Write("StreamC, StreamN, FireCEfflux, FireNEfflux, ");
            log.Write("Nuptake, Nresorbed, TotalSoilN, Nvol, avgfrassC,");
            log.WriteLine("");


        }

        //---------------------------------------------------------------------
        public static void InitializeMonthly(IInputParameters parameters)
        {
        
            string logFileName   = "Century-succession-monthly-log.csv"; 
            PlugIn.ModelCore.Log.WriteLine("   Opening Century-succession log file \"{0}\" ...", logFileName);
            try {
                logMonthly = new StreamWriter(logFileName);
            }
            catch (Exception err) {
                string mesg = string.Format("{0}", err.Message);
                throw new System.ApplicationException(mesg);
            }
            
            logMonthly.AutoFlush = true;
            logMonthly.Write("Time, Month, Ecoregion, NumSites,");
            logMonthly.Write("PPT, T, ");
            logMonthly.Write("NPPC, Resp, NEE, ");
            logMonthly.Write("Ndeposition,");
            logMonthly.WriteLine("");


        }


        //---------------------------------------------------------------------
        public static void WriteLogFile(int CurrentTime)
        {
            
            double[] avgAnnualPPT  = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgJJAtemp    = new double[PlugIn.ModelCore.Ecoregions.Count];
            
            double[] avgNEEc      = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgSOMtc      = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgAGB        = new double[PlugIn.ModelCore.Ecoregions.Count];
            
            double[] avgAGNPPtc      = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgBGNPPtc      = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgLittertc   = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgAgeMc = new double[PlugIn.ModelCore.Ecoregions.Count];
            
            double[] avgMineralN    = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgGrossMin    = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgTotalN      = new double[PlugIn.ModelCore.Ecoregions.Count];
            
            double[] avgCohortLeafC = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgCohortWoodC = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgWoodC       = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgCRootC      = new double[PlugIn.ModelCore.Ecoregions.Count];
            
            double[] avgCohortLeafN = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgCohortWoodN = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgWoodN       = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgCRootN      = new double[PlugIn.ModelCore.Ecoregions.Count];

            double[] avgSurfStrucC  = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgSurfMetaC   = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgSoilStrucC  = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgSoilMetaC   = new double[PlugIn.ModelCore.Ecoregions.Count];
            
            double[] avgSurfStrucN  = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgSurfMetaN   = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgSoilStrucN  = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgSoilMetaN   = new double[PlugIn.ModelCore.Ecoregions.Count];
            
            double[] avgSurfStrucNetMin = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgSurfMetaNetMin = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgSoilStrucNetMin = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgSoilMetaNetMin = new double[PlugIn.ModelCore.Ecoregions.Count];
            
            double[] avgSOM1surfC   = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgSOM1soilC   = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgSOM2C       = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgSOM3C       = new double[PlugIn.ModelCore.Ecoregions.Count];
            
            double[] avgSOM1surfN   = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgSOM1soilN   = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgSOM2N       = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgSOM3N       = new double[PlugIn.ModelCore.Ecoregions.Count];
            
            double[] avgSOM1surfNetMin = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgSOM1soilNetMin = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgSOM2NetMin = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgSOM3NetMin = new double[PlugIn.ModelCore.Ecoregions.Count];

            double[] avgStreamC = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgStreamN = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgFireCEfflux = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgFireNEfflux = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgNvol = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgNresorbed = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgTotalSoilN = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgNuptake = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgfrassC = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avglai = new double[PlugIn.ModelCore.Ecoregions.Count];            

            foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
            {
                avgAnnualPPT[ecoregion.Index] = 0.0;
                avgJJAtemp[ecoregion.Index] = 0.0;
                
                avgNEEc[ecoregion.Index] = 0.0;
                avgSOMtc[ecoregion.Index] = 0.0;
                avgAGB[ecoregion.Index] = 0.0;
                
                avgAGNPPtc[ecoregion.Index] = 0.0;
                avgBGNPPtc[ecoregion.Index] = 0.0;
                avgLittertc[ecoregion.Index] = 0.0;
                avgAgeMc[ecoregion.Index] = 0.0;
                
                avgMineralN[ecoregion.Index] = 0.0;
                avgGrossMin[ecoregion.Index] = 0.0;
                avgTotalN[ecoregion.Index] = 0.0;
                
                avgCohortLeafC[ecoregion.Index] = 0.0;
                avgCohortWoodC[ecoregion.Index] = 0.0;
                avgWoodC[ecoregion.Index] = 0.0;
                avgCRootC[ecoregion.Index] = 0.0;
                
                avgSurfStrucC[ecoregion.Index] = 0.0;
                avgSurfMetaC[ecoregion.Index] = 0.0;
                avgSoilStrucC[ecoregion.Index] = 0.0;
                avgSoilMetaC[ecoregion.Index] = 0.0;
                
                avgCohortLeafN[ecoregion.Index] = 0.0;
                avgCohortWoodN[ecoregion.Index] = 0.0;
                avgWoodN[ecoregion.Index] = 0.0;
                avgCRootN[ecoregion.Index] = 0.0;

                avgSurfStrucN[ecoregion.Index] = 0.0;
                avgSurfMetaN[ecoregion.Index] = 0.0;
                avgSoilStrucN[ecoregion.Index] = 0.0;
                avgSoilMetaN[ecoregion.Index] = 0.0;
                
                avgSurfStrucNetMin[ecoregion.Index] = 0.0;
                avgSurfMetaNetMin[ecoregion.Index] = 0.0;
                avgSoilStrucNetMin[ecoregion.Index] = 0.0;
                avgSoilMetaNetMin[ecoregion.Index] = 0.0;
                
                avgSOM1surfC[ecoregion.Index] = 0.0;
                avgSOM1soilC[ecoregion.Index] = 0.0;
                avgSOM2C[ecoregion.Index] = 0.0;
                avgSOM3C[ecoregion.Index] = 0.0;
                
                avgSOM1surfN[ecoregion.Index] = 0.0;
                avgSOM1soilN[ecoregion.Index] = 0.0;
                avgSOM2N[ecoregion.Index] = 0.0;
                avgSOM3N[ecoregion.Index] = 0.0;
                
                avgSOM1surfNetMin[ecoregion.Index] = 0.0;
                avgSOM1soilNetMin[ecoregion.Index] = 0.0;
                avgSOM2NetMin[ecoregion.Index] = 0.0;
                avgSOM3NetMin[ecoregion.Index] = 0.0;

                avgStreamC[ecoregion.Index] = 0.0;
                avgStreamN[ecoregion.Index] = 0.0;
                avgFireCEfflux[ecoregion.Index] = 0.0;
                avgFireNEfflux[ecoregion.Index] = 0.0;
                avgNuptake[ecoregion.Index] = 0.0;
                avgNresorbed[ecoregion.Index] = 0.0;
                avgTotalSoilN[ecoregion.Index] = 0.0;
                avgNvol[ecoregion.Index] = 0.0;
                avgfrassC[ecoregion.Index] = 0.0;
                
            }



            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];
                
                avgNEEc[ecoregion.Index]    += SiteVars.AnnualNEE[site];
                avgSOMtc[ecoregion.Index]    += GetOrganicCarbon(site);
                avgAGB[ecoregion.Index] += Century.ComputeLivingBiomass(SiteVars.Cohorts[site]); // + SiteVars.CohortWoodC[site]) * 2.13;
                
                avgAGNPPtc[ecoregion.Index]    += SiteVars.AGNPPcarbon[site];
                avgBGNPPtc[ecoregion.Index]    += SiteVars.BGNPPcarbon[site];
                avgLittertc[ecoregion.Index] += SiteVars.LitterfallC[site];
                avgAgeMc[ecoregion.Index] += SiteVars.AgeMortality[site] * 0.47;

                avgMineralN[ecoregion.Index] += SiteVars.MineralN[site];
                avgTotalN[ecoregion.Index]   += GetTotalNitrogen(site);
                avgGrossMin[ecoregion.Index] += SiteVars.GrossMineralization[site];

                avgCohortLeafC[ecoregion.Index] += SiteVars.CohortLeafC[site];
                avgCohortWoodC[ecoregion.Index] += SiteVars.CohortWoodC[site];
                avgWoodC[ecoregion.Index]       += SiteVars.SurfaceDeadWood[site].Carbon; 
                avgCRootC[ecoregion.Index]      += SiteVars.SoilDeadWood[site].Carbon; 
                
                avgSurfStrucC[ecoregion.Index] += SiteVars.SurfaceStructural[site].Carbon; 
                avgSurfMetaC[ecoregion.Index]  += SiteVars.SurfaceMetabolic[site].Carbon; 
                avgSoilStrucC[ecoregion.Index] += SiteVars.SoilStructural[site].Carbon; 
                avgSoilMetaC[ecoregion.Index]  += SiteVars.SoilMetabolic[site].Carbon; 

                avgSOM1surfC[ecoregion.Index] += SiteVars.SOM1surface[site].Carbon; 
                avgSOM1soilC[ecoregion.Index] += SiteVars.SOM1soil[site].Carbon; 
                avgSOM2C[ecoregion.Index]     += SiteVars.SOM2[site].Carbon; 
                avgSOM3C[ecoregion.Index]     += SiteVars.SOM3[site].Carbon; 

                avgCohortLeafN[ecoregion.Index]   += SiteVars.CohortLeafN[site];
                avgCohortWoodN[ecoregion.Index]   += SiteVars.CohortWoodN[site];
                avgWoodN[ecoregion.Index]     += SiteVars.SurfaceDeadWood[site].Nitrogen; 
                avgCRootN[ecoregion.Index]    += SiteVars.SoilDeadWood[site].Nitrogen; 
                
                avgSurfStrucN[ecoregion.Index] += SiteVars.SurfaceStructural[site].Nitrogen; 
                avgSurfMetaN[ecoregion.Index]  += SiteVars.SurfaceMetabolic[site].Nitrogen; 
                avgSoilStrucN[ecoregion.Index] += SiteVars.SoilStructural[site].Nitrogen; 
                avgSoilMetaN[ecoregion.Index]  += SiteVars.SoilMetabolic[site].Nitrogen; 
                
                avgSOM1surfN[ecoregion.Index] += SiteVars.SOM1surface[site].Nitrogen; 
                avgSOM1soilN[ecoregion.Index] += SiteVars.SOM1soil[site].Nitrogen; 
                avgSOM2N[ecoregion.Index]     += SiteVars.SOM2[site].Nitrogen; 
                avgSOM3N[ecoregion.Index]     += SiteVars.SOM3[site].Nitrogen;
                avgTotalSoilN[ecoregion.Index] += GetTotalSoilNitrogen(site);
                
                avgSurfStrucNetMin[ecoregion.Index] += SiteVars.SurfaceStructural[site].NetMineralization; 
                avgSurfMetaNetMin[ecoregion.Index]  += SiteVars.SurfaceMetabolic[site].NetMineralization; 
                avgSoilStrucNetMin[ecoregion.Index] += SiteVars.SoilStructural[site].NetMineralization; 
                avgSoilMetaNetMin[ecoregion.Index]  += SiteVars.SoilMetabolic[site].NetMineralization; 
                
                avgSOM1surfNetMin[ecoregion.Index] += SiteVars.SOM1surface[site].NetMineralization; 
                avgSOM1soilNetMin[ecoregion.Index] += SiteVars.SOM1soil[site].NetMineralization; 
                avgSOM2NetMin[ecoregion.Index]     += SiteVars.SOM2[site].NetMineralization; 
                avgSOM3NetMin[ecoregion.Index]     += SiteVars.SOM3[site].NetMineralization; 
                
                avgStreamC[ecoregion.Index] += SiteVars.Stream[site].Carbon;
                avgStreamN[ecoregion.Index] += SiteVars.Stream[site].Nitrogen; //+ SiteVars.NLoss[site];
                avgFireCEfflux[ecoregion.Index] += SiteVars.FireCEfflux[site];
                avgFireNEfflux[ecoregion.Index] += SiteVars.FireNEfflux[site];
                avgNresorbed[ecoregion.Index] += SiteVars.ResorbedN[site];
                avgNuptake[ecoregion.Index] += GetSoilNuptake(site);
                avgNvol[ecoregion.Index] += SiteVars.Nvol[site];
                avgfrassC[ecoregion.Index] += SiteVars.FrassC[site];

                               
                
            }
            
            foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
            {
                if(EcoregionData.ActiveSiteCount[ecoregion] > 0)
                {
                    log.Write("{0}, {1}, {2}, ", 
                        CurrentTime,                 
                        ecoregion.Name,                  
                        EcoregionData.ActiveSiteCount[ecoregion]       
                        );
                    log.Write("{0:0.00}, {1:0.0}, {2:0.0}, ", 
                        (avgNEEc[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion]),  
                        (avgSOMtc[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion]),  
                        (avgAGB[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion])  
                        );
                    log.Write("{0:0.00}, {1:0.0}, {2:0.0}, {3:0.0}, ",
                        (avgAGNPPtc[ecoregion.Index] / (double)EcoregionData.ActiveSiteCount[ecoregion]),
                        (avgBGNPPtc[ecoregion.Index] / (double)EcoregionData.ActiveSiteCount[ecoregion]),
                        (avgLittertc[ecoregion.Index] / (double)EcoregionData.ActiveSiteCount[ecoregion]),
                        (avgAgeMc[ecoregion.Index] / (double)EcoregionData.ActiveSiteCount[ecoregion])
                        );
                    log.Write("{0:0.0}, {1:0.0}, {2:0.0}, ", 
                        (avgMineralN[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion]),  
                        (avgTotalN[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion]),              
                        (avgGrossMin[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion])
                        );
                    log.Write("{0:0.0}, {1:0.0}, {2:0.0}, {3:0.0}, ", 
                        (avgCohortLeafC[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion]),              
                        (avgCohortWoodC[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion]),              
                        (avgWoodC[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion]), 
                        (avgCRootC[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion])           
                        );
                    log.Write("{0:0.0}, {1:0.0}, {2:0.0}, {3:0.0}, ", 
                        (avgSurfStrucC[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion]), 
                        (avgSurfMetaC[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion]),          
                        (avgSoilStrucC[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion]),           
                        (avgSoilMetaC[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion])
                        );
                    log.Write("{0:0.0}, {1:0.0}, {2:0.0}, {3:0.0}, ", 
                        (avgSOM1surfC[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion]), 
                        (avgSOM1soilC[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion]),          
                        (avgSOM2C[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion]),           
                        (avgSOM3C[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion])
                        );
                    log.Write("{0:0.00}, {1:0.00}, {2:0.00}, {3:0.00}, ", 
                        (avgCohortLeafN[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion]),              
                        (avgCohortWoodN[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion]),              
                        (avgWoodN[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion]),          
                        (avgCRootN[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion])
                        );
                    log.Write("{0:0.00}, {1:0.00}, {2:0.00}, {3:0.00}, ", 
                        (avgSurfStrucN[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion]), 
                        (avgSurfMetaN[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion]),          
                        (avgSoilStrucN[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion]),           
                        (avgSoilMetaN[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion])
                        );
                    log.Write("{0:0.00}, {1:0.00}, {2:0.00}, {3:0.00}, ", 
                        (avgSOM1surfN[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion]), 
                        (avgSOM1soilN[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion]),          
                        (avgSOM2N[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion]),           
                        (avgSOM3N[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion])
                        );
                    log.Write("{0:0.0}, {1:0.0}, {2:0.0}, {3:0.0}, ", 
                        (avgSurfStrucNetMin[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion]), 
                        (avgSurfMetaNetMin[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion]),          
                        (avgSoilStrucNetMin[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion]),           
                        (avgSoilMetaNetMin[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion])
                        );
                    log.Write("{0:0.0}, {1:0.0}, {2:0.0}, {3:0.0}, ", 
                        (avgSOM1surfNetMin[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion]), 
                        (avgSOM1soilNetMin[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion]),          
                        (avgSOM2NetMin[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion]),           
                        (avgSOM3NetMin[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion])
                        );
                    log.Write("{0:0.0000}, {1:0.0000}, {2:0.000}, {3:0.0}, ",
                        (avgStreamC[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion]),
                        (avgStreamN[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion]),
                        (avgFireCEfflux[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion]),
                        (avgFireNEfflux[ecoregion.Index] / (double)EcoregionData.ActiveSiteCount[ecoregion])
                        );
                    log.Write("{0:0.0000}, {1:0.0000}, {2:0.000}, {3:0.0}, ",
                        (avgNuptake[ecoregion.Index] / (double)EcoregionData.ActiveSiteCount[ecoregion]),
                        (avgNresorbed[ecoregion.Index] / (double)EcoregionData.ActiveSiteCount[ecoregion]),
                        (avgTotalSoilN[ecoregion.Index] / (double)EcoregionData.ActiveSiteCount[ecoregion]),
                        (avgNvol[ecoregion.Index] / (double)EcoregionData.ActiveSiteCount[ecoregion])
                        );
                    log.Write("{0:0.0000},  ",
                        (avgfrassC[ecoregion.Index] / (double)EcoregionData.ActiveSiteCount[ecoregion])
                        );
                    log.WriteLine("");
                }
               
            }
            //Reset back to zero:
            //These are being reset here because fire effects are handled in the first year of the 
            //growth loop but the reporting doesn't happen until after all growth is finished.
            SiteVars.FireCEfflux.ActiveSiteValues = 0.0;
            SiteVars.FireNEfflux.ActiveSiteValues = 0.0;

        }

        public static void WriteMonthlyLogFile(int month)
        {

            double[] ppt = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] airtemp = new double[PlugIn.ModelCore.Ecoregions.Count];

            double[] avgNPPtc = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgResp = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgNEE = new double[PlugIn.ModelCore.Ecoregions.Count];

            double[] Ndep = new double[PlugIn.ModelCore.Ecoregions.Count];

            foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
            {
                ppt[ecoregion.Index] = 0.0;
                airtemp[ecoregion.Index] = 0.0;

                avgNPPtc[ecoregion.Index] = 0.0;
                avgResp[ecoregion.Index] = 0.0;
                avgNEE[ecoregion.Index] = 0.0;

                Ndep[ecoregion.Index] = 0.0;
            }



            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];

                ppt[ecoregion.Index] = EcoregionData.AnnualWeather[ecoregion].MonthlyPrecip[month];
                airtemp[ecoregion.Index] = EcoregionData.AnnualWeather[ecoregion].MonthlyTemp[month];

                avgNPPtc[ecoregion.Index] += SiteVars.MonthlyAGNPPcarbon[site][month] + SiteVars.MonthlyBGNPPcarbon[site][month];
                avgResp[ecoregion.Index] += SiteVars.MonthlyResp[site][month];
                avgNEE[ecoregion.Index] += SiteVars.MonthlyNEE[site][month];

                SiteVars.AnnualNEE[site] += SiteVars.MonthlyNEE[site][month];

                Ndep[ecoregion.Index] = EcoregionData.AnnualWeather[ecoregion].MonthlyNdeposition[month];
            }

            foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
            {
                if (EcoregionData.ActiveSiteCount[ecoregion] > 0)
                {
                    logMonthly.Write("{0}, {1}, {2}, {3},",
                        PlugIn.ModelCore.CurrentTime,
                        month + 1,
                        ecoregion.Name,
                        EcoregionData.ActiveSiteCount[ecoregion]
                        );
                    logMonthly.Write("{0:0.0}, {1:0.0}, ",
                        ppt[ecoregion.Index],
                        airtemp[ecoregion.Index]
                        );
                    logMonthly.Write("{0:0.00}, {1:0.00}, {2:0.00}, ",
                        (avgNPPtc[ecoregion.Index] / (double)EcoregionData.ActiveSiteCount[ecoregion]),
                        (avgResp[ecoregion.Index] / (double)EcoregionData.ActiveSiteCount[ecoregion]),
                        (avgNEE[ecoregion.Index] / (double)EcoregionData.ActiveSiteCount[ecoregion])
                        );
                    logMonthly.Write("{0:0.00}, ",
                        Ndep[ecoregion.Index]
                        );
                    logMonthly.WriteLine("");
                }
            }
        }
        
        //Write log file for growth and limits
        public static void CreateCalibrateLogFile()
        {
            string logFileName = "Century-calibrate-log.csv";
            PlugIn.ModelCore.Log.WriteLine("   Opening Century calibrate log file \"{0}\" ...", logFileName);
            try
            {
                CalibrateLog = new StreamWriter(logFileName);
            }
            catch (Exception err)
            {
                string mesg = string.Format("{0}", err.Message);
                throw new System.ApplicationException(mesg);
            }

            CalibrateLog.AutoFlush = true;

            CalibrateLog.Write("Year, Month, EcoregionIndex, SpeciesName, CohortAge, CohortWoodB, CohortLeafB, ");  // from ComputeChange
            CalibrateLog.Write("MortalityAGEwood, MortalityAGEleaf, ");  // from ComputeAgeMortality
            CalibrateLog.Write("MortalityBIOwood, MortalityBIOleaf, ");  // from ComputeGrowthMortality
            CalibrateLog.Write("mineralNalloc, resorbedNalloc, ");  // from calculateN_Limit
            CalibrateLog.Write("limitLAI, limitH20, limitT, limitCapacity, limitN, ");  //from ComputeActualANPP
            CalibrateLog.Write("maxNPP, Bmax, Bsite, Bcohort, soilTemp, ");  //from ComputeActualANPP
            CalibrateLog.Write("actualWoodNPP, actualLeafNPP, ");  //from ComputeActualANPP
            CalibrateLog.Write("deltaWood, deltaLeaf, totalMortalityWood, totalMortalityLeaf, ");  // from ComputeChange
            CalibrateLog.WriteLine("resorbedNused, mineralNused, Ndemand,");  // from ComputeChange
            

        }
        
        
        //---------------------------------------------------------------------
        private static double GetTotalNitrogen(ActiveSite site)
        {
        
            
            double totalN =
                    + SiteVars.CohortLeafN[site] 
                    + SiteVars.CohortWoodN[site] 

                    + SiteVars.MineralN[site]
                    
                    + SiteVars.SurfaceDeadWood[site].Nitrogen
                    + SiteVars.SoilDeadWood[site].Nitrogen
                    
                    + SiteVars.SurfaceStructural[site].Nitrogen
                    + SiteVars.SoilStructural[site].Nitrogen
                    + SiteVars.SurfaceMetabolic[site].Nitrogen
                    + SiteVars.SoilMetabolic[site].Nitrogen

                    + SiteVars.SOM1surface[site].Nitrogen
                    + SiteVars.SOM1soil[site].Nitrogen
                    + SiteVars.SOM2[site].Nitrogen
                    + SiteVars.SOM3[site].Nitrogen
                    ;
        
            return totalN;
        }

        private static double GetTotalSoilNitrogen(ActiveSite site)
        {


            double totalsoilN =

                    +SiteVars.MineralN[site]

                   + SiteVars.SurfaceDeadWood[site].Nitrogen
                   + SiteVars.SoilDeadWood[site].Nitrogen

                    + SiteVars.SurfaceStructural[site].Nitrogen
                    + SiteVars.SoilStructural[site].Nitrogen
                    + SiteVars.SurfaceMetabolic[site].Nitrogen
            +SiteVars.SoilMetabolic[site].Nitrogen

            + SiteVars.SOM1surface[site].Nitrogen
            + SiteVars.SOM1soil[site].Nitrogen
            + SiteVars.SOM2[site].Nitrogen
            + SiteVars.SOM3[site].Nitrogen;
                    ;

            return totalsoilN;
        }
        //---------------------------------------------------------------------
        public static double GetOrganicCarbon(Site site)
        {
            double totalC = 
                    
                    SiteVars.SurfaceStructural[site].Carbon
                    + SiteVars.SoilStructural[site].Carbon
                    + SiteVars.SurfaceMetabolic[site].Carbon
                    + SiteVars.SoilMetabolic[site].Carbon

                    + SiteVars.SOM1surface[site].Carbon
                    + SiteVars.SOM1soil[site].Carbon
                    + SiteVars.SOM2[site].Carbon
                    + SiteVars.SOM3[site].Carbon
                    ;
        
            return totalC;
        }
        //---------------------------------------------------------------------
        private static double GetSoilNuptake(ActiveSite site)
        {
            double soilNuptake =

                    SiteVars.TotalNuptake[site]
                    
                    
                    ;

            return soilNuptake;
        }
        
    }
}
