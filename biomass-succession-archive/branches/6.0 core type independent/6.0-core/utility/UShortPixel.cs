//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo

using Wisc.Flel.GeospatialModeling.RasterIO;

namespace Landis.Extension.Succession.Biomass
{
    public class UShortPixel
        : SingleBandPixel<ushort>
    {
        public UShortPixel()
            : base()
        {
        }

        //---------------------------------------------------------------------

        public UShortPixel(ushort band0)
            : base(band0)
        {
        }
    }
}
