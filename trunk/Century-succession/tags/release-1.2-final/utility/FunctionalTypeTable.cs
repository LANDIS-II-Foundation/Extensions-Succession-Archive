//  Copyright 2008 Conservation Biology Institute
//  Authors:  Robert M. Scheller
//  License:  Available at  
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Edu.Wisc.Forest.Flel.Util;


namespace Landis.Extension.Succession.Century
{
    /// <summary>
    /// Definition of a Litter Type.
    /// </summary>
    public class FunctionalTypeTable
        //: IEditable<IFunctionalType[]>
    {

        //private IEditableFunctionalType[] parameters;
        private FunctionalType[] parameters;

       //---------------------------------------------------------------------
        public int Count
        {
            get {
                return parameters.Length;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The parameters for a functional type
        /// </summary>
        public FunctionalType this[int index]
        //public IEditableFunctionalType this[int index]
        {
            get {
                return parameters[index];
            }

            set {
                parameters[index] = value;
            }
        }
        
        
        //---------------------------------------------------------------------

        public FunctionalTypeTable(int index)
        {
            //parameters = new IEditableFunctionalType[index];
            parameters = new FunctionalType[index];
        }

        //---------------------------------------------------------------------
/*        public bool //IsComplete
        {
            get {
                foreach (FunctionalType editableParms in parameters) {
                //foreach (IEditableFunctionalType editableParms in parameters) {
                    if (editableParms != null && !editableParms.IsComplete)
                        return false;
                }
                return true;
            }
        }

        //---------------------------------------------------------------------

        public IFunctionalType[] //GetComplete()
        {
            if (IsComplete) {
                IFunctionalType[] eventParms = new IFunctionalType[parameters.Length];
                for (int i = 0; i < parameters.Length; i++) {
                    //IEditableFunctionalType editableParms = parameters[i];
                    FunctionalType editableParms = parameters[i];
                    if (editableParms != null)
                        eventParms[i] = editableParms.GetComplete();
                    else
                        eventParms[i] = new FunctionalType();
                }
                return eventParms;
            }
            else
                return null;
        }*/

    }
}
