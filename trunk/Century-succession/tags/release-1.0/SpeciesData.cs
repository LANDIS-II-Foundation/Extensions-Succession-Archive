//  Copyright 2008 Conservation Biology Institute
//  Authors:  Robert M. Scheller
//  License:  Available at  
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;
using System;
using System.IO;
using Landis.Succession;

using Landis.Ecoregions;
using Landis.Landscape;
using Landis.Species;
using Landis.Library.Climate;

namespace Landis.Extension.Succession.Century
{
    public class SpeciesData
    {

        public static Species.AuxParm<int> FuncType;
        public static Species.AuxParm<int> NTolerance;
        public static Species.AuxParm<int> GDDmin;
        public static Species.AuxParm<int> GDDmax;
        public static Species.AuxParm<int> MinJanTemp;
        public static Species.AuxParm<double> MaxDrought;
        public static Species.AuxParm<double> LeafLongevity;
        public static Species.AuxParm<bool> Epicormic;
        public static Species.AuxParm<double> LeafLignin;
        public static Species.AuxParm<double> WoodLignin;
        public static Species.AuxParm<double> CoarseRootLignin;
        public static Species.AuxParm<double> FineRootLignin;
        public static Species.AuxParm<double> LeafCN;
        public static Species.AuxParm<double> WoodCN;
        public static Species.AuxParm<double> CoarseRootCN;
        public static Species.AuxParm<double> LeafLitterCN;
        public static Species.AuxParm<double> FineRootLitterCN;
        //public static Species.AuxParm<double> NLimits;

        public static Species.AuxParm<Ecoregions.AuxParm<double>> EstablishProbability;
        public static Species.AuxParm<Ecoregions.AuxParm<int>> ANPP_MAX_Spp;
        public static Species.AuxParm<Ecoregions.AuxParm<int>> B_MAX_Spp;
        
        //private static StreamWriter log;

        //---------------------------------------------------------------------
        public static void Initialize(IParameters parameters)
        {
            FuncType            = parameters.SppFunctionalType;
            NTolerance          = parameters.NTolerance;
            GDDmin              = parameters.GDDmin;
            GDDmax              = parameters.GDDmax;
            MinJanTemp          = parameters.MinJanTemp;
            MaxDrought          = parameters.MaxDrought;
            LeafLongevity       = parameters.LeafLongevity;
            Epicormic           = parameters.Epicormic;
            LeafLignin          = parameters.LeafLignin;
            WoodLignin          = parameters.WoodLignin ;
            CoarseRootLignin    = parameters.CoarseRootLignin ;
            FineRootLignin      = parameters.FineRootLignin ;
            LeafCN              = parameters.LeafCN;
            WoodCN              = parameters.WoodCN;
            CoarseRootCN        = parameters.CoarseRootCN;
            LeafLitterCN        = parameters.FoliageLitterCN;
            FineRootLitterCN    = parameters.FineRootLitterCN;
            //NLimits = new Species.AuxParm<double>(Model.Core.Species);
            
            Establishment.Initialize();
            
            // The initial set of establishment probabilities:
            EstablishProbability = Establishment.GenerateNewEstablishProbabilities(parameters.Timestep);  
            Reproduction.ChangeEstablishProbabilities(Util.ToArray<double>(SpeciesData.EstablishProbability));

            
            ChangeParameters(parameters);
            
            
        }
        
        public static void ChangeParameters(Dynamic.IParameters parameters)
        {
            ANPP_MAX_Spp  = parameters.MaxANPP;
            B_MAX_Spp     = parameters.MaxBiomass;
        }

        /*public static void CalculateNGrowthLimits(ActiveSite site)
        {
            foreach(ISpecies species in Model.Core.Species)
            {
                NLimits[species] = AvailableN.GrowthReductionAvailableN(site, species);
            }
        }*/
        
    }
}
