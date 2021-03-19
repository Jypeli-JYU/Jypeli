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

using AdvanceMath;
using Physics2DDotNet.Shapes;

namespace Physics2DDotNet.PhysicsLogics
{
    /// <summary>
    /// Applys drag and buoyancy to all items in the engine.
    /// </summary>
    public sealed class GlobalFluidLogic : PhysicsLogic
    {
        sealed class Wrapper : IDisposable
        {
            public Body body;
            public IGlobalFluidAffectable affectable;
            public Wrapper(Body body)
            {
                this.body = body;
                this.body.ShapeChanged += OnShapeChanged;
                this.Calculate();
            }
            void Calculate()
            {
                affectable = body.Shape as IGlobalFluidAffectable;
            }
            void OnShapeChanged(object sender, EventArgs e)
            {
                Calculate();
            }
            public void Dispose()
            {
                this.body.ShapeChanged -= OnShapeChanged;
            }
        }
        static bool IsRemoved(Wrapper wrapper)
        {
            if (!wrapper.body.IsAdded)
            {
                wrapper.Dispose();
                return true;
            }
            return false;
        }

        Scalar dragCoefficient;
        Scalar density;
        Vector2D fluidVelocity;


        List<Wrapper> items;

        public GlobalFluidLogic(
            Scalar dragCoefficient,
            Scalar density,
            Vector2D fluidVelocity,
            Lifespan lifetime)
            : base(lifetime)
        {
            this.dragCoefficient = dragCoefficient;
            this.density = density;
            this.fluidVelocity = fluidVelocity;
            this.items = new List<Wrapper>();
            this.Order = 1;
        }

        public Scalar DragCoefficient
        {
            get { return dragCoefficient; }
            set { dragCoefficient = value; }
        }
        public Scalar Density
        {
            get { return density; }
            set { density = value; }
        }
        public Vector2D FluidVelocity
        {
            get { return fluidVelocity; }
            set { fluidVelocity = value; }
        }

        protected internal override void RunLogic(TimeStep step)
        {
            for (int index = 0; index < items.Count; ++index)
            {
                Wrapper wrapper = items[index];
                Body body = wrapper.body;
                if (wrapper.affectable == null ||
                    body.IgnoresPhysicsLogics ||
                   Scalar.IsPositiveInfinity(body.Mass.Mass))
                {
                    continue;
                }

                Vector2D centroid = wrapper.body.Matrices.ToWorldNormal * wrapper.affectable.Centroid;
                Vector2D buoyancyForce = body.State.Acceleration.Linear * wrapper.affectable.Area * -Density;
                wrapper.body.ApplyForce(buoyancyForce, centroid);

                Vector2D relativeVelocity = body.State.Velocity.Linear - FluidVelocity;
                Vector2D velocityDirection = relativeVelocity.Normalized;
                if (velocityDirection == Vector2D.Zero) { continue; }
                Vector2D dragDirection = body.Matrices.ToBodyNormal * velocityDirection.LeftHandNormal;
                DragInfo dragInfo = wrapper.affectable.GetFluidInfo(dragDirection);
                if (dragInfo.DragArea < .01f) { continue; }
                Scalar speedSq = relativeVelocity.MagnitudeSq;
                Scalar dragForceMag = -.5f * Density * speedSq * dragInfo.DragArea * DragCoefficient;
                Scalar maxDrag = -MathHelper.Sqrt(speedSq) * body.Mass.Mass * step.DtInv;
                if (dragForceMag < maxDrag)
                {
                    dragForceMag = maxDrag;
                }

                Vector2D dragForce = dragForceMag * velocityDirection;
                wrapper.body.ApplyForce(dragForce, body.Matrices.ToWorldNormal * dragInfo.DragCenter);

                wrapper.body.ApplyTorque(
                   -body.Mass.MomentOfInertia *
                   (body.Coefficients.DynamicFriction + Density + DragCoefficient) *
                   body.State.Velocity.Angular);
            }
        }
        protected internal override void RemoveExpiredBodies()
        {
            items.RemoveAll(IsRemoved);
        }
        protected internal override void AddBodyRange(List<Body> collection)
        {
            int newCapacity = collection.Count + items.Count;
            if (items.Capacity < newCapacity)
            {
                items.Capacity = newCapacity;
            }
            for (int index = 0; index < collection.Count; ++index)
            {
                items.Add(new Wrapper(collection[index]));
            }
        }
        protected internal override void Clear()
        {
            for(int index = 0; index < items.Count;++index)
            {
                items[index].Dispose();
            }
            items.Clear();
        }
    }
}