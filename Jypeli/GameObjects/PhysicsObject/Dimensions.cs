using Jypeli.Physics;

namespace Jypeli
{
    public partial class PhysicsObject : GameObject, IPhysicsObjectInternal
    {
        private BoundingRectangle _bRect = new BoundingRectangle();

        [Save]
        public IPhysicsBody Body { get; private set; }

        /// <summary>
        /// Olion sisältävä laatikko törmäyskäsittelyä varten.
        /// </summary>
        /// <value>The bounding rectangle.</value>
        public BoundingRectangle BoundingRectangle
        {
            get
            {
                _bRect.Position = this.Position;
                _bRect.Size = this.Size;
                return _bRect;
            }
        }

        private Vector prevPos;
        public override Vector Position
        {
            get { return Body.Position; }
            set 
            {
                if (_childObjects?.Count != 0)
                {
                    Vector change = value - prevPos;
                    AdjustChildPosition(change, Angle.Zero);
                }
                Body.Position = value;
                prevPos = value;
                if (Parent != null)
                    Body.RegenerateConnectedFixtures();
            }
        }

        private Angle prevAngle;
        [Save]
        public override Angle Angle
        {
            get { return Angle.FromRadians( Body.Angle ); }
            set 
            {
                if (_childObjects?.Count != 0)
                {
                    Angle change = value - prevAngle;
                    AdjustChildPosition(Vector.Zero, change);
                }
                Body.Angle = value.Radians;
                prevAngle = value;
                if (Parent != null)
                    Body.RegenerateConnectedFixtures();
            }
        }

        [Save]
        public override Vector Size
        {
            get { return (Vector)Body.Size; }
            set 
            { 
                Body.Size = value;
                /*if (Parent != null) // TODO: tästä tulee stackoverflow
                    Body.RegenerateConnectedFixtures();*/
            }
        }

        [Save]
        public override Shape Shape
        {
            get { return Body.Shape; }
            set 
            {
                Body.Shape = value;
                if (Parent != null)
                    Body.RegenerateConnectedFixtures();
            }
        }
    }
}
