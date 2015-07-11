//  Copyright 2007-2010 University of Nevada, Portland State University
//  Authors:  Sarah Ganschow, Robert M. Scheller
//  License:  Available at
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;
using Landis.Ecoregions;
using Landis.Landscape;
using Landis.Species;

using System;
using System.Collections.Generic;

namespace Landis.Biomass.NuCycling.Succession
{
    public class FireEffects
    {
        private static ISeverity[] fireSeverities;

        //---------------------------------------------------------------------

        public static void Initialize(IInputParameters parameters)
        {
            UpdateParameters(parameters);
        }

        //---------------------------------------------------------------------

        public static void UpdateParameters(DynamicChange.IInputParameters parameters)
        {

            fireSeverities = parameters.FireSeverities;
        }

        //---------------------------------------------------------------------

        public static ISeverity[] FireSeverities
        {
            get
            {
                return fireSeverities;
            }

            set
            {
                fireSeverities = value;
            }
        }


        //---------------------------------------------------------------------

        //public static void ProcessFireEffects(ActiveSite site)
        //{
            //ComputeFireEffects(site);



        //}

        //---------------------------------------------------------------------

        /// <summary>
        /// Computes fire effects on litter, coarse woody debris, mineral soil, and charcoal.
        ///   No effects on soil organic matter (negligible according to Johnson et al. 2001).
        /// </summary>
        public static void ComputeFireEffects(ActiveSite site)
        {

            byte severity = SiteVars.FireSeverity[site];

            if(severity < 1) return;


            Charcoal charcoal = SiteVars.Charcoal[site];
            List<PoolD> siteLitter = SiteVars.Litter[site];
            PoolD woodyDebris = SiteVars.WoodyDebris[site];
            MineralSoil mineralSoil = SiteVars.MineralSoil[site];

            double litterReduc = FireSeverities[severity - 1].LitterReduction;
            double woodyDebrisReduc = FireSeverities[severity - 1].WoodyDebrisReduction;

            //UI.WriteLine("Burning at severity={0}.  LitterReduction={1}, WoodReduction={2}.", severity, litterReduc, woodyDebrisReduc);

            //Portion of charcoal is consumed by fire (Czimczik et al. 2005).
            //  Assuming 80% consumption (boreal forest, Czimczik et al. 2005).
            double consumption = 0.8;
            double charcoalLossN = charcoal.ContentN * consumption;
            double charcoalLossP = charcoal.ContentP * consumption;

            charcoal.ContentC -= (charcoal.ContentC * consumption);
            charcoal.ContentN -= charcoalLossN;
            charcoal.ContentP -= charcoalLossP;

            //******************************************************************

            //Portion of litter is consumed.
            double litterLossC = 0.0;
            double litterLossN = 0.0;
            double litterLossP = 0.0;

            //100% reduction in litter.
            if (litterReduc == 1.0)
            {
                foreach (PoolD litter in siteLitter)
                {
                    litterLossC += litter.ContentC;
                    litterLossN += litter.ContentN;
                    litterLossP += litter.ContentP;
                }
                siteLitter.Clear();
            }

            else
            {
                foreach (PoolD litter in siteLitter)
                {
                    litterLossC = litter.ContentC * litterReduc;
                    litterLossN = litter.ContentN * litterReduc;
                    litterLossP = litter.ContentP * litterReduc;

                    litter.Mass -= (litter.Mass * litterReduc);
                    litter.ContentC -= litterLossC;
                    litter.ContentN -= litterLossN;
                    litter.ContentP -= litterLossP;
                }
            }

            //******************************************************************

            //Portion of woody debris is consumed.
            double woodyDebrisLossC = woodyDebris.ContentC * woodyDebrisReduc;
            double woodyDebrisLossN = woodyDebris.ContentN * woodyDebrisReduc;
            double woodyDebrisLossP = woodyDebris.ContentP * woodyDebrisReduc;

            woodyDebris.Mass -= (woodyDebris.Mass * woodyDebrisReduc);
            woodyDebris.ContentC -= woodyDebrisLossC;
            woodyDebris.ContentN -= woodyDebrisLossN;
            woodyDebris.ContentP -= woodyDebrisLossP;

            //******************************************************************

            //Portion of mass loss converted to black carbon of any sort.
            //  Assume 8%, the average value in Preston and Schmidt 2006.
            //  Insufficient information about conversion, but very important.
            charcoal.ContentC += ((litterLossC + woodyDebrisLossC) * 0.08);
            charcoal.ContentN += ((litterLossN + woodyDebrisLossN) * 0.08);
            charcoal.ContentP += ((litterLossP + woodyDebrisLossP) * 0.08);

            //Portion of nitrogen and phosphorus freed during consumption
            //  is added to mineral soil (Raison et al. 1985).
            mineralSoil.ContentN += ((charcoalLossN + litterLossN +
                woodyDebrisLossN) * 0.01);
            mineralSoil.ContentP += ((charcoalLossP + litterLossP +
                woodyDebrisLossP) * 0.42);

            //Reset fire severity
            SiteVars.FireSeverity[site] = 0;
            return;
        }
    }
}
