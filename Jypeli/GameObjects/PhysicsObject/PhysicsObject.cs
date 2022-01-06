using System;

namespace Jypeli
{
    /// <summary>
    /// Kappale joka noudattaa fysiikan lakeja, johon voi törmätä.
    /// Vaatii että käytössä on fysiikkapeli.
    /// </summary>
    [Save]
    public partial class PhysicsObject : GameObject, IPhysicsObjectInternal
    {
        /// <summary>
        /// Jättääkö painovoiman huomiotta.
        /// </summary>
        /// <value><c>true</c> if ignores gravity; otherwise, <c>false</c>.</value>
        public bool IgnoresGravity
        {
            get { return Body.IgnoresGravity; }
            set { Body.IgnoresGravity = value; }
        }

        /// <summary>
        /// Jättääkö fysiikkakentät (esim. painovoiman) huomiotta.
        /// </summary>
        /// <value><c>true</c> if ignores physics logics; otherwise, <c>false</c>.</value>
        public bool IgnoresPhysicsLogics
        {
            get { return Body.IgnoresPhysicsLogics; }
            set { Body.IgnoresPhysicsLogics = value; }
        }

        /// <summary>
        /// Rakenneolio, johon tämä olio kuuluu.
        /// </summary>
        public PhysicsStructure ParentStructure { get; internal set; }

        /// <summary>
        /// Jättääkö räjähdysten paineaallon huomioimatta
        /// </summary>
        public bool IgnoresExplosions { get; set; }

        /// <summary>
        /// Alustaa fysiikkaolion käyttöön.
        /// </summary>
        /// <param name="width">Leveys.</param>
        /// <param name="height">Korkeus.</param>
        /// <param name="shape">Muoto (esim. Shape.Circle).</param>
        /// <param name="x">Olion sijainnin X-koordinaatti.</param>
        /// <param name="y">Olion sijainnin Y-koordinaatti.</param>
        public PhysicsObject( double width, double height, Shape shape, double x = 0.0, double y = 0.0)
            : base( width, height, shape)
        {
            Initialize( width, height, shape );
            Position = new Vector(x, y);
        }

        /// <summary>
        /// Alustaa fysiikkaolion käyttöön.
        /// </summary>
        /// <param name="width">Leveys.</param>
        /// <param name="height">Korkeus.</param>
        public PhysicsObject( double width, double height )
            : this( width, height, Shape.Rectangle )
        {
        }

        /// <summary>
        /// Luo uuden fysiikkaolion.
        /// </summary>
        /// <param name="width">Leveys.</param>
        /// <param name="height">Korkeus.</param>
        /// <param name="x">Olion sijainnin X-koordinaatti.</param>
        /// <param name="y">Olion sijainnin Y-koordinaatti.</param>
        public PhysicsObject(double width, double height, double x, double y)
            : this(width, height, Shape.Rectangle, x, y)
        {
        }

        /// <summary>
        /// Alustaa fysiikkaolion käyttöön.
        /// </summary>
        /// <param name="animation">Animaatio tai kuva.</param>
		public PhysicsObject( Animation animation )
			: base( animation )
		{
            Initialize( animation.Width, animation.Height, Shape.Rectangle );
		}

        /// <summary>
        /// Luo fysiikkaolion, jonka muotona on säde.
        /// </summary>
        /// <param name="raySegment">Säde.</param>
        public PhysicsObject(RaySegment raySegment)
            : this(1, 1, raySegment)
        {
        }

        private void Initialize( double width, double height, Shape shape )
		{
            Body = PhysicsGameBase.Instance.Engine.CreateBody( this, width, height, shape );
			Body.Collided += this.OnCollided;
		}

        /// <summary>
        /// Alustaa fysiikkaolion käyttöön ja tekee siitä staattisen (liikkumattoman).
        /// </summary>
        /// <param name="width">Leveys.</param>
        /// <param name="height">Korkeus</param> 
        /// <param name="shape">Muoto (esim. Shape.Circle).</param>
        public static PhysicsObject CreateStaticObject( double width, double height, Shape shape )
        {
            var obj = new PhysicsObject( width, height, shape );
            obj.MakeStatic();
            return obj;
        }

        /// <summary>
        /// Alustaa fysiikkaolion käyttöön ja tekee siitä staattisen (liikkumattoman).
        /// </summary>
        /// <param name="width">Leveys.</param>
        /// <param name="height">Korkeus</param> 
        public static PhysicsObject CreateStaticObject( double width, double height )
        {
            var obj = new PhysicsObject( width, height );
            obj.MakeStatic();
            return obj;
        }

        /// <summary>
        /// Alustaa fysiikkaolion käyttöön.
        /// </summary>
        /// <param name="animation">Animaatio tai kuva.</param>
        public static PhysicsObject CreateStaticObject( Animation animation )
        {
            var obj = new PhysicsObject( animation );
            obj.MakeStatic();
            return obj;
        }

        /// <summary>
        /// Tekee oliosta staattisen eli liikkumattoman.
        /// </summary>
        public void MakeStatic()
        {
            Body.MakeStatic();
        }

        /// <summary>
        /// Onko olio tuhoutumassa.
        /// </summary>
        /// <value><c>true</c> if this instance is destroying; otherwise, <c>false</c>.</value>
        public bool IsDestroying
        {
            get { throw new NotImplementedException(); }
        }

        ///<inheritdoc/>
        public override void Update( Time time )
        {
            if ( Velocity.Magnitude > MaxVelocity )
                Velocity = Vector.FromLengthAndAngle( MaxVelocity, Velocity.Angle );
            if ( Math.Abs( AngularVelocity ) > MaxAngularVelocity )
                AngularVelocity = Math.Sign( AngularVelocity ) * MaxAngularVelocity;
            
            if(!IsDestroyed)
                Body.Update(time);

            base.Update( time );
        }
    }
}
