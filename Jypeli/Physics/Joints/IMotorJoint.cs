using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jypeli.Physics.Joints
{
    /// <summary>
    /// Rajapinta liitoksille jotka pitävät sisällään moottorin.
    /// Esimerkiksi Farseerin WheelJoint
    /// </summary>
    public interface IMotorJoint : IAxleJoint
    {
        /// <summary>
        /// Onko liitoksen sisältämä moottori päällä.
        /// </summary>
        public bool MotorEnabled { get; set; }

        /// <summary>
        /// Moottorin suurin pyörimisnopeus.
        /// </summary>
        public double MotorSpeed { get; set; }

        /// <summary>
        /// Vääntömomentti jolla moottori yrittää pyörittää siihen liitettyä kappaletta.
        /// </summary>
        public double MaxMotorTorque { get; set; }

        /// <summary>
        /// Akseli, jonka suhteen liitos joustaa.
        /// <c>Vector.One</c>(oletus) sallii liikkumisen x ja y-akselilla.
        /// <c>Vector.UnitY</c> sallii liikkeen vain Y-akselin suunnassa jne.
        /// </summary>
        public Vector Axis { get; set; }

    }
}
