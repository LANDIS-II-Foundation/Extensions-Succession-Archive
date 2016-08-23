//  Copyright 2007-2010 Portland State University, Conservation Biology Institute
//  Authors:  Robert M. Scheller

using Landis.RasterIO;

namespace Landis.Biomass.NuCycling.Succession
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
