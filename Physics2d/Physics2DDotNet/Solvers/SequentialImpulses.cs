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



// because this code was basically copied from Box2D
// Copyright (c) 2006 Erin Catto http://www.gphysics.com
#if UseDouble
using Scalar = System.Double;
#else
using Scalar = System.Single;
#endif
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;

using AdvanceMath;
using AdvanceMath.Geometry2D;
using Physics2DDotNet.Shapes;
using Physics2DDotNet.Joints;



namespace Physics2DDotNet.Solvers
{
    sealed class SequentialImpulsesTag
    {
        public ALVector2D lastAccel;
        public ALVector2D biasVelocity;
        public Body body;
        public SequentialImpulsesTag(Body body)
        {
            this.body = body;
        }
    }
    public sealed class SequentialImpulsesSolver : CollisionSolver
    {
        #region sub-classes
        sealed class ContactPoint : IContactPointInfo
        {
            public int id;
            public Vector2D position;
            public Vector2D normal;
            public Scalar distance;
            public Scalar Pn;
            public Scalar Pt;
            public Scalar Pnb;	// accumulated normal impulse for position bias
            public Scalar massNormal;
            public Scalar massTangent;
            public Scalar bias;
            public Vector2D r1;
            public Vector2D r2;


            Vector2D IContactPointInfo.Position
            {
                get { return position; }
            }
            Vector2D IContactPointInfo.Normal
            {
                get { return normal; }
            }
            Scalar IContactPointInfo.Distance
            {
                get { return distance; }
            }
        }
        sealed class Arbiter : IContact
        {


            static ContactPoint[] Empty = new ContactPoint[0];
            static Scalar ZeroClamp(Scalar value)
            {
                return ((value < 0) ? (0) : (value));
            }

            public event EventHandler Updated;

            public event EventHandler Ended;

            ContactState state;


            int lastUpdate;


            CircleShape circle1;
            CircleShape circle2;

            LinkedList<ContactPoint> contacts;
            ContactPoint[] contactsArray;

            public Body body1;
            public Body body2;
            SequentialImpulsesTag tag1;
            SequentialImpulsesTag tag2;

            SequentialImpulsesSolver parent;
            Scalar restitution;
            bool ignoresCollisionResponse;


            Scalar friction;
            public Arbiter(SequentialImpulsesSolver parent, Body body1, Body body2)
            {
                if (body1.ID < body2.ID)
                {
                    this.body1 = body1;
                    this.body2 = body2;
                }
                else
                {
                    this.body1 = body2;
                    this.body2 = body1;
                }
                this.tag1 = (SequentialImpulsesTag)this.body1.SolverTag;
                this.tag2 = (SequentialImpulsesTag)this.body2.SolverTag;
                this.circle1 = this.body1.Shape as CircleShape;
                this.circle2 = this.body2.Shape as CircleShape;
                this.friction = MathHelper.Sqrt(
                        this.body1.Coefficients.DynamicFriction *
                        this.body2.Coefficients.DynamicFriction);
                this.restitution = Math.Min(body1.Coefficients.Restitution, body2.Coefficients.Restitution);
                this.parent = parent;
                this.contacts = new LinkedList<ContactPoint>();
                this.lastUpdate = -1;
                this.state = ContactState.New;
                this.ignoresCollisionResponse = body1.IgnoresCollisionResponse || body2.IgnoresCollisionResponse;
            }


            public bool IgnoresCollisionResponse
            {
                get { return ignoresCollisionResponse; }
            }
            public ContactState State
            {
                get { return state; }
            }
            public int LastUpdate
            {
                get { return lastUpdate; }
            }
            bool everCollided;

            public void Update(TimeStep step)
            {
                if (lastUpdate != -1)
                {
                    if (lastUpdate != step.UpdateCount - 1)
                    {
                        state = ContactState.New;
                    }
                    else
                    {
                        state = ContactState.Old;
                    }
                }


                if ((!body1.IsFrozen || !body2.IsFrozen))
                {
                    if (circle1 != null && circle2 != null &&
                        !body1.IsTransformed && !body2.IsTransformed)
                    {
                        CollideCircles();
                    }
                    else
                    {
                        Collide();
                    }
                    UpdateContacts();
                    if (Collided)
                    {
                        body1.IsFrozen = false;
                        body2.IsFrozen = false;
                        everCollided = true;
                    }
                }
                if (Collided)
                {
                    lastUpdate = step.UpdateCount;
                }

                if (Updated != null &&
                    contactsArray.Length != 0)
                {
                    Updated(this, EventArgs.Empty);
                }
            }
            void UpdateContacts()
            {
                if (contacts.Count == 0)
                {
                    contactsArray = Empty;
                    return;
                }
                if (contactsArray == null || contactsArray.Length != contacts.Count)
                {
                    contactsArray = new ContactPoint[contacts.Count];
                }
                contacts.CopyTo(contactsArray, 0);
            }


            void CollideCircles()
            {
                Vector2D center1 = Vector2D.Zero;
                Vector2D center2 = Vector2D.Zero;
                Vector2D.Transform(ref  body1.Matrices.ToWorld, ref center1, out center1);
                Vector2D.Transform(ref  body2.Matrices.ToWorld, ref center2, out center2);
                Vector2D normal;
                Vector2D.Subtract(ref  center2, ref center1, out normal);
                Scalar distance;
                Vector2D.Normalize(ref normal, out distance, out normal);
                Scalar depth = distance - (circle1.Radius + circle2.Radius);
                if (depth > 0)
                {
                    contacts.Clear();
                }
                else
                {
                    ContactPoint contact;
                    if (contacts.First == null)
                    {
                        contact = new ContactPoint();
                        contacts.AddLast(contact);
                    }
                    else
                    {
                        contact = contacts.First.Value;
                    }
                    contact.distance = depth;
                    contact.normal = normal;
                    contact.position.X = center2.X - normal.X * circle2.Radius;
                    contact.position.Y = center2.Y - normal.Y * circle2.Radius;
                }
            }
            void Collide()
            {
                BoundingRectangle bb1 = body1.Rectangle;
                BoundingRectangle bb2 = body2.Rectangle;
                BoundingRectangle targetArea;
                BoundingRectangle.FromIntersection(ref bb1, ref bb2, out targetArea);
                LinkedListNode<ContactPoint> node = contacts.First;
                if (!body2.IgnoreVertexes &&
                    body1.Shape.CanGetIntersection)
                {
                    Collide(ref node, this.body1, this.body2, false, ref targetArea);
                }
                if (!body1.IgnoreVertexes &&
                    body2.Shape.CanGetIntersection)
                {
                    Collide(ref node, this.body2, this.body1, true, ref targetArea);
                }
            }
            void Collide(ref LinkedListNode<ContactPoint> node, Body b1, Body b2, bool inverse, ref BoundingRectangle targetArea)
            {
                Vector2D[] vertexes = b2.Shape.Vertexes;
                Vector2D[] normals = b2.Shape.VertexNormals;


                Matrix2x3 b2ToWorld = b2.Matrices.ToWorld;
                Matrix2x3 b1ToBody = b1.Matrices.ToBody;
                Matrix2x2 b1ToWorldNormal = b1.Matrices.ToWorldNormal;

                Matrix2x2 normalM;
                Matrix2x2.Multiply(ref b1.Matrices.ToBodyNormal, ref b2.Matrices.ToWorldNormal, out normalM);

                IntersectionInfo info = IntersectionInfo.Zero;
                ContainmentType contains;
                ContactPoint contact;

                for (int index = 0; index < vertexes.Length; ++index)
                {
                    Vector2D worldVertex;
                    Vector2D.Transform(ref b2ToWorld, ref vertexes[index], out worldVertex);
                    targetArea.Contains(ref worldVertex, out contains);
                    bool isBad = (contains != ContainmentType.Contains);
                    if (!isBad)
                    {
                        Vector2D bodyVertex;
                        Vector2D.Transform(ref b1ToBody, ref worldVertex, out bodyVertex);
                        isBad = !b1.Shape.TryGetIntersection(bodyVertex, out info);
                        if (!isBad && normals != null &&
                            !body1.IgnoresCollisionResponse &&
                            !body2.IgnoresCollisionResponse)
                        {
                            Vector2D normal;
                            Vector2D.Transform(ref normalM, ref  normals[index], out normal);
                            Scalar temp;
                            temp = Vector2D.Dot(info.Normal, normal);
                            isBad = temp >= 0;
                        }
                    }

                    int Id = (inverse) ? (index) : ((-vertexes.Length + index));
                    while (node != null && node.Value.id < Id) { node = node.Next; }

                    if (isBad)
                    {
                        if (node != null && node.Value.id == Id)
                        {
                            LinkedListNode<ContactPoint> nextNode = node.Next;
                            contacts.Remove(node);
                            node = nextNode;
                        }
                    }
                    else
                    {
                        if (node == null)
                        {
                            contact = new ContactPoint();
                            contact.id = Id;
                            contacts.AddLast(contact);
                        }
                        else if (node.Value.id == Id)
                        {
                            contact = node.Value;
                            node = node.Next;
                            if (!parent.warmStarting)
                            {
                                contact.Pn = 0;
                                contact.Pt = 0;
                                contact.Pnb = 0;
                            }
                        }
                        else
                        {
                            contact = new ContactPoint();
                            contact.id = Id;
                            contacts.AddBefore(node, contact);
                        }
                        contact.normal = Vector2D.Transform(b1ToWorldNormal, info.Normal);
                        contact.distance = info.Distance;
                        contact.position = worldVertex;
                        if (inverse)
                        {
                            Vector2D.Negate(ref contact.normal, out contact.normal);
                        }
                        Vector2D.Normalize(ref contact.normal, out contact.normal);
                    }
                }
            }
            public void PreApply(Scalar dtInv)
            {

                Scalar mass1Inv = body1.Mass.MassInv;
                Scalar I1Inv = body1.Mass.MomentOfInertiaInv;
                Scalar mass2Inv = body2.Mass.MassInv;
                Scalar I2Inv = body2.Mass.MomentOfInertiaInv;

                for (int index = 0; index < contactsArray.Length; ++index)
                {
                    ContactPoint c = contactsArray[index];
                    Vector2D.Subtract(ref c.position, ref body1.State.Position.Linear, out c.r1);
                    Vector2D.Subtract(ref c.position, ref body2.State.Position.Linear, out c.r2);

                    // Precompute normal mass, tangent mass, and bias.
                    PhysicsHelper.GetMassNormal(
                        ref c.r1, ref c.r2,
                        ref c.normal,
                        ref mass1Inv, ref I1Inv,
                        ref mass2Inv, ref I2Inv,
                        out c.massNormal);

                    Vector2D tangent;
                    PhysicsHelper.GetTangent(ref c.normal, out tangent);

                    PhysicsHelper.GetMassNormal(
                        ref c.r1, ref c.r2,
                        ref tangent,
                        ref mass1Inv, ref I1Inv,
                        ref mass2Inv, ref I2Inv,
                        out c.massTangent);

                    if (parent.positionCorrection)
                    {
                        c.bias = -parent.biasFactor * dtInv * Math.Min(0.0f, c.distance + parent.allowedPenetration);
                    }
                    else
                    {
                        c.bias = 0;
                    }
                    if (parent.accumulateImpulses)
                    {
                        // Apply normal + friction impulse
                        Vector2D vect1, vect2, P;

                        Scalar temp = (1 + this.restitution) * c.Pn;
                        Vector2D.Multiply(ref c.normal, ref temp, out vect1);
                        Vector2D.Multiply(ref tangent, ref c.Pt, out vect2);
                        Vector2D.Add(ref vect1, ref vect2, out P);

                        PhysicsHelper.SubtractImpulse(
                            ref body1.State.Velocity,
                            ref P,
                            ref c.r1,
                            ref mass1Inv,
                            ref I1Inv);

                        PhysicsHelper.AddImpulse(
                            ref body2.State.Velocity,
                            ref P,
                            ref c.r2,
                            ref mass2Inv,
                            ref I2Inv);
                    }
                    // Initialize bias impulse to zero.
                    c.Pnb = 0;
                }
                body1.ApplyProxy();
                body2.ApplyProxy();
            }
            public void Apply()
            {
                Body b1 = body1;
                Body b2 = body2;

                Scalar mass1Inv = b1.Mass.MassInv;
                Scalar I1Inv = b1.Mass.MomentOfInertiaInv;
                Scalar mass2Inv = b2.Mass.MassInv;
                Scalar I2Inv = b2.Mass.MomentOfInertiaInv;

                PhysicsState state1 = b1.State;
                PhysicsState state2 = b2.State;

                for (int index = 0; index < contactsArray.Length; ++index)
                {
                    ContactPoint c = contactsArray[index];

                    // Relative velocity at contact
                    Vector2D dv;
                    PhysicsHelper.GetRelativeVelocity(
                        ref state1.Velocity,
                        ref state2.Velocity,
                        ref c.r1, ref c.r2, out dv);

                    // Compute normal impulse
                    Scalar vn;
                    Vector2D.Dot(ref dv, ref c.normal, out vn);
                    //Scalar vn = Vector2D.Dot(dv, c.normal);

                    Scalar dPn;
                    if (parent.splitImpulse)
                    {
                        dPn = c.massNormal * (-vn);
                    }
                    else
                    {
                        dPn = c.massNormal * (c.bias - vn);
                    }


                    if (parent.accumulateImpulses)
                    {
                        // Clamp the accumulated impulse
                        Scalar Pn0 = c.Pn;
                        c.Pn = ZeroClamp(Pn0 + dPn);
                        //c.Pn = Math.Max(Pn0 + dPn, 0.0f);
                        dPn = c.Pn - Pn0;
                    }
                    else
                    {
                        //dPn = Math.Max(dPn, 0.0f);
                        dPn = ZeroClamp(dPn);
                    }

                    // Apply contact impulse
                    Vector2D Pn;
                    Vector2D.Multiply(ref  c.normal, ref dPn, out Pn);
                    //Vector2D Pn = dPn * c.normal;

                    PhysicsHelper.SubtractImpulse(
                        ref state1.Velocity,
                        ref Pn,
                        ref c.r1,
                        ref mass1Inv,
                        ref I1Inv);

                    PhysicsHelper.AddImpulse(
                        ref state2.Velocity,
                        ref Pn,
                        ref c.r2,
                        ref mass2Inv,
                        ref I2Inv);


                    if (parent.splitImpulse)
                    {
                        // Compute bias impulse
                        PhysicsHelper.GetRelativeVelocity(
                            ref tag1.biasVelocity,
                            ref tag2.biasVelocity,
                            ref c.r1, ref c.r2, out dv);



                        Scalar vnb;
                        Vector2D.Dot(ref dv, ref c.normal, out vnb);
                        //Scalar vnb = Vector2D.Dot(dv, c.normal);

                        Scalar dPnb = c.massNormal * (c.bias - vnb);
                        Scalar Pnb0 = c.Pnb;
                        c.Pnb = ZeroClamp(Pnb0 + dPnb);
                        // c.Pnb = Math.Max(Pnb0 + dPnb, 0.0f);
                        dPnb = c.Pnb - Pnb0;

                        Vector2D Pb;
                        Vector2D.Multiply(ref dPnb, ref c.normal, out Pb);
                        //Vector2D Pb = dPnb * c.normal;


                        PhysicsHelper.SubtractImpulse(
                            ref tag1.biasVelocity,
                            ref Pb,
                            ref c.r1,
                            ref mass1Inv,
                            ref I1Inv);

                        PhysicsHelper.AddImpulse(
                            ref tag2.biasVelocity,
                            ref Pb,
                            ref c.r2,
                            ref mass2Inv,
                            ref I2Inv);
                    }

                    // Relative velocity at contact

                    PhysicsHelper.GetRelativeVelocity(
                        ref state1.Velocity,
                        ref state2.Velocity,
                        ref c.r1, ref c.r2, out dv);


                    Vector2D tangent;
                    PhysicsHelper.GetTangent(ref c.normal, out tangent);

                    Scalar vt;
                    Vector2D.Dot(ref dv, ref tangent, out vt);
                    //Scalar vt = Vector2D.Dot(dv, tangent);
                    Scalar dPt = c.massTangent * (-vt);




                    if (parent.accumulateImpulses)
                    {
                        // Compute friction impulse
                        Scalar maxPt = friction * c.Pn;
                        // Clamp friction
                        Scalar oldTangentImpulse = c.Pt;
                        c.Pt = MathHelper.Clamp(oldTangentImpulse + dPt, -maxPt, maxPt);
                        dPt = c.Pt - oldTangentImpulse;
                    }
                    else
                    {
                        // Compute friction impulse
                        Scalar maxPt = friction * dPn;
                        dPt = MathHelper.Clamp(dPt, -maxPt, maxPt);
                    }


                    // Apply contact impulse
                    Vector2D Pt;
                    Vector2D.Multiply(ref tangent, ref dPt, out Pt);

                    //Vector2D Pt = dPt * tangent;

                    PhysicsHelper.SubtractImpulse(
                        ref state1.Velocity,
                        ref Pt,
                        ref c.r1,
                        ref mass1Inv,
                        ref I1Inv);

                    PhysicsHelper.AddImpulse(
                        ref state2.Velocity,
                        ref Pt,
                        ref c.r2,
                        ref mass2Inv,
                        ref I2Inv);
                }
                body1.ApplyProxy();
                body2.ApplyProxy();
            }
            public bool Collided
            {
                get { return contactsArray != null && contactsArray.Length > 0; }
            }
            public void OnRemoved()
            {
                if (everCollided)
                {
                    body1.IsFrozen = false;
                    body2.IsFrozen = false;
                   // body1.idleCount -= 3;
                   // body2.idleCount -= 3;
                }
                this.state = ContactState.Ended;
                if (Ended != null) { Ended(this, EventArgs.Empty); }
            }



            Body IContact.Body1
            {
                get { return body1; }
            }

            Body IContact.Body2
            {
                get { return body2; }
            }

            ReadOnlyCollection<IContactPointInfo> IContact.Points
            {
                get
                {
                    return new ReadOnlyCollection<IContactPointInfo>(
                        new Physics2DDotNet.Collections.ImplicitCastCollection<IContactPointInfo, ContactPoint>(contactsArray));
                }
            }





        }
        #endregion
        #region static
        static bool IsJointRemoved(ISequentialImpulsesJoint joint)
        {
            return !joint.IsAdded;
        }
        static bool IsTagRemoved(SequentialImpulsesTag tag)
        {
            return !tag.body.IsAdded;
        }
        #endregion
        #region fields
        Dictionary<long, Arbiter> arbiters;
        List<ISequentialImpulsesJoint> siJoints;
        List<SequentialImpulsesTag> tags;
        bool splitImpulse = true;
        bool accumulateImpulses = true;
        bool warmStarting = true;
        bool positionCorrection = true ;
        bool freezing = false;



        int freezeTimeout = 50;
        ALVector2D freezeVelocityTolerance = new ALVector2D(.1f, 5, 5);

        int removeTimout = 5;


        Scalar biasFactor = 0.7f;
        Scalar allowedPenetration = 0.1f;
        int iterations = 12;

        List<long> empty = new List<long>();
        List<Arbiter> arbs2 = new List<Arbiter>();

        #endregion
        #region constructors
        public SequentialImpulsesSolver()
        {
            arbiters = new Dictionary<long, Arbiter>();
            siJoints = new List<ISequentialImpulsesJoint>();
            tags = new List<SequentialImpulsesTag>();
        }
        #endregion
        #region properties
        public bool PositionCorrection
        {
            get { return positionCorrection; }
            set { positionCorrection = value; }
        }
        public bool AccumulateImpulses
        {
            get { return accumulateImpulses; }
            set { accumulateImpulses = value; }
        }
        public bool SplitImpulse
        {
            get { return splitImpulse; }
            set { splitImpulse = value; }
        }
        public bool WarmStarting
        {
            get { return warmStarting; }
            set { warmStarting = value; }
        }
        public Scalar BiasFactor
        {
            get { return biasFactor; }
            set { biasFactor = value; }
        }
        public Scalar AllowedPenetration
        {
            get { return allowedPenetration; }
            set { allowedPenetration = value; }
        }
        public int Iterations
        {
            get { return iterations; }
            set { iterations = value; }
        }

        public bool Freezing
        {
            get { return freezing; }
            set { freezing = value; }
        }
        public int FreezeTimeout
        {
            get { return freezeTimeout; }
            set { freezeTimeout = value; }
        }
        public ALVector2D FreezeVelocityTolerance
        {
            get { return freezeVelocityTolerance; }
            set { freezeVelocityTolerance = value; }
        }

        #endregion
        #region methods
        protected internal override bool TryGetIntersection(TimeStep step, Body first, Body second, out IContact contact)
        {
            long id = PairID.GetId(first.ID, second.ID);
            Arbiter arbiter;
            if (arbiters.TryGetValue(id, out arbiter))
            {
                arbiter.Update(step);
               /* if (!arbiter.Collided)
                {
                    arbiter.OnRemoved();
                    arbiters.Remove(id);
                }*/
            }
            else
            {
                arbiter = new Arbiter(this, first, second);
                arbiter.Update(step);
                //if (arbiter.Collided)
                //{
                    arbiters.Add(id, arbiter);
                //}
            }
            contact = arbiter;
            return arbiter.Collided;
        }

        Arbiter[] RemoveEmpty(TimeStep step)
        {
            foreach (KeyValuePair<long, Arbiter> pair in arbiters)
            {
                Arbiter value = pair.Value;
                //if (!value.Collided || value.LastUpdate != step.UpdateCount)
                if (value.LastUpdate + removeTimout < step.UpdateCount)
                {
                    pair.Value.OnRemoved();
                    empty.Add(pair.Key);
                }
                else if (value.IgnoresCollisionResponse ||
                    value.body1.IsFrozen &&
                    value.body2.IsFrozen)
                {

                }
                else if (value.Collided && value.LastUpdate == step.UpdateCount)
                {
                    arbs2.Add(value);
                }
            }
            for (int index = 0; index < empty.Count; ++index)
            {
                arbiters.Remove(empty[index]);
            }
            Arbiter[] result = arbs2.ToArray();
            empty.Clear();
            arbs2.Clear();
            return result;
        }

 
        protected internal override void Solve(TimeStep step)
        {
            Detect(step);
            Arbiter[] arbs = RemoveEmpty(step);
            this.Engine.RunLogic(step);
            if (freezing)
            {
                for (int index = 0; index < siJoints.Count; ++index)
                {
                    siJoints[index].CheckFrozen();
                }
            }
            for (int index = 0; index < tags.Count; ++index)
            {
                SequentialImpulsesTag tag = tags[index];
                tag.biasVelocity = ALVector2D.Zero;

                if (freezing)
                {
                    bool accelSame = tag.body.State.Acceleration == tag.lastAccel;

                    ALVector2D vel = tag.body.State.Velocity;
                    ALVector2D force = tag.body.State.ForceAccumulator;


                    bool isVelZero =
                        Math.Abs(vel.X) < freezeVelocityTolerance.X &&
                        Math.Abs(vel.Y) < freezeVelocityTolerance.Y &&
                        Math.Abs(vel.Angular) < freezeVelocityTolerance.Angular;
                    bool isForceZero = tag.body.State.ForceAccumulator == ALVector2D.Zero;


                    if (accelSame && isVelZero && isForceZero)
                    {
                        if (tag.body.Joints.Count == 0)
                        {
                            tag.body.idleCount++;
                        }
                        if (tag.body.idleCount > freezeTimeout)
                        {
                            tag.body.idleCount = freezeTimeout;
                            tag.body.IsFrozen = true;
                            tag.body.State.Velocity = ALVector2D.Zero;
                        }
                    }
                    else
                    {
                        tag.body.IsFrozen = false;
                        tag.body.idleCount = 0;
                    }
                    tag.lastAccel = tag.body.State.Acceleration;
                    if (tag.body.IsFrozen)
                    {
                        tag.body.State.ForceAccumulator = ALVector2D.Zero;
                        tag.body.State.Acceleration = ALVector2D.Zero;
                    }
                }

                tag.body.UpdateVelocity(step);
                tag.body.ClearForces();
            }
            for (int index = 0; index < arbs.Length; ++index)
            {
                arbs[index].PreApply(step.DtInv);
            }
            for (int index = 0; index < siJoints.Count; ++index)
            {
                siJoints[index].PreStep(step);
            }
            for (int i = 0; i < iterations; ++i)
            {
                for (int index = 0; index < arbs.Length; ++index)
                {
                    arbs[index].Apply();
                }
                for (int index = 0; index < siJoints.Count; ++index)
                {
                    siJoints[index].ApplyImpulse();
                }
            }
            for (int index = 0; index < tags.Count; ++index)
            {
                SequentialImpulsesTag tag = tags[index];
                if (splitImpulse)
                {
                    tag.body.UpdatePosition(step, ref tag.biasVelocity);
                }
                else
                {
                    tag.body.UpdatePosition(step);
                }
                tag.body.ApplyPosition();
            }
        }
        protected internal override void AddBodyRange(List<Body> collection)
        {
            foreach (Body item in collection)
            {
                if (item.SolverTag == null)
                {
                    SequentialImpulsesTag tag = new SequentialImpulsesTag(item);
                    SetTag(item, tag);
                    tags.Add(tag);
                }
                else
                {
                    tags.Add((SequentialImpulsesTag)item.SolverTag);
                }
            }
        }
        protected internal override void AddJointRange(List<Joint> collection)
        {
            ISequentialImpulsesJoint[] newJoints = new ISequentialImpulsesJoint[collection.Count];
            for (int index = 0; index < newJoints.Length; ++index)
            {
                newJoints[index] = (ISequentialImpulsesJoint)collection[index];
            }
            siJoints.AddRange(newJoints);
        }
        protected internal override void Clear()
        {
            arbiters.Clear();
            siJoints.Clear();
            tags.Clear();
        }
        protected internal override void RemoveExpiredJoints()
        {
            siJoints.RemoveAll(IsJointRemoved);
        }
        protected internal override void RemoveExpiredBodies()
        {
            tags.RemoveAll(IsTagRemoved);
        }
        protected internal override void CheckJoint(Joint joint)
        {
            if (!(joint is ISequentialImpulsesJoint))
            {
                throw new ArgumentException("The joint must implement ISequentialImpulsesJoint to be added to this solver.");
            }
        } 
        #endregion
    }
}