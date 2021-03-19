#region MIT License
/*
 * Copyright (c) 2005-2008 Jonathan Mark Porter. http://physics2d.googlepages.com/
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy 
 * of this software and associated documentation files (the "Software"), to deal 
 * in the Software without restriction, including without limitation the rights to 
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of 
 * the Software, and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be 
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
 * PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE 
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 */
#endregion




#if UseDouble
using Scalar = System.Double;
#else
using Scalar = System.Single;
#endif
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AdvanceMath;
using AdvanceMath.Geometry2D;
using Physics2DDotNet.Shapes;
using Physics2DDotNet.Joints;
using Physics2DDotNet.Ignorers;


namespace Physics2DDotNet
{
    /// <summary>
    /// This is the Physical Body that collides in the engine.
    /// </summary>
    public sealed class Body : IPhysicsEntity, IDuplicateable<Body>
    {
        #region static methods
        internal static MassInfo GetMassInfo(Scalar mass, IShape shape)
        {
            if (shape == null) { throw new ArgumentNullException("shape"); }
            return new MassInfo(mass, shape.Inertia * mass);
        }
        public static bool CanCollide(Body body1, Body body2)
        {
            if (body1 == null) { throw new ArgumentNullException("body1"); }
            if (body2 == null) { throw new ArgumentNullException("body2"); }
            return
                body1.isCollidable &&
                body2.isCollidable &&
                (body1.collisionIgnorer == null ||
                (Ignorer.CanCollide(body1, body2, body1.collisionIgnorer, body2.collisionIgnorer)))
                &&
                (body2.collisionIgnorer == null ||
                !body2.collisionIgnorer.BothNeeded ||
                (Ignorer.CanCollide(body2, body1, body2.collisionIgnorer, body1.collisionIgnorer)));
        }

        internal static void CreateProxy(Body body1, Body body2, Matrix2x2 transformation)
        {
            AddProxyList(body1, body2, transformation);
            AddProxyList(body2, body1, transformation.Inverted);
            AddProxySingle(body1, body2, transformation);
        }
        private static void AddProxyList(Body body1, Body body2, Matrix2x2 transformation)
        {
            for (LinkedListNode<BodyProxy> node = body2.proxies.First;
                  node != null;
                  node = node.Next)
            {
                BodyProxy proxyT = node.Value;
                AddProxySingle(body1, proxyT.Body2,
                    transformation * proxyT.transformation);
            }
        }
        private static void AddProxySingle(Body body1, Body body2, Matrix2x2 transformation)
        {
            BodyProxy proxy1 = new BodyProxy(body2,body1, transformation.Inverted);
            BodyProxy proxy2 = new BodyProxy(body1,body2, transformation);
            proxy1.invertedTwin = proxy2;
            proxy2.invertedTwin = proxy1;
            body1.proxies.AddLast(proxy2.node);
            body2.proxies.AddLast(proxy1.node);
        }

        #endregion
        #region events
        /// <summary>
        /// Raised when the Lifetime property has been Changed.
        /// </summary>
        public event EventHandler LifetimeChanged;
        /// <summary>
        /// Raised when the Shape of the Body has been Changed.
        /// </summary>
        public event EventHandler ShapeChanged;
        /// <summary>
        /// Raised when the object is Added to the engine but is not yet part of the update process.
        /// </summary>
        public event EventHandler Pending;
        /// <summary>
        /// Raised when the object is added to a Physics Engine.
        /// </summary>
        public event EventHandler Added;
        /// <summary>
        /// Raised when the object is Removed from a Physics Engine. 
        /// </summary>
        public event EventHandler<RemovedEventArgs> Removed;
        /// <summary>
        /// Raised when the Position has been Changed.
        /// Raised by either the Solver or a call to ApplyMatrix.
        /// </summary>
        public event EventHandler PositionChanged;
        /// <summary>
        /// Raised when the Body has been updated to a change in time.
        /// </summary>
        public event EventHandler<UpdatedEventArgs> Updated;
        /// <summary>
        /// Raised when the Body collides with another.
        /// </summary>
        public event EventHandler<CollisionEventArgs> Collided;
        /// <summary>
        /// Raised constantly when the Body is colliding with another.
        /// </summary>
        public event EventHandler<CollisionEventArgs> Colliding;
        /// <summary>
        /// Raised when a joint is added that affects this body.
        /// </summary>
        public event EventHandler<BodyJointEventArgs> JointAdded;
        /// <summary>
        /// Raised when a joint is removed that affects this body.
        /// </summary>
        public event EventHandler<BodyJointEventArgs> JointRemoved;

        #endregion
        #region fields
        Dictionary<string, object> tags;
        
        internal LinkedList<BodyProxy> proxies;
        private List<Joint> joints;
        private PhysicsEngine engine;
        private IShape shape;
        private PhysicsState state;
        private Matrices matrices;
        private MassInfo massInfo;
        private Coefficients coefficients;
        private Matrix2x3 transformation;
        private BoundingRectangle rectangle;
        private Lifespan lifetime;
        private Ignorer eventIgnorer;
        private Ignorer collisionIgnorer;

        private ALVector2D lastPosition;
        private Scalar linearDamping;
        private Scalar angularDamping;
        private int id;
        internal int idleCount;

        internal bool isChecked;
        private bool ignoreVertexes;
        private bool ignoresGravity;
        private bool ignoresPhysicsLogics;
        private bool ignoresCollisionResponce;
        private bool isAdded;
        private bool isCollidable;
        private bool isTransformed;
        private bool isEventable;
        private bool isBroadPhaseOnly;
        private bool transformChanged;
        private bool isFrozen;

        private object tag;
        private object solverTag;
        private object detectorTag;
        #endregion
        #region constructors
        /// <summary>
        /// Creates a new Body Instance.
        /// </summary>
        /// <param name="state">The State of the Body.</param>
        /// <param name="shape">The Shape of the Body.</param>
        /// <param name="mass">The mass of the Body The inertia will be aquired from the Shape.</param>
        /// <param name="coefficients">A object containing coefficients.</param>
        /// <param name="lifeTime">A object Describing how long the object will be in the engine.</param>
        public Body(
            PhysicsState state,
            IShape shape,
            Scalar mass,
            Coefficients coefficients,
            Lifespan lifetime)
            : this(
                state, shape,
                GetMassInfo(mass, shape),
                coefficients, lifetime) { }
        /// <summary>
        /// Creates a new Body Instance.
        /// </summary>
        /// <param name="state">The State of the Body.</param>
        /// <param name="shape">The Shape of the Body.</param>
        /// <param name="massInfo">A object describing the mass and inertia of the Body.</param>
        /// <param name="coefficients">A object containing coefficients.</param>
        /// <param name="lifeTime">A object Describing how long the object will be in the engine.</param>
        public Body(
            PhysicsState state,
            IShape shape,
            MassInfo massInfo,
            Coefficients coefficients,
            Lifespan lifetime)
        {
            if (state == null) { throw new ArgumentNullException("state"); }
            if (shape == null) { throw new ArgumentNullException("shape"); }
            if (massInfo == null) { throw new ArgumentNullException("massInfo"); }
            if (coefficients == null) { throw new ArgumentNullException("coefficients"); }
            if (lifetime == null) { throw new ArgumentNullException("lifetime"); }
            Initialize();
            this.matrices = new Matrices();
            this.transformation = Matrix2x3.Identity;

            this.state = new PhysicsState(state);
            this.Shape = shape;
            this.massInfo = massInfo;
            this.coefficients = coefficients;
            this.lifetime = lifetime;
            this.linearDamping = 1;
            this.angularDamping = 1;
            this.isCollidable = true;
            this.isEventable = true;
            this.ApplyPosition();
        }

        private Body(Body copy)
        {
            Initialize();
            this.ignoresCollisionResponce = copy.ignoresCollisionResponce;
            this.shape = copy.shape;
            this.massInfo = copy.massInfo;
            this.coefficients = copy.coefficients;
            this.collisionIgnorer = copy.collisionIgnorer;
            this.matrices = copy.matrices.Duplicate();
            this.state = copy.state.Duplicate();
            this.lifetime = copy.lifetime.Duplicate();

            this.transformation = copy.transformation;
            this.linearDamping = copy.linearDamping;
            this.angularDamping = copy.angularDamping;

            this.ignoresCollisionResponce = copy.ignoresCollisionResponce;
            this.ignoresGravity = copy.ignoresGravity;
            this.ignoresPhysicsLogics = copy.ignoresPhysicsLogics;
            this.isTransformed = copy.isTransformed;
            this.isCollidable = copy.isCollidable;
            this.isEventable = copy.isEventable;

            this.tag = copy.tag;
        }

        private void Initialize()
        {
            this.id = -1;
            this.joints = new List<Joint>();
            this.proxies = new LinkedList<BodyProxy>();
        }
        #endregion
        #region properties
        public Dictionary<string, object> Tags
        {
            get { if (tags == null) { new Dictionary<string, object>(); } return tags; }
        }

        public bool IsFrozen
        {
            get { return isFrozen; }
            internal set 
            {
                if (!value && 
                    massInfo.MassInv == 0 && 
                    massInfo.MomentOfInertiaInv == 0 && 
                    state.Velocity == ALVector2D.Zero)
                {
                    return;
                }
                
                isFrozen = value; }
        }

        /// <summary>
        /// This is the Baunding rectangle It is calculated on the call to apply matrix.
        /// </summary>
        public BoundingRectangle Rectangle
        {
            get { return rectangle; }
        }
        /// <summary>
        /// The Matrices that are tranfroming this bodies Shape.
        /// </summary>
        public Matrices Matrices
        {
            get { return matrices; }
        }
        /// <summary>
        /// Gets and Sets The value represents how much Linear velocity is kept each time step. 
        /// This Dampens the Body's Linear velocity a little per time step. Valid values are zero exclusive to one inclusive.  
        /// </summary>
        public Scalar LinearDamping
        {
            get { return linearDamping; }
            set
            {
                if (value <= 0 || value > 1) { throw new ArgumentOutOfRangeException("value"); }
                linearDamping = value;
            }
        }
        /// <summary>
        /// Gets and Sets The value represents how much Angular velocity is kept each time step. 
        /// This Dampens the Body's Angular velocity a little per time step. Valid values are zero exclusive to one inclusive.  
        /// </summary>
        public Scalar AngularDamping
        {
            get { return angularDamping; }
            set
            {
                if (value <= 0 || value > 1) { throw new ArgumentOutOfRangeException("value"); }
                angularDamping = value;
            }
        }
        /// <summary>
        /// These are bodies that are mirrors of this body. 
        /// It's useful for bodies that are being teleported.
        /// </summary>
        public IEnumerable<BodyProxy> Proxies
        {
            get
            {
                for (LinkedListNode<BodyProxy> node = proxies.First;
                    node != null;
                    node = node.Next)
                {
                    yield return node.Value;
                }
            }
        }
        /// <summary>
        /// The number of proxies that this body has.
        /// </summary>
        public int ProxiesCount { get { return proxies.Count; } }
        /// <summary>
        /// Unique ID of a PhysicsEntity in the PhysicsEngine
        /// Assigned on being Added.
        /// </summary>
        public int ID
        {
            get { return id; }
            internal set { id = value; }
        }
        /// <summary>
        /// Gets The PhysicsEngine the object is currently in. Null if it is in none.
        /// </summary>
        public PhysicsEngine Engine
        {
            get { return engine; }
        }
        /// <summary>
        /// Gets The current State of the object IE Velocity 
        /// </summary>
        public PhysicsState State
        {
            get { return state; }
        }
        /// <summary>
        /// Gets and Sets the Shape of the Body. 
        /// If setting the shape to a shape another body has it will duplicate the shape.
        /// </summary>
        public IShape Shape
        {
            set
            {
                if (value == null) { throw new ArgumentNullException("value"); }
                if (value != this.shape)
                {
                    this.shape = value;
                    if (ShapeChanged != null) { ShapeChanged(this, EventArgs.Empty); }
                }
            }
            get { return shape; }
        }
        /// <summary>
        /// Gets The MassInfo of the body.
        /// </summary>
        public MassInfo Mass
        {
            get { return massInfo; }
            set { massInfo = value; }
        }
        /// <summary>
        /// Gets and Sets the Ignore object that decides what collisons to ignore.
        /// </summary>
        public Ignorer CollisionIgnorer
        {
            get { return collisionIgnorer; }
            set { collisionIgnorer = value; }
        }
        /// <summary>
        /// Gets and Sets the Ignore object that decides what collison events to ignore.
        /// </summary>
        public Ignorer EventIgnorer
        {
            get { return eventIgnorer; }
            set { eventIgnorer = value; }
        }
        /// <summary>
        /// Gets and Sets the Coefficients for the class.
        /// </summary>
        public Coefficients Coefficients
        {
            get { return coefficients; }
            set
            {
                if (value == null) { throw new ArgumentNullException("value"); }
                coefficients = value;
            }
        }
        /// <summary>
        /// Gets and Sets the LifeTime of the object. The object will be removed from the engine when it is Expired.
        /// </summary>
        public Lifespan Lifetime
        {
            get
            {
                return lifetime;
            }
            set
            {
                if (value == null) { throw new ArgumentNullException("value"); }
                if (this.lifetime != value)
                {
                    lifetime = value;
                    if (LifetimeChanged != null) { LifetimeChanged(this, EventArgs.Empty); }
                }
            }
        }
        /// <summary>
        /// Gets and Sets a User defined object.
        /// </summary>
        public object Tag
        {
            get
            {
                return tag;
            }
            set
            {
                tag = value;
            }
        }
        /// <summary>
        /// Gets a Solver Defined object.
        /// </summary>
        public object SolverTag
        {
            get { return solverTag; }
            internal set { solverTag = value; }
        }
        /// <summary>
        /// Gets a Detector Defined object.
        /// </summary>
        public object DetectorTag
        {
            get { return detectorTag; }
            internal set { detectorTag = value; }
        }
        /// <summary>
        /// Gets the Joints attached to this body.
        /// </summary>
        public ReadOnlyCollection<Joint> Joints { get { return new ReadOnlyCollection<Joint>(joints); } }
        /// <summary>
        /// Gets and Sets if the Body will ignore Gravity.
        /// </summary>
        public bool IgnoresGravity
        {
            get { return ignoresGravity; }
            set { ignoresGravity = value; }
        }
        /// <summary>
        /// Gets and Sets if the Body will Ignore all Physics Logics
        /// </summary>
        public bool IgnoresPhysicsLogics
        {
            get { return ignoresPhysicsLogics; }
            set { ignoresPhysicsLogics = value; }
        }
        /// <summary>
        /// Gets and Sets if the Object will ignore the collison Responce but still generate the Collision event.
        /// </summary>
        public bool IgnoresCollisionResponse
        {
            get { return ignoresCollisionResponce; }
            set { ignoresCollisionResponce = value; }
        }
        /// <summary>
        /// Gets and Sets if this body's shape's Vertexes will be used in collision detection.
        /// </summary>
        public bool IgnoreVertexes
        {
            get { return ignoreVertexes; }
            set { ignoreVertexes = value; }
        }
        /// <summary>
        /// Gets if it has been added the the Engine's PendingQueue, but not yet added to the engine.
        /// </summary>
        public bool IsPending
        {
            get { return engine != null && !isAdded; }
        }
        /// <summary>
        /// Gets if the object has been added to the engine.
        /// </summary>
        public bool IsAdded
        {
            get
            {
                return isAdded;
            }
        }
        /// <summary>
        /// gets and sets if the body will have any collision detection ran on it.
        /// </summary>
        public bool IsCollidable
        {
            get { return isCollidable; }
            set { isCollidable = value; }
        }
        /// <summary>
        /// Gets and Sets if other objects will have their collided 
        /// event raised when colliding with this body
        /// </summary>
        public bool IsEventable
        {
            get { return isEventable; }
            set { isEventable = value; }
        }
        /// <summary>
        /// Gets and Sets if the Body will trigger the Collided event at the broadphase level
        /// </summary>
        public bool IsBroadPhaseOnly
        {
            get { return isBroadPhaseOnly; }
            set { isBroadPhaseOnly = value; }
        }
        /// <summary>
        /// Gets the Total Kinetic Energy of the Body.
        /// </summary>
        public Scalar KineticEnergy
        {
            get
            {
                Scalar velocityMag;
                Vector2D.GetMagnitude(ref state.Velocity.Linear,out velocityMag);
                return
                    .5f * (velocityMag * velocityMag * massInfo.Mass +
                    state.Velocity.Angular * state.Velocity.Angular * massInfo.MomentOfInertia);
            }
        }
        /// <summary>
        /// Gets and Sets the Matrix2x3 that transforms the Shape belonging to the Body.
        /// </summary>
        public Matrix2x3 Transformation
        {
            get { return transformation; }
            set
            {
                transformation = value;
                isTransformed = value != Matrix2x3.Identity;
                transformChanged = true;
            }
        }
        public bool IsTransformed{ get { return isTransformed; } }
        #endregion
        #region methods
        internal void AddJoint(Joint joint)
        {
            this.joints.Add(joint);
            if (JointAdded != null) { JointAdded(this, new BodyJointEventArgs(joint)); }
        }
        internal void RemoveJoint(Joint joint)
        {
            if (this.joints.Remove(joint))
            {
                if (JointRemoved != null) { JointRemoved(this, new BodyJointEventArgs(joint)); }
            }
        }
        /// <summary>
        /// This applys the proxy.
        /// This will cause all other bodies in the proxy list to have their velocity set 
        /// to this body’s. 
        /// With the appropriate transformations applied. 
        /// </summary>
        public void ApplyProxy()
        {
            if (proxies.Count == 0) { return; }
            for (LinkedListNode<BodyProxy> node = proxies.First;
                node != null;
                node = node.Next)
            {
                BodyProxy proxy = node.Value;
                PhysicsState state = proxy.Body2.state;
                state.Velocity.Angular = this.state.Velocity.Angular;
                Vector2D.Transform(
                    ref proxy.transformation,
                    ref this.state.Velocity.Linear,
                    out state.Velocity.Linear);
            }
        }
        /// <summary>
        /// This will remove this body from any proxy list it is a part of.
        /// </summary>
        private void RemoveFromProxy()
        {
            if (proxies.Count == 0) { return; }
            for (LinkedListNode<BodyProxy> node = proxies.First;
                node != null;
                node = node.Next)
            {
                BodyProxy proxy = node.Value.invertedTwin;
                proxy.node.List.Remove(proxy.node);
            }
            proxies.Clear();
        }

        public void UpdatePosition(TimeStep step)
        {
            state.Position.Linear.X += state.Velocity.Linear.X * step.Dt;
            state.Position.Linear.Y += state.Velocity.Linear.Y * step.Dt;
            state.Position.Angular += state.Velocity.Angular * step.Dt;
        }
        public void UpdatePosition(TimeStep step, ALVector2D extraVelocity)
        {
            UpdatePosition(step, ref extraVelocity);
        }
        [CLSCompliant(false)]
        public void UpdatePosition(TimeStep step, ref ALVector2D extraVelocity)
        {
            state.Position.Linear.X += (state.Velocity.Linear.X + extraVelocity.Linear.X) * step.Dt;
            state.Position.Linear.Y += (state.Velocity.Linear.Y + extraVelocity.Linear.Y) * step.Dt;
            state.Position.Angular += (state.Velocity.Angular + extraVelocity.Angular) * step.Dt;
        }
        public void UpdateVelocity(TimeStep step)
        {
            if (!isFrozen)
            {
                if (massInfo.MassInv != 0)
                {
                    UpdateAcceleration(ref state.Acceleration, ref state.ForceAccumulator);
                }
                UpdateVelocity(ref state.Velocity, ref state.Acceleration, step.Dt);
            }
            if (proxies.Count != 0)
            {
                ApplyProxy();
            }
        }
        void UpdateAcceleration(ref ALVector2D acceleration, ref ALVector2D forceAccumulator)
        {
            Scalar proxDamping = 1.0f / (proxies.Count + 1);
            acceleration.Linear.X = acceleration.Linear.X * proxDamping + forceAccumulator.Linear.X * massInfo.MassInv;
            acceleration.Linear.Y = acceleration.Linear.Y * proxDamping + forceAccumulator.Linear.Y * massInfo.MassInv;
            acceleration.Angular += forceAccumulator.Angular * massInfo.MomentOfInertiaInv;
        }
        void UpdateVelocity(ref ALVector2D velocity, ref ALVector2D acceleration, Scalar dt)
        {
            velocity.Linear.X = velocity.Linear.X * linearDamping + acceleration.Linear.X * dt;
            velocity.Linear.Y = velocity.Linear.Y * linearDamping + acceleration.Linear.Y * dt;
            velocity.Angular = velocity.Angular * angularDamping + acceleration.Angular * dt;
        }

        internal void UpdateTime(TimeStep step)
        {
            lifetime.Update(step);
            if (collisionIgnorer != null) { collisionIgnorer.UpdateTime(step); }
            if (Updated != null) { Updated(this, new UpdatedEventArgs(step)); }
        }
        /// <summary>
        /// Updates all the values caluclated from the State.Position.
        /// Re-calculates the Matrices property the re-calculates the Rectangle property
        /// from that.
        /// </summary>
        public void ApplyPosition()
        {
            MathHelper.ClampAngle(ref state.Position.Angular);
            Matrix2x3 matrix;
            ALVector2D.ToMatrix2x3(ref state.Position, out matrix);
            Matrix2x3.Multiply(ref matrix, ref transformation, out matrix);
            matrices.SetToWorld(ref matrix);
            shape.CalcBoundingRectangle(ref matrix, out rectangle);
            if (engine == null || !engine.inUpdate)
            {
                OnPositionChanged();
            }
        }

        /// <summary>
        /// Sets Acceleration and ForceAccumulator to Zero.
        /// </summary>
        public void ClearForces()
        {
            this.state.Acceleration = ALVector2D.Zero;
            this.state.ForceAccumulator = ALVector2D.Zero;
        }

        /// <summary>
        /// Applys a Force
        /// </summary>
        /// <param name="force">The direction and magnitude of the force</param>
        public void ApplyForce(Vector2D force)
        {
            Vector2D.Add(ref state.ForceAccumulator.Linear, ref force, out state.ForceAccumulator.Linear);
        }
        /// <summary>
        /// Applys a Force
        /// </summary>
        /// <param name="force">The direction and magnitude of the force</param>
        [CLSCompliant(false)]
        public void ApplyForce(ref Vector2D force)
        {
            Vector2D.Add(ref state.ForceAccumulator.Linear, ref force, out state.ForceAccumulator.Linear);
        }
        /// <summary>
        /// Applys a Force
        /// </summary>
        /// <param name="force">The direction and magnitude of the force</param>
        /// <param name="position">The Location where the force will be applied (Offset: Body Rotation: World) </param>
        public void ApplyForce(Vector2D force, Vector2D position)
        {
            Scalar torque;
            Vector2D.Add(ref state.ForceAccumulator.Linear, ref force, out state.ForceAccumulator.Linear);
            Vector2D.ZCross(ref position, ref force, out torque);
            state.ForceAccumulator.Angular += torque;
        }
        /// <summary>
        /// Applys a Force
        /// </summary>
        /// <param name="force">The direction and magnitude of the force</param>
        /// <param name="position">The Location where the force will be applied (Offset: Body Rotation: World) </param>
        [CLSCompliant(false)]
        public void ApplyForce(ref Vector2D force, ref Vector2D position)
        {
            Scalar torque;
            Vector2D.Add(ref state.ForceAccumulator.Linear, ref force, out state.ForceAccumulator.Linear);
            Vector2D.ZCross(ref position, ref force, out torque);
            state.ForceAccumulator.Angular += torque;
        }
        /// <summary>
        /// Applys Torque
        /// </summary>
        /// <param name="torque">The direction and magnitude of the torque</param>
        public void ApplyTorque(Scalar torque)
        {
            state.ForceAccumulator.Angular += torque;
        }

        /// <summary>
        /// Applys Impulse
        /// </summary>
        /// <param name="impulse">The direction and magnitude of the impulse</param>
        public void ApplyImpulse(Vector2D impulse)
        {
            ApplyImpulse(ref impulse);
        }
        /// <summary>
        /// Applys Impulse
        /// </summary>
        /// <param name="impulse">The direction and magnitude of the impulse.</param>
        [CLSCompliant(false)]
        public void ApplyImpulse(ref Vector2D impulse)
        {
            Scalar massInv = massInfo.MassInv;
            state.Velocity.Linear.X += impulse.X * massInv;
            state.Velocity.Linear.Y += impulse.Y * massInv;
        }
        /// <summary>
        /// Applys Impulse
        /// </summary>
        /// <param name="impulse">The direction and magnitude of the impulse.</param>
        /// <param name="position">The Location where the impulse will be applied (Offset: Body Rotation: World)</param>
        public void ApplyImpulse(Vector2D impulse, Vector2D position)
        {
            ApplyImpulse(ref impulse, ref position);
        }
        /// <summary>
        /// Applys Impulse
        /// </summary>
        /// <param name="impulse">The direction and magnitude of the impulse.</param>
        /// <param name="position">The Location where the impulse will be applied (Offset: Body Rotation: World)</param>
        [CLSCompliant(false)]
        public void ApplyImpulse(ref Vector2D impulse, ref Vector2D position)
        {
            Scalar massInv = massInfo.MassInv;
            Scalar IInv = massInfo.MomentOfInertiaInv;
            PhysicsHelper.AddImpulse(ref state.Velocity, ref impulse, ref position, ref massInv, ref IInv);
        }


        public Body Duplicate()
        {
            return new Body(this);
        }
        public object Clone()
        {
            return Duplicate();
        }

        async internal void OnCollision(TimeStep step, Body other, IContact contact)
        {
            if (Collided != null &&
                other.IsEventable &&
                (eventIgnorer == null ||
                Ignorer.CanCollide(this,other, eventIgnorer,other.eventIgnorer)))
            {
                await using var obj = new CollisionEventArgs(step, other, contact);
                Collided(this, obj);
            }
        }
        async internal void OnCollision(TimeStep step, Body other, object customIntersectionInfo)
        {
            if (Collided != null &&
                other.IsEventable &&
                (eventIgnorer == null ||
                Ignorer.CanCollide(this, other, eventIgnorer, other.eventIgnorer)))
            {
                await using var obj = new CollisionEventArgs(step, other, customIntersectionInfo);
                Collided(this, obj);
            }
        }

        async internal void OnColliding( TimeStep step, Body other, IContact contact )
        {
            if ( Colliding != null &&
                other.IsEventable &&
                ( eventIgnorer == null ||
                Ignorer.CanCollide( this, other, eventIgnorer, other.eventIgnorer ) ) )
            {
                await using var obj = new CollisionEventArgs(step, other, contact);
                Colliding(this, obj);
            }
        }

        internal void OnPositionChanged()
        {
            if (PositionChanged != null &&
                (transformChanged ||
                !ALVector2D.Equals(ref lastPosition, ref state.Position)))
            {
                PositionChanged(this, EventArgs.Empty);
                lastPosition = state.Position;
                transformChanged = false;
            }
        }
        internal void OnPending(PhysicsEngine engine)
        {
            this.isChecked = false;
            this.engine = engine;
            if (Pending != null) { Pending(this, EventArgs.Empty); }
        }
        internal void OnAdded()
        {
            this.isAdded = true;
            if (Added != null) { Added(this, EventArgs.Empty); }
        }
        internal void OnRemoved()
        {
            bool wasPending = this.IsPending;
            PhysicsEngine engine = this.engine;
            this.engine = null;
            this.id = -1;
            this.isAdded = false;
            this.RemoveFromProxy();
            if (Removed != null) { Removed(this, new RemovedEventArgs(engine, wasPending)); }
        }
        #endregion
    }
}