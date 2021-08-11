using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Keys = Silk.NET.Input.Key;

namespace Jypeli.Controls
{
    /// <summary>
    /// Näppäimistön tilan ylläpito
    /// </summary>
    public unsafe struct KeyboardState
    {
        private fixed bool keys[400];

        internal void SetKeyDown(Keys key)
        {
            keys[(int)key] = true;
        }

        internal void SetKeyUp(Keys key)
        {
            keys[(int)key] = false;
        }

        /// <summary>
        /// Onko annettu näppäin pohjassa
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool IsKeyDown(Key key)
        {
            return keys[(int)key];
        }

        /// <summary>
        /// Onko annettu näppäin ylhäällä
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool IsKeyUp(Key key)
        {
            return !keys[(int)key];
        }
    }
}
