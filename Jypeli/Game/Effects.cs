using System;
using System.Collections.Generic;
using Jypeli.Effects;

namespace Jypeli
{
    public partial class Game
    {
        private static List<Light> lights = new List<Light>();

#if !WINDOWS_PHONE
        /// <summary>
        /// Valoefektit.
        /// </summary>
        internal static List<Light> Lights { get { return lights; } }
#endif

        /// <summary>
        /// Tuuli. Vaikuttaa vain efekteihin
        /// </summary>
        public static Vector Wind { get; set; }

        /// <summary>
        /// Lisää valon peliin. Nykyisellään valoja voi olla ainoastaan
        /// yksi kappale. Toistaiseksi ei tuettu Windows Phonella.
        /// </summary>
        public void Add(Light light)
        {
#if !WINDOWS_PHONE
            if (light == null) throw new NullReferenceException("Tried to add a null light to game");

            if (lights.Count >= 1)
                throw new NotSupportedException("Only one light is supported");

            lights.Add(light);
#endif
        }

        /// <summary>
        /// Poistaa kaikki valoefektit.
        /// </summary>
        public void ClearLights()
        {
#if !WINDOWS_PHONE

            lights.Clear();
#endif
        }
    }
}
