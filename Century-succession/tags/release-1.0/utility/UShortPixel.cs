//  Copyright 2007 Conservation Biology Institute
//  Authors:  Robert M. Scheller
//  License:  Available at  
//  http://www.landis-ii.org/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Landis.RasterIO;

namespace Landis.Extension.Succession.Century
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
