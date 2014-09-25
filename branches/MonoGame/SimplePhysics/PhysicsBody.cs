using System;
using System.Collections.Generic;
using System.Text;
using Jypeli;

namespace Jypeli.Physics
{
    /// <summary>
    /// Fyysinen kappale.
    /// </summary>
    [Save]
    public class PhysicsBody : IPhysicsBody
    {
        [Save] private double _mass = 1.0;
        [Save] private double _massInv = 1.0;
        [Save] private double _restitution = 1.0;
        [Save] private double _staticFriction = 0.0;
        [Save] private double _kineticFriction = 0.0;

        [Save] double _angle = 0;
        [Save] int _anglemul = 0;

        BoundingRectangle boundingRect = new BoundingRectangle();


        /// <summary>
        /// Törmäystapahtuma.
        /// </summary>
        public event CollisionHandler<IPhysicsBody, IPhysicsBody> Collided;

        /// <summary>
        /// Kappaleen omistava fysiikkaolio.
        /// </summary>
        [Save]
        public IPhysicsObject Owner { get; internal set; }

        /// <summary>
        /// Kappaleen muoto.
        /// </summary>
        [Save]
        public Shape Shape { get; set; }

        /// <summary>
        /// Kappaleen massa (paino).
        /// </summary>
        public double Mass
        {
            get { return _mass; }
            set
            {
                _mass = value;
                _massInv = 1 / value;
            }
        }

        /// <summary>
        /// Kappaleen massan käänteisluku.
        /// </summary>
        public double MassInv
        {
            get { return _massInv; }
            set
            {
                _massInv = value;
                _mass = 1 / value;
            }
        }

        /// <summary>
        /// Kappaleen hitausmomentti.
        /// Ei vaikutusta tällä fysiikkamoottorilla sillä pyörimisliikettä
        /// ei ole toteutettu.
        /// </summary>
        public double MomentOfInertia
        {
            get { return double.PositiveInfinity; }
            set { }
        }

        /// <summary>
        /// Kappaleen kimmoisuuskerroin.
        /// </summary>
        public double Restitution
        {
            get { return _restitution; }
            set { _restitution = value; }
        }

        /// <summary>
        /// Kappaleen liikekitkakerroin.
        /// </summary>
        public double KineticFriction
        {
            get { return _kineticFriction; }
            set { _kineticFriction = value; }
        }

        /// <summary>
        /// Kappaleen lepokitkakerroin.
        /// </summary>
        public double StaticFriction
        {
            get { return _staticFriction; }
            set { _staticFriction = value; }
        }

        /// <summary>
        /// Kappaleen pyörimisliikkeen säilyminen.
        /// Ei käytössä tällä moottorilla.
        /// </summary>
        public double AngularDamping
        {
            get { return 1; }
            set { }
        }

        /// <summary>
        /// Kappaleen etenemisliikkeen säilyminen.
        /// Ei käytössä tällä moottorilla.
        /// </summary>
        public double LinearDamping
        {
            get { return 1; }
            set { }
        }

        /// <summary>
        /// Kappaleen paikka.
        /// </summary>
        [Save]
        public Vector Position { get; set; }

        /// <summary>
        /// Kappaleen nopeus.
        /// </summary>
        [Save]
        public Vector Velocity { get; set; }

        /// <summary>
        /// Kappaleen tämänhetkinen kiihtyvyys.
        /// Nollautuu joka päivityksellä.
        /// </summary>
        [Save]
        public Vector Acceleration { get; set; }

        /// <summary>
        /// Kappaleen koko.
        /// </summary>
        [Save]
        public Vector Size { get; set; }

        /// <summary>
        /// Kappaleen kulma (vain 90 asteen monikertoja tällä moottorilla)
        /// </summary>
        public double Angle
        {
            get { return _angle; }
            set
            {
                int mul = (int)( Math.Sign( value ) * 0.5 + 2 * value / Math.PI );

                while ( mul < 0 )
                    mul += 4;

                _anglemul = ( mul % 4 );
                //_angle = _anglemul * Math.PI / 2;
                _angle = value;
            }
        }

        /// <summary>
        /// Kappaleen kulmanopeus (aina 0 tällä moottorilla)
        /// </summary>
        public double AngularVelocity
        {
            get { return 0; }
            set { }
        }

        /// <summary>
        /// Kappaleen kulmakiihtyvyys (aina 0 tällä moottorilla)
        /// </summary>
        public double AngularAcceleration
        {
            get { return 0; }
            set { }
        }

        /// <summary>
        /// Olio, jolla voi välttää oliota osumasta tiettyihin muihin olioihin.
        /// </summary>
        public virtual Ignorer CollisionIgnorer { get; set; }

        /// <summary>
        /// Jätetäänkö törmäykset huomiotta.
        /// Huomaa että törmäystapahtuma tulee silti.
        /// </summary>
        public bool IgnoresCollisionResponse { get; set; }

        /// <summary>
        /// Jätetäänkö painovoima huomiotta.
        /// </summary>
        public bool IgnoresGravity { get; set; }

        /// <summary>
        /// Jätetäänkö kaikki fysiikan logiikat huomiotta.
        /// </summary>
        public bool IgnoresPhysicsLogics { get; set; }

        /// <summary>
        /// Luo uuden fyysisen kappaleen.
        /// Ei yleensä tarvetta käyttää itse (tee mieluummin PhysicsObject)
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="shape"></param>
        public PhysicsBody( double width, double height, Shape shape )
        {
            Shape = shape;
            Size = new Vector( width, height );
            CollisionIgnorer = new ObjectIgnorer();
        }

        /// <summary>
        /// Laukaisee törmäystapahtuman.
        /// </summary>
        /// <param name="anotherBody"></param>
        internal void OnCollided( PhysicsBody anotherBody )
        {
            if ( Collided != null )
                Collided( this, anotherBody );
        }

        /// <summary>
        /// Tekee kappaleesta staattisen.
        /// </summary>
        public void MakeStatic()
        {
            _mass = double.PositiveInfinity;
            _massInv = 0;
        }

        /// <summary>
        /// Palauttaa olion (efektiiviset) reunat.
        /// </summary>
        /// <returns></returns>
        internal BoundingRectangle GetBoundingRect()
        {
            bool rotated = _anglemul == 1 || _anglemul == 3;

            boundingRect.X = this.Position.X;
            boundingRect.Y = this.Position.Y;
            boundingRect.Width = rotated ? this.Size.Y : this.Size.X;
            boundingRect.Height = rotated ? this.Size.X : this.Size.Y;

            return boundingRect;
        }

        /// <summary>
        /// Asettaa törmäyksenvälttelyryhmän.
        /// </summary>
        /// <param name="ignorer"></param>
        public void SetCollisionIgnorer( Ignorer ignorer )
        {
            this.CollisionIgnorer = ignorer;
        }

        /// <summary>
        /// Työntää kappaletta tietyllä voimalla.
        /// Voima = massa * kiihtyvyys (Newton II)
        /// </summary>
        /// <param name="force">Voimavektori</param>
        public void ApplyForce( Vector force )
        {
            Acceleration += force * _massInv;
        }

        /// <summary>
        /// Lyö kappaletta tietyllä impulssilla.
        /// Impulssi = massa * nopeus
        /// </summary>
        /// <param name="impulse">Impulssivektori</param>
        public void ApplyImpulse( Vector impulse )
        {
            Velocity += impulse * _massInv;
        }

        /// <summary>
        /// Antaa kappaleelle vääntövoiman.
        /// Ei tee mitään tällä moottorilla (ei pyörimisliikettä)
        /// </summary>
        /// <param name="torque"></param>
        public void ApplyTorque( double torque )
        {
            // No angular motion implemented (yet?)
        }

        /// <summary>
        /// Pysäyttää kappaleen.
        /// </summary>
        public void Stop()
        {
            Acceleration = Vector.Zero;
            Velocity = Vector.Zero;
        }

        /// <summary>
        /// Pysäyttää kappaleen pyörimisen.
        /// Ei vaikutusta tällä moottorilla (ei pyörimisliikettä muutenkaan)
        /// </summary>
        public void StopAngular()
        {
            // No angular motion to stop :)
        }

        /// <summary>
        /// Pysäyttää kappaleen liikkeen vektorin suuntaisella akselilla.
        /// </summary>
        /// <param name="axis">Akseli</param>
        public void StopAxial( Vector axis )
        {
            Acceleration = Acceleration.Project( axis.LeftNormal );
            Velocity = Velocity.Project( axis.LeftNormal );
        }
    }
}
