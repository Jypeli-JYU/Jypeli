﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jypeli
{
    /// <summary>
    /// Lista, joka ilmoittaa muutoksistaan.
    /// </summary>
    /// <typeparam name="T">Listan alkion tyyppi.</typeparam>
    public interface INotifyList<T> : IEnumerable<T>
    {
        /// <summary>
        /// Tapahtuu kun listan sisältö muuttuu.
        /// </summary>
        event Action Changed;
    }
}
