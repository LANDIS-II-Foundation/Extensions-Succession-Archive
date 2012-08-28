 //  Copyright 2007-2010 Portland State University, University of Wisconsin-Madison
//  Author: Robert Scheller, Ben Sulman

using Edu.Wisc.Forest.Flel.Util;
using Landis.Core;
using Landis.SpatialModeling;
using System.Collections.Generic;
using Landis.Library.LeafBiomassCohorts;
using System;

namespace Landis.Extension.Succession.Century
{
    /// <summary>
    /// Calculations for an individual cohort's biomass.
    /// </summary>
    public class CohortBiomass
        : Landis.Library.LeafBiomassCohorts.ICalculator
    {

        /// <summary>
        /// The single instance of the biomass calculations that is used by
        /// the plug-in.
        /// </summary>
        public static CohortBiomass Calculator;

        //  Ecoregion where the cohort's site is located
        private IEcoregion ecoregion;
        public static double SpinupMortalityFraction;

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
        public float[] ComputeChange(ICohort cohort, ActiveSite site)
        {
            ecoregion = PlugIn.ModelCore.Ecoregion[site];

            // First call to the Calibrate Log:
            if (PlugIn.ModelCore.CurrentTime > 0 && OtherData.CalibrateMode)
                Outputs.CalibrateLog.Write("{0}, {1}, {2}, {3}, {4}, {5:0.0}, {6:0.0}, ", PlugIn.ModelCore.CurrentTime, Century.Month + 1, ecoregion.Index, cohort.Species.Name, cohort.Age, cohort.WoodBiomass, cohort.LeafBiomass);

            double siteBiomass = Century.ComputeLivingBiomass(SiteVars.Cohorts[site]);

            if(siteBiomass < 0)
                throw new ApplicationException("Error: Site biomass < 0");

            // ****** Mortality *******
            // Age-related mortality includes woody and standing leaf biomass.
            double[] mortalityAge = ComputeAgeMortality(cohort, site);

            //  Growth-related mortality
            double[] mortalityGrowth = ComputeGrowthMortality(cohort, site);

            double[] totalMortality = new double[2]{(mortalityAge[0] + mortalityGrowth[0]), (mortalityAge[1] + mortalityGrowth[1])};

            if(totalMortality[0] <= 0.0 || cohort.WoodBiomass <= 0.0)
                totalMortality[0] = 0.0;

            if(totalMortality[1] <= 0.0 || cohort.LeafBiomass <= 0.0)
                totalMortality[1] = 0.0;


            if((totalMortality[0] + totalMortality[1]) > (cohort.WoodBiomass + cohort.LeafBiomass))
            {
                PlugIn.ModelCore.Log.WriteLine("Warning: Mortality exceeds cohort biomass. M={0:0.0}, B={1:0.0}", (totalMortality[0] + totalMortality[1]), (cohort.WoodBiomass + cohort.LeafBiomass));
                PlugIn.ModelCore.Log.WriteLine("Warning: If M>B, then list mortality. Mage0={0:0.0}, Mgrow0={1:0.0}, Mage1={2:0.0}, Mgrow1={3:0.0},", mortalityAge[0], mortalityGrowth[0], mortalityAge[1] + mortalityGrowth[1]);
                throw new ApplicationException("Error: Mortality exceeds cohort biomass");

            }

            // ****** Growth *******
            double[] actualANPP = ComputeActualANPP(cohort, site, siteBiomass, mortalityAge);
            double defoliatedLeafBiomass = 0.0;
            double scorch = 0.0;

            if(Century.Month == 6)  //July = 6
            {
                // Defoliation ranges from 1.0 (total) to none (0.0).
                double defoliation = CohortDefoliation.Compute(cohort, site, (int) siteBiomass);

                if(defoliation > 1.0) defoliation = 1.0;

                defoliatedLeafBiomass = cohort.LeafBiomass * defoliation;
                ForestFloor.AddFrassLitter(defoliatedLeafBiomass, cohort.Species, site);

                if (SiteVars.FireSeverity != null && SiteVars.FireSeverity[site] > 0)
                    scorch = FireEffects.CrownScorching(cohort, SiteVars.FireSeverity[site]);

                if (scorch > 0.0)  // NEED TO DOUBLE CHECK WHAT CROWN SCORCHING RETURNS
                    totalMortality[1] = Math.Min(cohort.LeafBiomass, scorch + totalMortality[1]);
            }

            double totalNdemand = AvailableN.CalculateCohortNDemand(cohort.Species, site, actualANPP);
            double adjNdemand = totalNdemand;
            double resorbedNused = 0.0;
            double mineralNused = 0.0;

            // Treat Resorbed N first and only if it is spring time.
            if (Century.Month > 2 && Century.Month < 6)
            {
                double resorbedNallocation = Math.Max(0.0, AvailableN.GetResorbedNallocation(cohort));

                resorbedNused = resorbedNallocation - Math.Max(0.0, resorbedNallocation - totalNdemand);

                AvailableN.SetResorbedNallocation(cohort, Math.Max(0.0, resorbedNallocation - totalNdemand));

                adjNdemand = Math.Max(0.0, totalNdemand - resorbedNallocation);
            }

            // Reduce available N after taking into account that some N may have been provided
            // via resorption (above).
            double Nuptake = 0.0;
            if (SiteVars.MineralN[site] >= adjNdemand)
            {
                    SiteVars.MineralN[site] -= adjNdemand;
                    mineralNused = adjNdemand;
                    Nuptake = adjNdemand;
            }
            else
            {
                    double NdemandAdjusted = SiteVars.MineralN[site];
                    mineralNused = SiteVars.MineralN[site];
                    SiteVars.MineralN[site] = 0.0;
                    //actualANPP[0] = NdemandAdjusted / adjNdemand;
                    //actualANPP[1] = NdemandAdjusted / adjNdemand;

                    Nuptake = SiteVars.MineralN[site]; // *NdemandAdjusted / adjNdemand;
                    //PlugIn.ModelCore.Log.WriteLine("Yr={0},Mo={1}.     Adjusted ANPP:  ANPPleaf={2:0.0}, ANPPwood={3:0.0}.", PlugIn.ModelCore.CurrentTime, month + 1, actualANPP[1], actualANPP[0]);
            }
            SiteVars.TotalNuptake[site] += Nuptake;

            float deltaWood = (float) (actualANPP[0] - totalMortality[0]);
            float deltaLeaf = (float)(actualANPP[1] - totalMortality[1]);

            float[] deltas  = new float[2]{deltaWood, deltaLeaf};

            UpdateDeadBiomass(cohort.Species, site, totalMortality);
            CalculateNPPcarbon(site, actualANPP);

            if (OtherData.CalibrateMode && PlugIn.ModelCore.CurrentTime > 0)
            {
                Outputs.CalibrateLog.Write("{0:0.00}, {1:0.00}, {2:0.00}, {3:0.00},", deltaWood, deltaLeaf, totalMortality[0], totalMortality[1]);
                Outputs.CalibrateLog.WriteLine("{0:0.00}, {1:0.00}, {2:0.00}", resorbedNused, mineralNused, totalNdemand);
            }

            return deltas;
        }


        //---------------------------------------------------------------------

        private double[] ComputeActualANPP(ICohort    cohort,
                                         ActiveSite site,
                                         double    siteBiomass,
                                         double[]   mortalityAge)
        {

            double leafFractionNPP  = FunctionalType.Table[SpeciesData.FuncType[cohort.Species]].FCFRACleaf;
            double maxBiomass       = SpeciesData.B_MAX_Spp[cohort.Species][ecoregion];
            double sitelai          = SiteVars.LAI[site];
            double maxNPP           = SpeciesData.ANPP_MAX_Spp[cohort.Species][ecoregion];

            double limitT   = calculateTemp_Limit(site, cohort.Species);

            double limitH20 = calculateWater_Limit(site, ecoregion, cohort.Species);

            double limitLAI = calculateLAI_Limit(((double) cohort.LeafBiomass * 0.47), ((double) cohort.WoodBiomass * 0.47), cohort.Species);

            double limitCapacity = 1.0 - Math.Min(1.0, Math.Exp(siteBiomass / maxBiomass * 5.0) / Math.Exp(5.0));

            double potentialNPP = maxNPP * limitLAI * limitH20 * limitT * limitCapacity;

            double limitN = calculateN_Limit(site, cohort, potentialNPP, leafFractionNPP);

            potentialNPP *= limitN;

            //double potentialNPP = maxNPP * Math.Min(limitLAI, limitCapacity) * limitH20 * limitT * limitN;
            //double potentialNPP = maxNPP * Math.Min(limitLAI, limitCapacity) * limitH20 * limitT * limitN;

            //if (Double.IsNaN(limitT) || Double.IsNaN(limitH20) || Double.IsNaN(limitLAI) || Double.IsNaN(limitCapacity) || Double.IsNaN(limitN))
            //{
            //    PlugIn.ModelCore.Log.WriteLine("  A limit = NaN!  Will set to zero.");
            //    PlugIn.ModelCore.Log.WriteLine("  Yr={0},Mo={1}.     GROWTH LIMITS: LAI={2:0.00}, H20={3:0.00}, N={4:0.00}, T={5:0.00}, Capacity={6:0.0}", PlugIn.ModelCore.CurrentTime, month + 1, limitLAI, limitH20, limitN, limitT, limitCapacity);
            //    PlugIn.ModelCore.Log.WriteLine("  Yr={0},Mo={1}.     Other Information: MaxB={2}, Bsite={3}, Bcohort={4:0.0}, SoilT={5:0.0}.", PlugIn.ModelCore.CurrentTime, month + 1, maxBiomass, (int)siteBiomass, (cohort.WoodBiomass + cohort.LeafBiomass), SiteVars.SoilTemperature[site]);
            //}


            //  Age mortality is discounted from ANPP to prevent the over-
            //  estimation of growth.  ANPP cannot be negative.
            double actualANPP = Math.Max(0.0, potentialNPP - mortalityAge[0] - mortalityAge[1]);

            // Growth can be reduced by another extension via this method.
            // To date, no extension has been written to utilize this hook.
            double growthReduction = 0.0;
            growthReduction = CohortGrowthReduction.Compute(cohort, site);

            if (growthReduction > 0.0)
            {
                actualANPP *= (1.0 - growthReduction);
            }


            double leafNPP  = actualANPP * leafFractionNPP;
            double woodNPP  = actualANPP * (1.0 - leafFractionNPP);

            //  Adjust the leaf:wood NPP ratio to ensure that there is a minimal amount of leaf NPP,
            //  at the expense of wood NPP.
            double minimumLeafNPP = (double) cohort.WoodBiomass * 0.002;

            leafNPP         = Math.Max(leafNPP, minimumLeafNPP);
            if (actualANPP > 0.0)
                leafFractionNPP = leafNPP / actualANPP;
            else
                leafFractionNPP = 0.0;

            leafFractionNPP = Math.Min(1.0, leafFractionNPP);


            leafNPP  = actualANPP * leafFractionNPP;
            woodNPP  = actualANPP * (1.0 - leafFractionNPP);

            if (Double.IsNaN(leafNPP) || Double.IsNaN(woodNPP))
            {
                PlugIn.ModelCore.Log.WriteLine("  EITHER WOOD or LEAF NPP = NaN!  Will set to zero.");
                PlugIn.ModelCore.Log.WriteLine("  Yr={0},Mo={1}.     GROWTH LIMITS: LAI={2:0.00}, H20={3:0.00}, N={4:0.00}, T={5:0.00}, Capacity={6:0.0}", PlugIn.ModelCore.CurrentTime, Century.Month + 1, limitLAI, limitH20, limitN, limitT, limitCapacity);
                PlugIn.ModelCore.Log.WriteLine("  Yr={0},Mo={1}.     Other Information: MaxB={2}, Bsite={3}, Bcohort={4:0.0}, SoilT={5:0.0}.", PlugIn.ModelCore.CurrentTime, Century.Month + 1, maxBiomass, (int)siteBiomass, (cohort.WoodBiomass + cohort.LeafBiomass), SiteVars.SoilTemperature[site]);
                PlugIn.ModelCore.Log.WriteLine("  Yr={0},Mo={1}.     WoodNPP={2:0.00}, LeafNPP={3:0.00}.", PlugIn.ModelCore.CurrentTime, Century.Month + 1, woodNPP, leafNPP);
                if (Double.IsNaN(leafNPP))
                    leafNPP = 0.0;
                if (Double.IsNaN(woodNPP))
                    woodNPP = 0.0;

            }

            if (PlugIn.ModelCore.CurrentTime > 0 && OtherData.CalibrateMode)
            {
                Outputs.CalibrateLog.Write("{0:0.00}, {1:0.00}, {2:0.00}, {3:0.00}, {4:0.00}, ", limitLAI, limitH20, limitT, limitCapacity, limitN);
                Outputs.CalibrateLog.Write("{0}, {1}, {2}, {3:0.0}, {4:0.0}, ", maxNPP, maxBiomass, (int)siteBiomass, (cohort.WoodBiomass + cohort.LeafBiomass), SiteVars.SoilTemperature[site]);
                Outputs.CalibrateLog.Write("{0:0.00}, {1:0.00}, ", woodNPP, leafNPP);
            }



            return new double[2]{woodNPP, leafNPP};

        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Computes M_AGE_ij: the mortality caused by the aging of the cohort.
        /// See equation 6 in Scheller and Mladenoff, 2004.
        /// </summary>
        private double[] ComputeAgeMortality(ICohort cohort, ActiveSite site)
        {

            double monthAdjust = 1.0 / 12.0;
            double totalBiomass = (double) (cohort.WoodBiomass + cohort.LeafBiomass);
            //double fractionLeaf = (double) cohort.LeafBiomass / totalBiomass;
            double max_age      = (double) cohort.Species.Longevity;
            double d            = FunctionalType.Table[SpeciesData.FuncType[cohort.Species]].MortCurveShape;

            double M_AGE_wood =    cohort.WoodBiomass *  monthAdjust *
                                    Math.Exp((double) cohort.Age / max_age * d) / Math.Exp(d);

            double M_AGE_leaf =    cohort.LeafBiomass *  monthAdjust *
                                    Math.Exp((double) cohort.Age / max_age * d) / Math.Exp(d);

            if (PlugIn.ModelCore.CurrentTime <= 0 &&  SpinupMortalityFraction > 0.0)
            {
                M_AGE_wood += cohort.Biomass * SpinupMortalityFraction;
                M_AGE_leaf += cohort.Biomass * SpinupMortalityFraction;
            }

            M_AGE_wood = Math.Min(M_AGE_wood, cohort.WoodBiomass);
            M_AGE_leaf = Math.Min(M_AGE_leaf, cohort.LeafBiomass);

            double[] M_AGE = new double[2]{M_AGE_wood, M_AGE_leaf};

            SiteVars.AgeMortality[site] += (M_AGE_leaf + M_AGE_wood);

            if(M_AGE_wood < 0.0 || M_AGE_leaf < 0.0)
            {
                PlugIn.ModelCore.Log.WriteLine("Mwood={0}, Mleaf={1}.", M_AGE_wood, M_AGE_leaf);
                throw new ApplicationException("Error: Woody or Leaf Age Mortality is < 0");
            }

            if (PlugIn.ModelCore.CurrentTime > 0 && OtherData.CalibrateMode)
                Outputs.CalibrateLog.Write("{0:0.00}, {1:0.00}, ", M_AGE_wood, M_AGE_leaf);


            return M_AGE;
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Monthly mortality as a function of standing leaf and wood biomass.
        /// </summary>
        private double[] ComputeGrowthMortality(ICohort cohort, ActiveSite site)
        {
            //if(cohort.WoodBiomass <= 0 || cohort.LeafBiomass <= 0)
            //    return (new double[2]{0.0, 0.0});

            double M_wood = cohort.WoodBiomass * FunctionalType.Table[SpeciesData.FuncType[cohort.Species]].MonthlyWoodMortality;
            double M_leaf = 0.0;

            // Leaves and Needles dropped.
            if(SpeciesData.LeafLongevity[cohort.Species] > 1.0) // && month == FunctionalType.Table[SpeciesData.FuncType[cohort.Species]].LeafNeedleDrop-1)
            {
                M_leaf = cohort.LeafBiomass / SpeciesData.LeafLongevity[cohort.Species] / 12.0;  //Needle deposit spread across the year.
            }
            else
            {
                if(Century.Month+1 == FunctionalType.Table[SpeciesData.FuncType[cohort.Species]].LeafNeedleDrop)
                {
                    M_leaf = cohort.LeafBiomass / 2.0;  //spread across 2 months
                }
                if (Century.Month+1 > FunctionalType.Table[SpeciesData.FuncType[cohort.Species]].LeafNeedleDrop)
                {
                    M_leaf = cohort.LeafBiomass;  //drop the remainder
                }
            }

            double[] M_BIO = new double[2]{M_wood, M_leaf};

            if(M_wood < 0.0 || M_leaf < 0.0)
            {
                PlugIn.ModelCore.Log.WriteLine("Mwood={0}, Mleaf={1}.", M_wood, M_leaf);
                throw new ApplicationException("Error: Wood or Leaf Growth Mortality is < 0");
            }

            if (PlugIn.ModelCore.CurrentTime > 0 && OtherData.CalibrateMode)
                Outputs.CalibrateLog.Write("{0:0.00}, {1:0.00}, ", M_wood, M_leaf);

            return M_BIO;

        }


        //---------------------------------------------------------------------

        private void UpdateDeadBiomass(ISpecies species, ActiveSite site, double[] totalMortality)
        {


            double mortality_wood    = (double) totalMortality[0];
            double mortality_nonwood = (double) totalMortality[1];

            //  Add mortality to dead biomass pools.
            //  Coarse root mortality is assumed proportional to aboveground woody mortality
            //    mass is assumed 25% of aboveground wood (White et al. 2000, Niklas & Enquist 2002)
            if(mortality_wood > 0.0)
            {
                ForestFloor.AddWoodLitter(mortality_wood, species, site);
                Roots.AddCoarseRootLitter(mortality_wood, species, site);
            }

            if(mortality_nonwood > 0.0)
            {
                ForestFloor.AddResorbedFoliageLitter(mortality_nonwood, species, site);
                Roots.AddFineRootLitter(mortality_nonwood, species, site);
            }

            return;

        }


        //---------------------------------------------------------------------

        /// <summary>
        /// Computes the initial biomass for a cohort at a site.
        /// </summary>
        public static float[] InitialBiomass(ISpecies species, ISiteCohorts siteCohorts,
                                            ActiveSite  site)
        {
            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];

            double leafFrac = FunctionalType.Table[SpeciesData.FuncType[species]].FCFRACleaf;
            //double Ndemand = 0.0;

            double B_ACT = SiteVars.ActualSiteBiomass(site);
            double B_MAX = SpeciesData.B_MAX_Spp[species][ecoregion];

            //  Initial biomass exponentially declines in response to
            //  competition.
            double initialBiomass = 0.002 * B_MAX *
                                    Math.Exp(-1.6 * B_ACT / B_MAX);


            //Initial biomass is limited by nitrogen availability.
            //initialBiomass *= SpeciesData.NLimits[species];
            initialBiomass = Math.Max(initialBiomass, 5.0);

            double initialLeafB = initialBiomass * leafFrac;
            double initialWoodB = initialBiomass - initialLeafB;
            double[] initialB = new double[2]{initialWoodB, initialLeafB};


            //PlugIn.ModelCore.Log.WriteLine("Yr={0},Mo={1}, InitialB={2:0.0}, InitBleaf={3:0.00}, InitBwood={4:0.00}. LeafFrac={5:0.0}", PlugIn.ModelCore.CurrentTime, month, initialBiomass, initialB[1], initialB[0], leafFrac);
            //PlugIn.ModelCore.Log.WriteLine("Yr={0},Mo={1}, B_MAX={2:0.0}, B_ACT={3:0.00}", PlugIn.ModelCore.CurrentTime, month, B_MAX, B_ACT);

            // Note:  The following if statement is critical for ensuring that young cohorts
            // get established properly.
            /*if (SiteVars.MineralN[site] <= 0 || initialBiomass < 5.0)
            {
                initialBiomass = Math.Min(initialBiomass, 5.0);
                initialB[0] = initialBiomass * (1.0 - leafFrac);
                initialB[1] = initialBiomass * leafFrac;
            }*/

            //Ndemand = AvailableN.CalculateCohortNDemand(species, site, initialB);
            //SiteVars.MineralN[site] -= Math.Min(SiteVars.MineralN[site], Nreduction);

            /*if (SiteVars.MineralN[site] > Nreduction)
                SiteVars.MineralN[site] -= Nreduction;
            else
            {
                double NreductionAdjusted = SiteVars.MineralN[site];
                SiteVars.MineralN[site] = 0.0;
                initialB[0] = NreductionAdjusted / Nreduction;
                initialB[1] = NreductionAdjusted / Nreduction;
            }*/

            float[] initialWoodLeafBiomass = new float[2]{(float) initialB[0], (float) initialB[1]};

            return initialWoodLeafBiomass;
        }


        //---------------------------------------------------------------------
        /// <summary>
        /// Summarize NPP
        /// </summary>
        private static void CalculateNPPcarbon(ActiveSite site, double[] AGNPP)
        {
            double NPPwood = (double) AGNPP[0] * 0.47;
            double NPPleaf = (double) AGNPP[1] * 0.47;

            double NPPcoarseRoot = Roots.CalculateCoarseRoot(NPPwood);
            double NPPfineRoot = Roots.CalculateFineRoot(NPPleaf);

            if (Double.IsNaN(NPPwood) || Double.IsNaN(NPPleaf) || Double.IsNaN(NPPcoarseRoot) || Double.IsNaN(NPPfineRoot))
            {
                PlugIn.ModelCore.Log.WriteLine("  EITHER WOOD or LEAF NPP or COARSE ROOT or FINE ROOT = NaN!  Will set to zero.");
                PlugIn.ModelCore.Log.WriteLine("  Yr={0},Mo={1}.     WoodNPP={0}, LeafNPP={1}, CRootNPP={2}, FRootNPP={3}.", NPPwood, NPPleaf, NPPcoarseRoot, NPPfineRoot);
                if (Double.IsNaN(NPPleaf))
                    NPPleaf = 0.0;
                if (Double.IsNaN(NPPwood))
                    NPPwood = 0.0;
                if (Double.IsNaN(NPPcoarseRoot))
                    NPPcoarseRoot = 0.0;
                if (Double.IsNaN(NPPfineRoot))
                    NPPfineRoot = 0.0;
            }


            SiteVars.AGNPPcarbon[site] += NPPwood + NPPleaf;
            SiteVars.BGNPPcarbon[site] += NPPcoarseRoot + NPPfineRoot;
            SiteVars.MonthlyAGNPPcarbon[site][Century.Month] += NPPwood + NPPleaf;
            SiteVars.MonthlyBGNPPcarbon[site][Century.Month] += NPPcoarseRoot + NPPfineRoot;


        }

        //--------------------------------------------------------------------------
        //N limit is actual demand divided by maximum uptake.
        private double calculateN_Limit(ActiveSite site, ICohort cohort, double NPP, double leafFractionNPP)
        {

            //Get Cohort Mineral and Resorbed N allocation.
            double mineralNallocation = AvailableN.GetMineralNallocation(cohort);
            double resorbedNallocation = 0.0;

            if (Century.Month > 2 && Century.Month < 6)
                resorbedNallocation = AvailableN.GetResorbedNallocation(cohort);

            double LeafNPP = Math.Max(NPP * leafFractionNPP, 0.002 * cohort.WoodBiomass);
            double WoodNPP = NPP * (1.0 - leafFractionNPP);

            //double FineRootNPP = LeafNPP * 0.75;
            //double CoarseRootNPP = WoodNPP * 0.75;

            double limitN = 0.0;
            if (SpeciesData.NFixer[cohort.Species])
                limitN = 1.0;  // No limit for N-fixing shrubs
            else
            {
                // Divide allocation N by N demand here:
                //PlugIn.ModelCore.Log.WriteLine("  WoodNPP={0:0.00}, LeafNPP={1:0.00}, FineRootNPP={2:0.00}, CoarseRootNPP={3:0.00}.", WoodNPP, LeafNPP);

                double Ndemand = (AvailableN.CalculateCohortNDemand(cohort.Species, site, new double[] { WoodNPP, LeafNPP})); //, FineRootNPP, CoarseRootNPP }));

                if (Ndemand > 0.0)
                {
                    limitN = Math.Min(1.0, (mineralNallocation + resorbedNallocation) / Ndemand);
                    //PlugIn.ModelCore.Log.WriteLine("  mineralNallocation={0:0.00}, resorbedNallocation={1:0.00}, maximumNdemand={2:0.00}.", mineralNallocation, resorbedNallocation, Ndemand);

                }
                else
                    limitN = 1.0; // No demand means that it is a new or very small cohort.  Will allow it to grow anyways.
            }

            if (PlugIn.ModelCore.CurrentTime > 0 && OtherData.CalibrateMode)
                Outputs.CalibrateLog.Write("{0:0.00}, {1:0.00}, ", AvailableN.GetMineralNallocation(cohort), AvailableN.GetResorbedNallocation(cohort));

            return limitN;
        }
        //--------------------------------------------------------------------------
        // Originally from lacalc.f of CENTURY model

        private static double calculateLAI_Limit(double leafC, double largeWoodC, ISpecies species)
        {

            //...Calculate true LAI using leaf biomass and a biomass-to-LAI
            //     conversion parameter which is the slope of a regression
            //     line derived from LAI vs Foliar Mass for Slash Pine.

            //...Calculate theoretical LAI as a function of large wood mass.
            //     There is no strong consensus on the true nature of the relationship
            //     between LAI and stemwood mass.  Version 3.0 used a negative exponential
            //     relationship between leaf mass and large wood mass, which tended to
            //     break down in very large forests.  Many sutdies have cited as "general"
            //      an increase of LAI up to a maximum, then a decrease to a plateau value
            //     (e.g. Switzer et al. 1968, Gholz and Fisher 1982).  However, this
            //     response is not general, and seems to mostly be a feature of young
            //     pine plantations.  Northern hardwoods have shown a monotonic increase
            //     to a plateau  (e.g. Switzer et al. 1968).  Pacific Northwest conifers
            //     have shown a steady increase in LAI with no plateau evident (e.g.
            //     Gholz 1982).  In this version, we use a simple saturation fucntion in
            //     which LAI increases linearly against large wood mass initially, then
            //     approaches a plateau value.  The plateau value can be set very large to
            //     give a response of steadily increasing LAI with stemwood.

            //     References:
            //             1)  Switzer, G.L., L.E. Nelson and W.H. Smith 1968.
            //                 The mineral cycle in forest stands.  'Forest
            //                 Fertilization:  Theory and Practice'.  pp 1-9
            //                 Tenn. Valley Auth., Muscle Shoals, AL.
            //
            //             2)  Gholz, H.L., and F.R. Fisher 1982.  Organic matter
            //                 production and distribution in slash pine (Pinus
            //                 elliotii) plantations.  Ecology 63(6):  1827-1839.
            //
            //             3)  Gholz, H.L.  1982.  Environmental limits on aboveground
            //                 net primary production and biomass in vegetation zones of
            //                 the Pacific Northwest.  Ecology 63:469-481.

            //...Local variables

            double lai = 0.0;
            double laitop = -0.47;  // This is the value given for all biomes in the tree.100 file.
            double btolai = FunctionalType.Table[SpeciesData.FuncType[species]].BTOLAI;
            double klai   = FunctionalType.Table[SpeciesData.FuncType[species]].KLAI;
            double maxlai = FunctionalType.Table[SpeciesData.FuncType[species]].MAXLAI;

            double rlai = (leafC * 2.5) * btolai;

            double tlai = maxlai * largeWoodC/(klai + largeWoodC);

            //...Choose the LAI reducer on production.  I don't really understand
            //     why we take the average in the first case, but it will probably
            //     change...

            if (rlai < tlai) lai = (rlai + tlai) / 2.0;
            else lai = tlai;

            // This will allow us to set MAXLAI to zero such that LAI is completely dependent upon
            // foliar carbon, which may be necessary for simulating defoliation events.
            if(tlai <= 0.0) lai = rlai;

            //lai = tlai;  // Century 4.5 ignores rlai.


            // Limit aboveground wood production by leaf area
            //  index.
            //
            //       REF:    Efficiency of Tree Crowns and Stemwood
            //               Production at Different Canopy Leaf Densities
            //               by Waring, Newman, and Bell
            //               Forestry, Vol. 54, No. 2, 1981

            //totalLAI += lai;
            // if (totalLAI > EcoregionData.MaxLAI)
            // lai = 0.1;

            // The minimum LAI to calculate effect is 0.2.
            //if (lai < 0.5) lai = 0.5;
            if (lai < 0.1) lai = 0.1;

            //SiteVars.LAI[site] += lai; Tracking LAI.

            double LAI_limit = Math.Max(0.0, 1.0 - Math.Exp(laitop * lai));


            //PlugIn.ModelCore.Log.WriteLine("Yr={0},Mo={1}. Spp={2}, leafC={3:0.0}, woodC={4:0.00}.", PlugIn.ModelCore.CurrentTime, month + 1, species.Name, leafC, largeWoodC);
            //PlugIn.ModelCore.Log.WriteLine("Yr={0},Mo={1}. Spp={2}, lai={3:0.0}, woodC={4:0.00}.", PlugIn.ModelCore.CurrentTime, month + 1, species.Name, lai, largeWoodC);
            //PlugIn.ModelCore.Log.WriteLine("Yr={0},Mo={1}.     LAI Limits:  lai={2:0.0}, woodLAI={3:0.0}, leafLAI={4:0.0}, LAIlimit={5:0.00}.", PlugIn.ModelCore.CurrentTime, month + 1, lai, woodLAI, leafLAI, LAI_limit);

            return LAI_limit;

        }

        //---------------------------------------------------------------------------
        //... Originally from pprdwc(wc,x,pprpts) of CENTURY

        //...This funtion returns a value for potential plant production
        //     due to water content.  Basically you have an equation of a
        //     line with a moveable y-intercept depending on the soil type.
        //     The value passed in for x is ((avh2o(1) + prcurr(month) + irract)/pet)

        //     pprpts(1):  The minimum ratio of available water to pet which
        //                 would completely limit production assuming wc=0.
        //     pprpts(2):  The effect of wc on the intercept, allows the
        //                 user to increase the value of the intercept and
        //                 thereby increase the slope of the line.
        //     pprpts(3):  The lowest ratio of available water to pet at which
        //                 there is no restriction on production.
        private static double calculateWater_Limit(ActiveSite site, IEcoregion ecoregion, ISpecies species)
        {

            double pptprd = 0.0;
            double waterContent = EcoregionData.FieldCapacity[ecoregion] - EcoregionData.WiltingPoint[ecoregion];

            double tmoist = EcoregionData.AnnualWeather[ecoregion].MonthlyPrecip[Century.Month]; //rain + irract;

            double pet = EcoregionData.AnnualWeather[ecoregion].MonthlyPET[Century.Month];

            if (pet >= 0.01)
            {   //       Trees are allowed to access the whole soil profile -rm 2/97
                //         pptprd = (avh2o(1) + tmoist) / pet
                pptprd = (SiteVars.AvailableWater[site] + tmoist) / pet;
                //pptprd = SiteVars.AvailableWater[site] / pet;
                //PlugIn.ModelCore.Log.WriteLine("Yr={0},Mo={1}.     AvailH20={2:0.0}, MonthPPT={3:0.0}, PET={4:0.0}.", PlugIn.ModelCore.CurrentTime, month, SiteVars.AvailableWater[site],tmoist,pet);
            }
            else pptprd = 0.01;

            //...The equation for the y-intercept (intcpt) is A+B*WC.  A and B
            //     determine the effect of soil texture on plant production based
            //     on moisture.

            //...Old way:
            //      intcpt = 0.0 + 1.0 * wc
            //      The second point in the equation is (.8,1.0)
            //      slope = (1.0-0.0)/(.8-intcpt)
            //      pprdwc = 1.0+slope*(x-.8)

            //...New way:

            double pprpts1 = OtherData.PPRPTS1;
            double pprpts2 = FunctionalType.Table[SpeciesData.FuncType[species]].PPRPTS2;
            double pprpts3 = FunctionalType.Table[SpeciesData.FuncType[species]].PPRPTS3;

            double intcpt = pprpts1 + (pprpts2 * waterContent);
            double slope  = 1.0 / (pprpts3 - intcpt);
            double pprdwc = 1.0 + slope * (pptprd - pprpts3);

            if (pprdwc > 1.0)  pprdwc = 1.0;
            if (pprdwc < 0.01) pprdwc = 0.01;

            //PlugIn.ModelCore.Log.WriteLine("Yr={0},Mo={1}.     AvailH20={2:0.0}, MonthlyPPT={3:0.0}, PET={4:0.0}, PPTPRD={5:0.0}, Limit={6:0.0}.", PlugIn.ModelCore.CurrentTime, month, SiteVars.AvailableWater[site],tmoist, pet,pptprd,pprdwc);

            return pprdwc;
        }


        //-----------
        private double calculateTemp_Limit(ActiveSite site, ISpecies species)
        {
            //Originally from gpdf.f of CENTURY model
            //It calculates the limitation of soil temperature on aboveground forest potential production.
            //It is a function and only called by potcrp.f and potfor.f.

            //A1 is temperature. A2~A5 are paramters from tree.100

            //...This routine is functionally equivalent to the routine of the
            //     same name, described in the publication:

            //       Some Graphs and their Functional Forms
            //       Technical Report No. 153
            //       William Parton and George Innis (1972)
            //       Natural Resource Ecology Lab.
            //       Colorado State University
            //       Fort collins, Colorado  80523
            //...Local variables

            double A1 = SiteVars.SoilTemperature[site];
            double A2 = FunctionalType.Table[SpeciesData.FuncType[species]].PPDF1;
            double A3 = FunctionalType.Table[SpeciesData.FuncType[species]].PPDF2;
            double A4 = FunctionalType.Table[SpeciesData.FuncType[species]].PPDF3;
            double A5 = FunctionalType.Table[SpeciesData.FuncType[species]].PPDF4;

            double frac = (A3-A1) / (A3-A2);
            double U1 = 0.0;
            if (frac > 0.0)
                U1 = Math.Exp(A4 / A5 * (1.0 - Math.Pow(frac, A5))) * Math.Pow(frac, A4);

            //PlugIn.ModelCore.Log.WriteLine("  TEMPERATURE Limits:  Month={0}, Soil Temp={1:0.00}, Temp Limit={2:0.00}. [PPDF1={3:0.0},PPDF2={4:0.0},PPDF3={5:0.0},PPDF4={6:0.0}]", month+1, A1, U1,A2,A3,A4,A5);

            return U1;
        }


    }
}
