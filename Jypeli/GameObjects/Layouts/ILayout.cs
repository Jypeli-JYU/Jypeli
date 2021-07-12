#region MIT License
/*
 * Copyright (c) 2009-2012 University of Jyväskylä, Department of Mathematical
 * Information Technology.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
#endregion

/*
 * Authors: Tomi Karppinen, Tero Jäntti
 */

using System.Collections.Generic;
using System.ComponentModel;

namespace Jypeli
{
    /// <summary>
    /// Olion koon asettaminen asettelijan sisällä.
    /// </summary>
    public enum Sizing
    {
        /// <summary>
        /// Olio ei kasva suuremmaksi kuin sen <c>PreferredSize</c>.
        /// </summary>
        FixedSize,

        /// <summary>
        /// Olio käyttää kaiken vapaana olevan tilan ja kutistuu, jos tilaa ei ole tarpeeksi.
        /// </summary>
        Expanding
    }

    /// <summary>
    /// Leveyssuuntainen väli
    /// </summary>
    public class HorizontalSpacer : GameObject
    {
        /// <summary>
        /// Leveyssuuntainen väli
        /// </summary>
        public HorizontalSpacer()
            : base(1, 1)
        {
            Color = Color.Transparent;
            HorizontalSizing = Sizing.Expanding;
            VerticalSizing = Sizing.FixedSize;
        }
    }

    /// <summary>
    /// Pystysuuntainen väli
    /// </summary>
    public class VerticalSpacer : GameObject
    {
        /// <summary>
        /// Pystysuuntainen väli
        /// </summary>
        public VerticalSpacer()
            : base(1, 1)
        {
            Color = Color.Transparent;
            HorizontalSizing = Sizing.FixedSize;
            VerticalSizing = Sizing.Expanding;
        }
    }


    /// <summary>
    /// Rajapinta asettelijalle. Asettelija asettelee widgetin
    /// lapsioliot siten, että ne mahtuvat widgetin sisälle. Asettelija
    /// muuttaa lapsiolioiden kokoa sekä paikkaa. Asettelussa käytetään
    /// hyväksi lapsiolioiden ominaisuuksia <c>PreferredSize</c>,
    /// <c>HorizontalSizing</c> sekä <c>VerticalSizing</c>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface ILayout
    {
        /// <summary>
        /// Asettelijan omistaja
        /// </summary>
        GameObject Parent { get; set; }

        /// <summary>
        /// Kuinka tämän hallitsemat oliot asetellaan sivusuunnassa
        /// </summary>
        Sizing HorizontalSizing { get; }

        /// <summary>
        /// Kuinka tämän hallitsemat oliot asetellaan pystysuunnassa
        /// </summary>
        Sizing VerticalSizing { get; }

        /// <summary>
        /// Koko, jota oliolla tulisi olla asettelijan sisällä. Todellinen koko voi olla
        /// pienempi, jos tilaa ei ole tarpeeksi.
        /// </summary>
        Vector PreferredSize { get; }

        /// <summary>
        /// Yläreunaan jäävä tyhjä tila.
        /// </summary>
        double TopPadding { get; set; }

        /// <summary>
        /// Alareunaan jäävä tyhjä tila.
        /// </summary>
        double BottomPadding { get; set; }

        /// <summary>
        /// Vasempaan reunaan jäävä tyhjä tila.
        /// </summary>
        double LeftPadding { get; set; }

        /// <summary>
        /// Oikeaan reunaan jäävä tyhjä tila.
        /// </summary>
        double RightPadding { get; set; }

        /// <summary>
        /// Päivittää asettelijan koon
        /// </summary>
        /// <param name="objects"></param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        void UpdateSizeHints(IList<GameObject> objects);

        /// <summary>
        /// Päivittää asettelijan olioiden sijainnit
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="maximumSize"></param>

        [EditorBrowsable(EditorBrowsableState.Never)]
        void Update(IList<GameObject> objects, Vector maximumSize);
    }
}
