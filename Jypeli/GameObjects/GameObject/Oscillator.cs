using System;

namespace Jypeli.GameObjects
{
    /// <summary>
    /// Harmoninen värähtelijä.
    /// </summary>
    internal abstract class Oscillator : Updatable, Destroyable
    {
        protected double t = 0;

        public IGameObject Object;
        public double Frequency;
        public double Phase;
        public double Damping;

        public bool IsUpdated { get { return true; } }
        public bool IsDestroyed { get; private set; }

        public event Action Destroyed;

        public Oscillator( IGameObject obj, double f, double phase, double damping )
        {
            this.Object = obj;
            this.Frequency = f;
            this.Phase = phase;
            this.Damping = damping;
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

            Apply();
        }

        protected abstract void Apply();

        public void Destroy()
        {
            IsDestroyed = true;
            if ( Destroyed != null ) Destroyed();
        }

        public abstract void Stop();
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
            return Axis * Amplitude * GetDampingMultiplier() * Math.Sin( 2 * Math.PI * Frequency * t + Phase );
        }

        bool IsDynamic( IGameObject obj )
        {
            return obj is IPhysicsObject && !double.IsInfinity( ( (IPhysicsObject)obj ).Mass );
        }

        protected override void Apply()
        {
            if ( IsDynamic( Object ) )
            {
                IPhysicsObject physObj = (IPhysicsObject)Object;
                double d = ( Object.Position - Center ).ScalarProjection( Axis );
                double angularFreq = 2 * Math.PI * Frequency;
                double k = Math.Pow( angularFreq, 2 ) * physObj.Mass;
                double force = -k * d;
                double dampingForce = physObj.Velocity.ScalarProjection( Axis ) * Damping * physObj.Mass;
                double totalForce = force - dampingForce;

                physObj.Push( totalForce * Axis );
            }
            else
            {
                Object.Position = Center + GetOffset();
            }
        }

        public override void Stop()
        {
            if ( Object is IPhysicsObject )
            {
                ( (IPhysicsObject)Object ).StopAxial( Axis );
            }
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

            if ( Object is IPhysicsObject )
            {
                ( (IPhysicsObject)obj ).AngularVelocity = Direction * 2 * Amplitude.Radians * f;
            }
        }

        public UnlimitedAngle GetOffset()
        {
            return Direction * Amplitude * GetDampingMultiplier() * Math.Sin( 2 * Math.PI * Frequency * t + Phase );
        }

        bool IsDynamic( IGameObject obj )
        {
            return obj is IPhysicsObject && !double.IsInfinity( ( (IPhysicsObject)obj ).MomentOfInertia );
        }

        protected override void Apply()
        {
            if ( IsDynamic( Object ) )
            {
                IPhysicsObject physObj = (IPhysicsObject)Object;
                double d = Math.PI * ( Object.Angle.Radians - Center.Radians );
                double k = Math.Pow( Frequency, 2 ) * physObj.MomentOfInertia;
                double torque = -k * d;
                double dampingTorque = physObj.AngularVelocity * Damping * physObj.MomentOfInertia;
                double totalTorque = torque - dampingTorque;

                physObj.ApplyTorque( totalTorque );
            }
            else
            {
                Object.Angle = Angle.Sum( Center, GetOffset() );
            }
        }

        public override void Stop()
        {
            if ( Object is IPhysicsObject )
            {
                ( (IPhysicsObject)Object ).StopAngular();
            }
        }
    }
}
