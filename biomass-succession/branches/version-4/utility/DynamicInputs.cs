//  Copyright 2009 Conservation Biology Institute
//  Authors:  Robert M. Scheller
//  License:  Available at
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using System.Collections.Generic;
using System.IO;
using System;
using Landis.Ecoregions;
using Landis.Species;

namespace Landis.Biomass.Succession
{

    public class DynamicInputs
    {
        private static Dictionary<int, IDynamicInputRecord[,]> allData;
        private static IDynamicInputRecord[,] timestepData;

        public DynamicInputs()
        {
        }

        public static Dictionary<int, IDynamicInputRecord[,]> AllData
        {
            get {
                return allData;
            }
        }
        //---------------------------------------------------------------------
        public static IDynamicInputRecord[,] TimestepData
        {
            get {
                return timestepData;
            }
            set {
                timestepData = value;
            }
        }

        public static void Write()
        {
            foreach(ISpecies species in Model.Core.Species)
            {
                foreach(IEcoregion ecoregion in Model.Core.Ecoregions)
                {
                    UI.WriteLine("Spp={0}, Eco={1}, Pest={2:0.0}, maxANPP={3}, maxB={4}.", species.Name, ecoregion.Name,
                        timestepData[species.Index, ecoregion.Index].ProbEst,
                        timestepData[species.Index, ecoregion.Index].ANPP_MAX_Spp,
                        timestepData[species.Index, ecoregion.Index].B_MAX_Spp);

                }
            }

        }
        //---------------------------------------------------------------------
        public static void Initialize(string filename,
            bool writeOutput)
        {
            UI.WriteLine("Loading dynamic input data from file \"{0}\" ...", filename);
            DynamicInputsParser parser = new DynamicInputsParser();
            allData = Data.Load<Dictionary<int, IDynamicInputRecord[,]>>(filename, parser);

            timestepData = allData[0];
        }
    }

}
