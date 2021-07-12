#region MIT License
/*
 * Copyright (c) 2009 University of Jyväskylä, Department of Mathematical
 * Information Technology.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
#endregion

/*
 * Authors: Vesa Lappalainen, Tero Jäntti, Tomi Karppinen, Janne Nikkanen.
 */

using System;
using System.Collections.Generic;

namespace Jypeli.Assets
{
    /// <summary>
    /// Yksinkertainen tankki eli panssarivaunu.
    /// </summary>
    public class Tank : PhysicsObject
    {
        private static Image commonImage = null;
        private static Shape commonShape = null;

        private Cannon cannon;
        private List<PhysicsObject> wheels = new List<PhysicsObject>();
        private List<IAxleJoint> joints = new List<IAxleJoint>();
        //private IntMeter ammo = new IntMeter( 10 );
        private IntMeter hitPoints = new IntMeter( 10 );

        /// <summary>
        /// Tankin koko. Tätä ei voi muuttaa.
        /// </summary>
        public override Vector Size
        {
            get { return base.Size; }
            set { throw new NotImplementedException( "The size of the tank can not be changed." ); }
        }

        /// <summary>
        /// Tankin osumapisteet.
        /// Kun nämä menevät nollaan, tankki hajoaa.
        /// </summary>
        public IntMeter HitPoints
        {
            get { return hitPoints; }
        }

        /// <summary>
        /// Ammusten määrä.
        /// </summary>
        public IntMeter Ammo { get { return Cannon.Ammo; } }

        /// <summary>
        /// Tankin piippu.
        /// </summary>
        public Cannon Cannon { get { return cannon; } }


        /// <summary>
        /// Alustaa uuden tankin.
        /// </summary>
        public Tank( double width, double height )
            : base( width, height )
        {
            if ( commonImage == null )
                commonImage = Game.LoadImageFromResources( "Tank.png" );
            if ( commonShape == null )
                commonShape = Shape.FromImage(commonImage);
            Image = commonImage;
            Shape = commonShape;
            HitPoints.LowerLimit += Break;
            CollisionIgnorer = new ObjectIgnorer();

            cannon = new Cannon( Width * 0.75, Height * 0.2 );
            Cannon.Position = new Vector(0, Height * 0.25);
            Cannon.TimeBetweenUse = TimeSpan.FromSeconds( 0.5 );
            Cannon.Ammo.Value = 100;
            this.Add( Cannon );

            AddedToGame += AddWheels;
        }

        private void AddWheels()
        {
            PhysicsGameBase pg = Game.Instance as PhysicsGameBase;
            if ( pg == null ) throw new InvalidOperationException( "Cannot have a tank in non-physics game" );

            const int wheelCount = 6;

            double r = this.Width / ( 2 * wheelCount );
            double left = this.X - this.Width / 2 + r;
            double[] wheelYPositions = new double[wheelCount];
            for ( int i = 0; i < wheelYPositions.Length; i++ )
                wheelYPositions[i] = this.Y - this.Height / 2;
            wheelYPositions[0] = wheelYPositions[wheelCount - 1] = this.Position.Y - ( this.Height * 3 / 8 );

            for ( int i = 0; i < wheelCount; i++ )
            {
                PhysicsObject wheel = new PhysicsObject( 2 * r, 2 * r, Shape.Circle );
                wheel.Color = Color.Gray;
                wheel.CollisionIgnorer = this.CollisionIgnorer;
                wheels.Add( wheel );
                pg.Add( wheel );
                Vector axlePos = new Vector(left + i * (this.Width / wheelCount), wheelYPositions[i]);
                wheel.Position = axlePos;
                wheel.Mass = this.Mass / 20;
                wheel.KineticFriction = 1.0;

                if (pg.FarseerGame)
                {
                    IAxleJoint joint = pg.Engine.CreateJoint(this, wheel, JointTypes.WheelJoint);
                    
                    // TODO: Näille voisi lisäillä propertyt
                    Type type = joint.GetType();
                    type.GetProperty("Softness").SetMethod.Invoke(joint, new object[] { this.Mass/6 });
                    type.GetProperty("MotorEnabled").SetMethod.Invoke(joint, new object[] { true });
                    type.GetProperty("MaxMotorTorque").SetMethod.Invoke(joint, new object[] { 1000 });
                    type.GetProperty("Axis").SetMethod.Invoke(joint, new object[] { Vector.UnitY });

                    joints.Add(joint);
                }
                else
                {
                    wheel.Body.AngularDamping = 0.95f;
                    IAxleJoint joint = pg.Engine.CreateJoint(this, wheel, new Vector(axlePos.X, axlePos.Y));
                    joint.Softness = 0.01f;
                    joints.Add(joint);
                    pg.Add(joint);
                }
            }
        }

        /// <inheritdoc/>
        public override void Destroy()
        {
            foreach ( var j in joints )
                j.Destroy();
            foreach ( var w in wheels )
                w.Destroy();
            Cannon.Destroy();
            base.Destroy();
        }

        private void Break()
        {
            Explosion rajahdys = new Explosion( 150 );
            rajahdys.Force = 10;
            rajahdys.Position = Position;
            Game.Instance.Add( rajahdys );

            this.Destroy();
        }

        /// <summary>
        /// Kiihdyttää tankkia.
        /// </summary>
        /// <param name="power">
        ///     Physics2d: Teho välillä <c>-1.0</c>-<c>1.0</c>
        ///     Farseer: Renkaiden pyörimisnopeus, radiaaneina sekunnissa.
        /// </param>
        public void Accelerate( double power )
        {
            if (PhysicsGameBase.Instance.FarseerGame)
            {
                //TODO: Tähän voisi joskus pohtia paremman ratkaisun farseerilla.
                Type type = joints[0].GetType();
                System.Reflection.PropertyInfo pi = type.GetProperty("MotorSpeed");
                foreach (var j in joints)
                {
                    pi.SetMethod.Invoke(j, new object[] { power });
                } 
            }
            else
            {
                double realPower = power;
                if (power > 1.0)
                    realPower = 1.0;
                else if (power < -1.0)
                    realPower = -1.0;

                double torque = Mass * realPower * 3000;

                foreach (var w in wheels)
                {
                    w.Body.ApplyTorque((float)(torque / wheels.Count));
                }
            }

        }

        /// <summary>
        /// Ampuu halutulla voimalla.
        /// </summary>
        /// <param name="power">Voima.</param>
        public void Shoot( double power )
        {
            Cannon.Power.Value = power;
            Shoot();
        }

        /// <summary>
        /// Ampuu tankin tykillä, jos ammuksia on vielä jäljellä.
        /// </summary>
        public void Shoot()
        {
            Cannon.Shoot();
        }
    }
}
