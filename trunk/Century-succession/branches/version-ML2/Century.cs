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
        public static int Year;
        public static int Month;
        public static int MonthCnt;

        /// <summary>
        /// Grows all cohorts at a site for a specified number of years.
        /// Litter is decomposed following the Century model.
        /// </summary>
        public static ISiteCohorts Run(ActiveSite site,
                                       int         years,
                                       bool        isSuccessionTimeStep)
        {

            ISiteCohorts siteCohorts = SiteVars.Cohorts[site];
            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];
            //PlugIn.ModelCore.UI.WriteLine("SOM2C for = {0}.", SiteVars.SOM2[site].Carbon);

            // If in spin-up mode and calibration mode, then this needs to happen first.
            if (PlugIn.ModelCore.CurrentTime == 0 && OtherData.CalibrateMode)
            {
                MonthCnt = 11;  // adjustment for sequence
                AvailableN.CalculateMineralNfraction(site);
            }

            for (int y = 0; y < years; ++y) {

                Year = y + 1;

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

                for (MonthCnt = 0; MonthCnt < 12; MonthCnt++)
                {
                    //PlugIn.ModelCore.UI.WriteLine("SiteVars.MineralN = {0:0.00}, month = {1}.", SiteVars.MineralN[site], i);

                    Month = months[MonthCnt];
                    //CohortBiomass.month = month;
                    //CohortBiomass.centuryMonth = i;

                    SiteVars.MonthlyAGNPPcarbon[site][Month] = 0.0;
                    SiteVars.MonthlyBGNPPcarbon[site][Month] = 0.0;
                    SiteVars.MonthlyNEE[site][Month] = 0.0;
                    SiteVars.MonthlyResp[site][Month] = 0.0;
                    SiteVars.SourceSink[site].Carbon = 0.0;
                    SiteVars.TotalWoodBiomass[site] = Century.ComputeWoodBiomass((ActiveSite) site);
                    //SiteVars.LAI[site] = Century.ComputeLAI((ActiveSite)site);

                    //PlugIn.ModelCore.UI.WriteLine("MELISSA:  Yr {0} of {1}, Month={2}, MineralN={3:0.00}.", y, years, i + 1, SiteVars.MineralN[site]);


                    double monthlyNdeposition = EcoregionData.AnnualWeather[PlugIn.ModelCore.Ecoregion[site]].MonthlyNdeposition[Century.Month];
                    SiteVars.MineralN[site] += monthlyNdeposition;
                    //PlugIn.ModelCore.UI.WriteLine("Ndeposition={0},MineralN={1:0.00}.", monthlyNdeposition, SiteVars.MineralN[site]);

                    double liveBiomass = (double) ComputeLivingBiomass(siteCohorts);
                    double baseFlow, stormFlow;
                    SoilWater.Run(y, Month, liveBiomass, site, out baseFlow, out stormFlow);


                    // Reset N resorption if it is September
                    if (Month == 8)
                    {
                        AvailableN.CohortResorbedNallocation = new Dictionary<int, Dictionary<int, double>>();
                        ComputeResorbedN(siteCohorts, site, Month);
                    }

                    // Calculate mineral N fractions based on fine root biomass (leaf biomass) in July
                    if (Month == 6)
                    {
                        AvailableN.CalculateMineralNfraction(site);
                    }

                    // Calculate N allocation for each cohort
                    AvailableN.SetMineralNallocation(site);

                    if (MonthCnt==11)
                        siteCohorts.Grow(site, (y == years && isSuccessionTimeStep), true);
                    else
                        siteCohorts.Grow(site, (y == years && isSuccessionTimeStep), false);

                    WoodLayer.Decompose(site);
                    //PlugIn.ModelCore.UI.WriteLine("AfterWoodDecomp.  MineralN={0:0.00}.", SiteVars.MineralN[site]);
                    LitterLayer.Decompose(site);
                    //PlugIn.ModelCore.UI.WriteLine("AfterLitterDecomp.  MineralN={0:0.00}.", SiteVars.MineralN[site]);
                    SoilLayer.Decompose(site);
                    //PlugIn.ModelCore.UI.WriteLine("AfterSoilDecom.  MineralN={0:0.00}.", SiteVars.MineralN[site]);

                    //PlugIn.ModelCore.UI.WriteLine("SiteVars.MineralN = {0:0.00}, month = {1} - post Decompose.", SiteVars.MineralN[site], i);
                    //PlugIn.ModelCore.UI.WriteLine("After decomposition, SOM2C for = {0}.", SiteVars.SOM2[site].Carbon);

                    //...Volatilization loss as a function of the mineral n which
                    //     remains after uptake by plants.  ML added a correction factor for wetlands since their denitrification rate is double that of wetlands
                    //based on a review paper by Seitziner 2006.

                    //double volatilize = (SiteVars.MineralN[site] * EcoregionData.Denitrif[ecoregion]) / 12; // monthly value
                    double volatilize = (SiteVars.MineralN[site] * EcoregionData.Denitrif[ecoregion]); // this should be a monthly value * fraction

                    //PlugIn.ModelCore.UI.WriteLine("BeforeVol.  MineralN={0:0.00}.", SiteVars.MineralN[site]);

                    SiteVars.MineralN[site] -= volatilize;
                    SiteVars.SourceSink[site].Nitrogen += volatilize;
                    SiteVars.Nvol[site] += volatilize;

                    SoilWater.Leach(site, baseFlow, stormFlow);

                    SiteVars.MonthlyNEE[site][Month] -= SiteVars.MonthlyAGNPPcarbon[site][Month];
                    SiteVars.MonthlyNEE[site][Month] -= SiteVars.MonthlyBGNPPcarbon[site][Month];
                    SiteVars.MonthlyNEE[site][Month] += SiteVars.SourceSink[site].Carbon;

                }
            }

            ComputeTotalCohortCN(site, siteCohorts);

            return siteCohorts;
        }

        //---------------------------------------------------------------------

        public static void ComputeResorbedN(ISiteCohorts cohorts, ActiveSite site, int month)
        {
            if (cohorts != null)
                foreach (ISpeciesCohorts speciesCohorts in cohorts)
                    foreach (ICohort cohort in speciesCohorts)
                    {
                        // Resorbed N:  We are assuming that any leaves dropped as a function of normal
                        // growth and maintenance (e.g., fall senescence) will involve resorption of leaf N.
                        double resorbedN = AvailableN.CalculateResorbedN(site, cohort.Species, cohort.LeafBiomass, month);

                        AvailableN.SetResorbedNallocation(cohort, resorbedN);

                    }
            return;
        }


        //---------------------------------------------------------------------

        public static int ComputeLivingBiomass(ISiteCohorts cohorts)
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

            double fRootC = Roots.CalculateFineRoot(leafC);
            double cRootC = Roots.CalculateCoarseRoot(woodC);

            double totalC = leafC + woodC + fRootC + cRootC;

            double leafN  = leafC /  (double) SpeciesData.LeafCN[species];
            double woodN = woodC / (double) SpeciesData.WoodCN[species];
            double cRootN = cRootC / (double) FunctionalType.Table[SpeciesData.FuncType[species]].CoarseRootCN;
            double fRootN = fRootC / (double) FunctionalType.Table[SpeciesData.FuncType[species]].FineRootCN;

            //double totalN = woodN + cRootN + leafN + fRootN;

            //PlugIn.ModelCore.UI.WriteLine("month={0}, species={1}, leafB={2:0.0}, leafC={3:0.00}, leafN={4:0.0}, woodB={5:0.0}, woodC={6:0.000}, woodN={7:0.0}", Month, cohort.Species.Name, cohort.LeafBiomass, leafC, leafN, cohort.WoodBiomass, woodC, woodN);

            SiteVars.CohortLeafC[site] += leafC + fRootC;
            SiteVars.CohortLeafN[site] += leafN + fRootN;
            SiteVars.CohortWoodC[site] += woodC + cRootC;
            SiteVars.CohortWoodN[site] += woodN + cRootN;

            return;

        }

    }
}
