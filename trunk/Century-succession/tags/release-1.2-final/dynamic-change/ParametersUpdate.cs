using Edu.Wisc.Forest.Flel.Util;

namespace Landis.Extension.Succession.Century.Dynamic
{
    /// <summary>
    /// An update of the biomass parameters due to climate change.
    /// </summary>
    public class ParametersUpdate
    {
        private int year;
        private string file;
        private IParameters parameters;

        //---------------------------------------------------------------------

        /// <summary>
        /// The year that the update takes effect.
        /// </summary>
        public int Year
        {
            get {
                return year;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The path to the text file with the updated set of biomass
        /// parameters.
        /// </summary>
        public string File
        {
            get {
                return file;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The set of updated biomass parameters.
        /// </summary>
        /// <remarks>
        /// Initialized to null by the constructor.  The LoadParameters must
        /// be called to read the parameters from the associated file.
        /// </remarks>
        public IParameters Parameters
        {
            get {
                return parameters;
            }
        }

        //---------------------------------------------------------------------

        public ParametersUpdate(int    year,
                                string file)
        {
            this.year = year;
            this.file = file;
            this.parameters = null;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Loads the updated set of biomass parameters from the text file
        /// associated with this update.
        /// </summary>
        /// <param name="parameterParser">
        /// A parser to parse the text file's contents.
        /// </param>
        public void LoadParameters(ITextParser<IParameters> parameterParser)
        {
            parameters = Landis.Data.Load<IParameters>(file, parameterParser);
        }
    }
}
