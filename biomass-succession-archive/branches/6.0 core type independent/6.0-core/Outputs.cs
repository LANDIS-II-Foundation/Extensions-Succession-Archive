//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller

using Wisc.Flel.GeospatialModeling.RasterIO;
using Wisc.Flel.GeospatialModeling.Landscapes;
using Landis.Library.BiomassCohorts;
using Landis.Core;
using System.Collections.Generic;
using System.IO;
using System;


namespace Landis.Extension.Succession.Biomass
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
            PlugIn.ModelCore.Log.WriteLine("   Opening Biomass-succession log file \"{0}\" ...", logFileName);
            try {
                log = PlugIn.ModelCore.CreateTextFile(logFileName);
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

            double[] avgLiveB       = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgAG_NPP      = new double[PlugIn.ModelCore.Ecoregions.Count];
            double[] avgLitterB     = new double[PlugIn.ModelCore.Ecoregions.Count];


            foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
            {
                avgLiveB[ecoregion.Index] = 0.0;
                avgAG_NPP[ecoregion.Index] = 0.0;
                avgLitterB[ecoregion.Index] = 0.0;
            }



            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];
                int youngBiomass;  //ignored

                avgLiveB[ecoregion.Index]    += Cohorts.ComputeBiomass(SiteVars.Cohorts[site], out youngBiomass);
                avgAG_NPP[ecoregion.Index]   += SiteVars.AGNPP[site];
                avgLitterB[ecoregion.Index]  += SiteVars.Litter[site].Mass;
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
