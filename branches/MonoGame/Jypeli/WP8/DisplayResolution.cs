﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jypeli
{
    /// <summary>
    /// Puhelimen näytön tarkkuus.
    /// </summary>
    public enum DisplayResolution
    {
        /// <summary>
        /// Pieni tarkkuus (WVGA, 400 x 240).
        /// WP7-yhteensopivuustila, ei varsinaisesti paranna suorituskykyä.
        /// </summary>
        Small,

        /// <summary>
        /// Suuri tarkkuus (WVGA, 800 x 480).
        /// Oletus WP8:lla.
        /// </summary>
        Large,

        /// <summary>
        /// HD720-tarkkuus (720p, 1280 x 720).
        /// Ei toimi kaikilla puhelimilla.
        /// </summary>
        HD720
    }
}