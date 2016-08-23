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
    public class SpeciesData
    {
        public static Species.AuxParm<double> WoodyDebrisDecay;
        public static Species.AuxParm<double> LeafFractionC;
        public static Species.AuxParm<double> LeafFractionN;
        public static Species.AuxParm<double> LeafFractionP;
        public static Species.AuxParm<double> LitterFractionC;
        public static Species.AuxParm<double> LitterFractionN;
        public static Species.AuxParm<double> LitterFractionP;
        public static Species.AuxParm<double> WoodFractionC;
        public static Species.AuxParm<double> WoodFractionN;
        public static Species.AuxParm<double> WoodFractionP;
        public static Species.AuxParm<double> FRootFractionC;
        public static Species.AuxParm<double> FRootFractionN;
        public static Species.AuxParm<double> FRootFractionP;
        public static Species.AuxParm<double> LeafLignin;
        public static Species.AuxParm<int> NTolerance;
        //  Leaf longevity for each species
        public static Species.AuxParm<double> LeafLongevity;

        //  Shape parameter for the mortality curve for each species
        public static Species.AuxParm<double> MortCurveShapeParm;

        //  Establishment probability for each species in each ecoregion
        public static Species.AuxParm<Ecoregions.AuxParm<double>> EstablishProbability;

        //  Maximum ANPP for each species in each ecoregion
        public static Species.AuxParm<Ecoregions.AuxParm<int>> ANPP_MAX_Spp;

        //  Maximum possible biomass for each species in each ecoregion
        public static Species.AuxParm<Ecoregions.AuxParm<int>> B_MAX_Spp;

        //  Maximum biomass at any site in each ecoregion
        public static Ecoregions.AuxParm<int> B_MAX;



        //---------------------------------------------------------------------

        public static void Initialize(IInputParameters parameters)
        {
            B_MAX = new Ecoregions.AuxParm<int>(Model.Core.Ecoregions);
            UpdateParameters(parameters);

            WoodFractionC = parameters.WoodFractionC;
            WoodFractionN = parameters.WoodFractionN;
            WoodFractionP = parameters.WoodFractionP;
            LeafFractionC = parameters.LeafFractionC;
            LeafFractionN = parameters.LeafFractionN;
            LeafFractionP = parameters.LeafFractionP;
            LitterFractionC = parameters.LitterFractionC;
            LitterFractionN = parameters.LitterFractionN;
            LitterFractionP = parameters.LitterFractionP;
            FRootFractionC = parameters.FRootFractionC;
            FRootFractionN = parameters.FRootFractionN;
            FRootFractionP = parameters.FRootFractionP;
            LeafLignin = parameters.LeafLignin;
            NTolerance = parameters.NTolerance;
        }

        //---------------------------------------------------------------------

        public static void UpdateParameters(DynamicChange.IInputParameters parameters)
        {
            WoodyDebrisDecay = parameters.WoodyDecayRate;
            LeafLongevity = parameters.LeafLongevity;
            MortCurveShapeParm = parameters.MortCurveShapeParm;
            EstablishProbability = parameters.EstablishProbability;
            ANPP_MAX_Spp = parameters.MaxANPP;
            B_MAX_Spp = parameters.MaxBiomass;

            //  Fill in B_MAX array
            foreach (IEcoregion ecoregion in Model.Core.Ecoregions)
            {
                int largest_Biomass_MAX = 0;
                foreach (ISpecies species in Model.Core.Species)
                    largest_Biomass_MAX = Math.Max(largest_Biomass_MAX, B_MAX_Spp[species][ecoregion]);

                B_MAX[ecoregion] = largest_Biomass_MAX;
            }
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Converts a table indexed by species and ecoregion into a
        /// 2-dimensional array.
        /// </summary>
        public static T[,] ToArray<T>(Species.AuxParm<Ecoregions.AuxParm<T>> table)
        {
            T[,] array = new T[Model.Core.Ecoregions.Count, Model.Core.Species.Count];
            foreach (ISpecies species in Model.Core.Species)
            {
                foreach (IEcoregion ecoregion in Model.Core.Ecoregions)
                {
                    array[ecoregion.Index, species.Index] = table[species][ecoregion];
                }
            }
            return array;
        }
    }
}
