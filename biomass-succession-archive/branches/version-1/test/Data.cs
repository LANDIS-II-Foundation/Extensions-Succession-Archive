using Edu.Wisc.Forest.Flel.Util;
using Landis.Ecoregions;
using Landis.Species;
using System.IO;

namespace Landis.Test.Biomass.Succession
{
    public static class Data
    {
        private static NUnitInfo myNUnitInfo = new NUnitInfo();

        //---------------------------------------------------------------------

        public static readonly string Directory = myNUnitInfo.GetDataDir();
        public const string DirPlaceholder = "{data folder}";

        public static string MakeInputPath(string filename)
        {
            return Path.Combine(Directory, filename);
        }

        //---------------------------------------------------------------------

        public static FileLineReader OpenFile(string filename)
        {
            string path = Data.MakeInputPath(filename);
            return Landis.Data.OpenTextFile(path);
        }

        //---------------------------------------------------------------------

        static Data()
        {
            Output.WriteLine("{0} = \"{1}\"", DirPlaceholder, Directory);
        }

        //---------------------------------------------------------------------

        private static TextWriter writer = myNUnitInfo.GetTextWriter();

        public static TextWriter Output
        {
            get {
                return writer;
            }
        }

        //---------------------------------------------------------------------

        private static Species.IDataset speciesDataset;
        private static Ecoregions.IDataset ecoregionDataset;

        //---------------------------------------------------------------------

        public static Species.IDataset SpeciesDataset
        {
            get {
                if (speciesDataset == null)
                    speciesDataset = LoadSpecies();
                return speciesDataset;
            }
        }

        //---------------------------------------------------------------------

        public static Species.IDataset LoadSpecies()
        {
            Species.DatasetParser speciesParser = new Species.DatasetParser();
            FileLineReader reader = OpenFile("Species.txt");
            try {
                return speciesParser.Parse(reader);
            }
            finally {
                reader.Close();
            }
        }

        //---------------------------------------------------------------------

        public static Ecoregions.IDataset EcoregionDataset
        {
            get {
                if (ecoregionDataset == null)
                    ecoregionDataset = LoadEcoregions();
                return ecoregionDataset;
            }
        }

        //---------------------------------------------------------------------

        public static Ecoregions.IDataset LoadEcoregions()
        {
            Ecoregions.DatasetParser ecoregionsParser = new Ecoregions.DatasetParser();
            FileLineReader reader = OpenFile("Ecoregions.txt");
            try {
                return ecoregionsParser.Parse(reader);
            }
            finally {
                reader.Close();
            }
        }
    }
}
