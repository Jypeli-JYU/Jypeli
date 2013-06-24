using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jypeli
{
    /// <summary>
    /// Aliohjelmia ja ominaisuuksia, jotka toimivat vain puhelimessa. Voidaan kutsua myös muilla alustoilla,
    /// mutta tällöin mitään ei yksinkertaisesti tapahdu.
    /// </summary>
    public class Phone
    {
        private DisplayOrientation _displayOrientation = DisplayOrientation.Landscape;
        private DisplayResolution _displayResolution = DisplayResolution.Large;

        /// <summary>
        /// Puhelimen näytön tarkkuus.
        /// </summary>
        public DisplayResolution DisplayResolution
        {
            get { return _displayResolution; }
            set { _displayResolution = value; }
        }

        /// <summary>
        /// Puhelimen näytön asemointi.
        /// </summary>
        public DisplayOrientation DisplayOrientation
        {
            get { return _displayOrientation; }
            set { _displayOrientation = value; }
        }

        /// <summary>
        /// Värisyttää puhelinta.
        /// </summary>
        /// <param name="milliSeconds">Värinän kesto millisekunteina.</param>
        public void Vibrate( int milliSeconds )
        {
        }

        /// <summary>
        /// Lopettaa puhelimen värinän.
        /// </summary>
        public void StopVibrating()
        {
        }
    }
}
