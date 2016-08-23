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
    /// Methods for the relationships between available nitrogen and phosphorus and cohort growth.
    /// </summary>
    public class AvailableNuts
    {
        /// <summary>
        /// Reduces available N according to the amount of N necessary for growth (i.e., ANPP).
        /// </summary>
        public static double CohortUptakeAvailableN(double leafANPP,
                                                    double woodANPP,
                                                    ActiveSite site,
                                                    ISpecies species)
        {
            double leafLongevity = SpeciesData.LeafLongevity[species];
            double coarseRootANPP = Roots.CalculateCoarseRoot(woodANPP, leafLongevity);
            double fineRootANPP = Roots.CalculateFineRoot(leafANPP, leafLongevity);

            //Leaf nitrogen: leafANPP is multiplied by litter fraction N assuming
            //  the difference between leaf and litter N is translocated and
            //  cycled within the cohort.
            double leafN = leafANPP * SpeciesData.LeafFractionN[species];
            double woodN = woodANPP * SpeciesData.WoodFractionN[species];
            double coarseRootN = coarseRootANPP * SpeciesData.WoodFractionN[species];
            double fineRootN = fineRootANPP * SpeciesData.FRootFractionN[species];

            double Nreduction = leafN + woodN + coarseRootN + fineRootN;
            Nreduction = Math.Max(Nreduction, 0.0);

            return Nreduction;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Reduces available P according to the amount of P necessary for growth (i.e., ANPP).
        /// </summary>
        public static void CohortUptakeAvailableP(double leafANPP,
                                                  double woodANPP,
                                                  //double leafLongevity,
                                                  ActiveSite site,
                                                  ISpecies species,
                                                  MineralSoil mineralSoil)
        {
            double leafLongevity = SpeciesData.LeafLongevity[species];
            double coarseRootANPP = Roots.CalculateCoarseRoot(woodANPP, leafLongevity);
            double fineRootANPP = Roots.CalculateFineRoot(leafANPP, leafLongevity);

            //Leaf phosphorus: leafANPP is multiplied by litter fraction P
            //  assuming the difference between leaf and litter P is
            //  translocated and cycled within the cohort.
            double leafP = leafANPP * SpeciesData.LeafFractionP[species];
            double woodP = woodANPP * SpeciesData.WoodFractionP[species];
            double coarseRootP = coarseRootANPP * SpeciesData.WoodFractionP[species];
            double fineRootP = fineRootANPP * SpeciesData.FRootFractionP[species];

            double Preduction = leafP + woodP + coarseRootP + fineRootP;
            Preduction = Math.Max(Preduction, 0.0);

            mineralSoil.ContentP -= Preduction;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Reduces growth (ANPP) depending upon how much N is available.
        /// </summary>
        public static double GrowthReductionAvailableN(ActiveSite site,
                                                       ISpecies species,
                                                       double availableN)
        {
            int Ntolerance = SpeciesData.NTolerance[species];

            //Calc species soil N growth multiplier (Mitchell and Chandler
            //1939). Black Rock Forest Bull. 11. Aber et al. 1979. Can. J. For.
            //Res. 9:10 - 14.
            double a, b, c, d, e;
            double availMC = -170.0 + 4.0 * availableN;
            double soilNitrogenMultiplier = 0.0;

            //Not at all limited by nitrogen.
            if (Ntolerance >= 4)
                return 1.0;

            //Intolerant to low nitrogen.
            if (Ntolerance == 1)
            {
                a = 2.99;
                b = 207.43;
                c = 0.00175;
                d = -1.7;
                e = 1.0;
            }

            //Mid-tolerant of low nitrogen.
            else if (Ntolerance == 2)
            {
                a = 2.94;
                b = 117.52;
                c = 0.00234;
                d = -0.5;
                e = 0.5;
            }

            //Tolerant of low nitrogen.
            else if (Ntolerance == 3)
            {
                a = 2.79;
                b = 219.77;
                c = 0.00179;
                d = -0.3;
                e = 0.6;
            }

            else
                throw new System.ApplicationException("Error: Incorrect N tolerance value.");

            //Calculate scaled percent N in green leaves and soil nitrogen multiplier.
            //Equation 3 from Aber et al. 1979.
            double concNinLeaves = a * (1.0 - Math.Pow(10.0, ((-1.0 * c) * (availMC + b))));
            soilNitrogenMultiplier = d + (e * concNinLeaves);

            soilNitrogenMultiplier = Math.Min(1.0, soilNitrogenMultiplier);
            soilNitrogenMultiplier = Math.Max(0.0, soilNitrogenMultiplier);

            return soilNitrogenMultiplier;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Calculates available nitrogen from external and internal sources.
        /// </summary>
        public static double GetTransN(ISpecies species,
                                       double annualLeafTurnover,
                                       double leafMortality)
        {
            double transN = (annualLeafTurnover + leafMortality) * (SpeciesData.LeafFractionN[species] -
                SpeciesData.LitterFractionN[species]);
            return transN;
        }
    }
}
