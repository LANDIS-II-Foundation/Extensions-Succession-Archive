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


namespace Landis.Biomass.Succession
{
    public class Outputs
    {

        //private static string mapNameTemplate;
        // private static string logFileName;
        private static StreamWriter log;

        //---------------------------------------------------------------------
        public static void Initialize(IInputParameters parameters)
        {

            string logFileName   = "Biomass-succession-v3-log.csv";
            UI.WriteLine("   Opening Biomass-succession log file \"{0}\" ...", logFileName);
            try {
                log = Data.CreateTextFile(logFileName);
            }
            catch (Exception err) {
                string mesg = string.Format("{0}", err.Message);
                throw new System.ApplicationException(mesg);
            }

            log.AutoFlush = true;
            log.Write("Time, Ecoregion, NumSites,");
            log.Write("LiveB, AG_NPP, LitterB");
            log.WriteLine("");


        }

        //---------------------------------------------------------------------
        public static void WriteLogFile(int CurrentTime)
        {

            double[] avgLiveB       = new double[Model.Core.Ecoregions.Count];
            double[] avgAG_NPP      = new double[Model.Core.Ecoregions.Count];
            double[] avgLitterB     = new double[Model.Core.Ecoregions.Count];


            foreach (IEcoregion ecoregion in Model.Core.Ecoregions)
            {
                avgLiveB[ecoregion.Index] = 0.0;
                avgAG_NPP[ecoregion.Index] = 0.0;
                avgLitterB[ecoregion.Index] = 0.0;
            }



            foreach (ActiveSite site in Model.Core.Landscape)
            {
                IEcoregion ecoregion = Model.Core.Ecoregion[site];
                int youngBiomass;  //ignored

                avgLiveB[ecoregion.Index]    += Landis.Biomass.Cohorts.ComputeBiomass(SiteVars.Cohorts[site], out youngBiomass);
                avgAG_NPP[ecoregion.Index]   += SiteVars.AGNPP[site];
                avgLitterB[ecoregion.Index]  += SiteVars.Litter[site].Mass;
            }

            foreach (IEcoregion ecoregion in Model.Core.Ecoregions)
            {
                if(EcoregionData.ActiveSiteCount[ecoregion] > 0)
                {
                    log.Write("{0}, {1}, {2}, ",
                        CurrentTime,
                        ecoregion.Name,
                        EcoregionData.ActiveSiteCount[ecoregion]
                        );
                    log.Write("{0:0.00}, {1:0.0}, {2:0.0}",
                        (avgLiveB[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion]),
                        (avgAG_NPP[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion]),
                        (avgLitterB[ecoregion.Index] / (double) EcoregionData.ActiveSiteCount[ecoregion])
                        );
                    log.WriteLine("");
                }
            }
        }

    }
}
