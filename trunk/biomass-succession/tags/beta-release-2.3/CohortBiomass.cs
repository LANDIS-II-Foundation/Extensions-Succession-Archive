//  Copyright 2005-2010 Portland State University
//  Authors:  Robert M. Scheller
//  License:  Available at
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;

using Landis.Ecoregions;
using Landis.Landscape;
using Landis.Species;

using System;
using System.Collections.Generic;

namespace Landis.Biomass.Succession
{
    /// <summary>
    /// Calculations for an individual cohort's biomass.
    /// </summary>
    /// <remarks>
    /// References:
    /// <list type="">
    ///     <item>
    ///     Crow, T. R., 1978.  Biomass and production in three contiguous
    ///     forests in northern Wisconsin. Ecology 59(2):265-273.
    ///     </item>
    ///     <item>
    ///     Niklas, K. J., Enquist, B. J., 2002.  Canonical rules for plant
    ///     organ biomass partitioning and annual allocation.  Amer. J. Botany
    ///     89(5): 812-819.
    ///     </item>
    /// </list>
    /// </remarks>
    public class CohortBiomass
        : Biomass.ICalculator
    {

        //  Ecoregion where the cohort's site is located
        private IEcoregion ecoregion;

        //  Ratio of actual biomass to maximum biomass for the cohort.
        private double B_AP;

        //  Ratio of potential biomass to maximum biomass for the cohort.
        private double B_PM;

        //  Totaly mortality without annual leaf litter for the cohort
        private int M_noLeafLitter;

        private double growthReduction;
        private double defoliation;

        public static int SubYear;
        public static double SpinupMortalityFraction;

        public static double CanopyLightExtinction;

        //Nested dictionary of species,cohort
        //public static Dictionary<int, Dictionary<int, double>> CohortCompetition;


        //---------------------------------------------------------------------

        public int MortalityWithoutLeafLitter
        {
            get {
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
        /// Productivity (ANPP), age-related mortality (M_AGE), and development-
        /// related mortality (M_BIO).
        /// </summary>
        public int ComputeChange(ICohort    cohort,
                                 ActiveSite site,
                                 int        siteBiomass,
                                 int        prevYearSiteMortality)
        {

            ecoregion = Model.Core.Ecoregion[site];

            // First, calculate age-related mortality.
            // Age-related mortality will include woody and standing leaf biomass (=0 for deciduous trees).
            double mortalityAge = ComputeAgeMortality(cohort);

            double actualANPP = ComputeActualANPP(cohort, site, siteBiomass, prevYearSiteMortality);

            //  Age mortality is discounted from ANPP to prevent the over-
            //  estimation of mortality.  ANPP cannot be negative.
            actualANPP = Math.Max(1.0, actualANPP - mortalityAge);

            //  Growth-related mortality
            double mortalityGrowth = ComputeGrowthMortality(cohort, site, siteBiomass);

            //  Age-related mortality is discounted from growth-related
            //  mortality to prevent the under-estimation of mortality.  Cannot be negative.
            mortalityGrowth = Math.Max(0, mortalityGrowth - mortalityAge);

            //  Also ensure that growth mortality does not exceed actualANPP.
            mortalityGrowth = Math.Min(mortalityGrowth, actualANPP);

            //  Total mortality for the cohort
            double totalMortality = mortalityAge + mortalityGrowth;

            if(totalMortality > cohort.Biomass)
                throw new ApplicationException("Error: Mortality exceeds cohort biomass");

            // Defoliation ranges from 1.0 (total) to none (0.0).
            defoliation = CohortDefoliation.Compute(cohort, site, siteBiomass);
            double defoliationLoss = 0.0;
            if (defoliation > 0)
            {
                double standing_nonwood = ComputeFractionANPPleaf(cohort.Species) * actualANPP;
                defoliationLoss =  standing_nonwood * defoliation;
            }

            int deltaBiomass = (int) (actualANPP - totalMortality - defoliationLoss);
            double newBiomass =  cohort.Biomass + (double) deltaBiomass;

            double totalLitter = UpdateDeadBiomass(cohort, actualANPP, totalMortality, site, newBiomass);

            if(PlugIn.CalibrateMode)
            {
                UI.WriteLine("Yr={0}.   Calculate Delta Biomass...", (Model.Core.CurrentTime+SubYear));
                UI.WriteLine("Yr={0}.     Mgrowth={1:0.0}, Mage={2:0.0}, litter={3:0.00}.", (Model.Core.CurrentTime+SubYear), mortalityGrowth, mortalityAge, totalLitter);
                UI.WriteLine("Yr={0}.     DeltaB={1:0.0}, ANPPact={2:0.0}.", (Model.Core.CurrentTime+SubYear), deltaBiomass, actualANPP);
            }
            return deltaBiomass;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Computes M_AGE_ij: the mortality caused by the aging of the cohort.
        /// See equation 6 in Scheller and Mladenoff, 2004.
        /// </summary>
        private double ComputeAgeMortality(ICohort cohort)
        {
            double max_age = (double) cohort.Species.Longevity;
            double d = SpeciesData.MortCurveShapeParm[cohort.Species];

            double M_AGE = cohort.Biomass * Math.Exp((double) cohort.Age / max_age * d) / Math.Exp(d);

            M_AGE = Math.Min(M_AGE, cohort.Biomass);

            if(Model.Core.CurrentTime <= 0 && SpinupMortalityFraction > 0.0)
            {
                M_AGE += cohort.Biomass * SpinupMortalityFraction;
                if(PlugIn.CalibrateMode)
                    UI.WriteLine("Yr={0}. SpinupMortalityFraction={1:0.0000}, AdditionalMortality={2:0.0}, Spp={3}, Age={4}.", (Model.Core.CurrentTime+SubYear), SpinupMortalityFraction, (cohort.Biomass * SpinupMortalityFraction), cohort.Species.Name, cohort.Age);
            }

            return M_AGE;
        }

        //---------------------------------------------------------------------

        private double ComputeActualANPP(ICohort    cohort,
                                         ActiveSite site,
                                         int        siteBiomass,
                                         int        prevYearSiteMortality)
        {
            growthReduction = CohortGrowthReduction.Compute(cohort, site, siteBiomass);
            double growthShape = SpeciesData.GrowthCurveShapeParm[cohort.Species];


            double cohortBiomass = cohort.Biomass;
            double capacityReduction = 1.0;

            if(SiteVars.CapacityReduction != null && SiteVars.CapacityReduction[site] > 0)
            {
            	capacityReduction = 1.0 - SiteVars.CapacityReduction[site];
                if(PlugIn.CalibrateMode)
                    UI.WriteLine("Yr={0}. Capacity Remaining={1:0.00}, Spp={2}, Age={3} B={4}.", (Model.Core.CurrentTime+SubYear), capacityReduction, cohort.Species.Name, cohort.Age, cohort.Biomass);
            }

            double maxBiomass  = SpeciesData.B_MAX_Spp[cohort.Species][ecoregion] * capacityReduction;
            double maxANPP     = SpeciesData.ANPP_MAX_Spp[cohort.Species][ecoregion];

            double indexC = 1.0; // CalculateCompetition(site, cohort);

            if((indexC <= 0.0 && cohortBiomass > 0) || indexC > 1.0)
            {
                UI.WriteLine("Error: Competition Index [{0:0.00}] is <= 0.0 or > 1.0", indexC);
                UI.WriteLine("Yr={0}. SPECIES={1}, AGE={2}, B={3}", (Model.Core.CurrentTime+SubYear), cohort.Species.Name, cohort.Age, cohortBiomass);

                throw new ApplicationException("Application terminating.");
            }

            //  Potential biomass, equation 3 in Scheller and Mladenoff, 2004
            double potentialBiomass = Math.Max(1.0, maxBiomass - siteBiomass + cohortBiomass);

            //  Species can use new space from mortality immediately
            //  but not in the case of capacity reduction due to harvesting.
            if(capacityReduction >= 1.0)
            {
                potentialBiomass = Math.Max(potentialBiomass, prevYearSiteMortality);
                //CI = Math.Max(CI, (prevYearSiteMortality / (double) siteBiomass));
            }

            //  Ratio of cohort's actual biomass to potential biomass
            B_AP = Math.Min(1.0, cohortBiomass / potentialBiomass);

            //  Ratio of cohort's potential biomass to maximum biomass.  The
            //  ratio cannot be exceed 1.
            //B_PM = Math.Min(1.0, potentialBiomass / maxBiomass);
            B_PM = Math.Min(1.0, indexC);

            double lightTransmittance = Math.Exp(CanopyLightExtinction);

            //  Actual ANPP: equation (4) from Scheller & Mladenoff, 2004.
            //  Constants k1 and k2 control whether growth rate declines with
            //  age.  Set to default = 1.
            double actualANPP = maxANPP * Math.E * Math.Pow(B_AP, growthShape) * Math.Exp(-1 * Math.Pow(B_AP, growthShape)) * B_PM;

            //double LAIactual = SpeciesData.MaxLAI[cohort.Species] * actualANPP / maxANPP;
            //CanopyLightExtinction += (-0.54 * LAIactual);

            UI.WriteLine("CanopyLightExtinction = {0}, LightTransmittance = {1}.", CanopyLightExtinction, lightTransmittance);

            // Calculated actual ANPP can not exceed the limit set by the
            //  maximum ANPP times the ratio of potential to maximum biomass.
            //  This down regulates actual ANPP by the available growing space.

            actualANPP = Math.Min(maxANPP * B_PM, actualANPP);

            if(PlugIn.CalibrateMode) // && SiteVars.CapacityReduction != null && SiteVars.CapacityReduction[site] > 0.0)
            {
                if(Model.Core.CurrentTime <= 0)
                UI.WriteLine("Spinup Calculations...");
                UI.WriteLine("Yr={0}. SPECIES={1}, AGE={2}.", (Model.Core.CurrentTime+SubYear), cohort.Species.Name, cohort.Age);
                UI.WriteLine("Yr={0}.   Calculate ANPPactual...", (Model.Core.CurrentTime+SubYear));
                UI.WriteLine("Yr={0}.     MaxANPP={1}, MaxB={2:0}, Bsite={3}, Bcohort={4:0.0}, Bpot={5:0}.", (Model.Core.CurrentTime+SubYear), maxANPP, maxBiomass, (int) siteBiomass, cohort.Biomass, potentialBiomass);
                UI.WriteLine("Yr={0}.     B_PM={1:0.00}, B_AP={2:0.00}, actualANPP={3:0.0}.", (Model.Core.CurrentTime+SubYear), B_PM, B_AP, actualANPP);
            }

            if (growthReduction > 0)
                actualANPP *= (1.0 - growthReduction);


            return actualANPP;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The mortality caused by development processes,
        /// including self-thinning and loss of branches, twigs, etc.
        /// See equation 5 in Scheller and Mladenoff, 2004.
        /// </summary>
        private double ComputeGrowthMortality(ICohort cohort, ActiveSite site, int siteBiomass)
        {
            //double percentDefoliation = CohortDefoliation.Compute(cohort, site, siteBiomass);

            //const double y0 = 0.01;
            //const double r = 0.08;
            double M_BIO = 1.0;
            double maxANPP = SpeciesData.ANPP_MAX_Spp[cohort.Species][ecoregion];

            //double M_BIO = maxANPP *
            //        (y0 / (y0 + (1 - y0) * Math.Exp(-r / y0 * B_AP))) *
            //        B_PM;

            //Michaelis-Menton function:
            if (B_AP > 1.0)
                M_BIO = maxANPP * B_PM;
            else
                M_BIO = maxANPP * (2.0 * B_AP)/(1.0 + B_AP) * B_PM;

            //  Mortality should not exceed the amount of living biomass
            M_BIO = Math.Min(cohort.Biomass, M_BIO);

            //  Calculated actual ANPP can not exceed the limit set by the
            //  maximum ANPP times the ratio of potential to maximum biomass.
            //  This down regulates actual ANPP by the available growing space.

            M_BIO = Math.Min(maxANPP * B_PM, M_BIO);

            if (growthReduction > 0)
                M_BIO *= (1.0 - growthReduction);

            if(PlugIn.CalibrateMode) // && SiteVars.CapacityReduction != null && SiteVars.CapacityReduction[site] > 0.0)
            {
                UI.WriteLine("Yr={0}.   Calculate Mgrowth...", (Model.Core.CurrentTime+SubYear));
                UI.WriteLine("Yr={0}.     Mgrowth={1:0.0}.", (Model.Core.CurrentTime+SubYear), M_BIO);
            }

            return M_BIO;

        }

        //---------------------------------------------------------------------

        private double UpdateDeadBiomass(ICohort cohort, double actualANPP, double totalMortality, ActiveSite site, double newBiomass)
        {

            ISpecies species     = cohort.Species;
            double leafLongevity = SpeciesData.LeafLongevity[species];
            double cohortBiomass = newBiomass; // Mortality is for the current year's biomass.
            double leafFraction  = ComputeFractionANPPleaf(species);

            // First, deposit the a portion of the leaf mass directly onto the forest floor.
            // In this way, the actual amount of leaf biomass is added for the year.
            // In addition, add the equivalent portion of fine roots to the surface layer.

            double annualLeafANPP = actualANPP * leafFraction;

            ForestFloor.AddLitter(annualLeafANPP, species, site);

            // --------------------------------------------------------------------------------
            // The next section allocates mortality from standing (wood and leaf) biomass, i.e.,
            // biomass that has accrued from previous years' growth.

            // Subtract annual leaf growth as that was taken care of above.
            totalMortality -= annualLeafANPP;

            // Assume that standing foliage is equal to this years annualLeafANPP * leaf longevity
            // minus this years leaf ANPP.  This assumes that actual ANPP has been relatively constant
            // over the past 2 or 3 years (if coniferous).

            double standing_nonwood = (annualLeafANPP * leafLongevity) - annualLeafANPP;
            double standing_wood    = Math.Max(0, cohortBiomass - standing_nonwood);

            double fractionStandingNonwood = standing_nonwood / cohortBiomass;

            //  Assume that the remaining mortality is divided proportionally
            //  between the woody mass and non-woody mass (Niklaus & Enquist,
            //  2002).   Do not include current years growth.
            double mortality_nonwood = Math.Max(0.0, totalMortality * fractionStandingNonwood) ;
            double mortality_wood    = Math.Max(0.0, totalMortality - mortality_nonwood);

            if(mortality_wood < 0 || mortality_nonwood < 0)
                throw new ApplicationException("Error: Woody input is < 0");

            //  Add mortality to dead biomass pools.
            //  Coarse root mortality is assumed equal to aboveground woody mortality
            //    mass is assumed 25% of aboveground wood (White et al. 2000, Niklas & Enquist 2002)
            if(mortality_wood > 0)
            {
                //  Add mortality to dead biomass pools.
                ForestFloor.AddWoody((ushort) mortality_wood, species, site);
            }

            if(mortality_nonwood > 0)
            {
                ForestFloor.AddLitter(mortality_nonwood, species, site);
            }

            //  Total mortality not including annual leaf litter
            M_noLeafLitter = (int) mortality_wood;

            return (annualLeafANPP + mortality_nonwood + mortality_wood);

        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Computes the cohort's biomass that is leaf litter
        /// or other non-woody components.  Assumption is that remainder is woody.
        /// </summary>
        public static double ComputeStandingLeafBiomass(double ANPPactual, ICohort cohort)
        {

            double annualLeafFraction = ComputeFractionANPPleaf(cohort.Species);

            double annualFoliar = ANPPactual * annualLeafFraction;

            double B_nonwoody   = annualFoliar * SpeciesData.LeafLongevity[cohort.Species];

            //  Non-woody cannot be less than 2.5% or greater than leaf fraction of total
            //  biomass for a cohort.
            B_nonwoody = Math.Max(B_nonwoody, cohort.Biomass * 0.025);
            B_nonwoody = Math.Min(B_nonwoody, cohort.Biomass * annualLeafFraction);

            return B_nonwoody;
        }
        //---------------------------------------------------------------------

        public static double ComputeFractionANPPleaf(ISpecies species)
        {

            //  A portion of growth goes to creating leaves (Niklas and Enquist 2002).
            //  Approximate for angio and conifer:
            //  pg. 817, growth (G) ratios for leaf:stem (Table 4) = 0.54 or 35% leaf

            double leafFraction = 0.35;

            //  Approximately 3.5% of aboveground production goes to early leaf
            //  fall, bud scales, seed production, and herbivores (Crow 1978).
            //leafFraction += 0.035;

            return leafFraction;
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Computes the percentage of a cohort's standing biomass that is non-woody.
        /// This method is designed for external disturbance calls that need to
        /// estimate the amount of non-wood biomass.
        /// </summary>

        public Percentage ComputeNonWoodyPercentage(ICohort     cohort,
                                                    ActiveSite  site)
        {
            SiteCohorts siteCohorts = SiteVars.Cohorts[site];

            //CalculateCompetition(site, cohort);

            double actualANPP = ComputeActualANPP(cohort, site, siteCohorts.TotalBiomass,
                                siteCohorts.PrevYearMortality);

            double mortalityAge = ComputeAgeMortality(cohort);

            //  Age mortality is discounted from ANPP to prevent the over-
            //  estimation of mortality.  ANPP cannot be negative.
            actualANPP = Math.Max(0, actualANPP - mortalityAge);

            return new Percentage(ComputeStandingLeafBiomass(actualANPP, cohort) / cohort.Biomass);
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Computes the initial biomass for a cohort at a site.
        /// </summary>
        /// <remarks>
        /// Scheller, R. M. and Domingo, J. B. Biomass Succession (v1.0) for
        /// LANDIS-II: User Guide.  Available online at
        /// http://landis.forest.wisc.edu/documentation.
        /// </remarks>
        public static int InitialBiomass(ISpecies species,
                                            SiteCohorts siteCohorts,
                                            ActiveSite  site)
        {
            IEcoregion ecoregion = Model.Core.Ecoregion[site];
            //double B_ACT = ActualSiteBiomass(siteCohorts, site, out ecoregion);

            double B_ACT = (double) Cohorts.ComputeNonYoungBiomass(siteCohorts);

            double maxBiomass = SpeciesData.B_MAX_Spp[species][ecoregion];

            //  Initial biomass exponentially declines in response to
            //  competition.
            double initialBiomass = 0.02 * maxBiomass *
                                    Math.Exp(-1.6 * B_ACT / EcoregionData.B_MAX[ecoregion]);
                                    // -5.0 would give zero biomass at 1.2x maximum

            //  Initial biomass cannot be less than 1.
            initialBiomass = Math.Max(1.0, initialBiomass);

            return (int) initialBiomass;
        }


        //---------------------------------------------------------------------
        // New method for calculating competition limits.
        // Iterates through cohorts, assigning each a competitive efficiency

        private static double CalculateCompetition(ActiveSite site, ICohort cohort)
        {

            double CMultiplier = Math.Max(Math.Sqrt(cohort.Biomass), 1.0);
            double CMultTotal = CMultiplier;

            foreach (ISpeciesCohorts speciesCohorts in SiteVars.Cohorts[site])
                foreach (ICohort xcohort in speciesCohorts)
                {
                    if(xcohort.Age != cohort.Age || xcohort.Species.Index != cohort.Species.Index)
                    {
                        double tempCMultiplier = Math.Max(Math.Sqrt(xcohort.Biomass), 1.0);
                        CMultTotal += tempCMultiplier;
                    }
                }


            double Cfraction = CMultiplier / CMultTotal;

            return Cfraction;

            //CohortCompetition[cohort.Species.Index][cohort.Age-1] = Cfraction;
           //UI.WriteLine("   Site={0}/{1}.  Competition:  spp={2}, age={3}, B={4}, CI={5}.", site.Location.Row, site.Location.Column, cohort.Species.Name, cohortAge, cohort.Biomass, Cfraction);

        }
    }
}
