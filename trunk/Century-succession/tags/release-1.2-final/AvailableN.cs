//  Copyright 2009-2010 Portland State University
//  Authors:  Robert M. Scheller
//  License:  Available at
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;
using System.Collections.Generic;
using System;

using Landis.Ecoregions;
using Landis.Landscape;
using Landis.Species;
using Landis.Biomass;


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
        public static double CalculateNLimits(ActiveSite site)
        {
            // Iterate through the first time, assigning each cohort un un-normalized N multiplier
            double NMultTotal=0.0;
            foreach (ISpeciesCohorts speciesCohorts in SiteVars.SiteCohorts[site])
                foreach (ICohort cohort in speciesCohorts)
                {
                            int Ntolerance = SpeciesData.NTolerance[cohort.Species];

                        	//NMultiplier is a measure of how much N a cohort can gather relative to other cohorts
                        	double NMultiplier = CalculateNMultiplier(cohort.Biomass,Ntolerance);
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

            //Console.WriteLine("NMultTotal="+NMultTotal);

            //Iterate through a second time now that we have total N multiplier
            //Divide through by total and multiply by total available N so each cohort has a max N value
            //and the sum of cohort max N values is the site available N
            
            double totalNUptake = 0.0;
            foreach (ISpeciesCohorts speciesCohorts in SiteVars.SiteCohorts[site])
                		foreach (ICohort cohort in speciesCohorts)
                    	{
                    	   double NMultiplier=CohortNlimits[cohort.Species.Index][cohort.Age];
                    	   double Nfrac=NMultiplier/NMultTotal;
                           CohortNlimits[cohort.Species.Index][cohort.Age] = Nfrac *availableN;

                           totalNUptake += Nfrac * availableN;
                           //Console.WriteLine("species={0} age={1} NMult={2:0.00} Nfrac={3:0.0000}",cohort.Species.Name,cohort.Age,NMultiplier,Nfrac);
                    	}

            //Console.WriteLine("Total max N uptake = {0:0.0000}, availableN = {1:0.0000}, availableN-uptake={2:0.0000}", totalNUptake,availableN,availableN-totalNUptake);
            if ((availableN - totalNUptake) < -0.001 * availableN)
            {
                    UI.WriteLine("   ERROR:  Total max N uptake = {0:0.000}, availableN = {1:0.000}.", totalNUptake, availableN);
                    throw new ApplicationException("Error: Max N uptake > availableN.  See AvailableN.cs");
            }

            return 0.0;
        }

        //Calculates a multiplier for how much N a cohort can take up
        //Units are arbitrary, since it all gets normalized later
        //Start with a simple multiplier, so a tree with Ntolerance 3 takes up 3/2 more N than a tree with Ntolerance 2
        private static double CalculateNMultiplier(double biomass, int Ntolerance)
        {
            //return Math.Max(Math.Sqrt(biomass) * Ntolerance,1.0);
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

            //UI.WriteLine("ANPPleaf={0:0.0}, ANPPwood={1:0.0}, ANPPcRoot={2:0.0}, ANPPfRoot={3:0.0},", ANPPleaf, ANPPwood, ANPPcoarseRoot, ANPPfineRoot);

            if(Nreduction < 0.0)
            {
                UI.WriteLine("   ERROR:  TotalANPP-C={0:0.00} Nreduction={1:0.00}.", totalANPP_C, Nreduction);
                throw new ApplicationException("Error: N Reduction is < 0.  See AvailableN.cs");
            }

            if(Nreduction > SiteVars.MineralN[site])
            {
                Nreduction = SiteVars.MineralN[site];
                //double somPotentialUptake = SiteVars.SOM2[site].Nitrogen * 0.1;
                //double missingN = Nreduction - Math.Max(0.0, SiteVars.MineralN[site]);
                //double uptakeN = Math.Min(somPotentialUptake, missingN);

                //SiteVars.SOM2[site].Nitrogen -= uptakeN;
                //Nreduction += uptakeN;
                //UI.WriteLine("  Cohort directly accessing organic N.");

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
        //---------------------------------------------------------------------
        /// <summary>
        /// Reduces growth (ANPP) depending upon how much N is available.
        /// </summary>
        /*public static double GrowthReductionAvailableN(ActiveSite site, ISpecies species)
        {
            double availableN = SiteVars.MineralN[site] * 10.0 * 0.75;  // units to kg/ha to match the original equations
            //double availableN = SiteVars.PlantAvailableN[site] * 10.0;  // units to kg/ha to match the original equations


            //availableN += SiteVars.SOM2[site].Nitrogen * 0.1 * 10.0;

            int Ntolerance = SpeciesData.NTolerance[species];

            if (Ntolerance == 4)
                return 1.0;

            if(availableN <= 0.0)
                return 0.0;



            //Calc species soil N growth multiplier
            // Mitchell and Chandler. 1939. Black Rock Forest Bull. 11,
            // Aber et al. 1979. Can. J. For. Res. 9:10 - 14.
            double a = 0.0;
            double b = 0.0;
            double c = 0.0;
            double d = 0.0;
            double e = 0.0;
            double soilNitrogenMultiplier = 0.0;

            double availMC = -170.0 + 4.0 * availableN;

            if(Ntolerance == 1)  //Intolerant to low nitrogen
            {
                a = 2.99;
                b = 207.43;
                c = 0.00175;
                d = -1.7;
                e = 1.0;
            } else if (Ntolerance == 2) //Mid-tolerant of low nitrogen
            {
                a = 2.94;
                b = 117.52;
                c = 0.00234;
                d = -0.5;
                e = 0.5;
            } else if (Ntolerance == 3) //Tolerant of low nitrogen
            {
                a = 2.79;
                b = 219.77;
                c = 0.00179;
                d = -0.3;
                e = 0.6;
            } else if (Ntolerance >= 4) //Not at all limited by nitrogen
            {
                //Needs further review: NTolerance = 4, 5, or 6 means N-fixer
                //Adds N to the soil from nowhere--value needs to be scaled
                //SiteVars.MineralSoil[site].ContentN += 5;

                soilNitrogenMultiplier = 1.0;
            } else
            {
                UI.WriteLine("Species = {0}.  Ntolerance = {1}.", species.Name, Ntolerance);
                throw new System.ApplicationException("Error: Incorrect N tolerance value .");
            }

            //concNinLeaves = percent N in green leaves
            double concNinLeaves = a * (1.0 - System.Math.Pow(10.0, ((-1.0 * c) * (availMC + b))));

            // Limit concentration to +/- 20% of input leaf N concentration?
            // Testing reveals that this results in a greatly heightened mineralN and totalN
            // And this generally ignores available N and causes N tolerant species to have a lower
            // N limit..
            bool limitLeafNconcentration = false;
            if (limitLeafNconcentration)
            {
                double potentialLeafN = 1.0 / (SpeciesData.LeafCN[species] * 2.0) * 100.0;
                //UI.WriteLine("potentialLeafN={0},", potentialLeafN);
                concNinLeaves = Math.Max(concNinLeaves, potentialLeafN * 0.8);
                concNinLeaves = Math.Min(concNinLeaves, potentialLeafN * 1.2);
            }


            soilNitrogenMultiplier = d + (e * concNinLeaves); //(3) Aber 1979

            //UI.WriteLine("   Yr={0}. Spp={1}, Nx={2:0.00}, availN={3:0.00}, leafN={4}", Model.Core.CurrentTime, species.Name, soilNitrogenMultiplier, availableN, concNinLeaves);
            //Changing min Nmultiplier to 0.05 from 0.0, to allow some growth of N-intolerant trees --bsulman
            soilNitrogenMultiplier = Math.Min(1.0, soilNitrogenMultiplier);
            soilNitrogenMultiplier = Math.Max(0.05, soilNitrogenMultiplier);

            return soilNitrogenMultiplier;
        }*/

    }
}
