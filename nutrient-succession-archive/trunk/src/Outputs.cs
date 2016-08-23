//  Copyright 2007 Conservation Biology Institute
//  Authors:  Robert M. Scheller
//  License:  Available at  
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Landis.Cohorts;
using Landis.Ecoregions;
using Landis.Landscape;
using Landis.Species;
using Landis.RasterIO;
using System.IO;
using System;


namespace Landis.Biomass.NuCycling.Succession
{
    public class Outputs
    {
    
        private static StreamWriter log;

        //---------------------------------------------------------------------
        public static void Initialize(IInputParameters parameters)
        {
        
            string logFileName   = "Nutrient-succession-log.csv"; 
            UI.WriteLine("   Opening Nutrient-succession log file \"{0}\" ...", logFileName);
            try {
                log = Data.CreateTextFile(logFileName);
            }
            catch (Exception err) {
                string mesg = string.Format("{0}", err.Message);
                throw new System.ApplicationException(mesg);
            }
            
            log.AutoFlush = true;
            log.Write("Time, Ecoregion, NumSites,");
            log.Write("AGB, TotalC, TotalLiveC,");
            log.Write("TotalSOC, CharcoalC, TotalDetritalC,");
            log.Write("CNratio");
            log.WriteLine("");
        }


        //---------------------------------------------------------------------
        public static void WriteLogFile(int CurrentTime)
        {
            
            double[] avgAGB        = new double[Model.Core.Ecoregions.Count];
            double[] avgtotalC      = new double[Model.Core.Ecoregions.Count];
            double[] avgtotalLiveC = new double[Model.Core.Ecoregions.Count];
            double[] avgtotalSOC = new double[Model.Core.Ecoregions.Count];
            double[] avgCharcoalC = new double[Model.Core.Ecoregions.Count];
            double[] avgtotalDetritalC = new double[Model.Core.Ecoregions.Count];
            double[] avgCNratio = new double[Model.Core.Ecoregions.Count];

            foreach (IEcoregion ecoregion in Model.Core.Ecoregions)
            {
                avgAGB[ecoregion.Index] = 0.0;
                avgtotalC[ecoregion.Index] = 0.0;
                avgtotalLiveC[ecoregion.Index] = 0.0;
                avgtotalSOC[ecoregion.Index] = 0.0;
                avgCharcoalC[ecoregion.Index] = 0.0;
                avgtotalDetritalC[ecoregion.Index] = 0.0;
                avgCNratio[ecoregion.Index] = 0.0;

            }
            foreach (ActiveSite site in Model.Core.Landscape)
            {
                IEcoregion ecoregion = Model.Core.Ecoregion[site];
                int totalBiomass = (int) SiteVars.ComputeTotalBiomass(site);
                
                avgAGB[ecoregion.Index] += totalBiomass;
                avgtotalC[ecoregion.Index]    += SiteVars.ComputeTotalC(site, totalBiomass);
                avgtotalLiveC[ecoregion.Index] += SiteVars.ComputeTotalLiveC(site, totalBiomass);
                avgtotalSOC[ecoregion.Index] += SiteVars.ComputeTotalSOC(site);
                avgCharcoalC[ecoregion.Index] += SiteVars.ComputeCharcoalC(site);
                avgtotalDetritalC[ecoregion.Index] += SiteVars.ComputeDetritalC(site);
                avgCNratio[ecoregion.Index] += SiteVars.ComputeCNratio(site);
            }
            
            foreach (IEcoregion ecoregion in Model.Core.Ecoregions) // output tons/ha

            {
                if(EcoregionData.ActiveSiteCount[ecoregion] > 0)
                {
                    log.Write("{0}, {1}, {2}, ", 
                        CurrentTime,                 
                        ecoregion.Name,                  
                        EcoregionData.ActiveSiteCount[ecoregion]       
                        );
                    log.Write("{0:0.00}, {1:0.00}, {2:0.00},",
                        (avgAGB[ecoregion.Index] / (double)EcoregionData.ActiveSiteCount[ecoregion]/1000.0),
                        (avgtotalC[ecoregion.Index] / (double)EcoregionData.ActiveSiteCount[ecoregion]/1000.0),
                        (avgtotalLiveC[ecoregion.Index] / (double)EcoregionData.ActiveSiteCount[ecoregion]/1000.0)
                        );
                    log.Write("{0:0.00}, {1:0.000}, {2:0.00},",
                        (avgtotalSOC[ecoregion.Index] / (double)EcoregionData.ActiveSiteCount[ecoregion]/1000.0),
                        (avgCharcoalC[ecoregion.Index] / (double)EcoregionData.ActiveSiteCount[ecoregion]/1000.0),
                        (avgtotalDetritalC[ecoregion.Index] / (double)EcoregionData.ActiveSiteCount[ecoregion]/1000.0)
                        );
                    log.Write("{0:0.00}",
                        (avgCNratio[ecoregion.Index] / (double)EcoregionData.ActiveSiteCount[ecoregion])
                        );
                    log.WriteLine("");
                }
            }
        }

        
        //---------------------------------------------------------------------

        private IOutputRaster<UShortPixel> CreateMap(string path)
        {
            UI.WriteLine("Writing output map to {0} ...", path);
            return Model.Core.CreateRaster<UShortPixel>(path,
                                                        Model.Core.Landscape.Dimensions,
                                                        Model.Core.LandscapeMapMetadata);
        }
        
    }
}
