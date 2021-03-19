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

using AdvanceMath;

namespace Physics2DDotNet.PhysicsLogics
{
    /// <summary>
    /// A class that will apply Force to move an object to a certain point and stop it once it gets there.
    /// </summary>
    public sealed class MoveToPointLogic : PhysicsLogic
    {
        Scalar maxAcceleration;
        Scalar maxVelocity;
        Vector2D destination;
        Body body;
        /// <summary>
        /// Creates a new MoveToPointLogic object.
        /// </summary>
        /// <param name="body">The Body this logic will act on.</param>
        /// <param name="destination">The Point it will move the Body too.</param>
        /// <param name="maxAcceleration">The maximum acceleration to be applied to the Body</param>
        public MoveToPointLogic(Body body, Vector2D destination, Scalar maxAcceleration) : this(body, destination, maxAcceleration, Scalar.MaxValue) { }
        /// <summary>
        /// Creates a new MoveToPointLogic object.
        /// </summary>
        /// <param name="body">The Body this logic will act on.</param>
        /// <param name="destination">The Point it will move the Body too.</param>
        /// <param name="maxAcceleration">The maximum acceleration to be applied to the Body</param>
        /// <param name="maxVelocity">The maximum velocity this logic will accelerate the Body too.</param>
        public MoveToPointLogic(Body body, Vector2D destination, Scalar maxAcceleration, Scalar maxVelocity)
            : base(new Lifespan())
        {
            if (maxAcceleration <= 0) { throw new ArgumentOutOfRangeException("maxAcceleration", "maxAcceleration must be greater then zero"); }
            if (maxVelocity <= 0) { throw new ArgumentOutOfRangeException("maxVelocity", "maxVelocity must be greater then zero"); }
            if (body == null) { throw new ArgumentNullException("body"); }
            this.maxAcceleration = maxAcceleration;
            this.maxVelocity = maxVelocity;
            this.destination = destination;
            this.body = body;
        }

        protected internal override void RunLogic(TimeStep step)
        {
            Vector2D diff, normal, tangent;
            Scalar distance, velocity, tangentVelocity;

            Vector2D.Subtract(ref destination, ref body.State.Position.Linear, out diff);
            Vector2D.Normalize(ref diff, out distance, out normal);
            Vector2D.GetRightHandNormal(ref normal, out tangent);
            Vector2D.Dot(ref body.State.Velocity.Linear, ref normal, out velocity);
            Vector2D.Dot(ref body.State.Velocity.Linear, ref tangent, out tangentVelocity);

            if (distance < Math.Abs(step.Dt * (step.Dt * maxAcceleration - Math.Abs(velocity))))
            {
                body.State.Velocity.Linear = Vector2D.Zero;
                body.State.Position.Linear = destination;
                this.Lifetime.IsExpired = true;
            }
            else
            {
                Vector2D forceVector;
                //Normal Velocity
                Scalar accelNeeded = Math.Sign(velocity) * (velocity * velocity) / (2 * distance);
                Scalar trueAccel = (accelNeeded >= maxAcceleration) ? (-maxAcceleration) : (maxAcceleration);
                if (Math.Abs(trueAccel * step.Dt + velocity) > maxVelocity) // enforce Max velocity
                {
                    trueAccel = (maxVelocity - velocity) * step.DtInv;
                }
                Scalar force = body.Mass.Mass * trueAccel;
                Vector2D.Multiply(ref force, ref normal, out forceVector);
                body.ApplyForce(ref forceVector);

                //Tangent Velocity
                Scalar tangetAccelNeeded = -Math.Sign(tangentVelocity) * Math.Min(step.Dt * maxAcceleration, Math.Abs(tangentVelocity)) * step.DtInv;
                Scalar tangetForce = tangetAccelNeeded * body.Mass.Mass;
                Vector2D.Multiply(ref tangetForce, ref tangent, out forceVector);
                body.ApplyForce(ref forceVector);
            }
        }
    }
}