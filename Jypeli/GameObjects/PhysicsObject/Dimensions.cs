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

        public override Vector Position
        {
            get { return Body.Position; }
            set 
            {
                if (Parent != null)
                    _prevRelPos += Position - value;
                Body.Position = value;
            }
        }

        [Save]
        public override Angle Angle
        {
            get { return Angle.FromRadians( Body.Angle ); }
            set 
            {
                if (Parent != null)
                    _prevRelAngle += Angle - value;
                Body.Angle = value.Radians; 
            }
        }

        [Save]
        public override Vector Size
        {
            get { return (Vector)Body.Size; }
            set { Body.Size = value; }
        }

        [Save]
        public override Shape Shape
        {
            get { return Body.Shape; }
            set { Body.Shape = value; }
        }
    }
}
