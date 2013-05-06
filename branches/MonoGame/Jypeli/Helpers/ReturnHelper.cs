﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jypeli
{
    /// <summary>
    /// Apuluokka palautusarvoille.
    /// </summary>
    public static class ReturnHelper
    {
        /// <summary>
        /// Palauttaa listasta ensimmäisen olion, joka ei ole null.
        /// </summary>
        /// <typeparam name="T">Olioiden tyyppi</typeparam>
        /// <param name="list">Lista olioista</param>
        /// <returns>Ensimmäinen ei-null listasta, tai null jos mikään ei täsmää</returns>
        public static T ReturnFirstNotNull<T>(params T[] list) where T : class
        {
            for ( int i = 0; i < list.Length; i++ )
            {
                if ( list[i] != null )
                    return list[i];
            }

            return null;
        }

        public static bool IsNotNull( int howMany, params object[] list )
        {
            int notNull = 0;

            for ( int i = 0; i < list.Length; i++ )
            {
                if ( list[i] != null ) notNull++;
                if ( notNull >= howMany ) return true;
            }

            return false;
        }
    }
}
