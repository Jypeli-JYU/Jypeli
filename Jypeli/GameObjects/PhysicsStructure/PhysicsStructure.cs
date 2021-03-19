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

        public BoundingRectangle BoundingRectangle
        {
            get
            {
                if (objects.Count == 0)
                    return new BoundingRectangle();

                double top = objects[0].Top;
                double left = objects[0].Left;
                double bottom = objects[0].Bottom;
                double right = objects[0].Right;

                for (int i = 1; i < objects.Count; i++)
                {
                    if (objects[i].Top > top) top = objects[i].Top;
                    if (objects[i].Left < left) left = objects[i].Left;
                    if (objects[i].Bottom < bottom) bottom = objects[i].Bottom;
                    if (objects[i].Right > right) right = objects[i].Right;
                }

                return new BoundingRectangle(new Vector(left, top), new Vector(right, bottom));
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
                foreach (var obj in objects)
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
            get { return centerObject.Position; }
            set { centerObject.Position = value; }
        }

        public override Vector Size
        {
            get { return BoundingRectangle.Size; }
            set { throw new NotImplementedException("Setting size for a structure is not implemented."); }
        }

        public override Animation Animation
        {
            get { return null; }
            set { throw new InvalidOperationException("Physics structure has no image or animation."); }
        }

        public Vector TextureWrapSize
        {
            get { return new Vector(1, 1); }
            set { throw new NotImplementedException(); }
        }

        public bool TextureFillsShape
        {
            get { return false; }
            set { throw new NotImplementedException(); }
        }

        public Color Color
        {
            get { throw new NotImplementedException(); }
            set
            {
                foreach (var obj in objects)
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
            get { return centerObject.Angle; }
            set { centerObject.Angle = value; }
        }

        #endregion

        #region IPhysicsObject
        
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
            get { return centerObject.Mass; }
            set { centerObject.Mass = value; }
        }

        public bool IgnoresGravity
        {
            get { return centerObject.IgnoresGravity; }
            set { centerObject.IgnoresGravity = value; }
        }

        public bool IgnoresCollisionResponse
        {
            get { return centerObject.IgnoresCollisionResponse; }
            set { centerObject.IgnoresCollisionResponse = value; }
        }

        public bool IgnoresExplosions
        {
            get { return centerObject.IgnoresExplosions; }
            set { centerObject.IgnoresExplosions = value; }
        }

        public bool IgnoresPhysicsLogics
        {
            get { return centerObject.IgnoresPhysicsLogics; }
            set { centerObject.IgnoresPhysicsLogics = value; }
        }

        public Ignorer CollisionIgnorer
        {
            get { return centerObject.CollisionIgnorer; }
            set { centerObject.CollisionIgnorer = value; }
        }

        public int CollisionIgnoreGroup
        {
            get { return centerObject.CollisionIgnoreGroup; }
            set { centerObject.CollisionIgnoreGroup = value; }
        }

        public double StaticFriction
        {
            get { return centerObject.StaticFriction; }
            set { centerObject.StaticFriction = value; }
        }

        public double KineticFriction
        {
            get { return centerObject.KineticFriction; }
            set { centerObject.KineticFriction = value; }
        }

        public double Restitution
        {
            get { return centerObject.Restitution; }
            set { centerObject.Restitution = value; }
        }

        public double LinearDamping
        {
            get { return centerObject.LinearDamping; }
            set { centerObject.LinearDamping = value; }
        }

        public double AngularDamping
        {
            get { return centerObject.AngularDamping; }
            set { centerObject.AngularDamping = value; }
        }

        public Vector Velocity
        {
            get { return centerObject.Velocity; }
            set { centerObject.Velocity = value; }
        }

        public double AngularVelocity
        {
            get { return centerObject.AngularVelocity; }
            set { centerObject.AngularVelocity = value; }
        }

        public Vector Acceleration
        {
            get { return centerObject.Acceleration; }
            set { centerObject.Acceleration = value; }
        }

        public double AngularAcceleration
        {
            get { return centerObject.AngularAcceleration; }
            set { centerObject.AngularAcceleration = value; }
        }

        public double MomentOfInertia
        {
            get { return _setMomentOfInertia.HasValue ? _setMomentOfInertia.Value : _calcMomentOfInertia; }
            set { _setMomentOfInertia = value; }
        }

        /// <summary>
        /// Jos <c>false</c>, olio ei voi pyöriä.
        /// </summary>
        public bool CanRotate
        {
            get { return centerObject.CanRotate; }
            set { centerObject.CanRotate = value; }
        }

        #endregion

        /// <summary>
        /// Luo uuden tyhjän rakenteen.
        /// </summary>
        public PhysicsStructure()
        {
            centerObject = new PhysicsObject(1, 1) { IsVisible = false };
            objects = new List<PhysicsObject>();
            AssociatedListeners = new List<Listener>();
            AddedToGame += () => Game.Instance.Add(centerObject);
            Removed += () =>
            {
                Game.Instance.Remove(centerObject);
                objects.ForEach(o => Game.Instance.Remove(o)); // TODO: Kappaleet menevät nyt poistoon kahdesti, mutta niitä ei oikeasti poisteta ilman tätä.
            };

        }

        /// <summary>
        /// Luo uuden rakenteen ja varustaa sen fysiikkaolioilla.
        /// </summary>
        /// <param name="objs">Fysiikkaoliot</param>
        public PhysicsStructure(params PhysicsObject[] objs)
            : this()
        {
            for (int i = 0; i < objs.Length; i++)
            {
                Add(objs[i]);
            }
        }

        /// <summary>
        /// Kutsutaan kun törmätään.
        /// </summary>
        internal void OnCollided(IPhysicsObject part, IPhysicsObject target)
        {
            if (Collided != null)
                Collided(this, target);
        }

        /// <summary>
        /// Palauttaa rakenteeseen kuuluvat oliot.
        /// </summary>
        /// <typeparam name="T">Olion tyyppi rakenteessa (esim. PhysicsObject)</typeparam>
        /// <returns></returns>
        public IEnumerable<T> GetChildObjects<T>() where T : IGameObject
        {
            foreach (IGameObject o in Objects)
            {
                if (o is T)
                    yield return (T)o;
            }
        }

        /// <summary>
        /// Palauttaa rakenteeseen kuuluvat oliot.
        /// </summary>
        /// <typeparam name="T">Olion tyyppi rakenteessa (esim. PhysicsObject)</typeparam>
        /// <returns></returns>
        public IEnumerable<T> GetChildObjects<T>(Predicate<T> predicate) where T : IGameObject
        {
            foreach (IGameObject o in Objects)
            {
                if (o is T && predicate((T)o))
                    yield return (T)o;
            }
        }

        public override void Update(Time time)
        {
            foreach (var obj in objects)
            {
                if (obj.IsUpdated)
                    obj.Update(time);
            }
        }

        /// <summary>
        /// Lisää olion rakenteeseen.
        /// </summary>
        /// <param name="obj">Lisättävä olio</param>
        public void Add(IGameObject obj)
        {
            if (!(obj is PhysicsObject))
                throw new NotImplementedException("Currently only PhysicsObjects can be added to a structure.");

            if (!IsAddedToGame)
            {
                if (!obj.IsAddedToGame)
                    Game.Instance.Add(obj);
            }

            PhysicsObject physObj = (PhysicsObject)obj;

            centerObject.Add(physObj);
            objects.Add(physObj);
            physObj.ParentStructure = this;
            CalculateMomentOfInertia();

        }

        public void Remove(IGameObject obj)
        {
            if (!(obj is PhysicsObject))
                throw new NotImplementedException("Currently only PhysicsObjects can be added to a structure.");

            PhysicsObject physObj = (PhysicsObject)obj;

            if (!objects.Contains(physObj))
                return;

            physObj.ParentStructure = null;

            PhysicsGameBase.Instance.Remove(physObj);

            objects.Remove(physObj);
            centerObject.Body.RegenerateConnectedFixtures();
            CalculateMomentOfInertia();
        }

        private void CalculateMomentOfInertia()
        {
            Vector center = this.Position;
            _calcMomentOfInertia = 0;

            foreach (var part in objects)
            {
                double r = Vector.Distance(center, part.Position);
                _calcMomentOfInertia += part.Mass * r * r;
            }
        }

        public bool IsInside(Vector point)
        {
            foreach (var obj in objects)
            {
                if (obj.IsInside(point))
                    return true;
            }

            return false;
        }

        #region IPhysicsObject

        public void Hit(Vector impulse)
        {
            centerObject.Hit(impulse);
        }

        public void Push(Vector force)
        {
            centerObject.Push(force);
        }

        public void Push(Vector force, TimeSpan time)
        {
            centerObject.Push(force, time);
        }

        public void ApplyTorque(double torque)
        {
            centerObject.ApplyTorque(torque);
        }

        public void Move(Vector movement)
        {
            centerObject.Move(movement);
        }

        public override void MoveTo(Vector location, double speed, Action doWhenArrived)
        {
            centerObject.MoveTo(location, speed, doWhenArrived);
        }

        public void StopMoveTo()
        {
            centerObject.StopMoveTo();
        }

        public void Stop()
        {
            centerObject.Stop();
        }

        public void StopHorizontal()
        {
            centerObject.StopHorizontal();
        }

        public void StopVertical()
        {
            centerObject.StopVertical();
        }

        public void StopAxial(Vector axis)
        {
            centerObject.StopAxial(axis);
        }

        public void StopAngular()
        {
            centerObject.StopAngular();
        }

        #endregion

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
            if (Destroying != null)
                Destroying();
        }

        public override void Destroy()
        {
            IsDestroying = true;
            OnDestroying();
            Game.DoNextUpdate(ReallyDestroy);
        }

        protected virtual void ReallyDestroy()
        {
            foreach (var obj in objects) obj.Destroy();
            this.MaximumLifetime = new TimeSpan(0);
            base.Destroy();
        }

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
