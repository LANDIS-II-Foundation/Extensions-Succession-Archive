//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.SpatialModeling;
using Landis.Core;
using Landis.Library.BiomassCohorts;
using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;
using System;

namespace Landis.Extension.Succession.Biomass
{
    /// <summary>
    /// Calculations for an individual cohort's biomass.
    /// </summary>
    public class CohortBiomass
        : Landis.Library.BiomassCohorts.ICalculator
    {

        //  Ecoregion where the cohort's site is located
        private IEcoregion ecoregion;

        //  Ratio of actual biomass to maximum biomass for the cohort.
        private double B_AP;

        //  Ratio of potential biomass to maximum biomass for the cohort.
        private double B_PM;

        //  Totaly mortality without annual leaf litter for the cohort
        private int M_noLeafLitter;

        public static int SubYear;
        private double growthReduction;
        private double defoliation;
        public static double SpinupMortalityFraction;
        public static double CanopyLightExtinction;
        // private int siteBiomass;


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
        /// Productivity (ANPP), age-related mortality (M_AGE), and development-
        /// related mortality (M_BIO).
        /// </summary>
        public int ComputeChange(ICohort cohort,
                                 ActiveSite site)
                                 //int siteBiomass,
                                 //int prevYearSiteMortality)
        {
            int siteBiomass = SiteVars.TotalBiomass[site]; // (SiteVars.Cohorts[site]);
            ecoregion = PlugIn.ModelCore.Ecoregion[site];

            // First, calculate age-related mortality.
            // Age-related mortality will include woody and standing leaf biomass (=0 for deciduous trees).
            double mortalityAge = ComputeAgeMortality(cohort);

            double actualANPP = ComputeActualANPP(cohort, site);

            //  Age mortality is discounted from ANPP to prevent the over-
            //  estimation of mortality.  ANPP cannot be negative.
            actualANPP = Math.Max(1, actualANPP - mortalityAge);

            SiteVars.AGNPP[site] += actualANPP;

            // ---------------------------------------------------------
            //  Growth-related mortality
            double mortalityGrowth = ComputeGrowthMortality(cohort, site);

            //  Age-related mortality is discounted from growth-related
            //  mortality to prevent the under-estimation of mortality.  Cannot be negative.
            mortalityGrowth = Math.Max(0, mortalityGrowth - mortalityAge);

            //  Also ensure that growth mortality does not exceed actualANPP.
            mortalityGrowth = Math.Min(mortalityGrowth, actualANPP);

            //  Total mortality for the cohort
            double totalMortality = mortalityAge + mortalityGrowth;

            if (totalMortality > cohort.Biomass)
                throw new ApplicationException("Error: Mortality exceeds cohort biomass");

            PlugIn.CurrentYearSiteMortality += totalMortality;

            // ---------------------------------------------------------
            // Defoliation ranges from 1.0 (total) to none (0.0).
            // Defoliation is calculated by an external function, typically an extension
            // with a defoliation calculator.  The method CohortDefoliation.Compute is a delegate method
            // and lives within the defoliating extension.
            defoliation = CohortDefoliation.Compute(cohort, site, siteBiomass);
            double defoliationLoss = 0.0;
            if (defoliation > 0)
            {
                double standing_nonwood = ComputeFractionANPPleaf(cohort.Species) * actualANPP;
                defoliationLoss = standing_nonwood * defoliation;
            }
            // ---------------------------------------------------------

            int deltaBiomass = (int)(actualANPP - totalMortality - defoliationLoss);
            double newBiomass = cohort.Biomass + (double)deltaBiomass;

            double totalLitter = UpdateDeadBiomass(cohort, actualANPP, totalMortality, site, newBiomass);

            //CalculateCohortLight(cohort, actualANPP, newBiomass, site);

            if (PlugIn.CalibrateMode && PlugIn.ModelCore.CurrentTime > 0)
            {
                PlugIn.ModelCore.Log.WriteLine("Yr={0}. Calculate Delta Biomass...", (PlugIn.ModelCore.CurrentTime+SubYear));
                PlugIn.ModelCore.Log.WriteLine("Yr={0}.    Spp={1}, Age={2}.", (PlugIn.ModelCore.CurrentTime+SubYear), cohort.Species.Name, cohort.Age);
                PlugIn.ModelCore.Log.WriteLine("Yr={0}.    ANPPact={1:0.0}, Mtotal={2:0.0}, litter={3:0.00}.", (PlugIn.ModelCore.CurrentTime+SubYear), actualANPP, totalMortality, totalLitter);
                PlugIn.ModelCore.Log.WriteLine("Yr={0}.    DeltaB={1:0.0}, CohortB={2}, Bsite={3}", (PlugIn.ModelCore.CurrentTime+SubYear), deltaBiomass, cohort.Biomass, (int) siteBiomass);
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
            double max_age = (double)cohort.Species.Longevity;
            double d = SpeciesData.MortCurveShapeParm[cohort.Species];

            double M_AGE = cohort.Biomass * Math.Exp((double)cohort.Age / max_age * d) / Math.Exp(d);

            M_AGE = Math.Min(M_AGE, cohort.Biomass);

            if(PlugIn.ModelCore.CurrentTime <= 0 && SpinupMortalityFraction > 0.0)
            {
                M_AGE += cohort.Biomass * SpinupMortalityFraction;
                if(PlugIn.CalibrateMode)
                    PlugIn.ModelCore.Log.WriteLine("Yr={0}. SpinupMortalityFraction={1:0.0000}, AdditionalMortality={2:0.0}, Spp={3}, Age={4}.", (PlugIn.ModelCore.CurrentTime+SubYear), SpinupMortalityFraction, (cohort.Biomass * SpinupMortalityFraction), cohort.Species.Name, cohort.Age);
            }


            return M_AGE;
        }

        //---------------------------------------------------------------------

        private double ComputeActualANPP(ICohort cohort,
                                         ActiveSite site)
                                         //int siteBiomass,
                                         //int prevYearSiteMortality)
        {
            // double prevYearSiteMortality = SiteVars.PreviousYearMortality[site];
            int siteBiomass = SiteVars.TotalBiomass[site];

            growthReduction = CohortGrowthReduction.Compute(cohort, site);
            double growthShape = SpeciesData.GrowthCurveShapeParm[cohort.Species];

            double cohortBiomass = cohort.Biomass;
            double capacityReduction = 1.0;

            if(SiteVars.CapacityReduction != null && SiteVars.CapacityReduction[site] > 0)
            {
                capacityReduction = 1.0 - SiteVars.CapacityReduction[site];
                if(PlugIn.CalibrateMode)
                    PlugIn.ModelCore.Log.WriteLine("Yr={0}. Capacity Remaining={1:0.00}, Spp={2}, Age={3} B={4}.", (PlugIn.ModelCore.CurrentTime+SubYear), capacityReduction, cohort.Species.Name, cohort.Age, cohort.Biomass);
            }

            double maxBiomass  = SpeciesData.B_MAX_Spp[cohort.Species][ecoregion] * capacityReduction;
            double maxANPP = SpeciesData.ANPP_MAX_Spp[cohort.Species][ecoregion];

            //  Potential biomass, equation 3 in Scheller and Mladenoff, 2004
            double potentialBiomass = Math.Max(1.0, maxBiomass - siteBiomass + cohortBiomass);

            //  Species can use new space from mortality immediately
            //  but not in the case of capacity reduction due to harvesting.
            if(capacityReduction >= 1.0)
                potentialBiomass = Math.Max(potentialBiomass, SiteVars.PreviousYearMortality[site]);

            //  Ratio of cohort's actual biomass to potential biomass
            B_AP = cohortBiomass / potentialBiomass;

            double indexC = CalculateCompetition(site, cohort);

            if ((indexC <= 0.0 && cohortBiomass > 0) || indexC > 1.0)
            {
                PlugIn.ModelCore.Log.WriteLine("Error: Competition Index [{0:0.00}] is <= 0.0 or > 1.0", indexC);
                PlugIn.ModelCore.Log.WriteLine("Yr={0}. SPECIES={1}, AGE={2}, B={3}", (PlugIn.ModelCore.CurrentTime + SubYear), cohort.Species.Name, cohort.Age, cohortBiomass);

                throw new ApplicationException("Application terminating.");
            }


            //  Ratio of cohort's potential biomass to maximum biomass.  The
            //  ratio cannot be exceed 1.
            double indexOldSchool = Math.Min(1.0, potentialBiomass / maxBiomass);
            double initialMultiplier = (CanopyLightExtinction == 0.0 ? indexC : 1.0);
            double indexLightC = initialMultiplier * Math.Exp(CanopyLightExtinction);
            B_PM = indexLightC;
            // PlugIn.ModelCore.Log.WriteLine("indexC={0:0.00}, lightIndexC={1:0.00}, OldSchool={2:0.00}.", indexC, indexLightC, indexOldSchool);

            //  Actual ANPP: equation (4) from Scheller & Mladenoff, 2004.
            double actualANPP = maxANPP * Math.E * Math.Pow(B_AP, growthShape) * Math.Exp(-1 * Math.Pow(B_AP, growthShape)) * B_PM;

            // Calculated actual ANPP can not exceed the limit set by the
            //  maximum ANPP times the ratio of potential to maximum biomass.
            //  This down regulates actual ANPP by the available growing space.

            actualANPP = Math.Min(maxANPP * B_PM, actualANPP);

            if (growthReduction > 0)
                actualANPP *= (1.0 - growthReduction);

            //double LAIactual = SpeciesData.MAXLAI[cohort.Species] * actualANPP / maxANPP;
            //CanopyLightExtinction += (-1.0 * SpeciesData.LightExtinctionCoeff[cohort.Species] * LAIactual) * indexC;


            if(PlugIn.CalibrateMode && PlugIn.ModelCore.CurrentTime > 0)
            {
                PlugIn.ModelCore.Log.WriteLine("Yr={0}. Calculate ANPPactual...", (PlugIn.ModelCore.CurrentTime+SubYear));
                PlugIn.ModelCore.Log.WriteLine("Yr={0}.     Spp={1}, Age={2}.", (PlugIn.ModelCore.CurrentTime+SubYear), cohort.Species.Name, cohort.Age);
                PlugIn.ModelCore.Log.WriteLine("Yr={0}.     MaxANPP={1}, MaxB={2:0}, Bsite={3}, Bcohort={4:0.0}.", (PlugIn.ModelCore.CurrentTime+SubYear), maxANPP, maxBiomass, (int) siteBiomass, cohort.Biomass);
                PlugIn.ModelCore.Log.WriteLine("Yr={0}.     B_PM={1:0.0}, B_AP={2:0.0}, actualANPP={3:0.0}, capacityReduction={4:0.0}.", (PlugIn.ModelCore.CurrentTime+SubYear), B_PM, B_AP, actualANPP, capacityReduction);
                //PlugIn.ModelCore.Log.WriteLine("Yr={0}.     CanopyLightExtinction = {1:0.00}, LightTransmittance = {2:0.00}, LAIactual={3:0.0}.", (PlugIn.ModelCore.CurrentTime + SubYear), CanopyLightExtinction, B_PM, LAIactual);
            }

            return actualANPP;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The mortality caused by development processes,
        /// including self-thinning and loss of branches, twigs, etc.
        /// See equation 5 in Scheller and Mladenoff, 2004.
        /// </summary>
        private double ComputeGrowthMortality(ICohort cohort, ActiveSite site)
        {
            //double percentDefoliation = CohortDefoliation.Compute(cohort, site, siteBiomass);

            //const double y0 = 0.01;
            //const double r = 0.08;
            double maxANPP = SpeciesData.ANPP_MAX_Spp[cohort.Species][ecoregion];
            double M_BIO = 0.0;

            //Michaelis-Menton function:
            if (B_AP > 1.0)
                M_BIO = maxANPP * B_PM;
            else
                M_BIO = maxANPP * (2.0 * B_AP) / (1.0 + B_AP) * B_PM;

            //double M_BIO = maxANPP *
            //        (y0 / (y0 + (1 - y0) * Math.Exp(-r / y0 * B_AP))) *
            //        B_PM;

            //  Mortality should not exceed the amount of living biomass
            M_BIO = Math.Min(cohort.Biomass, M_BIO);

            //  Calculated actual ANPP can not exceed the limit set by the
            //  maximum ANPP times the ratio of potential to maximum biomass.
            //  This down regulates actual ANPP by the available growing space.

            M_BIO = Math.Min(maxANPP * B_PM, M_BIO);

            if (growthReduction > 0)
                M_BIO *= (1.0 - growthReduction);

            return M_BIO;

        }

        //---------------------------------------------------------------------

        private double UpdateDeadBiomass(ICohort cohort, double actualANPP, double totalMortality, ActiveSite site, double newBiomass)
        {

            ISpecies species = cohort.Species;
            double leafLongevity = SpeciesData.LeafLongevity[species];
            double cohortBiomass = newBiomass; // Mortality is for the current year's biomass.
            double leafFraction = ComputeFractionANPPleaf(species);

            // First, deposit the a portion of the leaf mass directly onto the forest floor.
            // In this way, the actual amount of leaf biomass is added for the year.

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
            double standing_wood = Math.Max(0, cohortBiomass - standing_nonwood);

            double fractionStandingNonwood = standing_nonwood / cohortBiomass;

            //  Assume that the remaining mortality is divided proportionally
            //  between the woody mass and non-woody mass (Niklaus & Enquist,
            //  2002).   Do not include current years growth.
            double mortality_nonwood = Math.Max(0.0, totalMortality * fractionStandingNonwood);
            double mortality_wood = Math.Max(0.0, totalMortality - mortality_nonwood);

            if (mortality_wood < 0 || mortality_nonwood < 0)
                throw new ApplicationException("Error: Woody input is < 0");

            //  Add mortality to dead biomass pools.

            if (mortality_wood > 0)
            {
                ForestFloor.AddWoody((ushort)mortality_wood, species, site);
            }

            if (mortality_nonwood > 0)
            {
                ForestFloor.AddLitter(mortality_nonwood, species, site);
            }

            //  Total mortality not including annual leaf litter
            M_noLeafLitter = (int)mortality_wood;

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

            double B_nonwoody = annualFoliar * SpeciesData.LeafLongevity[cohort.Species];

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
        /// This method is designed for external calls that need to
        /// estimate the amount of non-wood biomass.
        /// </summary>

        public Percentage ComputeNonWoodyPercentage(ICohort cohort,
                                                    ActiveSite site)
        {

            // IISiteCohorts ISiteCohorts = SiteVars.Cohorts[site];
            ISiteCohorts ISiteCohorts = SiteVars.Cohorts[site];

            double mortalityAge = ComputeAgeMortality(cohort);

            //if(ISiteCohorts == null) return new Percentage(0.0);

            // double actualANPP = ComputeActualANPP(cohort, site, ISiteCohorts.TotalBiomass,
            //                     ISiteCohorts.PrevYearMortality);

            double actualANPP = ComputeActualANPP(cohort, site); //, ISiteCohorts.TotalBiomass,
                                //ISiteCohorts.PrevYearMortality);
                                //ComputeActualANPP(cohort, site, ISiteCohorts.TotalBiomass(SiteVars.Cohorts[site]));
                                //ISiteCohorts.StaticPrevYearMortality);

            //  Age mortality is discounted from ANPP to prevent the over-
            //  estimation of mortality.  ANPP cannot be negative.
            actualANPP = Math.Max(0, actualANPP - mortalityAge);

            return new Percentage(ComputeStandingLeafBiomass(actualANPP, cohort) / cohort.Biomass);
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Computes the initial biomass for a cohort at a site.
        /// </summary>
        public static int InitialBiomass(ISpecies species,
                                         ISiteCohorts cohorts,
                                         ActiveSite site)
        {
            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];
            //double B_ACT = ActualSiteBiomass(ISiteCohorts, site, out ecoregion);

            double B_ACT = (double) Cohorts.ComputeNonYoungBiomass(cohorts);

            double maxBiomass = SpeciesData.B_MAX_Spp[species][ecoregion];

            double maxANPP = SpeciesData.ANPP_MAX_Spp[species][ecoregion];

            //  Initial biomass exponentially declines in response to
            //  competition.
            //double initialBiomass = 0.025 * maxBiomass * Math.Exp(-1.6 * B_ACT / EcoregionData.B_MAX[ecoregion]);
            double initialBiomass = maxANPP * Math.Exp(-1.6 * B_ACT / EcoregionData.B_MAX[ecoregion]);


            // Initial biomass cannot be greater than maxANPP
            initialBiomass = Math.Min(maxANPP, initialBiomass);

            //  Initial biomass cannot be less than 1.
            initialBiomass = Math.Max(1.0, initialBiomass);

            return (int)initialBiomass;
        }

        //---------------------------------------------------------------------
        // New method for calculating competition limits.
        // Iterates through cohorts, assigning each a competitive efficiency

        private static double CalculateCompetition(ActiveSite site, ICohort cohort)
        {
            double competitionPower = 0.95;
            double CMultiplier = Math.Max(Math.Pow(cohort.Biomass, competitionPower), 1.0);
            double CMultTotal = CMultiplier;
            // PlugIn.ModelCore.Log.WriteLine("Competition:  spp={0}, age={1}, CMultiplier={2:0}, CMultTotal={3:0}.", cohort.Species.Name, cohort.Age, CMultiplier, CMultTotal);

            foreach (ISpeciesCohorts speciesCohorts in SiteVars.Cohorts[site])
            {
                //write.speciesCohorts.Name;
                // PlugIn.ModelCore.Log.WriteLine("Name={0}, Cnt={1}", speciesCohorts.Species.Name, speciesCohorts.Count);
                /*foreach (ICohort ycohort in speciesCohorts)
                {
                    ICohort xcohort = (Landis.Library.BiomassCohorts.ICohort)ycohort;
                    if (xcohort.Age+1 != cohort.Age || xcohort.Species.Index != cohort.Species.Index)
                    {
                        double tempMultiplier = Math.Max(Math.Pow(xcohort.Biomass, competitionPower), 1.0);
                        CMultTotal += tempMultiplier;
                        //PlugIn.ModelCore.Log.WriteLine("Competition:  spp={0}, age={1}, CMultiplier={2:0}, CMultTotal={3:0}.", xcohort.Species.Name, xcohort.Age, tempMultiplier, CMultTotal);
                    }
                }*/

                foreach (ICohort xcohort in speciesCohorts)
                {
                    if (xcohort.Age + 1 != cohort.Age || xcohort.Species.Index != cohort.Species.Index)
                    {
                        double tempMultiplier = Math.Max(Math.Pow(xcohort.Biomass, competitionPower), 1.0);
                        CMultTotal += tempMultiplier;
                    }
                }
            }


            double Cfraction = CMultiplier / CMultTotal;
            //PlugIn.ModelCore.Log.WriteLine("Competition:  spp={0}, age={1}, CMultiplier={2:0}, CMultTotal={3:0}, CI={4:0.00}.", cohort.Species.Name, cohort.Age, CMultiplier, CMultTotal, Cfraction);

            return Cfraction;


        }
        //---------------------------------------------------------------------
        // Added 10/5/09 - BRM
        // Replaces CalculateCohortLAI
        /*
        private static void CalculateCohortLight(ICohort cohort, double actualANPP, double newBiomass, ActiveSite site)
        {
            ISpecies species = cohort.Species;
            double pctBioMaxLAI = SpeciesData.PctBioMaxLAI[species];
            double cohortBiomass = newBiomass;

            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];

            //double maxBiomass = SpeciesData.B_MAX_Spp[species][ecoregion];
            double maxBiomass = EcoregionData.B_MAX[ecoregion];
            double maxlai = SpeciesData.MAXLAI[species];
            double LAIactual = 0;
            double pctBiomass = (cohortBiomass / maxBiomass) * 100;
            if (pctBiomass >= pctBioMaxLAI)
            {
                LAIactual = maxlai;
            }
            else
            {
                double slope = 100.0 / pctBioMaxLAI;
                double pctLAI = pctBiomass * slope;
                LAIactual = (pctLAI * maxlai) / 100;
            }
            double cohortLightExt = SpeciesData.LightExtinctionCoeff[species] * LAIactual;
            double cohortLightTrans = Math.Exp(-1.0 * cohortLightExt);

            SiteVars.LightTrans[site] *= cohortLightTrans; //Combine cohortLightTrans for all cohorts on the site

        }*/

    }
}
