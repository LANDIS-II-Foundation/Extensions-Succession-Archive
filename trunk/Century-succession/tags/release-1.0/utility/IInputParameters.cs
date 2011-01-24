//  Copyright 2009 Conservation Biology Institute
//  Authors:  Robert M. Scheller
//  License:  Available at  
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Landis.Succession;
using Edu.Wisc.Forest.Flel.Util;
using System.Collections.Generic;

namespace Landis.Extension.Succession.Century
{
    /// <summary>
    /// The parameters for biomass succession.
    /// </summary>
    public interface IParameters
        : Dynamic.IParameters
    {
        int Timestep{ get;set;}
        SeedingAlgorithms SeedAlgorithm{ get;set;}
        string ClimateFile { get;set;}
        double SpinupMortalityFraction { get; set; }
        bool CalibrateMode { get; set; }
        WaterType WType {get;set;}
        double LigninDecayEffect { get; set; }

        //---------------------------------------------------------------------
        /// <summary>
        /// A suite of parameters for species functional groups
        /// </summary>
        FunctionalTypeTable FunctionalTypes
        {
            get;set;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// Parameters for fire effects on wood and leaf litter
        /// </summary>
        FireReductions[] FireReductionsTable
        {
            get;set;
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// The maximum relative biomass for each shade class.
        /// </summary>
        Ecoregions.AuxParm<Percentage>[] MinRelativeBiomass
        {
            get;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Definitions of sufficient light probabilities.
        /// </summary>
        List<ISufficientLight> LightClassProbabilities
        {
            get;
        }

        //---------------------------------------------------------------------

        Species.AuxParm<int> SppFunctionalType{get;}
        Species.AuxParm<int> NTolerance{get;}
        Species.AuxParm<int> GDDmin{get;}
        Species.AuxParm<int> GDDmax{get;}
        Species.AuxParm<int> MinJanTemp{get;}
        Species.AuxParm<double> MaxDrought{get;}
        Species.AuxParm<double> LeafLongevity {get;}
        Species.AuxParm<bool> Epicormic {get;}
        Species.AuxParm<double> LeafLignin {get;}
        Species.AuxParm<double> WoodLignin {get;}
        Species.AuxParm<double> CoarseRootLignin {get;}
        Species.AuxParm<double> FineRootLignin {get;}
        Species.AuxParm<double> LeafCN {get;}
        Species.AuxParm<double> WoodCN {get;}
        Species.AuxParm<double> CoarseRootCN {get;}
        Species.AuxParm<double> FoliageLitterCN {get;}
        Species.AuxParm<double> FineRootLitterCN {get;}

        Ecoregions.AuxParm<double> PercentClay {get;}
        Ecoregions.AuxParm<double> PercentSand {get;}
        Ecoregions.AuxParm<int>    SoilDepth {get;}
        Ecoregions.AuxParm<double> FieldCapacity {get;}
        Ecoregions.AuxParm<double> WiltingPoint {get;}
        Ecoregions.AuxParm<double> StormFlowFraction {get;}
        Ecoregions.AuxParm<double> BaseFlowFraction {get;}
        Ecoregions.AuxParm<double> Drain {get;}
        //Ecoregions.AuxParm<double> MonthlyNDeposition {get;}
        Ecoregions.AuxParm<double> AtmosNslope {get;}
        Ecoregions.AuxParm<double> AtmosNintercept {get;}
        Ecoregions.AuxParm<double> Latitude {get;}

        Ecoregions.AuxParm<double> InitialSOM1surfC {get;}
        Ecoregions.AuxParm<double> InitialSOM1surfN {get;}
        Ecoregions.AuxParm<double> InitialSOM1soilC {get;}
        Ecoregions.AuxParm<double> InitialSOM1soilN {get;}
        Ecoregions.AuxParm<double> InitialSOM2C {get;}
        Ecoregions.AuxParm<double> InitialSOM2N {get;}
        Ecoregions.AuxParm<double> InitialSOM3C {get;}
        Ecoregions.AuxParm<double> InitialSOM3N {get;}
        Ecoregions.AuxParm<double> InitialMineralN {get;}


        //---------------------------------------------------------------------

        /// <summary>
        /// Path to the optional file with the biomass parameters for age-only
        /// disturbances.
        /// </summary>
        string AgeOnlyDisturbanceParms
        {
            get;set;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// A list of zero or more updates to the biomass parameters because of
        /// climate change.
        /// </summary>
        List<Dynamic.ParametersUpdate> DynamicUpdates
        {
            get;
        }
    }
}
