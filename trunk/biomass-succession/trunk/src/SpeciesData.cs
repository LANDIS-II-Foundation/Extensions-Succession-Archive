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

        //  Establishment probability for each species in each ecoregion
        public static Species.AuxParm<Ecoregions.AuxParm<double>> EstablishProbability;
        //  Establishment probability modifier for each species in each ecoregion
        public static Species.AuxParm<Ecoregions.AuxParm<double>> EstablishModifier;

        //  Maximum ANPP for each species in each ecoregion
        public static Species.AuxParm<Ecoregions.AuxParm<int>> ANPP_MAX_Spp;

        //  Maximum possible biomass for each species in each ecoregion
        public static Species.AuxParm<Ecoregions.AuxParm<int>> B_MAX_Spp;

        //---------------------------------------------------------------------
        public static void Initialize(IInputParameters parameters)
        {
            LeafLignin              = parameters.LeafLignin;
            LeafLongevity           = parameters.LeafLongevity;
            MortCurveShapeParm      = parameters.MortCurveShapeParm;
            GrowthCurveShapeParm = parameters.GrowthCurveShapeParm;
            WoodyDebrisDecay = parameters.WoodyDecayRate;


        }

        public static void ChangeDynamicParameters(int year)
        {

            if(DynamicInputs.AllData.ContainsKey(year))
            {

                EstablishProbability = Util.CreateSpeciesEcoregionParm<double>(PlugIn.ModelCore.Species, PlugIn.ModelCore.Ecoregions);
                EstablishModifier = Util.CreateSpeciesEcoregionParm<double>(PlugIn.ModelCore.Species, PlugIn.ModelCore.Ecoregions);
                ANPP_MAX_Spp = Util.CreateSpeciesEcoregionParm<int>(PlugIn.ModelCore.Species, PlugIn.ModelCore.Ecoregions);
                B_MAX_Spp            = Util.CreateSpeciesEcoregionParm<int>(PlugIn.ModelCore.Species, PlugIn.ModelCore.Ecoregions);


                DynamicInputs.TimestepData = DynamicInputs.AllData[year];

                foreach(ISpecies species in PlugIn.ModelCore.Species)
                {
                    foreach(IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
                    {
                        if (!ecoregion.Active)
                            continue;
                        
                        EstablishProbability[species][ecoregion] = DynamicInputs.TimestepData[species.Index, ecoregion.Index].ProbEst;
                        EstablishModifier[species][ecoregion] = 1.0;
                        ANPP_MAX_Spp[species][ecoregion] = DynamicInputs.TimestepData[species.Index, ecoregion.Index].ANPP_MAX_Spp;
                        B_MAX_Spp[species][ecoregion] = DynamicInputs.TimestepData[species.Index, ecoregion.Index].B_MAX_Spp;

                    }
                }

                //if(PlugIn.CalibrateMode)
                //    DynamicInputs.Write();


                EcoregionData.UpdateB_MAX();
            }

        }


    }
}
