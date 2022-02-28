using System;
using System.Collections.Generic;

namespace Jypeli.GameObjects
{
    /// <summary>
    /// Harmoninen värähtelijä.
    /// </summary>
    internal abstract class Oscillator : Updatable, Destroyable
    {
        protected double t = 0;
        protected Angle prevAngle;
        protected Vector prevPos;
        protected bool distanceDecreasing = false;

        public IGameObject Object;
        public double Frequency;
        public double Phase;
        public double Damping;
        public Vector OriginalPosition;
        public Angle OriginalAngle;
        public bool stopGradually;

        public bool IsUpdated { get { return true; } }
        public bool IsDestroyed { get; private set; }

        public double W { get => 2 * Math.PI * Frequency; }

        public event Action Destroyed;

        public Oscillator( IGameObject obj, double f, double phase, double damping )
        {
            this.Object = obj;
            this.Frequency = f;
            this.Phase = phase;
            this.Damping = damping;
            this.OriginalPosition = obj.Position;
            this.OriginalAngle = obj.Angle;
        }

        public double GetDampingMultiplier()
        {
            return Math.Pow( Math.E, -Damping * t );
        }

        public void Update( Time time )
        {
            t += time.SinceLastUpdate.TotalSeconds;

            if ( GetDampingMultiplier() < 1e-12 )
            {
                Stop();
                Destroy();
            }
            if (!IsDestroyed)
            {
                Apply();
            }
        }

        protected abstract void Apply();

        public void Destroy()
        {
            IsDestroyed = true;
            if ( Destroyed != null ) Destroyed();
        }

        public abstract void Stop(bool returnToOriginalPosition = false, bool stopGradually = false);
    }

    /// <summary>
    /// Harmoninen värähtelijä akselin suhteen.
    /// </summary>
    internal class LinearOscillator : Oscillator
    {
        public Vector Center;
        public Vector Axis;
        public double Amplitude;

        public LinearOscillator( IGameObject obj, Vector axis, double a, double f, double phase, double damping )
            : base( obj, f, phase, damping )
        {
            this.Axis = axis.Normalize();
            this.Amplitude = a;
            this.Center = obj.Position - Axis * Amplitude * Math.Sin( phase );

            if ( Object is IPhysicsObject )
            {
                ( (IPhysicsObject)obj ).Velocity = Axis * Amplitude * 2 * Math.PI * f * Math.Cos( phase );
            }
        }

        public Vector GetOffset()
        {
            return Axis * Amplitude * GetDampingMultiplier() * Math.Sin(W * t + Phase);
        }

        public Vector GetVelocity()
        {
            return Axis * Amplitude * W * GetDampingMultiplier() * Math.Cos(W * t + Phase);
        }

        bool IsDynamic(IGameObject obj)
        {
            return obj is IPhysicsObject objp && objp.Mass != 0;
        }

        protected override void Apply()
        {
            if (IsDynamic(Object))
            {
                IPhysicsObject physObj = (IPhysicsObject)Object;
                double d = (Object.Position - Center).ScalarProjection(Axis);
                double angularFreq = 2 * Math.PI * Frequency;
                double k = Math.Pow(angularFreq, 2) * physObj.Mass;
                double force = -k * d;
                double dampingForce = physObj.Velocity.ScalarProjection(Axis) * Damping * physObj.Mass;
                double totalForce = force - dampingForce;

                physObj.Push(totalForce * Axis);
            }else if(Object is PhysicsObject obj)
            {
                obj.Velocity = GetVelocity();
            }
            else
            {
                Object.Position = Center + GetOffset();
            }
            if (stopGradually)
            {
                double origDst = Vector.Distance(OriginalPosition, prevPos);
                double currDst = Vector.Distance(OriginalPosition, Object.Position);
                if (!distanceDecreasing && origDst > currDst)
                {
                    distanceDecreasing = true;
                }
                if (distanceDecreasing && origDst < currDst)
                {
                    Stop(true);
                }
            }
            prevPos = Object.Position;
        }

        public override void Stop(bool returnToOriginalPosition = false, bool stopGradually = false)
        {
            if (!stopGradually)
            {
                if (Object is IPhysicsObject)
                {
                    ((IPhysicsObject)Object).Velocity = Vector.Zero;
                }
                if (returnToOriginalPosition)
                {
                    Object.Position = OriginalPosition;
                }
                Destroy();
            }
            this.stopGradually = stopGradually;
        }
    }

    /// <summary>
    /// Harmoninen värähtelijä pyörintäliikkeelle.
    /// </summary>
    internal class AngularOscillator : Oscillator
    {
        public double Direction;
        public Angle Center;
        public UnlimitedAngle Amplitude;

        public AngularOscillator( IGameObject obj, double dir, UnlimitedAngle a, double f, double damping )
            : base( obj, f, 0, damping )
        {
            this.Direction = Math.Sign( dir );
            this.Amplitude = a;
            this.Center = obj.Angle;
        }

        public UnlimitedAngle GetOffset()
        {
            return Direction * Amplitude * GetDampingMultiplier() * Math.Cos(W * t + Phase);
        }

        bool IsDynamic( IGameObject obj )
        {
            return obj is IPhysicsObject objp && objp.MomentOfInertia != 0;
        }

        public double GetAngularVelocity()
        {
            return -W * Amplitude.Radians * Math.Sin(W * t + Phase);
        }

        protected override void Apply()
        {
            if (IsDynamic(Object))
            {
                IPhysicsObject physObj = (IPhysicsObject)Object;
                double d = Math.Cos(W * t + Phase);
                double k = physObj.MomentOfInertia * Math.Pow(W, 2) * Amplitude.Radians;
                double torque = -k * d;
                double dampingTorque = physObj.AngularVelocity * Damping * physObj.MomentOfInertia;
                double totalTorque = torque - dampingTorque;

                physObj.ApplyTorque(totalTorque);
            }
            else if (Object is PhysicsObject obj)
            {
                obj.AngularVelocity = GetAngularVelocity();
            }
            else
            {
                Object.Angle = Angle.Sum(Center, GetOffset());
            }
            if (stopGradually)
            {
                double origDst = (OriginalAngle - prevAngle).Degrees;
                double currDst = (OriginalAngle - Object.Angle).Degrees;
                if (!distanceDecreasing && origDst > currDst)
                {
                    distanceDecreasing = true;
                }
                if (distanceDecreasing && origDst < currDst)
                {
                    Stop(true);
                }
            }
            prevAngle = Object.Angle;
        }

        public override void Stop(bool returnToOriginalPosition = false, bool stopGradually = false)
        {
            if (!stopGradually)
            {
                if (Object is IPhysicsObject)
                {
                    ((IPhysicsObject)Object).AngularVelocity = 0;
                }
                if (returnToOriginalPosition)
                {
                    Object.Angle = OriginalAngle;
                }
                
                Destroy();
            }
            else
            {
                double T = 1 / Frequency;
                Timer.SingleShot(T - t % T, () => Stop(returnToOriginalPosition));
            }
        }
    }
}
