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
 * Authors: Tero Jäntti, Tomi Karppinen, Janne Nikkanen.
 */

using System;

namespace Jypeli.Assets
{
    /// <summary>
    /// Räjähdys.
    /// </summary>
    public class Explosion : GameObject
    {
        SynchronousList<PhysicsObject> shockwaveHitObjects = new SynchronousList<PhysicsObject>();
        private static Image commonImage = null;
        private static SoundEffect commonSound = null;

        private GameObject shockWave;
        private bool initialized;

        /// <summary>
        /// Onko paineaalto käytössä.
        /// </summary>
        public bool UseShockWave { get; set; }

        /// <summary>
        /// Suurin säde, johon räjähdys voi kasvaa.
        /// </summary>
        public double MaxRadius { get; set; }

        /// <summary>
        /// Räjähdyksen ääniefekti.
        /// </summary>
        public SoundEffect Sound { get; set; }

        /// <summary>
        /// Räjähdyksen nykyinen säde.
        /// </summary>
        public double CurrentRadius
        {
            get { return Size.X; }
            private set
            {
                Size = new Vector( value, value );
            }
        }

        /// <summary>
        /// Paineaallon väri.
        /// <example>
        /// Shockwave.Color = Color.White
        /// </example>
        /// </summary>
        public Color ShockwaveColor
        {
            get { return shockWave.Color; }
            set { shockWave.Color = value; }
        }

        /// <summary>
        /// Räjähdyksen leviämisnopeus (pikseliä sekunnissa)
        /// </summary>
        public double Speed { get; set; }

        /// <summary>
        /// Voima, jolla räjähdyksen paineaallon uloin reuna heittää olioita räjähdyksestä poispäin.
        /// Vihje: voit käyttää myös negatiivisia arvoja, jolloin räjähdys imee olioita sisäänsä.
        /// </summary>
        public double Force { get; set; }

        /// <summary>
        /// Kuinka voimakas räjähdyksestä tuleva ääni on, väliltä 0-1.0.
        /// Oletusarvona 0.2.
        /// </summary>
        public double Volume { get; set; }

        /// <summary>
        /// Tapahtuu, kun paineaalto osuu peliolioon.
        /// </summary>
        public event Action<IPhysicsObject, Vector> ShockwaveReachesObject;

        /// <summary>
        /// Luo uuden räjähdyksen entisen pohjalta.
        /// </summary>
        /// <param name="src">Kopioitava räjähdys</param>
        public Explosion( Explosion src )
            : this( src.MaxRadius )
        {
            this.UseShockWave = src.UseShockWave;
            this.ShockwaveColor = src.ShockwaveColor;
            this.Speed = src.Speed;
            this.Force = src.Force;
        }

        /// <summary>
        /// Luo uuden räjähdyksen.
        /// </summary>
        /// <param name="radius">Räjähdyksen säde.</param>
        public Explosion( double radius )
            : base( 0.1, 0.1, Shape.Circle )
        {
            UseShockWave = true;
            MaxRadius = radius;
            Speed = 250.0;
            Force = 1000.0;
            Volume = 0.2;
            shockWave = new GameObject( 1, 1, Shape.Circle );
            shockWave.Color = new Color( 240, 248, 255, 60 );
            Add( shockWave );

            Game.AssertInitialized( PreloadContent );

            initialized = false;
            IsUpdated = true;
        }

        private void PreloadContent()
        {
            if ( commonImage == null ) commonImage = Game.LoadImageFromResources( "Explosion.png" );
            if ( commonSound == null ) commonSound = Game.LoadSoundEffectFromResources( "ExplosionSound.wav" );

            Image = commonImage;
            Sound = commonSound;
        }

        private void OnShockwaveReachesObject( IPhysicsObject obj, Vector swForce )
        {
            if ( ShockwaveReachesObject != null )
                ShockwaveReachesObject( obj, swForce );
        }

        /// <summary>
        /// Laukaisee aliohjelman handler, kun tämän räjähdyksen paineaalto osuu olioon o.
        /// </summary>
        /// <param name="o">Olio, johon paineaallon on osuttava</param>
        /// <param name="handler">Tapahtuman käsittelevä aliohjelma</param>
        public void AddShockwaveHandler( IPhysicsObject o, Action<IPhysicsObject, Vector> handler )
        {
            if ( o == null ) throw new NullReferenceException( "Object must not be null" );
            if ( handler == null ) throw new NullReferenceException( "Handler must not be null" );

            this.ShockwaveReachesObject += delegate( IPhysicsObject target, Vector shockForce )
            {
                if ( target == o )
                    handler( target, shockForce );
            };
        }

        /// <summary>
        /// Laukaisee aliohjelman handler, kun tämän räjähdyksen paineaalto osuu olioon o.
        /// </summary>
        /// <param name="tag">Olion tagi, johon paineaallon on osuttava</param>
        /// <param name="handler">Tapahtuman käsittelevä aliohjelma</param>
        public void AddShockwaveHandler(string tag, Action<IPhysicsObject, Vector> handler)
        {
            if (tag == null) throw new NullReferenceException("Tag must not be null");
            if (handler == null) throw new NullReferenceException("Handler must not be null");

            this.ShockwaveReachesObject += delegate(IPhysicsObject target, Vector shockForce)
            {
                string targetTagAsString = target.Tag as string;

                if (targetTagAsString != null && targetTagAsString == tag)
                    handler(target, shockForce);
            };
        }

        private void applyShockwave( PhysicsObject target, Vector distance )
        {
            double distanceFromEdge = distance.Magnitude - CurrentRadius;
            if ( distanceFromEdge >= 0 )
                return;

            double relDistance = ( CurrentRadius + distanceFromEdge ) / CurrentRadius;
            double shockQuotient = 1 / Math.Pow( relDistance, 2 );
            double shockForce = Force * shockQuotient;

            if ( Math.Abs( shockForce ) > float.Epsilon )
            {
                Vector shockVector = Vector.FromLengthAndAngle( shockForce, distance.Angle );
                target.Hit( shockVector );

                if ( !shockwaveHitObjects.Contains( target ) )
                {
                    OnShockwaveReachesObject( target, shockVector );
                    shockwaveHitObjects.Add( target );
                }
            }
        }

        /// <summary>
        /// Ajetaan kun pelitilannetta päivitetään. Päivityksen voi toteuttaa omassa luokassa toteuttamalla tämän
        /// metodin. Perityn luokan metodissa tulee kutsua kantaluokan metodia.
        /// </summary>
        public override void Update( Time time )
        {
            // this is done only once, after being added to the game.
            if ( !initialized )
            {
                PlaySound();
                shockwaveHitObjects.Clear();
                initialized = true;
            }

            if ( CurrentRadius > MaxRadius )
            {
                this.Destroy();
                return;
            }

            double dt = time.SinceLastUpdate.TotalSeconds;
            CurrentRadius += dt * Speed;
            shockwaveHitObjects.Update( time );

            if ( UseShockWave )
            {
                if ( Force > 0 )
                {
                    shockWave.Size = new Vector( 2 * CurrentRadius, 2 * CurrentRadius );
                }

                foreach ( var layer in Jypeli.Game.Instance.Layers )
                {
                    foreach ( var o in layer.Objects )
                    {
                        if ( o is PhysicsObject )
                        {
                            PhysicsObject po = (PhysicsObject)o;

                            if ( po.IgnoresExplosions )
                            {
                                // No shockwave
                                continue;
                            }

                            // Shockwave
                            Vector distance = o.Position - this.Position;
                            applyShockwave( po, distance );
                        }
                    }
                }
            }

            base.Update( time );
        }

        private void PlaySound()
        {
            if ( Sound == null )
                return;

            // play the sound
            double pitch = RandomGen.NextDouble( -0.1, 0.1 ); // add some variation to explosion sound
            double pan = this.Position.X / ( Game.Screen.Width / 2 ); // sound comes from left or right speaker
            pan = AdvanceMath.MathHelper.Clamp( (float)pan, (float)-1.0, (float)1.0 );
            if ( !double.IsNaN( pan ) )  // sometimes pan can be Nan, that is why this check is here
            {
                Sound.Play(Volume, pitch, pan );
            }
        }
    }
}
