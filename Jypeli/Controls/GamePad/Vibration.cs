using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Jypeli.Controls.GamePad
{
    internal class Vibration : Destroyable, Updatable
    {
        public double LifetimeLeft { get; private set; }
        public double leftMotor { get; private set; }
        public double rightMotor { get; private set; }
        public double leftAccel { get; set; }
        public double rightAccel { get; set; }

        public bool IsUpdated { get { return true; } }

        public Vibration( double lmotor, double rmotor, double laccel, double raccel, double life )
        {
            leftMotor = lmotor;
            rightMotor = rmotor;
            leftAccel = laccel;
            rightAccel = raccel;
            LifetimeLeft = life;
        }

        public void Update( Time time )
        {
            if ( LifetimeLeft <= 0 )
            {
                Destroy();
                return;
            }

            // Acceleration
            leftMotor += time.SinceLastUpdate.TotalSeconds * leftAccel;
            rightMotor += time.SinceLastUpdate.TotalSeconds * rightAccel;

            // Lifetime progression
            LifetimeLeft -= time.SinceLastUpdate.TotalSeconds;
        }
        
        public static void Execute( PlayerIndex p, IEnumerable<Vibration> vibrations )
        {
            // Total vibrations
            double lmotort = 0;
            double rmotort = 0;

            foreach ( var v in vibrations )
            {
                lmotort += v.leftMotor;
                rmotort += v.rightMotor;
            }

            // Clamp the results between 0 and 1
            lmotort = AdvanceMath.MathHelper.Clamp( (float)lmotort, 0, 1 );
            rmotort = AdvanceMath.MathHelper.Clamp( (float)rmotort, 0, 1 );

            // Set the vibration
            //XnaGamePad.SetVibration( p, (float)lmotort, (float)rmotort );
            // MonoGame: no support yet
        }

        #region Destroyable Members

        public bool IsDestroyed { get; private set; }

        public event Action Destroyed;
        
        public void Destroy()
        {
            if ( IsDestroyed ) return;
            IsDestroyed = true;
            if ( Destroyed != null )
                Destroyed();
        }

        #endregion
    }
}
