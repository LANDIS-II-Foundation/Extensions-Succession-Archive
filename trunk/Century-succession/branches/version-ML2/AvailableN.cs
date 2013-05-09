
//  Copyright 2007-2010 Portland State University, University of Wisconsin-Madison
//  Author: Robert Scheller, Melissa Lucash, Ben Sulman

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
        public static Dictionary<int, Dictionary<int,double>> CohortMineralNfraction;  //calculated once per year
        public static Dictionary<int, Dictionary<int, double>> CohortMineralNallocation;  //calculated monthly
        public static Dictionary<int, Dictionary<int, double>> CohortResorbedNallocation;

        //---------------------------------------------------------------------
        // Method for retrieving the available resorbed N for each cohort.
        // Return amount of resorbed N in g N m-2.
        public static double GetResorbedNallocation(ICohort cohort)
        {
            //int currentYear = PlugIn.ModelCore.CurrentTime;
            //int successionTime = PlugIn.SuccessionTimeStep;
            int cohortAddYear = GetAddYear(cohort); //currentYear - (cohort.Age - Century.Year) + (CohortBiomass.centuryMonth == 11 ? 1 : 0);
            //PlugIn.ModelCore.UI.WriteLine("GETResorbedNallocation: year={0}, mo={1}, species={2}, cohortAge={3}, cohortAddYear={4}.", PlugIn.ModelCore.CurrentTime, Century.Month, cohort.Species.Name, cohort.Age, cohortAddYear);
            double resorbedNallocation = 0.0;
            Dictionary<int, double> cohortDict;
            
            if (AvailableN.CohortResorbedNallocation.TryGetValue(cohort.Species.Index, out cohortDict))
                cohortDict.TryGetValue(cohortAddYear, out resorbedNallocation);

            //PlugIn.ModelCore.UI.WriteLine("GETResorbedNallocation: year={0}, mo={1}, species={2}, cohortAge={3}, cohortAddYear={4}.", PlugIn.ModelCore.CurrentTime, CohortBiomass.month, cohort.Species.Name, cohort.Age, cohortAddYear);

            return resorbedNallocation;
        }

        //---------------------------------------------------------------------
        // Method for setting the available resorbed N for each cohort.
        // Amount of resorbed N must be in units of g N m-2.
        public static void SetResorbedNallocation(ICohort cohort, double resorbedNallocation)
        {
            //int currentYear = PlugIn.ModelCore.CurrentTime;
            //int cohortAddYear = currentYear - (cohort.Age - Century.Year) + (CohortBiomass.centuryMonth == 11 ? 1 : 0);
            int cohortAddYear = GetAddYear(cohort); 
            //PlugIn.ModelCore.UI.WriteLine("SETResorbedNallocation: year={0}, mo={1}, species={2}, cohortAge={3}, cohortAddYear={4}.", PlugIn.ModelCore.CurrentTime, Century.Month, cohort.Species.Name, cohort.Age, cohortAddYear);
            //int cohortAddYear = currentYear - (cohort.Age - Century.Year) + (CohortBiomass.month == 11 ? 1 : 0);
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
        // Method for calculationg how much N should be resorbed, based the difference in N content between leaves and litterfall;
        // month is only included for logging purposes.
        public static double CalculateResorbedN(ActiveSite site, ISpecies species, double leafBiomass, int month)
        {
           
                double leafN = leafBiomass * 0.47 / SpeciesData.LeafCN[species];
                double litterN = leafBiomass * 0.47 / SpeciesData.LeafLitterCN[species];

                double resorbedN = leafN - litterN;

               //PlugIn.ModelCore.UI.WriteLine("Yr={0},Mo={1}.     leafN={2:0.00}, litterN={3:0.00}, resorbedN={4:0.00}.", PlugIn.ModelCore.CurrentTime, month + 1, leafN, litterN, resorbedN);

                SiteVars.ResorbedN[site] += resorbedN;


                return resorbedN;
           
            
        }   
         

        //---------------------------------------------------------------------
        // Method for calculating Mineral N allocation, called from Century.cs Run method before calling Grow
        // Iterates through cohorts, assigning each a portion of mineral N based on coarse root biomass.  Uses an exponential function to "distribute" 
        // the N more evenly between spp. so that the ones with the most woody biomass don't get all the N (L122).

        public static void CalculateMineralNfraction(Site site)
        {
            AvailableN.CohortMineralNfraction = new Dictionary<int, Dictionary<int, double>>();
            double NAllocTotal = 0.0;
            
            foreach (ISpeciesCohorts speciesCohorts in SiteVars.Cohorts[site])
            {
                foreach (ICohort cohort in speciesCohorts)
                {
                    int cohortAddYear = GetAddYear(cohort); 
                    //PlugIn.ModelCore.UI.WriteLine("CALCMineralNfraction: year={0}, mo={1}, species={2}, cohortAge={3}, cohortAddYear={4}.", PlugIn.ModelCore.CurrentTime, Century.Month, cohort.Species.Name, cohort.Age, cohortAddYear);
                    //int cohortAddYear = currentYear - (cohort.Age - Century.Year) + (CohortBiomass.centuryMonth == 11 ? 1 : 0);

                    //Nallocation is a measure of how much N a cohort can gather relative to other cohorts
                    //double Nallocation = Roots.CalculateFineRoot(cohort.LeafBiomass); 
                    double Nallocation = 1- Math.Exp((-Roots.CalculateCoarseRoot(cohort.WoodBiomass)*0.02));

                    if (Nallocation <= 0.0) //PlugIn.ModelCore.CurrentTime == 0)
                        Nallocation = Math.Max(Nallocation, cohort.WoodBiomass * 0.01);

                    //PlugIn.ModelCore.UI.WriteLine("Species = {0}, Age = {1}, Nallocation = {2}, WoodBiomass = {3}, LeafBioMass = {4}", cohort.Species.Name, cohort.Age, Nallocation, cohort.WoodBiomass, cohort.LeafBiomass);
                    NAllocTotal += Nallocation;
                    Dictionary<int, double> newEntry = new Dictionary<int, double>();
                    newEntry.Add(cohortAddYear, Nallocation);

                    if (CohortMineralNfraction.ContainsKey(cohort.Species.Index))
                    {
                        CohortMineralNfraction[cohort.Species.Index].Add(cohortAddYear, Nallocation);
                    }
                    else
                    {
                        CohortMineralNfraction.Add(cohort.Species.Index, newEntry);
                    }
                }

            }
            
            // Next relativize
            foreach (ISpeciesCohorts speciesCohorts in SiteVars.Cohorts[site])
            {
                //PlugIn.ModelCore.UI.WriteLine(" SpeciesCohorts = {0}", speciesCohorts.Species.Name);
                foreach (ICohort cohort in speciesCohorts)
                {
                    int cohortAddYear = GetAddYear(cohort); 
                    //PlugIn.ModelCore.UI.WriteLine("CALC2MineralNfraction: year={0}, mo={1}, species={2}, cohortAge={3}, cohortAddYear={4}.", PlugIn.ModelCore.CurrentTime, Century.Month, cohort.Species.Name, cohort.Age, cohortAddYear);
                    double Nallocation = CohortMineralNfraction[cohort.Species.Index][cohortAddYear];
                    double relativeNallocation = Nallocation / NAllocTotal;
                    CohortMineralNfraction[cohort.Species.Index][cohortAddYear] = relativeNallocation;

                    //PlugIn.ModelCore.UI.WriteLine("  Nallocation={0:0.00}, NAllocTotal={1:0.00}, relativeNallocation={2:0.00}.", Nallocation, NAllocTotal, relativeNallocation);

                    if (Double.IsNaN(relativeNallocation) || Double.IsNaN(Nallocation) || Double.IsNaN(NAllocTotal))
                    {
                        PlugIn.ModelCore.UI.WriteLine("  N ALLOCATION CALCULATION = NaN!  ");
                        PlugIn.ModelCore.UI.WriteLine("  Nallocation={0:0.00}, NAllocTotal={1:0.00}, relativeNallocation={2:0.00}.", Nallocation, NAllocTotal, relativeNallocation);
                        PlugIn.ModelCore.UI.WriteLine("  Wood={0:0.00}, Leaf={1:0.00}.", cohort.WoodBiomass, cohort.LeafBiomass);
                    }
                    //PlugIn.ModelCore.UI.WriteLine("Yr={0},Mo={1}. MineralNfraction={2:0.00}", PlugIn.ModelCore.CurrentTime, Century.Month, CohortMineralNfraction[cohort.Species.Index][cohortAddYear]);
                }
            }

        }

        // Calculates how much N a cohort gets, based on the amount of N available.

        public static void SetMineralNallocation(Site site)
        {
            AvailableN.CohortMineralNallocation = new Dictionary<int, Dictionary<int, double>>();
            
           double availableN = SiteVars.MineralN[site];  // g/m2
           Math.Max(availableN, 0.01);
                                   
            foreach (ISpeciesCohorts speciesCohorts in SiteVars.Cohorts[site])
            {
                foreach (ICohort cohort in speciesCohorts)
                {
                    int cohortAddYear = GetAddYear(cohort); 
                    if (Century.MonthCnt == 11) 
                        cohortAddYear--;
                    //PlugIn.ModelCore.UI.WriteLine("SETMineralNallocation: year={0}, mo={1}, species={2}, cohortAge={3}, cohortAddYear={4}.", PlugIn.ModelCore.CurrentTime, Century.Month, cohort.Species.Name, cohort.Age, cohortAddYear);

                    double Nfraction = 0.05;  //even a new cohort gets a little love
                    Dictionary<int, double> cohortDict = new Dictionary<int,double>();

                    if (AvailableN.CohortMineralNfraction.TryGetValue(cohort.Species.Index, out cohortDict))
                        cohortDict.TryGetValue(cohortAddYear, out Nfraction);
                    
                    double Nallocation = Nfraction * availableN;
                    //PlugIn.ModelCore.UI.WriteLine("  NallocationlimitedbymineralN={0:0.00}, Nfraction={1:0.00}, availableN={2:0.00}.", Nallocation, Nfraction, availableN);
                   
                    if (Double.IsNaN(Nallocation) || Double.IsNaN(Nfraction) || Double.IsNaN(availableN))
                    {
                        PlugIn.ModelCore.UI.WriteLine("  LIMIT N CALCULATION = NaN!  ");
                        PlugIn.ModelCore.UI.WriteLine("  Nallocation={0:0.00}, Nfraction={1:0.00}, availableN={2:0.00}.", Nallocation, Nfraction, availableN);
                    }

                    Dictionary<int, double> newEntry = new Dictionary<int, double>();
                    newEntry.Add(cohortAddYear, Nallocation);

                    if (CohortMineralNallocation.ContainsKey(cohort.Species.Index))
                    {
                        CohortMineralNallocation[cohort.Species.Index].Add(cohortAddYear, Nallocation);
                    }
                    else
                    {
                        CohortMineralNallocation.Add(cohort.Species.Index, newEntry);
                    }
                }
            }
            /*if (totalNUptake > availableN)
            {
                totalNUptake = availableN;
                //PlugIn.ModelCore.UI.WriteLine("   ERROR:  Total max N uptake = {0:0.000}, availableN = {1:0.000}.", totalNUptake, availableN);
                //throw new ApplicationException("Error: Max N uptake > availableN.  See AvailableN.cs");
            }
            SiteVars.TotalNuptake[site] = totalNUptake;*/
        
           
        }

        //---------------------------------------------------------------------
        // Method for retrieving the available mineral N for each cohort.
        // Return amount of resorbed N in g N m-2.
        public static double GetMineralNallocation(ICohort cohort)
        {
            //int currentYear = PlugIn.ModelCore.CurrentTime;
            //int successionTime = PlugIn.SuccessionTimeStep;
            int cohortAddYear = GetAddYear(cohort); 
            //int cohortAddYear = currentYear - (cohort.Age - Century.Year) + (CohortBiomass.centuryMonth == 11 ? 1 : 0);
            //PlugIn.ModelCore.UI.WriteLine("GETMineralNallocation: year={0}, mo={1}, species={2}, cohortAge={3}, cohortAddYear={4}.", PlugIn.ModelCore.CurrentTime, Century.Month, cohort.Species.Name, cohort.Age, cohortAddYear);
            //int cohortAddYear = currentYear - (cohort.Age - Century.Year) + (CohortBiomass.month == 11 ? 1 : 0);
            double mineralNallocation = 0.0;
            Dictionary<int, double> cohortDict;

            if (AvailableN.CohortMineralNallocation.TryGetValue(cohort.Species.Index, out cohortDict))
                cohortDict.TryGetValue(cohortAddYear, out mineralNallocation);

            return mineralNallocation;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Calculates cohort N demand depending upon how much N would be removed through growth (ANPP) of leaves, wood, coarse roots and fine roots.  
        /// Demand is then used to determine the amount of N that a cohort "wants".
        /// </summary>
        public static double CalculateCohortNDemand(ISpecies species, ActiveSite site, double[] ANPP)
        {

            if(ANPP[0] <= 0.0 && ANPP[1] <= 0.0)
                return 0.0;

            if (SpeciesData.NFixer[species])  // We fix our own N!
                return 0.0;

            double ANPPwood = 0.0;
            double ANPPleaf = 0.0;
            double ANPPcoarseRoot = 0.0;
            double ANPPfineRoot = 0.0;
            double woodN = 0.0;
            double coarseRootN = 0.0;
            double leafN = 0.0;
            double fineRootN = 0.0;

            if(ANPP[0] > 0.0)  // Wood
            {
                ANPPwood = ANPP[0];

                //if (actualANPP.Length > 2)
                //{
                //    ANPPcoarseRoot = actualANPP[2];

                //} else 
                //{
                ANPPcoarseRoot = Roots.CalculateCoarseRoot(ANPPwood);
                //}
                
                woodN       = ANPPwood * 0.47  / SpeciesData.WoodCN[species];
                coarseRootN = ANPPcoarseRoot * 0.47 / FunctionalType.Table[SpeciesData.FuncType[species]].CoarseRootCN;
            }

            if(ANPP[1] > 0.0)  // Leaf
            {
                ANPPleaf = ANPP[1];
                
                //if (actualANPP.Length > 2)
                //{
                //    ANPPfineRoot = actualANPP[3];
                //} else {

                ANPPfineRoot = Roots.CalculateFineRoot(ANPPleaf);
               
                //}
                leafN       = ANPPleaf * 0.47 / SpeciesData.LeafCN[species];
                fineRootN = ANPPfineRoot * 0.47 / FunctionalType.Table[SpeciesData.FuncType[species]].FineRootCN;

            }

            double totalANPP_C = (ANPPleaf + ANPPwood + ANPPcoarseRoot + ANPPfineRoot) * 0.47;
            double Ndemand = leafN + woodN + coarseRootN + fineRootN;

            //PlugIn.ModelCore.UI.WriteLine("ANPPleaf={0:0.0}, ANPPwood={1:0.0}, ANPPcRoot={2:0.0}, ANPPfRoot={3:0.0}, Nreduction={4:0.0},", ANPPleaf, ANPPwood, ANPPcoarseRoot, ANPPfineRoot,Nreduction);

            if(Ndemand < 0.0)
            {
                PlugIn.ModelCore.UI.WriteLine("   ERROR:  TotalANPP-C={0:0.00} Nreduction={1:0.00}.", totalANPP_C, Ndemand);
                throw new ApplicationException("Error: N Reduction is < 0.  See AvailableN.cs");
            }

            return Ndemand;
        }

        private static int GetAddYear(ICohort cohort)
        {
            int currentYear = PlugIn.ModelCore.CurrentTime;
            int cohortAddYear = currentYear - (cohort.Age - Century.Year);
            if (Century.MonthCnt == 11)
                cohortAddYear++; 
            return cohortAddYear;
        }


    }
}
