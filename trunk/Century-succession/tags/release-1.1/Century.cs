//  Copyright 2007-2010 Portland State University, University of Wisconsin-Madison
//  Author: Robert Scheller, Ben Sulman

using Landis.Core;
using Landis.SpatialModeling;
using Landis.Library.LeafBiomassCohorts;
using System.Collections.Generic;

namespace Landis.Extension.Succession.Century
{
    /// <summary>
    /// Utility methods.
    /// </summary>
    public class Century
    {


        /// <summary>
        /// Grows all cohorts at a site for a specified number of years.
        /// Litter is decomposed following the Century model.
        /// </summary>
        public static SiteCohorts Run(
                                       SiteCohorts siteCohorts,
                                       Location location,
                                       int         years,
                                       bool        isSuccessionTimeStep)
        {

            ActiveSite site = (ActiveSite) PlugIn.ModelCore.Landscape.GetSite(location);
            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];

            for (int y = 0; y < years; ++y) {

                SiteVars.ResetAnnualValues(site);

                if(y == 0 && SiteVars.FireSeverity != null && SiteVars.FireSeverity[site] > 0)
                    FireEffects.ReduceLayers(SiteVars.FireSeverity[site], site);

                // Do not reset annual climate if it has already happend for this year.
                if(!EcoregionData.ClimateUpdates[ecoregion][y + PlugIn.ModelCore.CurrentTime])
                {
                    EcoregionData.SetAnnualClimate(PlugIn.ModelCore.Ecoregion[site], y);

                    EcoregionData.ClimateUpdates[ecoregion][y + PlugIn.ModelCore.CurrentTime] = true;
                }

                // if spin-up phase, allow each initial community to have a unique climate
                if(PlugIn.ModelCore.CurrentTime == 0)
                {
                    EcoregionData.SetAnnualClimate(PlugIn.ModelCore.Ecoregion[site], y);
                }

                // Next, Grow and Decompose each month
                int[] months = new int[12]{6, 7, 8, 9, 10, 11, 0, 1, 2, 3, 4, 5};

                if(OtherData.CalibrateMode)
                    months = new int[12]{0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11};

                for (int i = 0; i < 12; i++)
                {

                    int month = months[i];



                    SiteVars.MonthlyAGNPPcarbon[site][month] = 0.0;
                    SiteVars.MonthlyBGNPPcarbon[site][month] = 0.0;
                    SiteVars.MonthlyNEE[site][month] = 0.0;
                    SiteVars.MonthlyResp[site][month] = 0.0;
                    SiteVars.SourceSink[site].Carbon = 0.0;
                    SiteVars.TotalWoodBiomass[site] = Century.ComputeWoodBiomass((ActiveSite) site);

                    double monthlyNdeposition = EcoregionData.AnnualWeather[PlugIn.ModelCore.Ecoregion[site]].MonthlyNdeposition[month];
                    SiteVars.MineralN[site] += monthlyNdeposition;
                    //PlugIn.ModelCore.Log.WriteLine("Month={0}, Ndeposition={1}.", i+1, monthlyNdeposition);

                    double liveBiomass = (double) ComputeLivingBiomass(siteCohorts);
                    SoilWater.Run(y, month, liveBiomass, site);
                    //SpeciesData.CalculateNGrowthLimits(site);

                    // Calculate N limitations for each cohort
                    AvailableN.CohortNlimits = new Dictionary<int, Dictionary<int,double>>();
                	AvailableN.CalculateNLimits(site);

                    CohortBiomass.month = month;
                    if(month==11)
                        siteCohorts.Grow(site, (y == years && isSuccessionTimeStep), true);
                    else
                        siteCohorts.Grow(site, (y == years && isSuccessionTimeStep), false);

                    WoodLayer.Decompose(site);
                    LitterLayer.Decompose(site);
                    SoilLayer.Decompose(site);

                    //...Volatilization loss as a function of the mineral n which
                    //     remains after uptake by plants
                    double volatilize = SiteVars.MineralN[site] * 0.02 * OtherData.MonthAdjust; // value from ffix.100
                    SiteVars.MineralN[site] -= volatilize;
                    SiteVars.SourceSink[site].Nitrogen += volatilize;
                    //SoilWater.Leach(site);

                    SiteVars.MonthlyNEE[site][month] -= SiteVars.MonthlyAGNPPcarbon[site][month];
                    SiteVars.MonthlyNEE[site][month] -= SiteVars.MonthlyBGNPPcarbon[site][month];
                    SiteVars.MonthlyNEE[site][month] += SiteVars.SourceSink[site].Carbon;

                }
            }

            ComputeTotalCohortCN(site, siteCohorts);

            return siteCohorts;
        }
        //---------------------------------------------------------------------

        private static int ComputeLivingBiomass(ISiteCohorts cohorts)
        {
            int total = 0;
            if (cohorts != null)
                foreach (ISpeciesCohorts speciesCohorts in cohorts)
                    foreach (ICohort cohort in speciesCohorts)
                        total += (int) (cohort.WoodBiomass + cohort.LeafBiomass);
                    //total += ComputeBiomass(speciesCohorts);
            return total;
        }

        //---------------------------------------------------------------------

        public static double ComputeWoodBiomass(ActiveSite site)
        {
            double woodBiomass = 0;
            if (SiteVars.Cohorts[site] != null)
                foreach (ISpeciesCohorts speciesCohorts in SiteVars.Cohorts[site])
                    foreach (ICohort cohort in speciesCohorts)
                        woodBiomass += cohort.WoodBiomass;
            return woodBiomass;
        }

        //---------------------------------------------------------------------
        private static void ComputeTotalCohortCN(ActiveSite site, ISiteCohorts cohorts)
        {
            SiteVars.CohortLeafC[site] = 0;
            SiteVars.CohortLeafN[site] = 0;
            SiteVars.CohortWoodC[site] = 0;
            SiteVars.CohortWoodN[site] = 0;

            if (cohorts != null)
                foreach (ISpeciesCohorts speciesCohorts in cohorts)
                    foreach (ICohort cohort in speciesCohorts)
                        CalculateCohortCN(site, cohort);
            return;
        }

        /// <summary>
        /// Summarize cohort C&N for output.
        /// </summary>
        private static void CalculateCohortCN(ActiveSite site, ICohort cohort)
        {

            ISpecies species = cohort.Species;

            double leafC = cohort.LeafBiomass * 0.47;
            double woodC = cohort.WoodBiomass * 0.47;

            //PlugIn.ModelCore.Log.WriteLine("month={0}, leafC={1:0.00}, woodC={2:0.000}", month, leafC, woodC);

            double fRootC = Roots.CalculateFineRoot(leafC);
            double cRootC = Roots.CalculateCoarseRoot(woodC);

            double totalC = leafC + woodC + fRootC + cRootC;

            double leafN  = leafC /       SpeciesData.LeafCN[species];
            double woodN  = woodC /       SpeciesData.WoodCN[species];
            double cRootN = cRootC /      SpeciesData.CoarseRootCN[species];
            double fRootN = fRootC /      SpeciesData.FineRootLitterCN[species];

            double totalN = woodN + cRootN + leafN + fRootN;

            SiteVars.CohortLeafC[site] += leafC + fRootC;
            SiteVars.CohortLeafN[site] += leafN + fRootN;
            SiteVars.CohortWoodC[site] += woodC + cRootC;
            SiteVars.CohortWoodN[site] += woodN + cRootN;

            return;

        }
    }
}
