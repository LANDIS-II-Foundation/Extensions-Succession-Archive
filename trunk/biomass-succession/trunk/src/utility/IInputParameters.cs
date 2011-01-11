//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.Library.Succession;
using Landis.Core;

using Edu.Wisc.Forest.Flel.Util;

using System.Collections.Generic;
using System.Diagnostics;

namespace Landis.Extension.Succession.Biomass
{
    /// <summary>
    /// The parameters for biomass succession.
    /// </summary>
    public interface IInputParameters
    {
        int Timestep {get; set;}
        SeedingAlgorithms SeedAlgorithm {get; set;}
        //---------------------------------------------------------------------

        InputValue<string> InitialCommunities
        {
            get;
            set;
        }

        //---------------------------------------------------------------------

        InputValue<string> InitialCommunitiesMap
        {
            get;
            set;
        }
        bool CalibrateMode { get; set; }
        double SpinupMortalityFraction {get; set;}
        List<ISufficientLight> LightClassProbabilities {get; set;}
        Ecoregions.AuxParm<Percentage>[] MinRelativeBiomass{get;}
        Species.AuxParm<double> LeafLongevity {get; }
        Species.AuxParm<double> WoodyDecayRate {get; }
        Species.AuxParm<double> MortCurveShapeParm {get; }
        Species.AuxParm<double> GrowthCurveShapeParm { get; }
        Species.AuxParm<double> LeafLignin { get; }
        //Species.AuxParm<double> MAXLAI {get;}
        //Species.AuxParm<double> LightExtinctionCoeff {get;}
        //Species.AuxParm<double> PctBioMaxLAI { get;}

        Ecoregions.AuxParm<int> AET {get;}

        //double PctSun1 { get; set;}
        //double PctSun2 { get; set;}
        //double PctSun3 { get; set;}
        //double PctSun4 { get; set;}
        //double PctSun5 { get; set;}

        string DynamicInputFile {get;set;}

        string AgeOnlyDisturbanceParms{get; set;}
    }

    
}
