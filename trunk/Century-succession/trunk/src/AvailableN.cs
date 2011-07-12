//  Copyright 2007-2010 Portland State University, University of Wisconsin-Madison
//  Author: Robert Scheller, Ben Sulman

using Edu.Wisc.Forest.Flel.Util;
using System.Collections.Generic;
using System;
using Landis.Core;
using Landis.SpatialModeling;
using Landis.Library.LeafBiomassCohorts;

namespace Landis.Extension.Succession.Century
{
    /// <summary>
    /// </summary>
    public class AvailableN
    {
        //Nested dictionary of species,cohort
        public static Dictionary<int, Dictionary<int,double>> CohortNlimits;

        //New method for calculating N limits, called from Century.cs Run method before calling Grow
        //Iterates through cohorts, assigning each a N gathering efficiency based on fine root biomass
        //and N tolerance.
        public static void CalculateNLimits(Site site)
        {
            // Iterate through the first time, assigning each cohort un un-normalized N multiplier
            double NMultTotal=0.0;
            foreach (ISpeciesCohorts speciesCohorts in SiteVars.Cohorts[site])
                foreach (ICohort cohort in speciesCohorts)
                {
                    int Ntolerance = SpeciesData.NTolerance[cohort.Species];

                    //NMultiplier is a measure of how much N a cohort can gather relative to other cohorts
                    double NMultiplier = CalculateNMultiplier(cohort.Biomass, Ntolerance);
                    NMultTotal += NMultiplier;
                    Dictionary<int,double> newEntry = new Dictionary<int,double>();
                    newEntry.Add(cohort.Age,NMultiplier);

                    if (CohortNlimits.ContainsKey(cohort.Species.Index))
                    {
                        CohortNlimits[cohort.Species.Index].Add(cohort.Age,NMultiplier);
                    }
                    else
                    {
                        CohortNlimits.Add(cohort.Species.Index,newEntry);
                    }
                }

            double availableN = SiteVars.MineralN[site];  // g/m2

            //Iterate through a second time now that we have total N multiplier
            //Divide through by total and multiply by total available N so each cohort has a max N value
            //and the sum of cohort max N values is the site available N
            
            double totalNUptake = 0.0;
            foreach (ISpeciesCohorts speciesCohorts in SiteVars.Cohorts[site])
            {
                foreach (ICohort cohort in speciesCohorts)
                {
                    double NMultiplier = CohortNlimits[cohort.Species.Index][cohort.Age];
                    double Nfrac = NMultiplier / NMultTotal;
                    CohortNlimits[cohort.Species.Index][cohort.Age] = Nfrac * availableN;
                    totalNUptake += Nfrac * availableN;
                }
            }
            if (totalNUptake > availableN)
            {
                totalNUptake = availableN;
                //PlugIn.ModelCore.Log.WriteLine("   ERROR:  Total max N uptake = {0:0.000}, availableN = {1:0.000}.", totalNUptake, availableN);
                //throw new ApplicationException("Error: Max N uptake > availableN.  See AvailableN.cs");
            }

            return; 
        }

        //Calculates a multiplier for how much N a cohort can take up
        //Units are arbitrary, since it all gets normalized later
        //Start with a simple multiplier, so a tree with Ntolerance 3 takes up 3/2 more N than a tree with Ntolerance 2
        private static double CalculateNMultiplier(double biomass, int Ntolerance)
        {
            if (Ntolerance == 4) Ntolerance = 0;  // N-fixing shrubs do not take up N

            return Math.Max(Math.Pow(biomass, 0.2) * Ntolerance, 1.0);
        }


        /// <summary>
        /// Reduces Available N depending upon how much N was removed through growth (ANPP).
        /// </summary>
        public static double CohortUptakeAvailableN(ISpecies species, ActiveSite site, double[] actualANPP)
        {

            if(actualANPP[0] <= 0.0 && actualANPP[1] <= 0.0)
                return 0.0;

            double ANPPwood = 0.0;
            double ANPPcoarseRoot = 0.0;
            double ANPPleaf = 0.0;
            double ANPPfineRoot = 0.0;
            double woodN = 0.0;
            double coarseRootN = 0.0;
            double leafN = 0.0;
            double fineRootN = 0.0;

            if(actualANPP[0] > 0.0)
            {
                ANPPwood = actualANPP[0];
                ANPPcoarseRoot = Roots.CalculateCoarseRoot(ANPPwood);
                woodN       = ANPPwood * 0.5  / SpeciesData.WoodCN[species];
                coarseRootN = ANPPcoarseRoot * 0.5  / SpeciesData.CoarseRootCN[species];
            }

            if(actualANPP[1] > 0.0)
            {
                ANPPleaf = actualANPP[1];
                ANPPfineRoot = Roots.CalculateFineRoot(ANPPleaf);
                leafN       = ANPPleaf * 0.5 / SpeciesData.LeafLitterCN[species];
                fineRootN   = ANPPfineRoot * 0.5  / SpeciesData.FineRootLitterCN[species];
            }

            double totalANPP_C = (ANPPleaf + ANPPwood + ANPPcoarseRoot + ANPPfineRoot) * 0.5;
            double Nreduction = leafN + woodN + coarseRootN + fineRootN;

            //PlugIn.ModelCore.Log.WriteLine("ANPPleaf={0:0.0}, ANPPwood={1:0.0}, ANPPcRoot={2:0.0}, ANPPfRoot={3:0.0},", ANPPleaf, ANPPwood, ANPPcoarseRoot, ANPPfineRoot);

            if(Nreduction < 0.0)
            {
                PlugIn.ModelCore.Log.WriteLine("   ERROR:  TotalANPP-C={0:0.00} Nreduction={1:0.00}.", totalANPP_C, Nreduction);
                throw new ApplicationException("Error: N Reduction is < 0.  See AvailableN.cs");
            }

            if(Nreduction > SiteVars.MineralN[site])
            {
                Nreduction = SiteVars.MineralN[site];
            }

            return Nreduction;
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Calculates available nitrogen from external and internal sources.
        /// </summary>
        public static double GetTranslocatedN(ISpecies species,
                                       double annualLeafTurnover,
                                       double leafMortality)
        {
            double leafFractionN = 1.0 / (SpeciesData.LeafCN[species] * 2.0);
            double litterFractionN = 1.0 / (SpeciesData.LeafLitterCN[species] * 2.0);

            double transN = (annualLeafTurnover + leafMortality) *
                            (leafFractionN - litterFractionN);
            return transN;
        }
    }
}
