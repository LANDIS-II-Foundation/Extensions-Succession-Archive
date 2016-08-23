//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.SpatialModeling;
//using Landis.Library.BiomassCohorts;
using Landis.Core;
using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;
using Landis.Library.Succession;

namespace Landis.Extension.Succession.AgeOnly
{
    public class SpeciesData
    {

        //  Establishment probability for each species in each ecoregion
        public static Species.AuxParm<Ecoregions.AuxParm<double>> EstablishProbability;

        public static void ChangeDynamicParameters(int year)
        {

            if(DynamicInputs.AllData.ContainsKey(year))
            {

                EstablishProbability = CreateSpeciesEcoregionParm<double>(PlugIn.ModelCore.Species, PlugIn.ModelCore.Ecoregions);

                DynamicInputs.TimestepData = DynamicInputs.AllData[year];

                foreach(ISpecies species in PlugIn.ModelCore.Species)
                {
                    foreach(IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
                    {
                        if (!ecoregion.Active)
                            continue;

                        try
                        {
                            EstablishProbability[species][ecoregion] = DynamicInputs.TimestepData[species.Index, ecoregion.Index].ProbEst;
                        }
                        catch (System.NullReferenceException)
                        {
                            string mesg = string.Format("Cannot find data for Spp={0}, Eco={1}, Year={2}.", species.Name, ecoregion.Name, year);
                            throw new System.ApplicationException(mesg);

                        }
                    }
                }

                //EcoregionData.UpdateB_MAX();
            }

        }

        public static Species.AuxParm<Ecoregions.AuxParm<T>> CreateSpeciesEcoregionParm<T>(ISpeciesDataset speciesDataset, IEcoregionDataset ecoregionDataset)
        {
            Species.AuxParm<Ecoregions.AuxParm<T>> newParm;
            newParm = new Species.AuxParm<Ecoregions.AuxParm<T>>(speciesDataset);
            foreach (ISpecies species in speciesDataset)
            {
                newParm[species] = new Ecoregions.AuxParm<T>(ecoregionDataset);
            }
            return newParm;
        }

    }
}
