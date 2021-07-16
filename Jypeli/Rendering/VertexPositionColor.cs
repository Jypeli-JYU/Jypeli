using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Jypeli.Rendering
{
    public struct VertexPositionColor
    {
        public Vector3 Position;

        public float colorR;
        public float colorG;
        public float colorB;
        public float colorA;

        public VertexPositionColor(Vector3 position, Color color)
        {
            // Värit pitää antaa floattina väliltä 0.0 - 1.0
            this.Position = position;
            colorR = color.RedComponent / 255f;
            colorG = color.GreenComponent / 255f;
            colorB = color.BlueComponent / 255f;
            colorA = color.AlphaComponent / 255f;
        }

        public override string ToString()
        {
            return $"Position: {Position} Color: {colorR}, {colorG}, {colorB}, {colorA}";
        }
    }
}
