using System;
using System.Collections.Generic;
using System.Linq;

namespace Jypeli
{
    /// <summary>
    /// Rakenne, joka pitää fysiikkaoliot kiinteän matkan päässä toisistaan.
    /// </summary>
    public class PhysicsStructure : GameObjects.GameObjectBase, IPhysicsObjectInternal, GameObjectContainer
    {
        private List<PhysicsObject> objects;

        /// <inheritdoc/>
        public Func<IPhysicsObject, IPhysicsObject, bool> CollisionIgnoreFunc { get; set; }

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
        /// Fysiikkastruktuurin ympäröivä neliö
        /// </summary>
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

        #region IGameObject

        //public event Action AddedToGame;

        bool _isVisible = true;
        bool _ignoresLighting = false;
        PhysicsObject centerObject;

        /// <summary>
        /// Onko olio näkyvissä.
        /// </summary>
        public bool IsVisible
        {
            get { return _isVisible; }
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

        /// <summary>
        /// Tähän liittyvät näppäinkuuntelijat
        /// </summary>
        public List<Listener> AssociatedListeners { get; private set; }

        /// <summary>
        /// Rakenteen paikka pelimaailmassa.
        /// </summary>
        public override Vector Position
        {
            get { return centerObject.Position; }
            set { centerObject.Position = value; }
        }

        /// <summary>
        /// Ei toteutettu
        /// </summary>
        public override Vector Size
        {
            get { return BoundingRectangle.Size; }
            set { throw new NotImplementedException("Setting size for a structure is not implemented."); }
        }

        /// <summary>
        /// Ei toteutettu
        /// </summary>
        public override Animation Animation
        {
            get { return null; }
            set { throw new InvalidOperationException("Physics structure has no image or animation."); }
        }

        /// <summary>
        /// Ei toteutettu
        /// </summary>
        public Vector TextureWrapSize
        {
            get { return new Vector(1, 1); }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Ei toteutettu
        /// </summary>
        public bool TextureFillsShape
        {
            get { return false; }
            set { throw new NotImplementedException(); }
        }

        ///<inheritdoc/>
        public Color Color
        {
            get { return Color.Transparent; }
            set
            {
                foreach (var obj in objects)
                    obj.Color = value;
            }
        }

        /// <summary>
        /// Ei toteutettu
        /// </summary>
        public Shape Shape
        {
            get { return Shape.Rectangle; }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Olion kulma
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

        /// <summary>
        /// Massa
        /// </summary>
        public double Mass
        {
            get { return objects.Sum(item => item.Mass); }
            set { objects.ForEach(item => item.Mass = value); }
        }

        /// <summary>
        /// Jätetäänkö painovoima huomioimatta
        /// </summary>
        public bool IgnoresGravity
        {
            get { return centerObject.IgnoresGravity; }
            set 
            {
                objects.ForEach(item => item.IgnoresGravity = value);
                centerObject.IgnoresGravity = value;
            }
        }

        /// <summary>
        /// Jätetäänkö törmäykset huomioimatta
        /// </summary>
        public bool IgnoresCollisionResponse
        {
            get { return centerObject.IgnoresCollisionResponse; }
            set 
            {
                objects.ForEach(item => item.IgnoresCollisionResponse = value);
                centerObject.IgnoresCollisionResponse = value;
            }
        }

        /// <summary>
        /// Jätetäänkö räjähdyksien paineaalto huomioimatta
        /// </summary>
        public bool IgnoresExplosions
        {
            get { return centerObject.IgnoresExplosions; }
            set 
            {
                objects.ForEach(item => item.IgnoresExplosions = value);
                centerObject.IgnoresExplosions = value;
            }
        }

        /// <summary>
        /// Jättääkö fysiikkalogiikat huomioimatta
        /// </summary>
        public bool IgnoresPhysicsLogics
        {
            get { return centerObject.IgnoresPhysicsLogics; }
            set
            {
                objects.ForEach(item => item.IgnoresPhysicsLogics = value);
                centerObject.IgnoresPhysicsLogics = value;
            }
        }

        /// <summary>
        /// Olio, jolla voidaan välttää törmäykset muihin olioihin
        /// </summary>
        public Ignorer CollisionIgnorer
        {
            get { return centerObject.CollisionIgnorer; }
            set
            {
                objects.ForEach(item => item.CollisionIgnorer = value);
                centerObject.CollisionIgnorer = value;
            }
        }

        /// <summary>
        /// Törmäysryhmä.
        /// Oliot jotka ovat samassa törmäysryhmässä menevät toistensa läpi.
        /// Jos ryhmä on nolla tai negatiivinen, sillä ei ole vaikutusta.
        /// </summary>
        public int CollisionIgnoreGroup
        {
            get { return centerObject.CollisionIgnoreGroup; }
            set
            {
                objects.ForEach(item => item.CollisionIgnoreGroup = value);
                centerObject.CollisionIgnoreGroup = value;
            }
        }

        /// <summary>
        /// Lepokitka
        /// </summary>
        public double StaticFriction
        {
            get { return centerObject.StaticFriction; }
            set
            {
                objects.ForEach(item => item.StaticFriction = value);
                centerObject.StaticFriction = value;
            }
        }

        /// <summary>
        /// Liikekita
        /// </summary>
        public double KineticFriction
        {
            get { return centerObject.KineticFriction; }
            set
            {
                objects.ForEach(item => item.KineticFriction = value);
                centerObject.KineticFriction = value;
            }
        }

        /// <summary>
        /// Kimmoisuuskerroin (0 = ei kimmoisa, 1 = täysin kimmoisa, yli 1 = saa energiaa tyhjästä)
        /// </summary>
        public double Restitution
        {
            get { return centerObject.Restitution; }
            set
            {
                objects.ForEach(item => item.Restitution = value);
                centerObject.Restitution = value;
            }
        }

        /// <summary>
        /// Nopeuskerroin.
        /// Pienempi arvo kuin 1 (esim. 0.998) toimii kuten kitka / ilmanvastus.
        /// </summary>
        public double LinearDamping
        {
            get { return centerObject.LinearDamping; }
            set
            {
                objects.ForEach(item => item.LinearDamping = value);
                centerObject.LinearDamping = value;
            }
        }

        /// <summary>
        /// Kulmanopeuskerroin.
        /// Pienempi arvo kuin 1 (esim. 0.998) toimii kuten kitka / ilmanvastus.
        /// </summary>
        public double AngularDamping
        {
            get { return centerObject.AngularDamping; }
            set { centerObject.AngularDamping = value; }
        }

        /// <summary>
        /// Keskipisteen nopeus
        /// </summary>
        public Vector Velocity
        {
            get { return centerObject.Velocity; }
            set { centerObject.Velocity = value; }
        }

        /// <summary>
        /// Keskipisteen kulmanopeus
        /// </summary>
        public double AngularVelocity
        {
            get { return centerObject.AngularVelocity; }
            set { centerObject.AngularVelocity = value; }
        }

        /// <summary>
        /// Keskipisteen kiihtyvyys
        /// </summary>
        public Vector Acceleration
        {
            get { return centerObject.Acceleration; }
            set { centerObject.Acceleration = value; }
        }

        /// <summary>
        /// Keskipisteen kulmakiihtyvyys
        /// </summary>
        public double AngularAcceleration
        {
            get { return centerObject.AngularAcceleration; }
            set { centerObject.AngularAcceleration = value; }
        }

        /// <summary>
        /// Hitausmomentti
        /// </summary>
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

        ///<inheritdoc/>
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

        /// <summary>
        /// Poistaa kappaleen fysiikkastruktuurista
        /// </summary>
        /// <param name="obj"></param>
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

        /// <summary>
        /// Onko piste fysiikkarakenteen sisällä
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Kohdistaa rakenteen keskipisteeseen impulssin
        /// </summary>
        /// <param name="impulse"></param>
        public void Hit(Vector impulse)
        {
            centerObject.Hit(impulse);
        }

        /// <summary>
        /// Työntää rakenteen keskipistettä
        /// </summary>
        /// <param name="force"></param>
        public void Push(Vector force)
        {
            centerObject.Push(force);
        }

        /// <summary>
        /// Työntää keskipistettä jonkin ajan
        /// </summary>
        /// <param name="force"></param>
        /// <param name="time"></param>
        public void Push(Vector force, TimeSpan time)
        {
            centerObject.Push(force, time);
        }

        /// <summary>
        /// Kohdistaa vääntömomentin
        /// </summary>
        /// <param name="torque"></param>
        public void ApplyTorque(double torque)
        {
            centerObject.ApplyTorque(torque);
        }

        /// <summary>
        /// Liikuttaa rakennetta
        /// </summary>
        /// <param name="movement"></param>
        public void Move(Vector movement)
        {
            centerObject.Move(movement);
        }

        ///<inheritdoc/>
        public override void MoveTo(Vector location, double speed, Action doWhenArrived)
        {
            centerObject.MoveTo(location, speed, doWhenArrived);
        }

        /// <summary>
        /// Lopettaa liikkumisen kohdetta kohti
        /// </summary>
        public void StopMoveTo()
        {
            centerObject.StopMoveTo();
        }

        /// <summary>
        /// Pysäyttää liikkeen
        /// </summary>
        public void Stop()
        {
            centerObject.Stop();
        }

        /// <summary>
        /// Pysäyttää sivusuuntaisen liikkeen
        /// </summary>
        public void StopHorizontal()
        {
            centerObject.StopHorizontal();
        }

        /// <summary>
        /// Pysäyttää pystysuuntaisen liikkeen
        /// </summary>
        public void StopVertical()
        {
            centerObject.StopVertical();
        }

        /// <summary>
        /// Pysäyttää annetun akselin suuntaisen liikkeen
        /// </summary>
        /// <param name="axis"></param>
        public void StopAxial(Vector axis)
        {
            centerObject.StopAxial(axis);
        }

        /// <summary>
        /// Pysäyttää pyörimisen
        /// </summary>
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

        /// <summary>
        /// Kun oliota käydään tuhoamaan
        /// </summary>
        protected void OnDestroying()
        {
            if (Destroying != null)
                Destroying();
        }

        /// <summary>
        /// Tuhoaa olion
        /// </summary>
        public override void Destroy()
        {
            IsDestroying = true;
            OnDestroying();
            Game.DoNextUpdate(ReallyDestroy);
        }

        /// <summary>
        /// Tuhoaa olion välittömästi, ei kutsu <c>OnDestroying</c> funktiota.
        /// </summary>
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
            get { throw new NotImplementedException(); } // TODO: Tämän ehkä voi poistaa, saattaa toimia.
            set { throw new NotImplementedException(); }
        }

        #endregion
    }
}
