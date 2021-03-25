using FarseerPhysics.Dynamics;
using System;

namespace Jypeli
{
    /// <summary>
    /// Peli, jossa on fysiikan laskenta mukana. Peliin lisätyt <code>PhysicsObject</code>-oliot
    /// käyttäytyvät fysiikan lakien mukaan.
    /// </summary>
    public class PhysicsGame : PhysicsGameBase
    {
        /// <summary>
        /// Käynnissä olevan fysiikkapelin pääolio.
        /// </summary>
        public static new PhysicsGame Instance
        {
            get
            {
                if (Game.Instance == null) throw new InvalidOperationException("Game class is not initialized");
                if (!(Game.Instance is PhysicsGameBase)) throw new InvalidOperationException("Game class is not PhysicsGame");

                return (PhysicsGame)(Game.Instance);
            }
        }

        /// <summary>
        /// Alustaa uuden fysiikkapelin.
        /// </summary>
        public PhysicsGame()
            : base(new World(new Vector(0, 0)))
        {
            FarseerGame = true;
        }
    }
}
