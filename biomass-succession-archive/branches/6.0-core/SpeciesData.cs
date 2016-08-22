//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.SpatialModeling;
using Landis.Library.BiomassCohorts;
using Landis.Core;
using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;
using Landis.Library.Succession;

namespace Landis.Extension.Succession.Biomass
{
    public class SpeciesData
    {

        public static Species.AuxParm<double> WoodyDebrisDecay;
        public static Species.AuxParm<double> LeafLignin;
        public static Species.AuxParm<double> LeafLongevity;
        public static Species.AuxParm<double> MortCurveShapeParm;
        public static Species.AuxParm<double> GrowthCurveShapeParm;

        // Large wood mass in grams per square meter (g C /m2) at which
        // half of the theoretical maximum leaf area (MAXLAI) is achieved.
        //public static Species.AuxParm<int> KLAI;

        // Biomass to leaf area index (LAI) conversion factor for trees.  This is a biome-specific parameters.  Some values that have been used:
        // arctic tundra - 0.008
        // arid savanna/shrubland - 0.007
        // boreal forest - 0.004
        // coniferous/deciduous mix forest - 0.007
        // grassland - 0.008
        // maritime coniferous forest - 0.004
        // temperate coniferous forest - 0.004
        // temperate coniferous savanna - 0.004
        // temperate deciduous savanna - 0.01
        // temperate mixed savanna - 0.007
        // tropical evergreen forest - 0.01
        // tropical savanna - 0.006
        // warm temperate deciduous forest - 0.01
        //public static Species.AuxParm<double> BTOLAI;

        // Maximum Leaf Area Index
        public static Species.AuxParm<double> MAXLAI;

        // Determines forest floor light as a function of total LAI
        public static Species.AuxParm<double> LightExtinctionCoeff;

        // Determines the percent of Max Biomass that corresponds to 100% Max LAI
        public static Species.AuxParm<double> PctBioMaxLAI;

        //  Establishment probability for each species in each ecoregion
        public static Species.AuxParm<Ecoregions.AuxParm<double>> EstablishProbability;

        //  Maximum ANPP for each species in each ecoregion
        public static Species.AuxParm<Ecoregions.AuxParm<int>> ANPP_MAX_Spp;

        //  Maximum possible biomass for each species in each ecoregion
        public static Species.AuxParm<Ecoregions.AuxParm<int>> B_MAX_Spp;

        //---------------------------------------------------------------------
        public static void Initialize(IInputParameters parameters)
        {
            //ChangeParameters(parameters);

            LeafLignin              = parameters.LeafLignin;
            LeafLongevity           = parameters.LeafLongevity;
            MortCurveShapeParm      = parameters.MortCurveShapeParm;
            GrowthCurveShapeParm = parameters.GrowthCurveShapeParm;
            WoodyDebrisDecay = parameters.WoodyDecayRate;

            //KLAI                    = parameters.KLAI;
            //BTOLAI                  = parameters.BTOLAI;
            MAXLAI                  = parameters.MAXLAI;
            LightExtinctionCoeff    = parameters.LightExtinctionCoeff;
            PctBioMaxLAI            = parameters.PctBioMaxLAI;

        }
        //public static void ChangeParameters(DynamicChange.IParameters parameters)

        public static void ChangeDynamicParameters(int year)
        {

            if(DynamicInputs.AllData.ContainsKey(year))
            {

                EstablishProbability = Util.CreateSpeciesEcoregionParm<double>(PlugIn.ModelCore.Species, PlugIn.ModelCore.Ecoregions);
                ANPP_MAX_Spp         = Util.CreateSpeciesEcoregionParm<int>(PlugIn.ModelCore.Species, PlugIn.ModelCore.Ecoregions);
                B_MAX_Spp            = Util.CreateSpeciesEcoregionParm<int>(PlugIn.ModelCore.Species, PlugIn.ModelCore.Ecoregions);


                DynamicInputs.TimestepData = DynamicInputs.AllData[year];

                foreach(ISpecies species in PlugIn.ModelCore.Species)
                {
                    foreach(IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
                    {
                        EstablishProbability[species][ecoregion] = DynamicInputs.TimestepData[species.Index, ecoregion.Index].ProbEst;
                        ANPP_MAX_Spp[species][ecoregion] = DynamicInputs.TimestepData[species.Index, ecoregion.Index].ANPP_MAX_Spp;
                        B_MAX_Spp[species][ecoregion] = DynamicInputs.TimestepData[species.Index, ecoregion.Index].B_MAX_Spp;
                    }
                }

                //if(PlugIn.CalibrateMode)
                //    DynamicInputs.Write();

                //Reproduction.ChangeEstablishProbabilities(Util.ToArray<double>(EstablishProbability));

                EcoregionData.UpdateB_MAX();
            }

        }


    }
}
