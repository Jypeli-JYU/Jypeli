using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Physics2DDotNet.Shapes;

namespace Jypeli.Physics
{
    /// <summary>
    /// Tasainen tai epätasainen pinta.
    /// </summary>
    public class SimpleSurface : Surface
    {
        double[] heights = null;
        double scale = 1.0;
        private double p;
        private double min;
        private double max;
        private int points;
        private int maxchange;
               
        /// <summary>
        /// Luo tasaisen pinnan.
        /// </summary>
        /// <param name="width">Pinnan leveys</param>
        /// <param name="height">Pinnan korkeus</param>
        public SimpleSurface( double width, double height )
            : base( width, height, Shape.Rectangle )
        {
            MakeStatic();
        }                       

        public double GetGroundHeight( double x )
        {
            return x < Left || x > Right ? Top : Bottom;
        }

        public Vector GetGroundNormal( double x )
        {
            return Vector.UnitY;
        }
    }
}
