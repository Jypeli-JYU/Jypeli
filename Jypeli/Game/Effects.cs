using System;
using System.Collections.Generic;
using Jypeli.Effects;

namespace Jypeli
{
    public partial class Game
    {
        private static List<Light> lights = new List<Light>();

        /// <summary>
        /// Valoefektit.
        /// </summary>
        internal static List<Light> Lights { get { return lights; } }

        /// <summary>
        /// Tuuli. Vaikuttaa vain efekteihin
        /// </summary>
        public static Vector Wind { get; set; }

        /// <summary>
        /// Lisää valon peliin.
        /// </summary>
        public void Add(Light light)
        {
            if (light == null) throw new NullReferenceException("Tried to add a null light to game");

            lights.Add(light);
        }

        /// <summary>
        /// Poistaa kaikki valoefektit.
        /// </summary>
        public void ClearLights()
        {
            lights.Clear();
        }
    }
}
