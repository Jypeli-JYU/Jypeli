using AdvanceMath;
using System;
using System.Collections.Generic;

namespace Jypeli
{
    /// <summary>
    /// Rakenne, joka pitää fysiikkaoliot kiinteän matkan päässä toisistaan.
    /// </summary>
    public class PhysicsStructure : GameObjects.GameObjectBase, IPhysicsObjectInternal, GameObjectContainer
    {
        private double _softness = 0;
        private List<PhysicsObject> objects;

        /// <summary>
        /// Onko rakenne lisätty peliin.
        /// </summary>
        public bool IsAddedToGame { get; set; }

        /// <summary>
        /// Rakenteeseen kuuluvat oliot.
        /// </summary>
        public IList<PhysicsObject> Objects
        {
            get { return objects; }
        }

        /// <summary>
        /// Rakenteeseen kuuluvien olioiden määrä.
        /// </summary>
        public int ObjectCount
        {
            get { return objects.Count; }
        }

        /// <summary>
        /// Rakenteeseen kuuluvat liitokset.
        /// </summary>
        internal List<IAxleJoint> Joints { get; private set; }

        /// <summary>
        /// Olioiden välisten liitosten pehmeys.
        /// </summary>
        public double Softness
        {
            get { return _softness; }
            set
            {
                _softness = value;

                for ( int i = 0; i < Joints.Count; i++ )
                {
                    Joints[i].Softness = value;
                }
            }
        }

        public BoundingRectangle BoundingRectangle
        {
            get
            {
                if ( objects.Count == 0 )
                    return new BoundingRectangle();

                double top = objects[0].Top;
                double left = objects[0].Left;
                double bottom = objects[0].Bottom;
                double right = objects[0].Right;

                for ( int i = 1; i < objects.Count; i++ )
                {
                    if ( objects[i].Top > top ) top = objects[i].Top;
                    if ( objects[i].Left < left ) left = objects[i].Left;
                    if ( objects[i].Bottom < bottom ) bottom = objects[i].Bottom;
                    if ( objects[i].Right > right ) right = objects[i].Right;
                }

                return new BoundingRectangle( new Vector( left, top ), new Vector( right, bottom ) );
            }
        }

        #region Tagged

        public object Tag { get; set; }

        #endregion

        #region IGameObject

        //public event Action AddedToGame;

        bool _isVisible = true;
        bool _ignoresLighting = false;
        PhysicsObject centerObject;

        public bool IsVisible
        {
            get { return false; }
            set
            {
                foreach ( var obj in objects )
                {
                    obj.IsVisible = value;
                }

                _isVisible = value;
            }
        }

        /// <summary>
        /// Jättääkö olio kentän valaistuksen huomiotta.
        /// </summary>
        public bool IgnoresLighting
        {
            get { return _ignoresLighting; }
            set
            {
                objects.ForEach(o => o.IgnoresLighting = value);
                _ignoresLighting = value;
            }
        }

        public List<Listener> AssociatedListeners { get; private set; }

        /// <summary>
        /// Rakenteen paikka pelimaailmassa.
        /// </summary>
        public override Vector Position
        {
            get
            {
                return centerObject.Position;
            }
            set
            {
                Vector delta = value - Position;

                foreach ( var obj in objects )
                {
                    obj.Position += delta;
                }

                centerObject.Position = value;
            }
        }

        public override Vector Size
        {
            get
            {
                return BoundingRectangle.Size;
            }
            set
            {
                throw new NotImplementedException( "Setting size for a structure is not implemented." );
            }
        }

        public override Animation Animation
        {
            get { return null; }
            set
            {
                throw new InvalidOperationException( "Physics structure has no image or animation." );
            }
        }

        public Vector TextureWrapSize
        {
            get { return new Vector( 1, 1 ); }
            set { throw new NotImplementedException(); }
        }

        public bool TextureFillsShape
        {
            get { return false; }
            set { throw new NotImplementedException(); }
        }

        public Color Color
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                foreach ( var obj in objects )
                    obj.Color = value;
            }
        }

        public Shape Shape
        {
            get { return Shape.Rectangle; }
            set { throw new NotImplementedException(); }
        }

        //TODO
        /// <summary>
        /// HUOM!
        /// Fysiikkamoottorin bugin takia joillain kappaleilla tämän käyttö voi tuottaa "haamuvoimia", kappale lähtee itsestään pyörimään.
        /// Joko aseta CanRotate = false, tai 
        /// </summary>
        public override Angle Angle
        {
            get
            {
                IEnumerable<double> angles = objects.ConvertAll<PhysicsObject, double>(delegate (PhysicsObject o) { return o.Angle.Degrees; });
                return Angle.FromDegrees(angles.Average()); // TODO: IEnumerable<Angle>.Average
            }
            set
            {
                foreach (var obj in objects)
                {
                    //obj.Angle = value; // Tämä tuottaa hyvin paljon haamuvoimia
                    obj.Position = (new Matrix2x2(value.Cos, -value.Sin, value.Sin, value.Cos)) * (Vector2D)obj.Position; // Tämä ei niin paljon?
                }
            }
        }

        #endregion

        #region IPhysicsObject

        bool _ignoresGravity = false;
        bool _ignoresCollisionResponse = false;
        bool _ignoresExplosions = false;
        bool _ignoresPhysicsLogics = false;
        Ignorer _collisionIgnorer = null;
        int _collisionIgnoreGroup = 0;
        double _sfriction = 0.4;
        double _kfriction = 0.4;
        double _restitution = 0.5;
        double _linearDamping = 1;
        double _angularDamping = 1;
        double? _setMomentOfInertia = null;
        double _calcMomentOfInertia = 0;

        /// <summary>
        /// Tapahtuu kun olio törmää toiseen.
        /// </summary>
        public event CollisionHandler<IPhysicsObject, IPhysicsObject> Collided;

        /// <summary>
        /// Rakenneolio, johon tämä olio kuuluu.
        /// </summary>
        public PhysicsStructure ParentStructure
        {
            // No nested physics structures for now
            get { return null; }
        }

        public double Mass
        {
            get
            {
                double totalMass = 0;
                objects.ForEach( o => totalMass += o.Mass );
                return totalMass;
            }
            set
            {
                double massMultiplier = value / this.Mass;
                objects.ForEach( o => o.Mass *= massMultiplier );
            }
        }

        public bool IgnoresGravity
        {
            get { return _ignoresGravity; }
            set
            {
                objects.ForEach( o => o.IgnoresGravity = value );
                _ignoresGravity = value;
            }
        }

        public bool IgnoresCollisionResponse
        {
            get { return _ignoresCollisionResponse; }
            set
            {
                objects.ForEach( o => o.IgnoresCollisionResponse = value );
                _ignoresCollisionResponse = value;
            }
        }

        public bool IgnoresExplosions
        {
            get { return _ignoresExplosions; }
            set
            {
                objects.ForEach( o => o.IgnoresExplosions = value );
                _ignoresExplosions = value;
            }
        }

        public bool IgnoresPhysicsLogics
        {
            get { return _ignoresPhysicsLogics; }
            set
            {
                objects.ForEach( o => o.IgnoresPhysicsLogics = value );
                _ignoresPhysicsLogics = value;
            }
        }

        public Ignorer CollisionIgnorer
        {
            get { return _collisionIgnorer; }
            set
            {
                objects.ForEach( o => o.CollisionIgnorer = value );
                _collisionIgnorer = value;
            }
        }

        public int CollisionIgnoreGroup
        {
            get { return _collisionIgnoreGroup; }
            set
            {
                objects.ForEach( o => o.CollisionIgnoreGroup = value );
                _collisionIgnoreGroup = value;
            }
        }

        public double StaticFriction
        {
            get { return _sfriction; }
            set
            {
                objects.ForEach( o => o.StaticFriction = value );
                _sfriction = value;
            }
        }

        public double KineticFriction
        {
            get { return _kfriction; }
            set
            {
                objects.ForEach( o => o.KineticFriction = value );
                _kfriction = value;
            }
        }

        public double Restitution
        {
            get { return _restitution; }
            set
            {
                objects.ForEach( o => o.Restitution = value );
                _restitution = value;
            }
        }

        public double LinearDamping
        {
            get { return _linearDamping; }
            set
            {
                objects.ForEach( o => o.LinearDamping = value );
                _linearDamping = value;
            }
        }

        public double AngularDamping
        {
            get { return _angularDamping; }
            set
            {
                objects.ForEach( o => o.AngularDamping = value );
                _angularDamping = value;
            }
        }

        public Vector Velocity
        {
            get
            {
                var velocities = objects.ConvertAll<PhysicsObject, Vector>( delegate( PhysicsObject o ) { return o.Velocity; } );
                return Vector.Average( velocities );
            }
            set
            {
                foreach ( var obj in objects )
                    obj.Velocity = value;
            }
        }

        public double AngularVelocity
        {
            get
            {
                IEnumerable<double> velocities = objects.ConvertAll<PhysicsObject, double>( delegate( PhysicsObject o ) { return o.AngularVelocity; } );
                return velocities.Average();
            }
            set
            {
                foreach ( var obj in objects )
                    obj.AngularVelocity = value;
            }
        }

        public Vector Acceleration
        {
            get
            {
                var accs = objects.ConvertAll<PhysicsObject, Vector>( delegate( PhysicsObject o ) { return o.Acceleration; } );
                return Vector.Average( accs );
            }
            set
            {
                foreach ( var obj in objects )
                    obj.Acceleration = value;
            }
        }

        public double AngularAcceleration
        {
            get
            {
                IEnumerable<double> accs = objects.ConvertAll<PhysicsObject, double>( delegate( PhysicsObject o ) { return o.AngularAcceleration; } );
                return accs.Average();
            }
            set
            {
                foreach ( var obj in objects )
                    obj.AngularAcceleration = value;
            }
        }

        public double MomentOfInertia
        {
            get
            {
                return _setMomentOfInertia.HasValue ? _setMomentOfInertia.Value : _calcMomentOfInertia;
            }
            set
            {
                _setMomentOfInertia = value;
            }
        }

        /// <summary>
        /// Jos <c>false</c>, olio ei voi pyöriä.
        /// </summary>
        public bool CanRotate
        {
            get { return !double.IsPositiveInfinity( MomentOfInertia ); }
            set
            {
                if ( !value )
                {
                    foreach (var obj in objects)
                        obj.CanRotate = false;
                    //MomentOfInertia = double.PositiveInfinity;
                }
                else
                {
                    foreach (var obj in objects)
                        obj.CanRotate = true;
                    CalculateMomentOfInertia();
                    _setMomentOfInertia = null;
                }
            }
        }

        #endregion

        /// <summary>
        /// Luo uuden tyhjän rakenteen.
        /// </summary>
        public PhysicsStructure()
        {
            centerObject = new PhysicsObject( 1, 1 ) { IgnoresPhysicsLogics = true, IsVisible = false };
            objects = new List<PhysicsObject>();
            Joints = new List<IAxleJoint>();
            _collisionIgnorer = new ObjectIgnorer();
            AssociatedListeners = new List<Listener>();
            AddedToGame += AddJoints;
            Removed += RemoveJoints;
            Add(centerObject); // Tämä pitää lisätä, sillä muuten jossain tilanteissa structuren ensimmäinen kappale pyörii keskiakselinsa ympäri.
        }

        private void AddJoints()
        {
            Joints.ForEach( PhysicsGameBase.Instance.Add );
        }

        private void RemoveJoints()
        {
            Joints.ForEach( PhysicsGameBase.Instance.Remove );
        }

        /// <summary>
        /// Luo uuden rakenteen ja varustaa sen fysiikkaolioilla.
        /// </summary>
        /// <param name="objs">Fysiikkaoliot</param>
        public PhysicsStructure( params PhysicsObject[] objs )
            : this()
        {
            for ( int i = 0; i < objs.Length; i++ )
            {
                Add( objs[i] );
            }
        }

        /// <summary>
        /// Kutsutaan kun törmätään.
        /// </summary>
        internal void OnCollided( IPhysicsObject part, IPhysicsObject target )
        {
            if ( Collided != null )
                Collided( this, target );
        }

        /// <summary>
        /// Palauttaa rakenteeseen kuuluvat oliot.
        /// </summary>
        /// <typeparam name="T">Olion tyyppi rakenteessa (esim. PhysicsObject)</typeparam>
        /// <returns></returns>
        public IEnumerable<T> GetChildObjects<T>() where T : IGameObject
        {
            foreach ( IGameObject o in Objects )
            {
                if ( o is T )
                    yield return (T)o;
            }
        }

        /// <summary>
        /// Palauttaa rakenteeseen kuuluvat oliot.
        /// </summary>
        /// <typeparam name="T">Olion tyyppi rakenteessa (esim. PhysicsObject)</typeparam>
        /// <returns></returns>
        public IEnumerable<T> GetChildObjects<T>( Predicate<T> predicate ) where T : IGameObject
        {
            foreach ( IGameObject o in Objects )
            {
                if ( o is T && predicate( (T)o ) )
                    yield return (T)o;
            }
        }

        public void Update( Time time ) // new vai override?
        {
            foreach ( var obj in objects )
            {
                if ( obj.IsUpdated )
                    obj.Update( time );
            }
        }

        /// <summary>
        /// Lisää olion rakenteeseen.
        /// </summary>
        /// <param name="obj">Lisättävä olio</param>
        public void Add( IGameObject obj )
        {
            if ( !( obj is PhysicsObject ) )
                throw new NotImplementedException( "Currently only PhysicsObjects can be added to a structure." );

            if ( !IsAddedToGame )
            {
                // Add to game and use relative coordinates
                obj.Position += this.Position;

                if ( !obj.IsAddedToGame )
                    Game.Instance.Add( obj );
            }

            PhysicsObject physObj = (PhysicsObject)obj;
            physObj.ParentStructure = this;
            physObj.IsVisible = _isVisible;
            physObj.IgnoresGravity = _ignoresGravity;
            physObj.IgnoresCollisionResponse = _ignoresCollisionResponse;
            physObj.IgnoresExplosions = _ignoresExplosions;
            physObj.IgnoresPhysicsLogics = _ignoresPhysicsLogics;
            physObj.CollisionIgnoreGroup = _collisionIgnoreGroup;
            physObj.CollisionIgnorer = _collisionIgnorer;
            physObj.Restitution = _restitution;
            physObj.StaticFriction = _sfriction;
            physObj.KineticFriction = _kfriction;
            physObj.LinearDamping = _linearDamping;
            physObj.AngularDamping = _angularDamping;

            physObj.Collided += this.OnCollided;

            var game = PhysicsGameBase.Instance;

            foreach ( var existing in objects )
            {
                if (Game.Instance.FarseerGame)
                {
                    IAxleJoint joint = game.Engine.CreateJoint(physObj, existing, JointTypes.WeldJoint);
                    joint.Softness = _softness;
                    Joints.Add(joint);
                    game.Add(joint);
                }
                else
                {
                    IAxleJoint joint = game.Engine.CreateJoint(physObj, existing, existing.Position);
                    joint.Softness = _softness;
                    Joints.Add(joint);
                    game.Add(joint);
                }
            }

            objects.Add( physObj );
            CalculateMomentOfInertia();
        }

        public void Remove( IGameObject obj )
        {
            if ( !( obj is PhysicsObject ) )
                throw new NotImplementedException( "Currently only PhysicsObjects can be added to a structure." );

            PhysicsObject physObj = (PhysicsObject)obj;

            if ( !objects.Contains( physObj ) )
                return;

            physObj.ParentStructure = null;
            physObj.CollisionIgnorer = null;
            physObj.CollisionIgnoreGroup = 0;
            physObj.Collided -= this.OnCollided;

            foreach ( var joint in Joints.FindAll( j => j.Object1 == physObj || j.Object2 == physObj ) )
            {
                joint.Destroy();
                PhysicsGameBase.Instance.Remove( joint );
            }

            objects.Remove( physObj );
            CalculateMomentOfInertia();
        }

        private void CalculateMomentOfInertia()
        {
            Vector center = this.Position;
            _calcMomentOfInertia = 0;

            foreach ( var part in objects )
            {
                double r = Vector.Distance( center, part.Position );
                _calcMomentOfInertia += part.Mass * r * r;
            }
        }

        public bool IsInside( Vector point )
        {
            foreach ( var obj in objects )
            {
                if ( obj.IsInside( point ) )
                    return true;
            }

            return false;
        }

        #region IPhysicsObject

        public void Hit( Vector impulse )
        {
            Vector velocity = impulse / Mass;

            foreach ( var obj in objects )
            {
                obj.Hit( velocity * obj.Mass );
            }
        }

        public void Push( Vector force )
        {
            Vector acceleration = force / Mass;

            foreach ( var obj in objects )
            {
                obj.Push( acceleration * obj.Mass );
            }
        }

        public void Push( Vector force, TimeSpan time )
        {
            Vector acceleration = force / Mass;

            foreach ( var obj in objects )
            {
                obj.Push( acceleration * obj.Mass, time );
            }
        }

        public void ApplyTorque( double torque )
        {
            if ( MomentOfInertia == 0 || double.IsInfinity( MomentOfInertia ) )
                return;

            double angularAcc = torque / MomentOfInertia;
            Vector center = this.Position;

            foreach ( var obj in objects )
            {
                Vector radius = obj.Position - center;
                double linearAcc = radius.Magnitude * angularAcc;
                obj.Push( linearAcc * obj.Mass * radius.LeftNormal );
            }
        }

        public void Move( Vector movement )
        {
            foreach ( var obj in objects )
            {
                obj.Move( movement );
            }
        }

        public override void MoveTo( Vector location, double speed, Action doWhenArrived )
        {
            centerObject.MoveTo( location, speed, doWhenArrived );

            foreach ( var obj in objects )
            {
                Vector displacement = obj.Position - centerObject.Position;
                obj.MoveTo( location + displacement, speed );
            }
        }

        public void StopMoveTo()
        {
            objects.ForEach( o => o.StopMoveTo() );
        }

        public void Stop()
        {
            objects.ForEach( o => o.Stop() );
        }

        public void StopHorizontal()
        {
            objects.ForEach( o => o.StopHorizontal() );
        }

        public void StopVertical()
        {
            objects.ForEach( o => o.StopVertical() );
        }

        public void StopAxial( Vector axis )
        {
            objects.ForEach( o => o.StopAxial( axis ) );
        }

        public void StopAngular()
        {
            objects.ForEach( o => o.StopAngular() );
        }

        #endregion

        #region DelayedDestroyable

        #region DelayedDestroyable

        /// <summary>
        /// Onko olio tuhoutumassa.
        /// </summary>
        public bool IsDestroying { get; private set; }

        /// <summary>
        /// Tapahtuu, kun olion tuhoaminen alkaa.
        /// </summary> 
        public event Action Destroying;

        protected void OnDestroying()
        {
            if ( Destroying != null )
                Destroying();
        }

        public override void Destroy()
        {
            IsDestroying = true;
            OnDestroying();
            Game.DoNextUpdate( ReallyDestroy );
        }

        protected virtual void ReallyDestroy()
        {
            foreach ( var joint in Joints ) joint.Destroy();
            foreach ( var obj in objects ) obj.Destroy();
            this.MaximumLifetime = new TimeSpan( 0 );
            base.Destroy();
        }

        #endregion

        #endregion

        #region Unimplemented Members / IGameObject

        public IGameObject Parent
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        #endregion
    }
}
