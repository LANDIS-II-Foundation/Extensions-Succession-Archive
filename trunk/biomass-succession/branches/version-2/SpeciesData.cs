//  Copyright 2007 University of Nevada, University of Wisconsin
//  Authors:  Sarah Ganschow, Robert M. Scheller, James B. Domingo
//  License:  Available at
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;
using System;

using Landis.Ecoregions;
using Landis.Landscape;
using Landis.Species;

namespace Landis.Biomass.Succession
{
    public class SpeciesData
    {

        public static Species.AuxParm<double> WoodyDebrisDecay;
        public static Species.AuxParm<double> LeafLignin;
        public static Species.AuxParm<double> LeafLongevity;

        //  Shape parameter for the mortality curve for each species
        public static Species.AuxParm<double> MortCurveShapeParm;
        public static Species.AuxParm<double> GrowthCurveShapeParm;

        //  Establishment probability for each species in each ecoregion
        public static Species.AuxParm<Ecoregions.AuxParm<double>> EstablishProbability;

        //  Maximum ANPP for each species in each ecoregion
        public static Species.AuxParm<Ecoregions.AuxParm<int>> ANPP_MAX_Spp;

        //  Maximum possible biomass for each species in each ecoregion
        public static Species.AuxParm<Ecoregions.AuxParm<int>> B_MAX_Spp;

        //---------------------------------------------------------------------
        public static void Initialize(IParameters parameters)
        {
            ChangeParameters(parameters);

            LeafLignin = parameters.LeafLignin;
            LeafLongevity = parameters.LeafLongevity;
            MortCurveShapeParm = parameters.MortCurveShapeParm;
            GrowthCurveShapeParm = parameters.GrowthCurveShapeParm;

        }
        public static void ChangeParameters(ClimateChange.IParameters parameters)
        {
            WoodyDebrisDecay        = parameters.WoodyDecayRate;
            EstablishProbability    = parameters.EstablishProbability;
            ANPP_MAX_Spp            = parameters.MaxANPP;
            B_MAX_Spp               = parameters.MaxBiomass;

        }


    }
}
