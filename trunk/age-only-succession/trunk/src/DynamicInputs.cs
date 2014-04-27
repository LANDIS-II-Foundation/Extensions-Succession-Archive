//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.Core;
using System.Collections.Generic;
using System.IO;

namespace Landis.Extension.Succession.AgeOnly
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
            foreach(ISpecies species in PlugIn.ModelCore.Species)
            {
                foreach(IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
                {
                    if (!ecoregion.Active)
                        continue;

                    PlugIn.ModelCore.UI.WriteLine("Spp={0}, Eco={1}, Pest={2:0.0}.", species.Name, ecoregion.Name,
                        timestepData[species.Index, ecoregion.Index].ProbEst);

                }
            }

        }
        //---------------------------------------------------------------------
        public static void Initialize(string filename, bool writeOutput)
        {
            PlugIn.ModelCore.UI.WriteLine("   Loading dynamic input data from file \"{0}\" ...", filename);
            DynamicInputsParser parser = new DynamicInputsParser();
            try
            {
                allData = Landis.Data.Load<Dictionary<int, IDynamicInputRecord[,]>>(filename, parser);
            }
            catch (FileNotFoundException)
            {
                string mesg = string.Format("Error: The file {0} does not exist", filename);
                throw new System.ApplicationException(mesg);
            }

            if (!allData.ContainsKey(0))
            {
                string mesg = string.Format("Error: There is no input for timestep zero (and there must be).");
                throw new System.ApplicationException(mesg);
            }


            timestepData = allData[0];
        }
    }

}
