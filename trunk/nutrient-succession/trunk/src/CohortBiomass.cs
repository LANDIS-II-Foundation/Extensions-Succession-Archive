//  Copyright 2007-2010 University of Nevada, Portland State University
//  Authors:  Sarah Ganschow, Robert M. Scheller
//  License:  Available at
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;
using Landis.Ecoregions;
using Landis.Landscape;
using Landis.Species;

using System;

namespace Landis.Biomass.NuCycling.Succession
{
    /// <summary>
    /// Calculations for an individual cohort's biomass.
    /// </summary>
    public class CohortBiomass
        : Biomass.ICalculator
    {
        /// <summary>
        /// The single instance of the biomass calculations that is used by
        ///   the plug-in.
        /// </summary>
        public static CohortBiomass Calculator;

        //Ecoregion where the cohort's site is located.
        private IEcoregion ecoregion;

        //Ratio of actual biomass to maximum biomass for the cohort.
        private double B_AP;

        //Ratio of potential biomass to maximum biomass for the cohort.
        private double B_PM;

        //Total mortality without annual leaf litter for the cohort.
        private int M_noLeafLitter;

        // Max NPP adjusted to account for temperature and moisture
        private double adjustedNPP;

        //---------------------------------------------------------------------

        public int MortalityWithoutLeafLitter
        {
            get
            {
                return M_noLeafLitter;
            }
        }

        //---------------------------------------------------------------------

        public CohortBiomass()
        {
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Computes the change in a cohort's biomass due to Annual Net Primary
        ///   Productivity (ANPP), age-related mortality (M_AGE), and development-
        ///   related mortality (M_BIO).
        /// </summary>
        public float[] ComputeChange(ICohort cohort,
                                 ActiveSite site
                                 //int siteBiomass,
                                 //int prevYearSiteMortality
            )
        {
            ecoregion = Model.Core.Ecoregion[site];
            int siteBiomass = (int)(SiteVars.TotalWoodBiomass[site]);
            int prevYearSiteMortality = (int)(SiteVars.PrevYearMortality[site]);

            double leafLong = SpeciesData.LeafLongevity[cohort.Species];

            // Calculate annual leaf turnover
            double annualLeafTurnover = (double)cohort.LeafBiomass / leafLong;

            // Calculate age-related mortality.
            // Age-related mortality will include woody and standing leaf biomass (=0 for deciduous trees).
            double[] mortalityAge = ComputeAgeMortality(cohort);

            double[] actualANPP = ComputeActualANPP(cohort, site, siteBiomass, prevYearSiteMortality, mortalityAge);

            //Nitrogen needs for ANPP cannot exceed available nitrogen.
            double Nreduction = AvailableNuts.CohortUptakeAvailableN((actualANPP[1] + annualLeafTurnover), 
                actualANPP[0], site, cohort.Species);

            //  Growth-related mortality
            double[] mortalityGrowth = ComputeGrowthMortality(cohort, site, actualANPP, mortalityAge);

            // Save growth-related leaf mortality for the last month
            // mortalityGrowth[1] = 0.0;

            //  Total mortality for the cohort
            double[] totalMortality = new double[2] { (mortalityAge[0] + mortalityGrowth[0]), (mortalityAge[1] + mortalityGrowth[1]) };

            if ((totalMortality[0] + totalMortality[1]) > (cohort.WoodBiomass + cohort.LeafBiomass))
            {
                totalMortality[0] = cohort.WoodBiomass * 0.9;
                totalMortality[1] = cohort.LeafBiomass * 0.9;
            }

            M_noLeafLitter = (int)totalMortality[0];

            //Update dead biomass.
            UpdateDeadBiomass(cohort, site, totalMortality);

            //Ensure all translocated N is used.
            double transN = AvailableNuts.GetTransN(cohort.Species, annualLeafTurnover, totalMortality[1]);
            if (Nreduction < transN)
            {
                //Account for lack of additional leaf turnover
                double overageLeafTurnover = annualLeafTurnover * (transN / Nreduction - 1);
                double ratioleafANPP = actualANPP[1] / (actualANPP[0] + actualANPP[1]);
                actualANPP[0] = actualANPP[0] * (transN / Nreduction) + overageLeafTurnover * (1 - ratioleafANPP);
                actualANPP[1] = actualANPP[1] * (transN / Nreduction) + overageLeafTurnover * ratioleafANPP;
            }

            // Make sure uptake doesn't occur if available N is negative
            if (SiteVars.MineralSoil[site].ContentN < 0)
            {
                actualANPP[0] *= (transN / Nreduction);
                actualANPP[1] *= (transN / Nreduction);
                annualLeafTurnover *= (transN / Nreduction);
                Nreduction = 0;
            }

            // Make sure uptake doesn't exceed available N
            else if (Nreduction > (SiteVars.MineralSoil[site].ContentN + transN))
            {
                annualLeafTurnover *= ((SiteVars.MineralSoil[site].ContentN + transN) / Nreduction);
                actualANPP[0] *= ((SiteVars.MineralSoil[site].ContentN + transN) / Nreduction);
                actualANPP[1] *= ((SiteVars.MineralSoil[site].ContentN + transN) / Nreduction);
                Nreduction = SiteVars.MineralSoil[site].ContentN + transN;
            }

            //Reduce available nitrogen due to cohort uptake.
            // N-fixers do not take N from the soil (assume 100% of needs are fixed).
            //  N-fixers with N tolerance = (5 or 6) fix N, giving negative Nreduction.
            //   The amount of fixation is scaled by cohort biomass.
            int nTol = SpeciesData.NTolerance[cohort.Species];
            if (nTol == 4)
                Nreduction = 0.0;
            else if (nTol == 5)
            {
                // Range of N fixation is 0.5 - 10 kg/ha/yr)
                Nreduction = 0 - ((cohort.LeafBiomass + cohort.WoodBiomass) /
                    (float)(SpeciesData.B_MAX_Spp[cohort.Species][ecoregion]) * (10 - 0.5) + 0.5);
            }
            else if (nTol == 6)
            {
                // Range of N fixation is 10 - 50 kg/ha/yr)
                Nreduction = 0 - ((cohort.LeafBiomass + cohort.WoodBiomass) /
                    (float)(SpeciesData.B_MAX_Spp[cohort.Species][ecoregion]) * (50 - 10) + 10);
            }
            else
                Nreduction = Math.Max(Nreduction - transN, 0.0);
            SiteVars.MineralSoil[site].ContentN -= Nreduction;

            //Reduce available phosphorus due to cohort uptake.
            AvailableNuts.CohortUptakeAvailableP((actualANPP[1] + annualLeafTurnover), actualANPP[0], site,
                cohort.Species, SiteVars.MineralSoil[site]);

            //Calculate coarse and fine root ANPP and add to masses.
            Roots.AddLiveCoarseRoots(actualANPP[0],
                cohort.Species, site, SiteVars.CoarseRoots[site]);
            Roots.AddLiveFineRoots((actualANPP[1] + annualLeafTurnover), cohort.Species,
                site, SiteVars.FineRoots[site]);

            //Compute changes in wood and leaf biomass.
            // Note: annualLeafTurnover represents the amount of leaf turnover replaced, which
            //    can equal actual turnover or is reduced given N limitation as above.
            float deltaWood = (float)(actualANPP[0] - totalMortality[0]);
            float deltaLeaf = (float)(actualANPP[1] - totalMortality[1] - 
                cohort.LeafBiomass / leafLong + annualLeafTurnover);

            float[] deltas = new float[2] { deltaWood, deltaLeaf };

            return deltas;
        }


        //---------------------------------------------------------------------

        private double[] ComputeActualANPP(ICohort cohort,
                                         ActiveSite site,
                                         double siteBiomass,
                                         double prevYearSiteMortality,
                                         double[] mortalityAge)
        {
            double cohortBiomass = (cohort.WoodBiomass + cohort.LeafBiomass);
            double leafFractionNPP = ComputeFractionANPPleaf(cohort.Species);

            double maxBiomass = SpeciesData.B_MAX_Spp[cohort.Species][ecoregion];
            double maxNPP = SpeciesData.ANPP_MAX_Spp[cohort.Species][ecoregion];

            double limitN = AvailableNuts.GrowthReductionAvailableN(site, cohort.Species, SiteVars.MineralSoil[site].ContentN);

            adjustedNPP = maxNPP * limitN;

            //UI.WriteLine("Limits:  Month={0}, LAI={1:0.00}, H20={2:0.00}, N={3:0.00}.", month, limitLAI, limitH20, limitN);

            //  Potential biomass, equation 3 in Scheller and Mladenoff, 2004
            double potentialBiomass = Math.Max(0.0, maxBiomass - siteBiomass) + cohortBiomass;

            //  Species can use new space immediately
            potentialBiomass = Math.Max(potentialBiomass, prevYearSiteMortality + cohortBiomass);

            //  Ratio of cohort's actual biomass to potential biomass
            B_AP = cohortBiomass / potentialBiomass;

            //  Ratio of cohort's potential biomass to maximum biomass.  The
            //  ratio cannot be exceed 1.
            B_PM = Math.Min(1.0, potentialBiomass / maxBiomass);

            //  Actual ANPP: equation (4) from Scheller & Mladenoff, 2004.
            //  This accounts for competitive interactions.
            //  Constants k1 and k2 are ignored for now.
            double actualANPP = adjustedNPP * Math.E * B_AP * Math.Exp(-1 * B_AP) * B_PM;

            //  Age mortality is discounted from ANPP to prevent the over-
            //  estimation of growth.  ANPP cannot be negative.
            actualANPP = Math.Max(0.0, actualANPP - mortalityAge[0] - mortalityAge[1]);

            double leafNPP = (actualANPP * leafFractionNPP);

            //  Adjust the leaf:wood NPP ratio to ensure that there is a minimal amount of leaf NPP,
            //  at the expense of wood NPP.
            double minimumLeafNPP = (cohortBiomass * 0.0005);
            leafNPP = Math.Max(leafNPP, minimumLeafNPP);

            leafFractionNPP = Math.Min(1.0, leafNPP / actualANPP);

            double woodNPP = actualANPP * (1.0 - leafFractionNPP);

            //UI.WriteLine("Year={0}, Mo={1}, Bleaf={2:0.00}, leafNPP={3:0.000}, Bwood={4:0.00}, woodNPP={5:0.000}.", Model.Core.CurrentTime, month, cohort.LeafBiomass, leafNPP, cohort.WoodBiomass, woodNPP);

            return new double[2] { woodNPP, leafNPP };

        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Computes M_AGE_ij: the mortality caused by the aging of the cohort.
        /// See equation 6 in Scheller and Mladenoff, 2004.
        /// </summary>
        private double[] ComputeAgeMortality(ICohort cohort)
        {
            double totalBiomass = (double)(cohort.WoodBiomass + cohort.LeafBiomass);
            double fractionLeaf = (double)cohort.LeafBiomass / totalBiomass;
            double max_age = (double)cohort.Species.Longevity;
            double d = SpeciesData.MortCurveShapeParm[cohort.Species];

            double M_AGE_wood = cohort.WoodBiomass *
                                    Math.Exp((double)cohort.Age / max_age * d) / Math.Exp(d);

            double M_AGE_leaf = cohort.LeafBiomass *
                                    Math.Exp((double)cohort.Age / max_age * d) / Math.Exp(d);

            M_AGE_wood = Math.Min(M_AGE_wood, cohort.WoodBiomass);
            M_AGE_leaf = Math.Min(M_AGE_leaf, cohort.LeafBiomass);

            double[] M_AGE = new double[2] { M_AGE_wood, M_AGE_leaf };

            return M_AGE;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// See equation 5 in Scheller and Mladenoff, 2004.
        /// The mortality caused by development processes,
        /// including self-thinning and loss of branches, twigs, etc.
        /// Because this equation is a function of the monthly NPP, no need to adjust for monthly time step.
        /// </summary>
        private double[] ComputeGrowthMortality(ICohort cohort, ActiveSite site, double[] actualNPP, double[] mortalityAge)
        {
            if (cohort.WoodBiomass <= 0 || cohort.LeafBiomass <= 0)
                return (new double[2] { 0.0, 0.0 });

            if (actualNPP[0] <= 0 || actualNPP[1] <= 0)
                return (new double[2] { 0.0, 0.0 });

            const double y0 = 0.01;
            const double r = 0.08;
            double totalNPP = (double)(actualNPP[0] + actualNPP[1]);
            double fractionLeaf = actualNPP[1] / totalNPP;

            double M_BIO_total = totalNPP *
                    (y0 / (y0 + (1.0 - y0) * Math.Exp(-r / y0 * B_AP))) *
                    B_PM;

            //  Age-related mortality is discounted from growth-related
            //  mortality to prevent the under-estimation of mortality.  Cannot be negative.
            double M_BIO_wood = Math.Max(0.0, (M_BIO_total * (1.0 - fractionLeaf)) - mortalityAge[0]);
            double M_BIO_leaf = Math.Max(0.0, (M_BIO_total * fractionLeaf) - mortalityAge[1]);

            //  Also ensure that growth mortality does not exceed 90% of adjustedNPP.
            double fractionNPP = 0.9;
            M_BIO_wood = Math.Min(M_BIO_wood, (actualNPP[0] * fractionNPP));
            M_BIO_leaf = Math.Min(M_BIO_leaf, (actualNPP[1] * fractionNPP));

            //  Mortality should not exceed the amount of living biomass
            M_BIO_wood = Math.Min(M_BIO_wood, cohort.WoodBiomass);
            M_BIO_leaf = Math.Min(M_BIO_leaf, cohort.LeafBiomass);

            //UI.WriteLine("Year={0},Month={1}, Mbio-WOOD: NPPtotal={2:0.0}, Mbio={3:0.00}, B_PM={4:0.00}, B_AP={5:0.00}.", Model.Core.CurrentTime, month, totalNPP, M_BIO_wood, B_PM, B_AP);

            double[] M_BIO = new double[2] { M_BIO_wood, M_BIO_leaf };

            return M_BIO;

        }

        //---------------------------------------------------------------------

        private void UpdateDeadBiomass(ICohort cohort, ActiveSite site, double[] totalMortality)
        {

            if (totalMortality[0] <= 0.0 || cohort.WoodBiomass <= 0.0)
                totalMortality[0] = 0.0;

            if (totalMortality[1] <= 0.0 || cohort.LeafBiomass <= 0.0)
                totalMortality[1] = 0.0;

            SiteVars.CurrentYearMortality[site] += totalMortality[0] + totalMortality[1];

            ISpecies species = cohort.Species;
            double leafLongevity = SpeciesData.LeafLongevity[species];

            //Deposit annual leaf turnover into litter pool and annual fine
            //  roots turnover into dead fine roots pool. 0.8 was used to
            //  calibrate the model to steady-state nitrogen. Without this
            //  reduction, total N increases by 0.038% per year. Needs to be
            //  tested against conifer species.
            double annualLeafTurnover = (double)cohort.LeafBiomass / leafLongevity;
            ForestFloor.AddLitter(annualLeafTurnover, species, site, SiteVars.LitterAdd[site]);
            Roots.KillFineRoots(Roots.CalculateFineRoot(annualLeafTurnover, leafLongevity), species, SiteVars.DeadFineRootsAdd[site]);
            Roots.ReduceFineRoots(Roots.CalculateFineRoot(annualLeafTurnover, leafLongevity), species, SiteVars.FineRoots[site]);

            // --------------------------------------------------------------------------------
            // The next section allocates mortality from standing (wood and leaf) biomass, i.e.,
            // biomass that has accrued from previous years' growth.

            double mortality_wood = (double)totalMortality[0];
            double mortality_leaf = (double)totalMortality[1];

            if (mortality_wood < 0 || mortality_leaf < 0)
            {
                UI.WriteLine("Mwood={0}, Mleaf={1}.", mortality_wood, mortality_leaf);
                throw new ApplicationException("Error: Woody input is < 0");
            }
            //  Add mortality to dead biomass pools.
            //  Coarse root mortality is assumed equal to aboveground woody mortality
            //    mass is assumed 25% of aboveground wood (White et al. 2000, Niklas & Enquist 2002)
            if (mortality_wood > 0.0)
            {
                ForestFloor.AddWoodyDebris(mortality_wood, species,
                    SiteVars.WoodyDebris[site]);
                ForestFloor.AddWoodyDebris(Roots.CalculateCoarseRoot(mortality_wood, leafLongevity),
                    species, SiteVars.WoodyDebris[site]);
                Roots.ReduceCoarseRoots(Roots.CalculateCoarseRoot(mortality_wood, leafLongevity),
                    species, SiteVars.CoarseRoots[site]);

            }

            if (mortality_leaf > 0.0)
            {
                ForestFloor.AddLitter(mortality_leaf, species, site, SiteVars.LitterAdd[site]);
                Roots.KillFineRoots(Roots.CalculateFineRoot(mortality_leaf, leafLongevity), species,
                    SiteVars.DeadFineRootsAdd[site]);
                Roots.ReduceFineRoots(Roots.CalculateFineRoot(mortality_leaf, leafLongevity), species,
                    SiteVars.FineRoots[site]);

            }

            return;
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Computes the portion of growth that goes to leaves (Niklas and
        ///   Enquist 2002). Differentiates angiosperms (leaf:stem:root ratios
        ///   are 32:50:9, with 39% of aboveground being leaf growth) from evergreens
        ///   (ratio of 48:35:17, with 57% of aboveground being leaf) using
        ///   leaf longevity.
        /// </summary>
        public static double ComputeFractionANPPleaf(ISpecies species)
        {
            double leafFraction = 0.39;
            if (SpeciesData.LeafLongevity[species] > 1.0) leafFraction = 0.57;

            return leafFraction;
        }


        //---------------------------------------------------------------------

        /// <summary>
        /// Computes the initial biomass for a cohort at a site, including
        ///   roots, and reduces available nitrogen and phosphorus.
        /// </summary>
        public static float[] InitialBiomass(SiteCohorts siteCohorts,
                                         ActiveSite site,
                                         ISpecies species)
        {
            IEcoregion ecoregion = Model.Core.Ecoregion[site];
            double B_ACT = ActualSiteBiomass(siteCohorts, site);

            //Initial biomass exponentially declines in response to competition.
            double initialBiomass = 0.025 * SpeciesData.B_MAX[ecoregion] *
                Math.Exp(-1.6 * B_ACT / SpeciesData.B_MAX[ecoregion]);

            //Initial biomass is limited by nitrogen availability.
            initialBiomass *= AvailableNuts.GrowthReductionAvailableN(site,
                species, SiteVars.MineralSoil[site].ContentN);
            double initialLeafBiomass = initialBiomass * ComputeFractionANPPleaf(species);

            //Nitrogen needs for ANPP cannot exceed available nitrogen.
            double Nreduction = AvailableNuts.CohortUptakeAvailableN(initialLeafBiomass,
                initialBiomass - initialLeafBiomass, site, species);

            if (SiteVars.MineralSoil[site].ContentN <= 0)
            {
                initialBiomass = Math.Min(initialBiomass, 5.0);
                initialLeafBiomass = initialBiomass * ComputeFractionANPPleaf(species);

                Nreduction = AvailableNuts.CohortUptakeAvailableN(initialLeafBiomass,
                    initialBiomass - initialLeafBiomass, site, species);
            }
            else if (Nreduction > SiteVars.MineralSoil[site].ContentN)
            {
                double diffRatio = SiteVars.MineralSoil[site].ContentN / Nreduction;
                initialBiomass *= diffRatio;
                initialLeafBiomass *= diffRatio;

                Nreduction = AvailableNuts.CohortUptakeAvailableN(initialLeafBiomass,
                    initialBiomass - initialLeafBiomass, site, species);
            }

            //Initial biomass cannot be less than 5.
            initialBiomass = Math.Max(initialBiomass, 5.0);

            //Calculate coarse and fine root masses.
            Roots.AddLiveCoarseRoots(initialBiomass - initialLeafBiomass,
                species, site, SiteVars.CoarseRoots[site]);
            Roots.AddLiveFineRoots(initialLeafBiomass, species, site,
                SiteVars.FineRoots[site]);

            //Reduce available nitrogen due to cohort's initial uptake.
            SiteVars.MineralSoil[site].ContentN -= Nreduction;

            //Reduce available phosphorus due to cohort's initial uptake.
            AvailableNuts.CohortUptakeAvailableP(initialLeafBiomass, initialBiomass - initialLeafBiomass,
                site, species, SiteVars.MineralSoil[site]);

            float[] initialWoodLeafBiomass = new float[2]{(float) (initialBiomass - initialLeafBiomass),
                (float) (initialLeafBiomass)};

            return initialWoodLeafBiomass;
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Computes the actual biomass at a site.  The biomass is the total
        /// of all the site's cohorts except young ones.  The total is limited
        /// to being no more than the site's maximum biomass less the previous
        /// year's mortality at the site.
        /// </summary>
        public static double ActualSiteBiomass(SiteCohorts siteCohorts,
                                               ActiveSite site)
        {
            IEcoregion ecoregion = Model.Core.Ecoregion[site];

            if (siteCohorts == null)
                return 0.0;

            int youngBiomass;
            int totalBiomass = Landis.Biomass.Cohorts.ComputeBiomass(siteCohorts, out youngBiomass);
            double B_ACT = totalBiomass - youngBiomass;

            int lastMortality = (int) SiteVars.PrevYearMortality[site]; //siteCohorts.PrevYearMortality;
            B_ACT = System.Math.Min(SpeciesData.B_MAX[ecoregion] - lastMortality, B_ACT);

            return B_ACT;
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Computes the shade at a site.
        /// </summary>
        public static byte ComputeShade(ActiveSite site)
        {
            IEcoregion ecoregion = Model.Core.Ecoregion[site];;
            double B_ACT = ActualSiteBiomass(SiteVars.Cohorts[site], site);

            //  Relative living biomass (ratio of actual to maximum site
            //    biomass).
            double B_AM = B_ACT / SpeciesData.B_MAX[ecoregion];

            for (byte shade = 5; shade >= 1; shade--)
            {
                if (B_AM >= EcoregionData.MinRelativeBiomass[shade][ecoregion])
                    return shade;
            }
            return 0;
        }
    }

}
