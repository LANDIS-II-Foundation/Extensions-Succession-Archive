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
        public static Dictionary<int, Dictionary<int,double>> CohortMineralNallocation;
        public static Dictionary<int, Dictionary<int, double>> CohortResorbedNallocation;

        //---------------------------------------------------------------------
        // Method for retrieving the available resorbed N for each cohort.
        // Return amount of resorbed N in g N m-2.
        public static double GetResorbedNallocation(ICohort cohort)
        {
            int currentYear = PlugIn.ModelCore.CurrentTime;
            int successionTime = PlugIn.SuccessionTimeStep;
            int cohortAddYear = currentYear - cohort.Age - currentYear % successionTime;
            double resorbedNallocation = 0.0;
            Dictionary<int, double> cohortDict;
            
            if (AvailableN.CohortResorbedNallocation.TryGetValue(cohort.Species.Index, out cohortDict))
                cohortDict.TryGetValue(cohortAddYear, out resorbedNallocation);

            return resorbedNallocation;
        }

        //---------------------------------------------------------------------
        // Method for setting the available resorbed N for each cohort.
        // Amount of resorbed N must be in units of g N m-2.
        public static void SetResorbedNallocation(ICohort cohort, double resorbedNallocation)
        {
            int currentYear = PlugIn.ModelCore.CurrentTime;
            int successionTime = PlugIn.SuccessionTimeStep;
            int cohortAddYear = currentYear - cohort.Age - currentYear % successionTime;
            Dictionary<int, double> cohortDict;
            double oldResorbedNallocation;

            // If the dictionary entry exists for the cohort, overwrite it:
            if (AvailableN.CohortResorbedNallocation.TryGetValue(cohort.Species.Index, out cohortDict))
                if (cohortDict.TryGetValue(cohortAddYear, out oldResorbedNallocation))
                {
                    CohortResorbedNallocation[cohort.Species.Index][cohortAddYear] = resorbedNallocation;
                    return;
                }

            // If the dictionary does not exist for the cohort, create it:
            Dictionary<int, double> newEntry = new Dictionary<int, double>();
            newEntry.Add(cohortAddYear, resorbedNallocation);

            if (CohortResorbedNallocation.ContainsKey(cohort.Species.Index))
            {
                CohortResorbedNallocation[cohort.Species.Index].Add(cohortAddYear, resorbedNallocation);
            }
            else
            {
                CohortResorbedNallocation.Add(cohort.Species.Index, newEntry);
            }
            return;
        }

        //---------------------------------------------------------------------
        // Method for calculationg how much N should be resorbed.
        // month is only included for logging purposes.
        public static double CalculateResorbedN(ISpecies species, double leafBiomass, int month)
        {
            // Translocated N = Leaf Bioamss * Some percentage of leaf N
            // Leaf N calculate from Leaf CN ratio
            // This means that we will need to adjust the leaf litter CN appropriately.
            // Or should translocated N be calculated from the difference between leaf and litter CN??

            double leafN = leafBiomass * 0.47 / SpeciesData.LeafCN[species];
            double litterN = leafBiomass * 0.47 / SpeciesData.LeafLitterCN[species];

            double resorbedN = leafN - litterN;

            if (OtherData.CalibrateMode && PlugIn.ModelCore.CurrentTime > 0)
            {
                PlugIn.ModelCore.Log.WriteLine("Yr={0},Mo={1}.     leafN={2:0.00}, litterN={3:0.00}, resorbedN={4:0.00}.", PlugIn.ModelCore.CurrentTime, month + 1, leafN, litterN, resorbedN);
            }


            return resorbedN;
        }

        //---------------------------------------------------------------------
        // Method for calculating Mineral N allocation, called from Century.cs Run method before calling Grow
        // Iterates through cohorts, assigning each a portion of mineral N based on fine root biomass.
        public static void CalculateMineralNallocation(Site site)
        {
            // Iterate through the first time, assigning each cohort un un-normalized N multiplier
            double NAllocTotal=0.0;
            foreach (ISpeciesCohorts speciesCohorts in SiteVars.Cohorts[site])
                foreach (ICohort cohort in speciesCohorts)
                {

                    //Nallocation is a measure of how much N a cohort can gather relative to other cohorts
                    double Nallocation = Roots.CalculateFineRoot(cohort.LeafBiomass);
                    NAllocTotal += Nallocation;
                    Dictionary<int,double> newEntry = new Dictionary<int,double>();
                    newEntry.Add(cohort.Age,Nallocation);

                    if (CohortMineralNallocation.ContainsKey(cohort.Species.Index))
                    {
                        CohortMineralNallocation[cohort.Species.Index].Add(cohort.Age,Nallocation);
                    }
                    else
                    {
                        CohortMineralNallocation.Add(cohort.Species.Index,newEntry);
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
                    double Nallocation = CohortMineralNallocation[cohort.Species.Index][cohort.Age];
                    double Nfrac = Nallocation / NAllocTotal;
                    CohortMineralNallocation[cohort.Species.Index][cohort.Age] = Nfrac * availableN;
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

        //---------------------------------------------------------------------
        /// <summary>
        /// Calculates cohort N demand depending upon how much N would be removed through growth (ANPP).
        /// </summary>
        public static double CalculateCohortNDemand(ISpecies species, ActiveSite site, double[] actualANPP)
        {

            if(actualANPP[0] <= 0.0 && actualANPP[1] <= 0.0)
                return 0.0;

            if (SpeciesData.NTolerance[species] == 4)
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
                woodN       = ANPPwood * 0.47  / SpeciesData.WoodCN[species];
                coarseRootN = ANPPcoarseRoot * 0.47  / SpeciesData.CoarseRootCN[species];
            }

            if(actualANPP[1] > 0.0)
            {
                ANPPleaf = actualANPP[1];
                ANPPfineRoot = Roots.CalculateFineRoot(ANPPleaf);
                leafN       = ANPPleaf * 0.47 / SpeciesData.LeafCN[species];
                fineRootN   = ANPPfineRoot * 0.47  / SpeciesData.FineRootCN[species];
            }

            double totalANPP_C = (ANPPleaf + ANPPwood + ANPPcoarseRoot + ANPPfineRoot) * 0.47;
            double Nreduction = leafN + woodN + coarseRootN + fineRootN;

            //PlugIn.ModelCore.Log.WriteLine("ANPPleaf={0:0.0}, ANPPwood={1:0.0}, ANPPcRoot={2:0.0}, ANPPfRoot={3:0.0},", ANPPleaf, ANPPwood, ANPPcoarseRoot, ANPPfineRoot);

            if(Nreduction < 0.0)
            {
                PlugIn.ModelCore.Log.WriteLine("   ERROR:  TotalANPP-C={0:0.00} Nreduction={1:0.00}.", totalANPP_C, Nreduction);
                throw new ApplicationException("Error: N Reduction is < 0.  See AvailableN.cs");
            }

            return Nreduction;
        }

    }
}
