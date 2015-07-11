using Landis.RasterIO;

namespace Landis.Biomass.Succession
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
