//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Srinivas S., Robert M. Scheller, James B. Domingo

using Landis.SpatialModeling;

namespace Landis.Extension.Succession.Biomass
{
    public class UIntPixel : Pixel
    {
        public Band<uint> MapCode  = "The numeric code for each ecoregion";

        public UIntPixel() 
        {
            SetBands(MapCode);
        }
    }
}
